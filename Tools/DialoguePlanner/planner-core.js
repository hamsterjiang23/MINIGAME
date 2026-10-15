(function (root) {
  'use strict';
  const ID = /^[A-Za-z_][A-Za-z0-9_.-]{0,127}$/;
  const JUMP = /^<<jump ([A-Za-z_][A-Za-z0-9_.-]{0,127})>>$/;
  const MIN_INT = -2147483648, MAX_INT = 2147483647;
  const isInt = value => Number.isInteger(value) && value >= MIN_INT && value <= MAX_INT;
  const dispositions = ['treat', 'purify', 'release'];
  const copy = value => JSON.parse(JSON.stringify(value));
  const escapeText = value => String(value).replace(/[\\#<>{}\[\]]/g, char => '\\' + char);
  const unescapeText = value => value.replace(/\\([\\#<>{}\[\]])/g, '$1');
  function splitTags(value) {
    const tags = {}, regex = /(?<!\S)#([A-Za-z_][A-Za-z0-9_]*):([^\s]+)/g;
    const first = regex.exec(value);
    const text = (first ? value.slice(0, first.index) : value).trim(), tail = first ? value.slice(first.index) : '';
    regex.lastIndex = 0;
    let match;
    while ((match = regex.exec(tail))) { if (Object.hasOwn(tags, match[1])) return { text, tags, valid: false, error: 'duplicate-tag' }; tags[match[1]] = match[2]; }
    const remainder = tail.replace(/(?<!\S)#([A-Za-z_][A-Za-z0-9_]*):([^\s]+)/g, '').trim();
    if (remainder || text.includes('//') || !text) return { text, tags, valid: false, error: 'unsupported-text' };
    for (let i = 0; i < text.length; i++) {
      if (text[i] === '\\') { if (++i >= text.length || !'\\#<>[]{}'.includes(text[i])) return { text, tags, valid: false, error: 'escape' }; }
      else if ('#<>[]{}'.includes(text[i])) return { text, tags, valid: false, error: 'unsupported-text' };
    }
    return { text, tags, valid: true };
  }
  function issue(code, message, line = 0, node = '', field = '', severity = 'error') {
    return { code, message, line, node, field, severity };
  }
  function parseSource(source) {
    const nodes = [], diagnostics = [], seen = new Set();
    const lines = String(source).split(/\r\n|\n|\r/);
    const add = (code, message, line, node = '', field = '') => diagnostics.push(issue(code, message, line, node, field));
    let i = 0;
    while (i < lines.length) {
      if (!lines[i].trim() || lines[i].trim().startsWith('//')) { i++; continue; }
      const start = i, headers = {}, comments = [];
      while (i < lines.length && lines[i].trim() !== '---') {
        const text = lines[i].trim();
        if (!text || text.startsWith('//')) { if (text) comments.push(text); i++; continue; }
        const match = /^(title|portrait|round|kind|patient|day)\s*:\s*(.*?)\s*$/.exec(text);
        if (!match || Object.hasOwn(headers, match[1])) add('unsupported-header', '未知或重复的节点头；仅支持 title / portrait / round / kind / patient / day。', i + 1);
        else headers[match[1]] = match[2];
        i++;
      }
      if (i === lines.length) { add('missing-body', '缺少节点正文分隔符 ---。', start + 1); break; }
      i++;
      const body = [];
      while (i < lines.length && lines[i].trim() !== '===') { if (lines[i].trim().startsWith('//')) comments.push(lines[i].trim()); else if (lines[i].trim()) body.push({ text: lines[i].trim(), line: i + 1 }); i++; }
      if (i === lines.length) add('missing-end', '缺少节点结束分隔符 ===。', start + 1, headers.title);
      else i++;
      const node = { id: headers.title || '', portrait: headers.portrait || '', round: headers.round || '', kind: headers.kind || '', patient: headers.patient || '', day: headers.day || '', hasPatient: Object.hasOwn(headers, 'patient'), hasDay: Object.hasOwn(headers, 'day'), comments, lineTag: '', line: start + 1, start, end: i, options: [], speaker: '', text: '', next: '' };
      if (!ID.test(node.id)) add('node-id', 'ID以英文字母或下划线开头，最多128位，后续可含字母、数字、下划线、点和连字符。', start + 1, node.id, 'id');
      if (seen.has(node.id)) add('duplicate-node', '节点ID重复。', start + 1, node.id, 'id');
      seen.add(node.id);
      if (body[0]?.text.startsWith('->')) {
        for (let b = 0; b < body.length; b += 2) {
          const item = body[b], jump = body[b + 1];
          const parts = splitTags(item.text.replace(/^->\s*/, ''));
          const target = jump && JUMP.exec(jump.text);
          if (!parts.valid && parts.error === 'escape') add('escape', '反斜线只能转义反斜线或 # < > [ ] { }，不支持其他转义。', item.line, node.id, 'text');
          if (!item.text.startsWith('-> ') || !parts.valid || !parts.text || !ID.test(parts.tags.id || '') || Object.keys(parts.tags).some(t => !['id', 'trust', 'disposition', 'line'].includes(t)) || (parts.tags.trust !== undefined && (!/^[+-]?\d+$/.test(parts.tags.trust) || !isInt(Number(parts.tags.trust)))) || (parts.tags.disposition && !dispositions.includes(parts.tags.disposition)) || !target) { add('unsupported-option', '选项需要文字、#id、32位整数 #trust 或 #disposition，下一行必须为 <<jump 目标ID>>。', item.line, node.id, 'options'); continue; }
          if (!parts.tags.disposition && parts.tags.trust === undefined) add('unsupported-option', '普通选项必须声明 #trust:整数。', item.line, node.id, 'trust');
          if (parts.tags.disposition && parts.tags.trust !== undefined) add('measure-effect', '处置选项不能声明 #trust，包括 #trust:0。', item.line, node.id, 'trust');
          node.options.push({ id: parts.tags.id, text: unescapeText(parts.text), trust: Number(parts.tags.trust || 0), disposition: parts.tags.disposition || '', lineTag: parts.tags.line || '', next: target[1], line: item.line });
        }
        const inferred = node.options.some(o => o.disposition) ? 'measure' : 'choice';
        if (node.kind && node.kind !== inferred) add('node-kind', 'kind 与正文类型不一致。', node.line, node.id, 'kind');
        node.kind = inferred;
      } else {
        node.kind = node.kind || 'line';
        if (node.kind !== 'line' || body.length !== 2 || !JUMP.test(body[1]?.text || '')) {
          add('unsupported-body', '首版每个发言节点支持一条角色发言和一个 <<jump 目标ID>>；任意 Yarn 条件、变量、命令需先扩展导入器。', node.line, node.id, 'body');
        } else {
          const speech = /^([^:]+):\s*(.+)$/.exec(body[0].text), parts = splitTags(speech ? speech[2] : '');
          if (!parts.valid && parts.error === 'escape') add('escape', '反斜线只能转义反斜线或 # < > [ ] { }，不支持其他转义。', body[0].line, node.id, 'text');
          if (!speech || !parts.valid || Object.keys(parts.tags).some(t => t !== 'line') || /[<>#{}\[\]\\]/.test(speech[1])) add('unsupported-line', '发言格式：角色名: 台词；角色名不能含控制字符，台词不支持插值和额外标签。', body[0].line, node.id, 'text');
          else { node.speaker = speech[1].trim(); node.text = unescapeText(parts.text); node.lineTag = parts.tags.line || ''; }
          node.next = JUMP.exec(body[1].text)[1];
        }
      }
      nodes.push(node);
      if (nodes.length > 1024) { add('node-limit', '首版最多支持1024个源节点。', node.line, node.id); break; }
    }
    if (!nodes.length) add('empty-source', '没有找到可编辑的 Yarn 节点。', 1);
    return { nodes, diagnostics, source: String(source) };
  }
  function validatePackage(pkg) {
    const diagnostics = [];
    if (!pkg || typeof pkg !== 'object' || Array.isArray(pkg)) return { nodes: [], source: '', diagnostics: [issue('package', '文件必须是对话内容包 JSON 对象。')] };
    for (const key of Object.keys(pkg)) if (!['formatVersion', 'packageId', 'contentVersion', 'yarnSource', 'portraits', 'profile'].includes(key)) diagnostics.push(issue('package-field', '不支持的包字段：' + key, 0, '', key));
    if (pkg.formatVersion !== 1) diagnostics.push(issue('version', '只支持 formatVersion: 1。'));
    if (JSON.stringify(pkg).length > 4 * 1024 * 1024) diagnostics.push(issue('package-size', 'JSON内容包不能超过4 MiB字符。'));
    for (const key of ['packageId', 'contentVersion', 'yarnSource']) if (typeof pkg[key] !== 'string' || !pkg[key].trim()) diagnostics.push(issue('metadata', key + ' 必须为非空字符串。', 0, '', key));
    const parsed = parseSource(typeof pkg.yarnSource === 'string' ? pkg.yarnSource : '');
    if (typeof pkg.yarnSource === 'string' && pkg.yarnSource.length > 2 * 1024 * 1024) diagnostics.push(issue('source-size', 'Yarn 源码不能超过2 MiB字符。', 1));
    if (!ID.test(pkg.packageId || '') || typeof pkg.contentVersion === 'string' && pkg.contentVersion.length > 128) diagnostics.push(issue('package-id', '包ID必须为稳定ASCII标识，内容版本最多128字符。', 0, '', 'packageId'));
    diagnostics.push(...parsed.diagnostics);
    const profile = pkg.profile || {};
    for (const key of Object.keys(profile)) if (!['patientId', 'patientName', 'day', 'startNode', 'endNode', 'initialTrust', 'releaseThreshold', 'initialQuota'].includes(key)) diagnostics.push(issue('profile-field', '不支持的规则字段：' + key, 0, '', 'profile.' + key));
    for (const key of ['patientId', 'patientName', 'startNode', 'endNode']) if (typeof profile[key] !== 'string' || !profile[key].trim()) diagnostics.push(issue('profile', 'profile.' + key + ' 必须为非空字符串。', 0, '', 'profile.' + key));
    for (const key of ['day', 'initialTrust', 'releaseThreshold', 'initialQuota']) if (!isInt(profile[key])) diagnostics.push(issue('profile', 'profile.' + key + ' 必须为32位有符号整数。', 0, '', 'profile.' + key));
    for (const key of ['patientId', 'startNode', 'endNode']) if (!ID.test(profile[key] || '')) diagnostics.push(issue('profile-id', 'profile.' + key + ' 必须为稳定ASCII标识。', 0, '', 'profile.' + key));
    if (profile.day !== 1 || profile.initialQuota < 0) diagnostics.push(issue('profile-range', '首版仅接受 Day1 独立演示，initialQuota 不能为负数。'));
    const portraitIds = new Set();
    if (!Array.isArray(pkg.portraits)) diagnostics.push(issue('portraits', 'portraits 必须为数组。'));
    for (const portrait of Array.isArray(pkg.portraits) ? pkg.portraits : []) {
      if (!portrait || typeof portrait.id !== 'string' || !ID.test(portrait.id) || portraitIds.has(portrait.id)) diagnostics.push(issue('portrait-id', '立绘ID为空、不合法或重复。'));
      else portraitIds.add(portrait.id);
      if (!portrait || typeof portrait.label !== 'string' || !portrait.label.trim()) diagnostics.push(issue('portrait-label', '立绘必须具有非空名称。'));
      if (portrait) for (const key of Object.keys(portrait)) if (!['id', 'label'].includes(key)) diagnostics.push(issue('portrait-field', '不支持的立绘字段：' + key));
    }
    const map = new Map(parsed.nodes.map(n => [n.id, n])), allIds = new Set();
    if (!map.has(profile.startNode)) diagnostics.push(issue('start', '入口节点不存在：' + profile.startNode, 0, '', 'profile.startNode'));
    if (!ID.test(profile.endNode || '') || map.has(profile.endNode)) diagnostics.push(issue('end', '结束ID必须合法且作为外部终点，不能同时定义节点。', 0, '', 'profile.endNode'));
    const checkTarget = (target, node, field, line) => { if (!map.has(target) && target !== profile.endNode) diagnostics.push(issue('missing-target', '目标不存在：' + target, line, node, field)); };
    for (const node of parsed.nodes) {
      if (node.kind !== 'measure' && !node.round) diagnostics.push(issue('round', '发言和普通选项节点必须声明 round。', node.line, node.id, 'round'));
      if (node.hasPatient && node.patient !== profile.patientId) diagnostics.push(issue('patient', '节点 patient 与包 profile.patientId 不一致。', node.line, node.id, 'patient'));
      if (node.hasDay && (!/^\d+$/.test(node.day) || Number(node.day) !== profile.day)) diagnostics.push(issue('day', '节点 day 与包 profile.day 不一致。', node.line, node.id, 'day'));
      if (node.portrait && !portraitIds.has(node.portrait)) diagnostics.push(issue('unknown-portrait', '未登记立绘：' + node.portrait, node.line, node.id, 'portrait'));
      if (node.kind === 'line') {
        checkTarget(node.next, node.id, 'next', node.line);
        if (!node.portrait) diagnostics.push(issue('unknown-portrait', '发言节点缺少立绘ID。', node.line, node.id, 'portrait'));
        if (node.text.length > 32) diagnostics.push(issue('long-text', '台词超过32字，请在640×360预览检查换行。', node.line, node.id, 'text', 'warning'));
      } else {
        if (node.options.length < 1 || node.options.length > 4) diagnostics.push(issue('option-count', '每组选项必须为1至4项。', node.line, node.id, 'options'));
        const acts = new Set();
        for (const option of node.options) {
          if (!isInt(option.trust)) diagnostics.push(issue('trust-range', '信任变化必须为32位有符号整数。', option.line, node.id, option.id + '.trust'));
          if (allIds.has(option.id)) diagnostics.push(issue('duplicate-id', '选项ID重复：' + option.id, option.line, node.id, 'options'));
          allIds.add(option.id);
          checkTarget(option.next, node.id, option.id + '.next', option.line);
          if (node.kind === 'measure' && (!dispositions.includes(option.disposition) || acts.has(option.disposition))) diagnostics.push(issue('disposition', '处置必须具有唯一的 treat / purify / release 类型。', option.line, node.id, 'options'));
          if (node.kind === 'choice' && option.disposition) diagnostics.push(issue('disposition', '普通选项不能包含处置。', option.line, node.id));
          acts.add(option.disposition);
          if (option.text.length > 30) diagnostics.push(issue('long-option', '回复超过30字，请检查终端是否溢出。', option.line, node.id, option.id, 'warning'));
        }
        if (node.kind === 'measure' && dispositions.some(d => !acts.has(d))) diagnostics.push(issue('dispositions', '措施节点需要治疗、净化、释放三种处置。', node.line, node.id));
      }
    }
    const reached = new Set(), visiting = new Set();
    function walk(id) {
      if (!map.has(id)) return;
      if (visiting.has(id)) { diagnostics.push(issue('cycle', '检测到循环跳转，首版需要有限会谈。', map.get(id).line, id, 'next')); return; }
      if (reached.has(id)) return;
      reached.add(id); visiting.add(id);
      const n = map.get(id); (n.kind === 'line' ? [n.next] : n.options.map(o => o.next)).forEach(walk);
      visiting.delete(id);
    }
    walk(profile.startNode);
    const reachable = new Set(reached);
    for (const n of parsed.nodes) walk(n.id);
    for (const n of parsed.nodes) if (!reachable.has(n.id)) diagnostics.push(issue('unreachable', '入口无法到达此节点。', n.line, n.id, '', 'warning'));
    if (!diagnostics.some(d => d.severity === 'error')) validateRounds(profile, map, diagnostics);
    return { ...parsed, diagnostics };
  }
  function validateRounds(profile, map, diagnostics) {
    const fail = (code, message, node, field = '') => { diagnostics.push(issue(code, message, node?.line || 0, node?.id || '', field)); return false; };
    const rounds = [], names = new Set(); let cursor = profile.startNode;
    while (cursor !== profile.endNode) {
      const entry = map.get(cursor); if (entry.kind === 'measure') break;
      if (!entry.round || names.has(entry.round)) { fail('round', '连续会谈组必须具有互不重复的 round。', entry, 'round'); return; }
      names.add(entry.round); let choice = entry;
      while (choice.kind === 'line') {
        if (choice.round !== entry.round) { fail('intro-round', '轮首发言必须先到达本轮选项，不能跳进另一轮。', choice, 'next'); return; }
        if (choice.next === profile.endNode) { fail('intro-end', '轮首发言不能直接跳到外部结束标记。', choice, 'next'); return; }
        choice = map.get(choice.next);
      }
      if (choice.kind !== 'choice' || choice.round !== entry.round) { fail('round-choice', '每个普通会谈组必须进入同一轮的选项组。', choice, 'round'); return; }
      let nextRound = null;
      for (const option of choice.options) {
        let target = option.next;
        while (target !== profile.endNode) {
          const n = map.get(target);
          if (n.kind === 'measure' || n.round !== entry.round) break;
          if (n.kind !== 'line') { fail('nested-choice', '选项回应只能包含发言，直到下一轮开始。', n); return; }
          target = n.next;
        }
        if (target === profile.endNode) { fail('early-end', '普通回复必须进入下一会谈组或处置，不能直接结束。', { id: choice.id, line: option.line }, option.id + '.next'); return; }
        if (nextRound !== null && nextRound !== target) { fail('nonconvergent-round', '同组选项必须汇合到同一个下一轮入口。', { id: choice.id, line: option.line }, option.id + '.next'); return; }
        nextRound = target;
      }
      rounds.push({ entry, choice }); cursor = nextRound;
      if (rounds.length > 128) { fail('round-limit', '首版最多支持128个连续会谈组。', entry); return; }
    }
    if (!rounds.length || cursor === profile.endNode || map.get(cursor)?.kind !== 'measure') { fail('measure-route', '至少需要一个普通会谈组，且最终进入处置节点。', map.get(cursor) || map.get(profile.startNode)); return; }
    const measure = map.get(cursor);
    for (const option of measure.options) {
      let target = option.next, count = 0;
      while (target !== profile.endNode) { const n = map.get(target); if (n.kind !== 'line') { fail('reaction-route', '处置反应只能经过发言节点，最终进入结束标记。', n); return; } count++; target = n.next; }
      if (!count) { fail('reaction-empty', '每种处置至少需要一条反应发言。', { id: measure.id, line: option.line }, option.id + '.next'); return; }
    }
    if ([...map.values()].filter(n => n.kind === 'measure').length !== 1) { fail('measure-count', '首版内容包必须恰好包含一个处置节点。', measure); return; }
    let min = profile.initialTrust, max = profile.initialTrust;
    for (const { entry, choice } of rounds) {
      min += Math.min(...choice.options.map(o => o.trust)); max += Math.max(...choice.options.map(o => o.trust));
      if (min < MIN_INT || max > MAX_INT) { fail('trust-overflow', '至少一条回复路线会导致核心32位信任整数溢出。', entry, 'trust'); return; }
    }
  }
  function serializeNode(node) {
    const header = ['title: ' + node.id, ...(node.portrait ? ['portrait: ' + node.portrait] : []), ...(node.round ? ['round: ' + node.round] : []), ...(node.hasPatient || node.patient ? ['patient: ' + node.patient] : []), ...(node.hasDay || node.day ? ['day: ' + node.day] : []), 'kind: ' + node.kind, '---'];
    const body = node.kind === 'line' ? [node.speaker + ': ' + escapeText(node.text) + (node.lineTag ? ' #line:' + node.lineTag : ''), '<<jump ' + node.next + '>>'] : node.options.flatMap(o => ['-> ' + escapeText(o.text) + ' #id:' + o.id + (o.disposition ? ' #disposition:' + o.disposition : ' #trust:' + o.trust) + (o.lineTag ? ' #line:' + o.lineTag : ''), '    <<jump ' + o.next + '>>']);
    return [...header, ...(node.comments || []), ...body, '==='].join('\n');
  }
  function replaceNode(source, before, after) {
    const eol = source.match(/\r\n|\n|\r/)?.[0] || '\n';
    const lines = source.split(/\r\n|\n|\r/);
    const replacement = after ? serializeNode(after).split('\n') : [];
    lines.splice(before.start, before.end - before.start, ...replacement);
    return lines.join(eol);
  }
  function references(nodes, id) {
    return nodes.flatMap(n => n.kind === 'line' ? (n.next === id ? [{ node: n.id, field: 'next' }] : []) : n.options.filter(o => o.next === id).map(o => ({ node: n.id, field: o.id + '.next' })));
  }
  function createPreview(pkg) {
    const parsed = validatePackage(pkg);
    if (parsed.diagnostics.some(d => d.severity === 'error')) throw new Error('请先修复内容包错误。');
    const map = new Map(parsed.nodes.map(n => [n.id, n]));
    const state = { node: '', trust: pkg.profile.initialTrust, quota: pkg.profile.initialQuota, phase: 'roster', portrait: '', speech: '', disposition: '', log: [], trace: [], history: false, done: false };
    function go(id) {
      state.node = id; state.trace.push(id);
      if (id === pkg.profile.endNode) { state.phase = 'complete'; state.done = true; return state; }
      const node = map.get(id); if (!node) throw new Error('缺失节点：' + id);
      state.phase = node.kind;
      if (node.kind === 'line') { state.speech = node.text; state.portrait = node.portrait; state.log.push({ speaker: node.speaker, text: node.text }); }
      return state;
    }
    return { state, map, enter() { if (!state.done && state.phase === 'roster') go(pkg.profile.startNode); return state; }, jump(id) { state.history = false; state.trust = pkg.profile.initialTrust; state.quota = pkg.profile.initialQuota; state.log = []; state.trace = []; state.disposition = ''; state.done = false; return go(id); }, step() { if (!state.history && state.phase === 'line') go(map.get(state.node).next); return state; }, enabled(option) { return !state.history && !state.disposition && (option.disposition !== 'release' || state.trust >= pkg.profile.releaseThreshold); }, pick(index) { const n = map.get(state.node), o = n?.options[index]; if (state.history || !['choice', 'measure'].includes(state.phase) || !o || !this.enabled(o)) return state; state.trust += o.trust; state.log.push({ speaker: '值班员', text: o.disposition ? '【处置】' + o.text : o.text }); if (o.disposition) { state.disposition = o.disposition; if (o.disposition === 'purify') state.quota--; } return go(o.next); } };
  }
  const api = { parseSource, validatePackage, serializeNode, replaceNode, references, createPreview, copy, ID };
  if (typeof module === 'object' && module.exports) module.exports = api;
  else root.DialoguePlannerCore = api;
})(typeof globalThis === 'object' ? globalThis : this);
