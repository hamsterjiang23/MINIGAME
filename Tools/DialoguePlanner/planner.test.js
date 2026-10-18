'use strict';
const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const C = require('./planner-core.js');
const root = path.resolve(__dirname, '../..');
const sample = JSON.parse(fs.readFileSync(path.join(root, 'UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json'), 'utf8'));
const original = JSON.parse(fs.readFileSync(path.join(root, 'docs/.source-data/inspection-terminal-day1-v0.5.2.data.json'), 'utf8'));
const errors = pkg => C.validatePackage(pkg).diagnostics.filter(d => d.severity === 'error');

test('V2 campaigns are rejected before parsing or replacing the current V1 document', () => {
  const campaign = JSON.parse(fs.readFileSync(path.join(root, 'UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json'), 'utf8'));
  const before = JSON.stringify(campaign), current = C.copy(sample);
  const result = C.validatePackage(campaign);
  assert.deepEqual(result.nodes, []);
  assert.equal(result.diagnostics.length, 1);
  assert.equal(result.diagnostics[0].code, 'campaign-read-only');
  assert.match(result.diagnostics[0].message, /Tools\/DialogueCampaign/);
  assert.throws(() => C.assertEditablePackage(campaign), /V2 全流程/);
  assert.throws(() => C.createPreview(campaign), /V2 全流程/);
  assert.equal(JSON.stringify(campaign), before);
  assert.deepEqual(current, sample);
  assert.doesNotThrow(() => C.assertEditablePackage(sample));
});
test('canonical package parses 50 lines, 6 choices, one measure and no errors', () => {
  const parsed = C.validatePackage(sample);
  assert.equal(parsed.nodes.filter(n => n.kind === 'line').length, 50);
  assert.equal(parsed.nodes.filter(n => n.kind === 'choice').length, 6);
  assert.equal(parsed.nodes.filter(n => n.kind === 'measure').length, 1);
  assert.deepEqual(errors(sample), []);
});
test('no-edit file roundtrip preserves source verbatim, including comments and CRLF', () => {
  const pkg = C.copy(sample); pkg.yarnSource = '// source provenance comment\r\n' + pkg.yarnSource.replace(/\r?\n/g, '\r\n');
  const reopened = JSON.parse(JSON.stringify(pkg));
  assert.equal(reopened.yarnSource, pkg.yarnSource);
  assert.deepEqual(errors(reopened), []);
});
test('targeted form edits preserve every other source node and original IDs', () => {
  const pkg = C.copy(sample), before = C.parseSource(pkg.yarnSource).nodes.find(n => n.id === 'D1_R1_OPEN1');
  const edited = { ...before, text: '修改后的台词 #1 {记号} [方括号] <箭头> \\反斜线', portrait: 'ML04', next: 'D1_R1_CHOICE' };
  pkg.yarnSource = C.replaceNode(pkg.yarnSource, before, edited);
  const parsed = C.validatePackage(pkg), after = parsed.nodes.find(n => n.id === before.id);
  assert.equal(after.text, edited.text); assert.equal(after.portrait, 'ML04'); assert.equal(after.next, edited.next);
  assert.deepEqual(parsed.nodes.map(n => n.id), C.parseSource(sample.yarnSource).nodes.map(n => n.id));
  const originalNode = C.parseSource(sample.yarnSource).nodes.find(n => n.id === 'D1_R2_OPEN1');
  const afterNode = parsed.nodes.find(n => n.id === originalNode.id);
  assert.equal(pkg.yarnSource.split('\n').slice(afterNode.start, afterNode.end).join('\n'), sample.yarnSource.split('\n').slice(originalNode.start, originalNode.end).join('\n'));
  assert.equal(parsed.diagnostics.filter(d => d.severity === 'error').length, 0);
});
test('option text/trust/next edits survive export with stable option ID and escaped text', () => {
  const pkg = C.copy(sample), node = C.parseSource(pkg.yarnSource).nodes.find(n => n.id === 'D1_R3_CHOICE');
  const edited = C.copy(node); Object.assign(edited.options[1], { text: '编辑 #回复', trust: -6, next: 'D1_R3_CLOSING1' });
  pkg.yarnSource = C.replaceNode(pkg.yarnSource, node, edited);
  const result = C.parseSource(pkg.yarnSource).nodes.find(n => n.id === node.id).options[1];
  assert.equal(result.id, 'D1_R3_O2'); assert.equal(result.text, '编辑 #回复'); assert.equal(result.trust, -6); assert.equal(result.next, 'D1_R3_CLOSING1');
  assert.deepEqual(errors(pkg), []);
});
test('unsupported Yarn stays untouched and disables preview, with source line diagnostics', () => {
  const pkg = C.copy(sample); pkg.yarnSource = pkg.yarnSource.replace('米洛: 这里还要检测吗？', '<<set $trust = 999>>\n米洛: 这里还要检测吗？');
  const saved = pkg.yarnSource, parsed = C.validatePackage(pkg);
  assert.ok(parsed.diagnostics.some(d => d.code === 'unsupported-body' && d.line > 0));
  assert.throws(() => C.createPreview(pkg), /修复/); assert.equal(pkg.yarnSource, saved);
});
test('missing targets, cycles, unknown portraits, duplicate option IDs, profile day fail with actionable locations', () => {
  const source = sample.yarnSource;
  for (const [badSource, code] of [[source.replace('<<jump D1_R1_OPEN2>>', '<<jump UNKNOWN>>'), 'missing-target'], [source.replace('<<jump D1_R1_OPEN2>>', '<<jump D1_R1_OPEN1>>'), 'cycle'], [source.replace('portrait: ML01', 'portrait: UNKNOWN'), 'unknown-portrait'], [source.replace('#id:D1_R1_O2', '#id:D1_R1_O1'), 'duplicate-id']]) {
    const failure = errors({ ...sample, yarnSource: badSource }).find(d => d.code === code);
    assert.ok(failure, code); assert.ok(failure.line > 0); assert.ok(failure.node);
  }
  const pkg = C.copy(sample); pkg.profile.day = 2; assert.ok(errors(pkg).some(d => d.code === 'profile-range'));
});
test('add, clone, delete preserve existing IDs and report dangling references', () => {
  const pkg = C.copy(sample), nodes = C.parseSource(pkg.yarnSource).nodes, originalNode = nodes[0];
  const cloned = { ...C.copy(originalNode), id: 'D1_NEW_COPY' };
  pkg.yarnSource += '\n' + C.serializeNode(cloned) + '\n';
  assert.ok(C.validatePackage(pkg).diagnostics.some(d => d.code === 'unreachable' && d.node === cloned.id));
  const removed = C.parseSource(pkg.yarnSource).nodes.find(n => n.id === 'D1_R1_OPEN2');
  assert.deepEqual(C.references(nodes, removed.id), [{ node: 'D1_R1_OPEN1', field: 'next' }]);
  pkg.yarnSource = C.replaceNode(pkg.yarnSource, removed, null);
  assert.ok(errors(pkg).some(d => d.code === 'missing-target' && d.node === 'D1_R1_OPEN1'));
  assert.ok(C.parseSource(pkg.yarnSource).nodes.some(n => n.id === cloned.id));
});
test('recognized tags accept any order and optional patient/day/line metadata', () => {
  const pkg = C.copy(sample);
  pkg.yarnSource = pkg.yarnSource.replace('#id:D1_R1_O1 #trust:0', '#trust:0 #id:D1_R1_O1').replace('title: D1_R1_OPEN1', 'title: D1_R1_OPEN1\npatient: child\nday: 1').replace('米洛: 这里还要检测吗？', '米洛: 这里还要检测吗？ #line:original_1');
  assert.deepEqual(errors(pkg), []);
  const n = C.parseSource(pkg.yarnSource).nodes[0]; assert.equal(n.patient, 'child'); assert.equal(n.day, '1'); assert.equal(n.lineTag, 'original_1');
  assert.match(C.serializeNode(n), /#line:original_1/);
});
test('form rewrite retains source comments, including a header provenance comment', () => {
  const pkg = C.copy(sample);
  pkg.yarnSource = pkg.yarnSource.replace('title: D1_R1_OPEN1', 'title: D1_R1_OPEN1\n// source: workbook / cell B12').replace('米洛: 这里还要检测吗？', '// content note\n米洛: 这里还要检测吗？');
  const n = C.parseSource(pkg.yarnSource).nodes[0]; n.text = '保留来源的修改';
  pkg.yarnSource = C.replaceNode(pkg.yarnSource, n, n);
  assert.match(pkg.yarnSource, /source: workbook \/ cell B12/); assert.match(pkg.yarnSource, /content note/); assert.deepEqual(errors(pkg), []);
});
test('native parity: possible positive and negative answer paths must remain within int32 trust', () => {
  for (const initial of [2147483647, -2147483648]) {
    const pkg = C.copy(sample); pkg.profile.initialTrust = initial;
    const d = errors(pkg).find(d => d.code === 'trust-overflow');
    assert.ok(d); assert.ok(d.line > 0); assert.equal(d.node, 'D1_R3_OPEN1'); assert.equal(d.field, 'trust');
    assert.throws(() => C.createPreview(pkg), /修复/);
  }
  const pkg = C.copy(sample); pkg.profile.initialTrust = 2147483648; assert.ok(errors(pkg).some(d => d.code === 'profile' && d.field === 'profile.initialTrust'));
});
test('native parity: a choice group must converge to the same next-round entry', () => {
  const pkg = C.copy(sample), n = C.parseSource(pkg.yarnSource).nodes.find(n => n.id === 'D1_R1_CHOICE');
  n.options[0].next = 'D1_R3_OPEN1'; pkg.yarnSource = C.replaceNode(pkg.yarnSource, n, n);
  const source = pkg.yarnSource, d = errors(pkg).find(d => d.code === 'nonconvergent-round');
  assert.ok(d); assert.equal(d.node, 'D1_R1_CHOICE'); assert.ok(d.line > 0); assert.match(d.field, /\.next$/); assert.equal(pkg.yarnSource, source);
});
test('native parity: unknown or trailing escape and unescaped single angle punctuation fail at source line', () => {
  for (const bad of ['含\\q的台词', '末尾反斜线\\', '包含<箭头', '包含>箭头']) {
    const pkg = C.copy(sample); pkg.yarnSource = pkg.yarnSource.replace('这里还要检测吗？', bad); const source = pkg.yarnSource;
    assert.ok(errors(pkg).some(d => ['escape', 'unsupported-line'].includes(d.code) && d.line > 0 && d.node === 'D1_R1_OPEN1'));
    assert.equal(pkg.yarnSource, source);
  }
});
test('native parity: measure trust metadata is forbidden even when zero, source retained verbatim', () => {
  const pkg = C.copy(sample); pkg.yarnSource = pkg.yarnSource.replace('#disposition:treat', '#disposition:treat #trust:0');
  const source = pkg.yarnSource, d = errors(pkg).find(d => d.code === 'measure-effect');
  assert.ok(d); assert.equal(d.node, 'D1_MEASURE'); assert.ok(d.line > 0); assert.equal(d.field, 'trust'); assert.equal(pkg.yarnSource, source);
});
test('native parity: identifiers accept dot/hyphen up to128 and option line tags survive editing', () => {
  const pkg = C.copy(sample); pkg.yarnSource = pkg.yarnSource.replaceAll('D1_R1_OPEN1', 'Day1.Intro-1').replace('#id:D1_R1_O1 #trust:0', '#id:D1_R1_O1 #trust:0 #line:original_option'); pkg.profile.startNode = 'Day1.Intro-1';
  assert.deepEqual(errors(pkg), []);
  const node = C.parseSource(pkg.yarnSource).nodes.find(n => n.id === 'D1_R1_CHOICE');
  assert.equal(node.options[0].lineTag, 'original_option'); assert.match(C.serializeNode(node), /#line:original_option/);
  assert.ok(C.ID.test('A' + 'x'.repeat(127))); assert.ok(!C.ID.test('A' + 'x'.repeat(128)));
});
test('native parity: unreachable cycles and missing round headers still fail validation', () => {
  const pkg = C.copy(sample), node = C.copy(C.parseSource(pkg.yarnSource).nodes[0]); node.id = 'UNREACHABLE_LOOP'; node.next = node.id;
  pkg.yarnSource += '\n' + C.serializeNode(node); assert.ok(errors(pkg).some(d => d.code === 'cycle' && d.node === node.id));
  const missing = C.copy(sample); missing.yarnSource = missing.yarnSource.replace('round: R1\n', ''); assert.ok(errors(missing).some(d => d.code === 'round' && d.node === 'D1_R1_OPEN1'));
});
test('native parity: groups cannot end early and dispositions require nonempty line-only reactions', () => {
  for (const [id, field, target, code] of [['D1_R1_CHOICE', 'option', 'END_DAY1', 'early-end'], ['D1_MEASURE', 'option', 'END_DAY1', 'reaction-empty'], ['D1_MEASURE', 'option', 'D1_R1_CHOICE', 'cycle']]) {
    const pkg = C.copy(sample), node = C.parseSource(pkg.yarnSource).nodes.find(n => n.id === id); node.options[0].next = target; pkg.yarnSource = C.replaceNode(pkg.yarnSource, node, node);
    assert.ok(errors(pkg).some(d => d.code === code), code);
  }
});
test('every one of 1296 Day1 reply paths agrees with original DATA; disposition effects and history are bounded', () => {
  const groups = Object.keys(original.choices), routes = [];
  function enumerate(indices) { if (indices.length === groups.length) { routes.push(indices); return; } for (let i = 0; i < original.choices[groups[indices.length]].length; i++) enumerate([...indices, i]); }
  enumerate([]); assert.equal(routes.length, 1296);
  for (const route of routes) {
    const p = C.createPreview(sample); p.enter(); let id = sample.profile.startNode, trust = 0, steps = 0;
    while (id !== 'D1_MEASURE') {
      assert.equal(p.state.node, id);
      if (original.nodes[id]) { const n = original.nodes[id]; assert.equal(p.state.speech, n.text); assert.equal(p.state.portrait, n.portrait); p.step(); id = n.next; }
      else { const index = route[groups.indexOf(id)], o = original.choices[id][index]; trust += o.delta; p.pick(index); id = o.next; }
      assert.ok(++steps < 100);
    }
    assert.equal(p.state.trust, trust);
    const measure = p.map.get(id), release = measure.options.find(o => o.disposition === 'release');
    assert.equal(p.enabled(release), trust >= 3);
    const count = p.state.log.length; p.state.history = true; p.pick(0); assert.equal(p.state.node, 'D1_MEASURE'); assert.equal(p.state.log.length, count); p.state.history = false;
    const purify = measure.options.findIndex(o => o.disposition === 'purify'); p.pick(purify); assert.equal(p.state.quota, 4); p.pick(purify); assert.equal(p.state.quota, 4);
    while (p.state.phase === 'line') p.step(); assert.equal(p.state.phase, 'complete'); assert.equal(p.state.node, 'END_DAY1');
  }
});
test('all three disposition reaction branches match original subject lines', () => {
  for (const [name, type] of [['治疗', 'treat'], ['净化', 'purify'], ['释放', 'release']]) {
    const p = C.createPreview(sample); p.jump('D1_MEASURE'); p.state.trust = 3;
    p.pick(p.map.get(p.state.node).options.findIndex(o => o.disposition === type));
    let id = original.measures[name].next;
    while (id !== 'END_DAY1') { assert.equal(p.state.node, id); assert.equal(p.state.speech, original.nodes[id].text); p.step(); id = original.nodes[id].next; }
    assert.equal(p.state.phase, 'complete'); assert.equal(p.state.disposition, type);
  }
});
