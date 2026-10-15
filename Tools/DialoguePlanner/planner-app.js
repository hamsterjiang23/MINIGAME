(function () {
  'use strict';
  const C = window.DialoguePlannerCore, $ = id => document.getElementById(id);
  const labels = { line: '发言', choice: '选项', measure: '处置' };
  let pkg = JSON.parse($('initial-package').textContent), parsed, selected = '', preview = null;
  let dirty = false, formDirty = false, sourceDirty = false, metadataDirty = false, finishTimer = null, previewSelection = -1, choiceNode = '';
  const el = (tag, text, className) => { const x = document.createElement(tag); if (text !== undefined) x.textContent = text; if (className) x.className = className; return x; };
  function markDirty() { dirty = true; renderStatus(); }
  function renderStatus() {
    $('package-status').textContent = (pkg.packageId || '未命名') + ' / v' + (pkg.contentVersion || '?') + ' / 离线项目';
    $('save-status').textContent = dirty || formDirty || sourceDirty || metadataDirty ? '有未保存修改' : '当前项目未改动';
    $('save-status').className = dirty || formDirty || sourceDirty || metadataDirty ? 'dirty' : '';
  }
  function field(parent, title, value, options = {}) {
    const label = el('label', undefined, 'field'), cap = el('span', title); label.append(cap);
    const control = el(options.multiline ? 'textarea' : options.values ? 'select' : 'input');
    if (options.values) for (const entry of options.values) { const item = typeof entry === 'string' ? { value: entry, label: entry } : entry; const opt = el('option', item.label); opt.value = item.value; control.append(opt); }
    control.value = value ?? ''; if (options.type) control.type = options.type;
    if (options.readOnly) control.readOnly = true;
    if (options.id) control.dataset.field = options.id;
    label.append(control); parent.append(label); return control;
  }
  function errors() { return parsed.diagnostics.filter(d => d.severity === 'error'); }
  function choose(id) {
    if (!discardDraft()) return;
    selected = id; renderNodes(); renderEditor();
  }
  function discardDraft() {
    if ((formDirty || sourceDirty || metadataDirty) && !window.confirm('当前表单、元数据或源码草稿尚未应用。放弃草稿并切换？')) return false;
    formDirty = false; sourceDirty = false; metadataDirty = false; $('source').value = pkg.yarnSource || ''; renderMetadata(); renderStatus(); return true;
  }
  function recalc(resetPreview = true) {
    parsed = C.validatePackage(pkg);
    if (!parsed.nodes.some(n => n.id === selected)) selected = parsed.nodes[0]?.id || '';
    if (resetPreview) { if (finishTimer) clearTimeout(finishTimer); finishTimer = null; preview = errors().length ? null : C.createPreview(pkg); }
    $('source').value = pkg.yarnSource || ''; formDirty = false; sourceDirty = false; metadataDirty = false; previewSelection = -1; choiceNode = '';
    renderNodes(); renderEditor(); renderDiagnostics(); renderMetadata(); renderStatus(); renderPreview();
  }
  function renderNodes() {
    const list = $('nodes'); list.replaceChildren();
    const search = $('filter').value.trim().toLowerCase();
    $('node-count').textContent = String(parsed.nodes.length);
    for (const node of parsed.nodes) {
      if (search && ![node.id, node.round, node.text, ...node.options.map(o => o.text)].join(' ').toLowerCase().includes(search)) continue;
      const button = el('button', undefined, 'node-item' + (node.id === selected ? ' active' : ''));
      const title = el('span', node.id, 'node-id'); title.append(el('span', labels[node.kind], 'node-type'));
      button.append(title, el('span', (node.round ? node.round + ' · ' : '') + (node.text || node.options.map(o => o.text).join(' / ')), 'node-caption'));
      button.onclick = () => choose(node.id); list.append(button);
    }
    for (const id of ['add-line', 'add-choice', 'clone', 'delete']) $(id).disabled = errors().some(d => d.code.startsWith('unsupported') || ['missing-end', 'missing-body', 'measure-effect', 'escape'].includes(d.code));
  }
  function renderEditor() {
    const box = $('editor'); box.replaceChildren();
    const unsupported = parsed.diagnostics.some(d => ['unsupported-header', 'unsupported-option', 'unsupported-body', 'unsupported-line', 'unsupported-text', 'missing-body', 'missing-end', 'measure-effect', 'escape'].includes(d.code));
    if (unsupported) { box.append(el('p', '源码包含不支持的语法。已保留原文；请打开 Yarn 源码修正，表单编辑与预览已暂停。', 'notice error')); return; }
    const original = parsed.nodes.find(n => n.id === selected);
    if (!original) { box.append(el('p', '选择或新建节点。', 'notice')); return; }
    const node = C.copy(original);
    const heading = el('div', undefined, 'panel-heading'); heading.append(el('h2', node.id), el('span', labels[node.kind], 'muted')); box.append(heading);
    box.append(el('p', 'ID 稳定保留；新增或复制时指定新 ID。', 'muted'));
    const grid = el('div', undefined, 'form-grid'); box.append(grid);
    field(grid, '轮次', node.round, { id: 'round' });
    if (node.kind === 'line') {
      field(grid, '立绘 ID', node.portrait, { id: 'portrait', values: [{ value: '', label: '请选择' }, ...(Array.isArray(pkg.portraits) ? pkg.portraits.map(p => ({ value: p.id, label: p.id + ' / ' + (p.label || p.id) })) : [])] });
      field(box, '发言角色', node.speaker, { id: 'speaker' });
      field(box, '台词', node.text, { id: 'text', multiline: true });
      field(box, '后继节点 ID', node.next, { id: 'next' });
    } else {
      const optionsBox = el('div'); box.append(optionsBox); let addOption = null;
      function renderOptions() {
        optionsBox.replaceChildren();
        if (addOption) addOption.disabled = node.options.length >= 4;
        node.options.forEach((option, index) => {
          const card = el('section', undefined, 'option-card'); card.dataset.option = index;
          const tools = el('div', undefined, 'option-tools'); tools.append(el('h3', '回复 ' + (index + 1)));
          const buttons = el('div', undefined, 'row');
          for (const [text, action, disabled] of [['↑', () => moveOption(index, -1), index === 0], ['↓', () => moveOption(index, 1), index === node.options.length - 1], ['删除回复', () => { captureOptions(); node.options.splice(index, 1); formDirty = true; renderOptions(); renderStatus(); }, node.kind === 'measure']]) {
            const button = el('button', text); button.type = 'button'; button.disabled = disabled; button.onclick = action; buttons.append(button);
          }
          tools.append(buttons); card.append(tools);
          field(card, '回复 ID', option.id, { id: 'id', readOnly: true });
          field(card, '回复文字', option.text, { id: 'text', multiline: true });
          const row = el('div', undefined, 'form-grid'); card.append(row);
          if (node.kind === 'choice') field(row, '信任增减', option.trust, { id: 'trust', type: 'number' });
          else field(row, '处置类型', option.disposition, { id: 'disposition', values: [{ value: 'treat', label: '治疗' }, { value: 'purify', label: '净化' }, { value: 'release', label: '释放' }] });
          field(row, '后继节点 ID', option.next, { id: 'next' }); optionsBox.append(card);
        });
      }
      function captureOptions() { for (const card of optionsBox.querySelectorAll('[data-option]')) for (const control of card.querySelectorAll('[data-field]')) node.options[Number(card.dataset.option)][control.dataset.field] = control.dataset.field === 'trust' ? Number(control.value) : control.value.trim(); }
      function moveOption(index, delta) { captureOptions(); const next = index + delta; [node.options[index], node.options[next]] = [node.options[next], node.options[index]]; formDirty = true; renderOptions(); renderStatus(); }
      renderOptions();
      if (node.kind === 'choice') { const add = el('button', '＋添加回复'); addOption = add; add.disabled = node.options.length >= 4; add.onclick = () => { captureOptions(); const id = window.prompt('新回复的稳定 ID', node.id.replace(/_CHOICE$/, '') + '_O' + (node.options.length + 1)); if (!id) return; if (!C.ID.test(id) || allIds().has(id) || node.options.some(o => o.id === id)) { alert('ID不合法或已存在。'); return; } node.options.push({ id, text: '请输入回复', trust: 0, next: pkg.profile.endNode, disposition: '' }); formDirty = true; renderOptions(); add.disabled = node.options.length >= 4; renderStatus(); }; box.append(add); }
    }
    const refs = C.references(parsed.nodes, node.id);
    box.append(el('p', '被引用：' + (refs.map(r => r.node + '.' + r.field).join('、') || '无') + (pkg.profile?.startNode === node.id ? '；当前入口节点' : ''), 'notice'));
    const apply = el('button', '应用节点修改', 'primary'); apply.onclick = () => {
      if ((metadataDirty || sourceDirty) && !confirm('应用节点会放弃尚未应用的元数据或源码草稿。继续？')) return;
      for (const control of box.querySelectorAll('[data-field]')) {
        const card = control.closest('[data-option]');
        if (card) node.options[Number(card.dataset.option)][control.dataset.field] = control.dataset.field === 'trust' ? Number(control.value) : control.value.trim();
        else node[control.dataset.field] = control.value.trim();
      }
      if ((node.kind === 'line' && (!node.text || !node.speaker || /\r|\n/.test(node.text + node.speaker) || /[:<>#{}\[\]\\]/.test(node.speaker))) || node.options.some(o => !o.text || /\r|\n/.test(o.text) || !Number.isInteger(o.trust) || o.trust < -2147483648 || o.trust > 2147483647)) { alert('台词与回复必须非空且为单行；角色名不能含冒号或Yarn控制字符；信任必须为32位有符号整数。中文会在终端自动换行。'); return; }
      pkg.yarnSource = C.replaceNode(pkg.yarnSource, original, node); markDirty(); recalc();
    }; box.append(apply);
    box.oninput = () => { formDirty = true; renderStatus(); };
  }
  function allIds() { return new Set(parsed.nodes.flatMap(n => [n.id, ...n.options.map(o => o.id)])); }
  function addNode(kind) {
    if (!discardDraft()) return;
    const id = prompt('新节点的稳定 ID', 'D1_NEW_' + (parsed.nodes.length + 1));
    if (!id) return; if (!C.ID.test(id) || allIds().has(id) || id === pkg.profile?.endNode) { alert('ID不合法或已存在。'); return; }
    const parent = parsed.nodes.find(n => n.id === selected);
    const node = { id, kind, round: parent?.round || 'R1', portrait: pkg.portraits?.[0]?.id || '', speaker: pkg.profile?.patientName || '角色', text: '请输入台词', next: pkg.profile?.endNode || 'END', options: kind === 'choice' ? [{ id: id + '_O1', text: '请输入回复', trust: 0, next: pkg.profile?.endNode || 'END', disposition: '' }] : [] };
    pkg.yarnSource += '\n\n' + C.serializeNode(node) + '\n'; selected = id; markDirty(); recalc();
  }
  function renderMetadata() {
    const box = $('metadata'); box.replaceChildren();
    const controls = [];
    for (const key of ['packageId', 'contentVersion']) controls.push({ target: pkg, key, control: field(box, key === 'packageId' ? '包 ID' : '内容版本', pkg[key]) });
    const profile = pkg.profile || {};
    box.append(el('p', 'HTML 对照演示配置；不覆盖正式游戏规则。', 'notice'));
    const titles = { patientId: '角色 ID', patientName: '角色名', day: '天数', startNode: '入口节点', endNode: '结束标记', initialTrust: '初始信任', releaseThreshold: '释放阈值', initialQuota: '净化剩余' };
    for (const key of Object.keys(titles)) controls.push({ target: profile, key, numeric: ['day', 'initialTrust', 'releaseThreshold', 'initialQuota'].includes(key), control: field(box, titles[key], profile[key]) });
    const portraits = field(box, '立绘登记（每行 ID=名称）', (Array.isArray(pkg.portraits) ? pkg.portraits : []).map(p => p.id + '=' + (p.label || p.id)).join('\n'), { multiline: true });
    const apply = el('button', '应用元数据'); apply.onclick = () => {
      if ((formDirty || sourceDirty) && !confirm('应用元数据会放弃尚未应用的表单或源码草稿。继续？')) return;
      for (const entry of controls) entry.target[entry.key] = entry.numeric ? Number(entry.control.value) : entry.control.value.trim();
      pkg.profile = profile;
      pkg.portraits = portraits.value.split(/\r?\n/).filter(v => v.trim()).map(v => { const at = v.indexOf('='); return { id: (at < 0 ? v : v.slice(0, at)).trim(), label: (at < 0 ? v : v.slice(at + 1)).trim() }; });
      markDirty(); recalc();
    }; box.append(apply); box.oninput = () => { metadataDirty = true; renderStatus(); };
  }
  function renderDiagnostics() {
    const box = $('diagnostics'); box.replaceChildren();
    const count = errors().length, warnings = parsed.diagnostics.length - count;
    $('diagnostic-count').textContent = count + ' 错误 / ' + warnings + ' 提示';
    $('publish').disabled = count > 0;
    $('restart').disabled = $('jump').disabled = count > 0;
    if (!parsed.diagnostics.length) box.append(el('p', '结构检查通过；仍需 Unity 原生编译与运行验收。', 'notice'));
    for (const d of parsed.diagnostics) {
      const button = el('button', (d.severity === 'error' ? '错误' : '提示') + ' · ' + (d.node || '内容包') + (d.field ? '.' + d.field : '') + (d.line ? ' · L' + d.line : '') + '：' + d.message, 'diagnostic ' + d.severity);
      button.onclick = () => { if (!discardDraft()) return; if (d.node) { selected = d.node; renderNodes(); renderEditor(); } showTab('source'); if (d.line) { const lines = $('source').value.split('\n'); const offset = lines.slice(0, d.line - 1).reduce((s, line) => s + line.length + 1, 0); $('source').focus(); $('source').setSelectionRange(offset, offset + (lines[d.line - 1] || '').length); $('source').scrollTop = (d.line - 1) * 20; } };
      box.append(button);
    }
  }
  function frame(className, title) { const f = el('div', undefined, 'terminal-frame ' + className); f.append(el('div', title, 'terminal-title')); const b = el('div', undefined, 'terminal-body'); f.append(b); return { frame: f, body: b }; }
  function actionButton(text, action, disabled = false) { const b = el('button', text); b.disabled = disabled; b.onclick = () => { action(); renderPreview(); $('terminal').focus({ preventScroll: true }); }; return b; }
  function renderPreview() {
    const box = $('terminal'); box.replaceChildren();
    box.append(el('div', '人员监控与复核终端        PMS / 3.14', 'term-header'), el('div', '第三安置所  /  B区  /  ' + (preview?.state.phase === 'roster' ? '对象目录' : '人员复核'), 'term-path'), el('div', 'PMS / LOCAL                 SEC.047 / CH 01', 'term-footer'));
    if (!preview) { box.append(el('p', '请先修复内容包错误，再预览。', 'notice error')); $('debug').textContent = '预览暂停'; $('trace').textContent = ''; return; }
    const s = preview.state, node = preview.map.get(s.node);
    $('debug').textContent = '演示调试 · ' + (s.node || '对象目录') + ' / 信任 ' + s.trust + ' / 阈值 ' + pkg.profile.releaseThreshold + ' / 净化剩余 ' + s.quota;
    $('trace').textContent = s.trace.join(' → ');
    if (s.phase === 'roster') {
      const grid = el('div', undefined, 'roster-grid'), list = el('div', undefined, 'roster-card'), detail = el('div', undefined, 'roster-card');
      list.append(el('div', '观察对象 / 登记名册'), el('div', pkg.profile.patientName + ' / ' + pkg.profile.patientId + '    ' + (s.done ? '已归档' : '待审'), 'roster-row'));
      detail.append(el('p', '登记档案 / 对象预览'), el('p', pkg.profile.patientName), el('p', 'DAY ' + String(pkg.profile.day).padStart(2, '0')), actionButton(s.done ? '已归档 / 禁止进入' : '进入审查  ↵', () => preview.enter(), s.done));
      grid.append(list, detail); box.append(grid);
    } else {
      const record = frame('record', '个体记录'); record.body.append(el('p', pkg.profile.patientId), el('p', '对象　' + pkg.profile.patientName), el('p', '单元　B-12'), el('p', '状态　' + (s.disposition ? '已处置' : '观察中')), el('p', '立绘　' + s.portrait), el('p', '记录摘录'), el('p', '暂无正式配置'));
      const monitor = frame('monitor', '实时监控 / B-12'); monitor.body.style.padding = '2px'; const video = el('div', undefined, 'video' + (s.phase === 'complete' ? ' lost' : s.phase === 'finishing' ? ' glitch' : ''));
      if (s.phase === 'complete') video.textContent = 'CH 01 / SIGNAL LOST';
      else if (s.phase !== 'finishing') video.append(el('div', undefined, 'placeholder-head'), el('div', undefined, 'placeholder-body'), el('div', 'CAM B12-01 / ' + s.portrait, 'portrait-label'), el('div', '立绘占位 / 尚未接入正式角色素材', 'placeholder-caption'));
      monitor.body.append(video, el('div', s.phase === 'complete' ? '视讯：断开　拾音：断开' : 'CH 01 / 视频：正常　拾音：接通'));
      const lamp = frame('lamp', '核验结果'), lampLine = el('p', '离所核验'); lampLine.append(el('span', '', 'lamp-dot ' + (s.phase === 'measure' ? (s.trust >= pkg.profile.releaseThreshold ? 'green' : 'red') : ''))); lamp.body.append(lampLine, el('p', '净化剩余　' + String(s.quota).padStart(2, '0')));
      const measures = frame('measures', '处置');
      for (const [type, text] of [['release', '释放'], ['treat', '治疗'], ['purify', '净化']]) {
        const index = node?.options.findIndex(o => o.disposition === type) ?? -1;
        measures.body.append(actionButton(text, () => pick(index), s.phase !== 'measure' || index < 0 || !preview.enabled(node.options[index])));
      }
      const speech = frame('speech', '当前发言'); speech.body.textContent = s.speech;
      const answers = frame('answers', s.history ? '会谈记录' : '应答 / ' + pkg.profile.patientId);
      const history = actionButton(s.history ? '返回应答' : '查看记录', () => { s.history = !s.history; }); history.className = 'history-button'; answers.frame.append(history);
      answers.body.className = 'answer-body';
      if (s.history) for (const row of s.log) { const entry = el('div', undefined, 'history-row'); entry.append(el('span', row.speaker), document.createTextNode(row.text)); answers.body.append(entry); }
      else if (s.phase === 'line') answers.body.append(actionButton('下一句  ↵', () => step()));
      else if (s.phase === 'choice') { if (choiceNode !== s.node) { choiceNode = s.node; previewSelection = -1; } node.options.forEach((o, index) => { const button = actionButton(String(index + 1).padStart(2, '0') + '  ' + o.text, () => pick(index)); button.className = 'answer-option'; if (previewSelection === index) { button.style.background = '#455b4e'; button.style.color = '#f1f0e7'; } answers.body.append(button); }); }
      else if (s.phase === 'measure') answers.body.append(el('p', '会谈完成，请选择处置。'));
      else if (s.phase === 'complete') answers.body.append(actionButton('返回对象清单', () => { s.phase = 'roster'; }));
      box.append(record.frame, monitor.frame, lamp.frame, measures.frame, speech.frame, answers.frame);
    }
    fitPreview();
  }
  function ending() { if (preview.state.phase !== 'complete') return; preview.state.phase = 'finishing'; finishTimer = setTimeout(() => { preview.state.phase = 'complete'; finishTimer = null; renderPreview(); }, 680); }
  function step() { preview.step(); ending(); }
  function pick(index) { preview.pick(index); ending(); }
  function fitPreview() { const scale = Math.min(1.5, $('preview-viewport').parentElement.clientWidth / 640 - 0.04); $('terminal').style.transform = 'scale(' + scale + ')'; $('preview-viewport').style.height = (360 * scale) + 'px'; }
  function showTab(tab) { $('form-panel').hidden = tab !== 'form'; $('source-panel').hidden = tab !== 'source'; $('tab-form').className = tab === 'form' ? 'selected' : ''; $('tab-source').className = tab === 'source' ? 'selected' : ''; }
  function download(name, content, mime) { const url = URL.createObjectURL(new Blob([content], { type: mime })); const a = el('a'); a.href = url; a.download = name; document.body.append(a); a.click(); a.remove(); setTimeout(() => URL.revokeObjectURL(url), 1000); }
  function canSave() { if (formDirty || sourceDirty || metadataDirty) { alert('请先应用节点修改、元数据或源码草稿，再保存。'); return false; } return true; }
  $('open').onclick = () => { if (!discardDraft()) return; if (dirty && !confirm('当前项目尚未保存。继续打开其他文件？')) return; $('file').click(); };
  $('file').onchange = async event => { const file = event.target.files[0]; if (!file) return; try { const loaded = JSON.parse(await file.text()); if (!loaded || typeof loaded !== 'object' || Array.isArray(loaded)) throw new Error('文件必须是内容包JSON对象。'); if (typeof loaded.yarnSource !== 'string' || !loaded.profile || typeof loaded.profile !== 'object' || Array.isArray(loaded.profile) || !Array.isArray(loaded.portraits) || loaded.portraits.some(p => !p || typeof p !== 'object' || typeof p.id !== 'string')) throw new Error('内容包需要 yarnSource 字符串、profile 对象和 portraits 数组；原项目保持不变。'); C.validatePackage(loaded); pkg = loaded; dirty = false; selected = ''; recalc(); $('save-status').textContent = '已打开 ' + file.name; } catch (error) { alert('读取失败：' + error.message); } event.target.value = ''; };
  $('save').onclick = () => { if (!canSave()) return; download((pkg.packageId || 'dialogue') + '.project.json', JSON.stringify(pkg, null, 2) + '\n', 'application/json'); dirty = false; renderStatus(); $('save-status').textContent = '已发起项目下载；请保留本地文件'; };
  $('publish').onclick = () => { if (!canSave()) return; parsed = C.validatePackage(pkg); if (errors().length) return; download((pkg.packageId || 'dialogue') + '.dialogue.json', JSON.stringify(pkg, null, 2) + '\n', 'application/json'); dirty = false; renderStatus(); $('save-status').textContent = '已发起 Unity 包下载；请在 Unity 导入并原生编译'; };
  $('export-yarn').onclick = () => { if (canSave()) download((pkg.packageId || 'dialogue') + '.yarn', pkg.yarnSource || '', 'text/plain;charset=utf-8'); };
  $('filter').oninput = renderNodes;
  $('add-line').onclick = () => addNode('line'); $('add-choice').onclick = () => addNode('choice');
  $('clone').onclick = () => {
    if (!discardDraft()) return; const original = parsed.nodes.find(n => n.id === selected); if (!original) return;
    const id = prompt('副本节点的新 ID', original.id + '_COPY'); if (!id) return;
    if (!C.ID.test(id) || allIds().has(id) || id === pkg.profile.endNode) { alert('ID不合法或已存在。'); return; }
    const node = C.copy(original); node.id = id; node.options.forEach((o, i) => { o.id = id + '_O' + (i + 1); });
    if (node.options.some(o => allIds().has(o.id))) { alert('副本回复ID冲突，请使用其他节点ID。'); return; }
    pkg.yarnSource += '\n\n' + C.serializeNode(node) + '\n'; selected = id; markDirty(); recalc();
  };
  $('delete').onclick = () => {
    if (!discardDraft()) return; const node = parsed.nodes.find(n => n.id === selected); if (!node) return;
    const refs = C.references(parsed.nodes, node.id); const message = '删除 ' + node.id + '？\n' + (refs.length ? '仍被引用：\n' + refs.map(r => r.node + '.' + r.field).join('\n') + '\n删除后需修复这些目标。' : '没有其他节点引用。') + (pkg.profile.startNode === node.id ? '\n这是入口节点，删除后需重新设置入口。' : '');
    if (!confirm(message)) return; pkg.yarnSource = C.replaceNode(pkg.yarnSource, node, null); markDirty(); recalc();
  };
  $('source').oninput = () => { sourceDirty = true; renderStatus(); };
  $('apply-source').onclick = () => { if ((formDirty || metadataDirty) && !confirm('覆盖尚未应用的表单或元数据草稿？')) return; pkg.yarnSource = $('source').value; markDirty(); recalc(); };
  $('tab-form').onclick = () => showTab('form'); $('tab-source').onclick = () => showTab('source');
  $('restart').onclick = () => { if (finishTimer) clearTimeout(finishTimer); finishTimer = null; preview = C.createPreview(pkg); renderPreview(); };
  $('jump').onclick = () => { if (!preview || !preview.map.has(selected)) return; if (finishTimer) clearTimeout(finishTimer); finishTimer = null; preview.jump(selected); renderPreview(); };
  $('terminal').onkeydown = event => {
    if (!preview || ['INPUT', 'TEXTAREA', 'SELECT'].includes(event.target.tagName)) return;
    if (event.key === 'Escape' && preview.state.history) { preview.state.history = false; renderPreview(); event.preventDefault(); return; }
    if (preview.state.history) return;
    if (event.key === 'Enter' || event.key === ' ') { if (preview.state.phase === 'roster') preview.enter(); else if (preview.state.phase === 'line') step(); else if (preview.state.phase === 'choice' && previewSelection >= 0 && event.key === 'Enter') pick(previewSelection); else if (preview.state.phase === 'complete' && event.key === 'Enter') preview.state.phase = 'roster'; else return; renderPreview(); event.preventDefault(); }
    else if (/^[1-4]$/.test(event.key) && preview.state.phase === 'choice') { pick(Number(event.key) - 1); renderPreview(); event.preventDefault(); }
    else if (['ArrowDown', 'ArrowUp'].includes(event.key) && preview.state.phase === 'choice') { const count = preview.map.get(preview.state.node).options.length; previewSelection = previewSelection < 0 ? (event.key === 'ArrowDown' ? 0 : count - 1) : (previewSelection + (event.key === 'ArrowDown' ? 1 : count - 1)) % count; renderPreview(); event.preventDefault(); }
  };
  window.addEventListener('resize', fitPreview);
  window.addEventListener('beforeunload', event => { if (dirty || formDirty || sourceDirty || metadataDirty) { event.preventDefault(); event.returnValue = ''; } });
  recalc();
})();
