#!/usr/bin/env node
/**
 * Extend the supplied HTML v0.8.12 Canvas atlas without re-rasterizing its glyphs.
 * Usage: node Art/TerminalHtmlMatched/extend-glyph-atlas.cjs [--probe-only]
 * Optional: --repo PATH --source-root PATH --node-modules PATH --browser PATH
 * Requires Playwright and pngjs. Uses a new, isolated headless browser profile.
 */
'use strict';

const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { pathToFileURL } = require('node:url');
const assert = require('node:assert/strict');

const args = process.argv.slice(2);
const option = (name, fallback) => args.includes(name) ? args[args.indexOf(name) + 1] : fallback;
const repo = path.resolve(option('--repo', path.join(__dirname, '../..')));
const modules = option('--node-modules', process.env.PMS_NODE_MODULES ||
  'C:/Users/admin/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules');
const { chromium } = require(path.join(modules, 'playwright'));
const { PNG } = require(path.join(modules, 'pngjs'));
const sourceRoot = path.resolve(option('--source-root', path.join(__dirname, 'Source')));
const atlasRelative = 'UnityProject/Assets/PMSAmberTerminal/Art/FontAtlas';
const output = path.join(repo, atlasRelative);
const original = sourceRoot;
const toolPath = path.join(sourceRoot, 'HTML原字形导出.html');
const reportPath = path.join(__dirname, 'glyph-coverage.json');
const hash = data => crypto.createHash('sha256').update(data).digest('hex');
const relative = file => path.relative(repo, file).replaceAll('\\', '/');
const glyphKey = glyph => `${glyph.code}/${glyph.size}/${glyph.rgb}`;
const bmpCodes = text => [...new Set([...text].map(ch => ch.codePointAt(0))
  .filter(code => code >= 32 && code <= 0xffff && !(code >= 0xd800 && code <= 0xdfff)))];
const jsonStrings = object => typeof object === 'string' ? [object] :
  object && typeof object === 'object' ? Object.values(object).flatMap(jsonStrings) : [];
const walk = directory => fs.readdirSync(directory, { withFileTypes: true }).flatMap(entry =>
  entry.isDirectory() ? walk(path.join(directory, entry.name)) : [path.join(directory, entry.name)]);

function collectSources() {
  const campaignPath = path.join(repo, 'UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json');
  const runtime = path.join(repo, 'UnityProject/Assets/MiniGame/Runtime');
  const demoPath = path.join(sourceRoot, 'Dialogue_Reference.json');
  const sources = [{ file: campaignPath, kind: 'campaign', json: true },
    { file: path.join(repo, 'UnityProject/Assets/MiniGame/Resources/MiniGame/Endings/Dialogue.json'), kind: 'ending-dialogue', json: true },
    { file: path.join(repo, 'UnityProject/Assets/MiniGame/Resources/MiniGame/Supervisor/Dialogue.json'), kind: 'supervisor-dialogue', json: true },
    ...walk(runtime).filter(file => file.endsWith('.cs')).sort().map(file => ({ file, kind: 'runtime', json: false })),
    { file: demoPath, kind: 'terminal-demo', json: true }];
  // Other checked-in terminal JSON content is included without importing metrics themselves.
  const demoContent = path.join(repo, 'UnityProject/Assets/MiniGame/Content');
  for (const file of walk(demoContent).filter(file => /terminal/i.test(file) && file.endsWith('.json'))) {
    if (!sources.some(source => source.file === file)) sources.push({ file, kind: 'terminal-demo', json: true });
  }
  for (const file of walk(demoContent).filter(file => file.endsWith('.yarn'))) {
    if (!sources.some(source => source.file === file)) sources.push({ file, kind: 'yarn-content', json: false });
  }
  return sources.map(source => {
    const bytes = fs.readFileSync(source.file);
    const text = bytes.toString('utf8').replace(/^\uFEFF/, '');
    const strings = source.json ? jsonStrings(JSON.parse(text)) : [text];
    const codes = bmpCodes(strings.join(''));
    return { path: relative(source.file), kind: source.kind, sha256: hash(bytes),
      stringCount: strings.length, codes, nonBmp: [...new Set([...strings.join('')]
        .map(ch => ch.codePointAt(0)).filter(code => code > 0xffff))] };
  });
}

function pack(glyphs, side) {
  let x = 1, y = 1, rowHeight = 0;
  const packed = [];
  for (const glyph of glyphs) {
    const item = { ...glyph, x: 0, y: 0 };
    if (glyph.w && glyph.h) {
      if (x + glyph.w + 1 > side) { x = 1; y += rowHeight + 2; rowHeight = 0; }
      if (y + glyph.h + 1 >= side) return null;
      item.x = x; item.y = y;
      x += glyph.w + 2;
      rowHeight = Math.max(rowHeight, glyph.h);
    }
    packed.push(item);
  }
  return { glyphs: packed, usedHeight: y + rowHeight + 1 };
}

function copyGlyph(source, glyph, target, packed) {
  for (let y = 0; y < glyph.h; y++) {
    const offset = ((glyph.y + y) * source.width + glyph.x) * 4;
    source.data.copy(target.data, ((packed.y + y) * target.width + packed.x) * 4,
      offset, offset + glyph.w * 4);
  }
}

function compareGlyph(source, glyph, target, packed) {
  assert.equal(glyph.code, packed.code);
  for (const field of ['size', 'rgb', 'w', 'h', 'offsetX', 'offsetY', 'advance'])
    assert.equal(glyph[field], packed[field], `Original glyph changed: ${glyphKey(glyph)} ${field}`);
  for (let y = 0; y < glyph.h; y++) {
    const a = ((glyph.y + y) * source.width + glyph.x) * 4;
    const b = ((packed.y + y) * target.width + packed.x) * 4;
    assert.equal(source.data.compare(target.data, b, b + glyph.w * 4, a, a + glyph.w * 4), 0,
      `Original glyph RGBA changed: ${glyphKey(glyph)}`);
  }
}

async function main() {
  const oldMetricsBytes = fs.readFileSync(path.join(original, 'HTML_FontMetrics.json'));
  const oldPngBytes = fs.readFileSync(path.join(original, 'HTML_SimSun_Lucida.png'));
  const oldMetrics = JSON.parse(oldMetricsBytes.toString('utf8'));
  const oldPng = PNG.sync.read(oldPngBytes);
  const sources = collectSources();
  const codes = new Set(oldMetrics.glyphs.map(glyph => glyph.code));
  sources.forEach(source => source.codes.forEach(code => codes.add(code)));
  const characters = [...codes].sort((a, b) => a - b);
  const existing = new Map(oldMetrics.glyphs.map(glyph => [glyphKey(glyph), glyph]));
  const browserPath = option('--browser', [
    'C:/Program Files/Google/Chrome/Application/chrome.exe',
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'
  ].find(file => fs.existsSync(file)));
  const browser = await chromium.launch({ headless: true, executablePath: browserPath,
    args: ['--disable-gpu', '--force-color-profile=srgb'] });
  try {
    const page = await browser.newPage({ deviceScaleFactor: 1 });
    await page.goto(pathToFileURL(toolPath).href);
    const configuration = await page.evaluate(() => ({ sizes, colors,
      simsun: document.fonts.check("14px 'SimSun'"),
      lucida: document.fonts.check("14px 'Lucida Console'") }));
    assert(configuration.simsun && configuration.lucida, 'Install Windows SimSun and Lucida Console first.');
    const requests = [];
    for (const color of configuration.colors) for (const size of configuration.sizes) for (const code of characters) {
      const request = { code, size, rgb: parseInt(color.slice(1), 16) };
      if (!existing.has(glyphKey(request))) requests.push(request);
    }
    console.log(JSON.stringify({ originalCodepoints: oldMetrics.glyphs.length / 35,
      requestedCodepoints: characters.length, preservedGlyphs: oldMetrics.glyphs.length,
      addedGlyphs: requests.length, probeOnly: args.includes('--probe-only') }));
    const selected = args.includes('--probe-only') ? requests.filter(glyph => glyph.size === 14).slice(0, 8) : requests;
    const rendered = await page.evaluate(({ requests, side }) => {
      // Exact v0.8.12 exporter rasterization, crop, offsets and spacing rules.
      const t = document.createElement('canvas'); t.width = t.height = 72;
      const g = t.getContext('2d', { willReadFrequently: true }); g.textBaseline = 'top';
      const atlas = document.createElement('canvas'); atlas.width = atlas.height = side;
      const ag = atlas.getContext('2d'); ag.imageSmoothingEnabled = false;
      const glyphs = []; let x = 1, y = 1, rh = 0;
      for (const request of requests) {
        const ch = String.fromCharCode(request.code), han = request.code > 255;
        g.clearRect(0, 0, 72, 72);
        g.font = han ? `${request.size}px 'SimSun','Noto Serif CJK SC',serif` :
          `${request.size}px 'Lucida Console','Courier New',monospace`;
        g.fillStyle = `#${request.rgb.toString(16).padStart(6, '0')}`;
        g.fillText(ch, 12, 12);
        const d = g.getImageData(0, 0, 72, 72).data;
        let minX = 72, minY = 72, maxX = -1, maxY = -1;
        for (let yy = 0; yy < 72; yy++) for (let xx = 0; xx < 72; xx++) if (d[(yy * 72 + xx) * 4 + 3]) {
          minX = Math.min(minX, xx); minY = Math.min(minY, yy);
          maxX = Math.max(maxX, xx); maxY = Math.max(maxY, yy);
        }
        const item = { ...request, x: 0, y: 0, w: 0, h: 0, offsetX: 0, offsetY: 0,
          advance: han ? request.size : Math.ceil(request.size * .6) };
        if (maxX >= 0) {
          const w = maxX - minX + 1, h = maxY - minY + 1;
          if (x + w + 1 > side) { x = 1; y += rh + 2; rh = 0; }
          if (y + h + 1 >= side) throw Error('Intermediate atlas overflow.');
          ag.drawImage(t, minX, minY, w, h, x, y, w, h);
          Object.assign(item, { x, y, w, h, offsetX: minX - 12, offsetY: minY - 12 });
          x += w + 2; rh = Math.max(rh, h);
        }
        glyphs.push(item);
      }
      return { glyphs, dataUrl: atlas.toDataURL('image/png'), usedHeight: y + rh + 1 };
    }, { requests: selected, side: 4096 });
    if (args.includes('--probe-only')) {
      console.log(JSON.stringify({ browser: browser.version(), configuration,
        probeGlyphs: rendered.glyphs, usedHeight: rendered.usedHeight }));
      return;
    }
    const newPng = PNG.sync.read(Buffer.from(rendered.dataUrl.split(',')[1], 'base64'));
    const combined = [...oldMetrics.glyphs, ...rendered.glyphs];
    let packed, side;
    for (const candidate of [2048, 4096, 8192]) {
      packed = pack(combined, candidate);
      if (packed) { side = candidate; break; }
    }
    assert(packed, 'Expanded atlas exceeds the maximum 8192 square.');
    const finalPng = new PNG({ width: side, height: side, colorType: 6 });
    combined.forEach((glyph, index) => copyGlyph(index < oldMetrics.glyphs.length ? oldPng : newPng,
      glyph, finalPng, packed.glyphs[index]));
    oldMetrics.glyphs.forEach((glyph, index) => compareGlyph(oldPng, glyph, finalPng, packed.glyphs[index]));
    const lookup = new Set(packed.glyphs.map(glyphKey));
    const coverage = sources.map(source => {
      const missing = [];
      for (const code of source.codes) for (const size of configuration.sizes) for (const color of configuration.colors) {
        const rgb = parseInt(color.slice(1), 16);
        if (!lookup.has(glyphKey({ code, size, rgb }))) missing.push({ code, size, rgb });
      }
      return { ...source, codepoints: source.codes.length, codes: undefined,
        missingGlyphs: missing.length, missing };
    });
    assert(coverage.every(source => source.missingGlyphs === 0), 'Coverage incomplete.');
    const metrics = { reference: oldMetrics.reference + '; Campaign extension: original glyph RGBA and metrics preserved',
      atlasWidth: side, atlasHeight: side, glyphs: packed.glyphs };
    const pngBytes = PNG.sync.write(finalPng, { colorType: 6, inputColorType: 6 });
    const metricsBytes = Buffer.from(JSON.stringify(metrics));
    const report = { generator: 'Art/TerminalHtmlMatched/extend-glyph-atlas.cjs',
      originalExporter: { path: relative(toolPath), sha256: hash(fs.readFileSync(toolPath)) },
      originalAtlas: { width: oldPng.width, height: oldPng.height, glyphs: oldMetrics.glyphs.length,
        pngSha256: hash(oldPngBytes), metricsSha256: hash(oldMetricsBytes) },
      browser: browser.version(), fonts: ['C:/Windows/Fonts/simsun.ttc', 'C:/Windows/Fonts/lucon.ttf']
        .map(file => ({ file, sha256: hash(fs.readFileSync(file)) })),
      rasterRules: { renderer: 'Windows Chromium Canvas2D', textBaseline: 'top',
        origin: [12, 12], scratchSize: [72, 72], sizes: configuration.sizes, colors: configuration.colors,
        crop: 'nonzero-alpha bounding rectangle', padding: 2, deviceScaleFactor: 1 },
      preservation: { preservedGlyphs: oldMetrics.glyphs.length, changedOriginalGlyphRgba: 0,
        changedOriginalGlyphMetrics: 0, packingCoordinatesMayChange: true,
        note: 'Original glyphs are copied byte-for-byte from the vendor PNG, never re-rasterized.' },
      output: { width: side, height: side, usedHeight: packed.usedHeight, codepoints: characters.length,
        glyphs: packed.glyphs.length, addedGlyphs: rendered.glyphs.length,
        pngBytes: pngBytes.length, metricsBytes: metricsBytes.length,
        pngSha256: hash(pngBytes), metricsSha256: hash(metricsBytes) },
      coverage: { missingGlyphs: coverage.reduce((sum, source) => sum + source.missingGlyphs, 0),
        nonBmp: [...new Set(sources.flatMap(source => source.nonBmp))], sources: coverage } };
    assert(report.coverage.nonBmp.length === 0, 'Source contains non-BMP characters unsupported by PMSBitmapFont char API.');
    fs.writeFileSync(path.join(output, 'HTML_SimSun_Lucida.png'), pngBytes);
    fs.writeFileSync(path.join(output, 'HTML_FontMetrics.json'), metricsBytes);
    const roundTripPng = PNG.sync.read(fs.readFileSync(path.join(output, 'HTML_SimSun_Lucida.png')));
    const roundTripMetrics = JSON.parse(fs.readFileSync(path.join(output, 'HTML_FontMetrics.json'), 'utf8'));
    assert.equal(roundTripPng.width, roundTripMetrics.atlasWidth);
    assert.equal(roundTripPng.height, roundTripMetrics.atlasHeight);
    oldMetrics.glyphs.forEach((glyph, index) => compareGlyph(oldPng, glyph, roundTripPng, roundTripMetrics.glyphs[index]));
    for (const glyph of roundTripMetrics.glyphs) {
      assert(glyph.x >= 0 && glyph.y >= 0 && glyph.x + glyph.w <= side && glyph.y + glyph.h <= side,
        `Glyph out of atlas bounds: ${glyphKey(glyph)}`);
    }
    report.preservation.roundTripPngAndJsonVerified = true;
    fs.writeFileSync(reportPath, JSON.stringify(report, null, 2) + '\n');
    console.log(JSON.stringify({ output: report.output, preservation: report.preservation,
      missingGlyphs: report.coverage.missingGlyphs, sources: coverage.length }));
  } finally { await browser.close(); }
}

main().catch(error => { console.error(error); process.exitCode = 1; });
