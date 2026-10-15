'use strict';
const fs = require('node:fs');
const path = require('node:path');
const dir = __dirname, root = path.resolve(dir, '../..');
const samplePath = path.join(root, 'UnityProject/Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json');
const sample = JSON.parse(fs.readFileSync(samplePath, 'utf8'));
const C = require('./planner-core.js');
const result = C.validatePackage(sample);
if (result.diagnostics.some(d => d.severity === 'error')) throw new Error(JSON.stringify(result.diagnostics));
let html = fs.readFileSync(path.join(dir, 'planner.template.html'), 'utf8');
const inserts = {
  '/*__CSS__*/': fs.readFileSync(path.join(dir, 'planner.css'), 'utf8'),
  '/*__SAMPLE__*/': JSON.stringify(sample).replace(/</g, '\\u003c'),
  '/*__CORE__*/': fs.readFileSync(path.join(dir, 'planner-core.js'), 'utf8'),
  '/*__APP__*/': fs.readFileSync(path.join(dir, 'planner-app.js'), 'utf8')
};
for (const [marker, content] of Object.entries(inserts)) html = html.replace(marker, () => content);
const output = path.join(dir, 'DialoguePlanner.html');
if (process.argv.includes('--check')) {
  if (!fs.existsSync(output) || fs.readFileSync(output, 'utf8') !== html) throw new Error('Offline HTML is stale; run node Tools/DialoguePlanner/build.js.');
  console.log('Offline HTML matches source files and canonical sample (' + result.nodes.length + ' nodes).');
} else {
  fs.writeFileSync(output, html, 'utf8');
  console.log('Built ' + path.relative(root, output) + ' (' + result.nodes.length + ' nodes, ' + Buffer.byteLength(html) + ' bytes).');
}
