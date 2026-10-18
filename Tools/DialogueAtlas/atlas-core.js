/* Pure graph and path operations, shared with the offline browser tests. */
const AtlasCore = (() => {
  const nodeMap = book => new Map(book.nodes.map(n => [n.id, n]));
  function eligibleEdges(book, path) {
    const current = path.at(-1);
    const map = nodeMap(book);
    const action = [...path].reverse().map(id => map.get(id)?.action).find(Boolean) || '';
    return book.edges.filter(e => e.source === current && (!e.condition || e.condition === action));
  }
  function extend(book, path, target) {
    const map = nodeMap(book), result = [...path];
    let id = target;
    for (let guard = 0; guard < book.nodes.length; guard++) {
      if (!map.has(id)) throw new Error('Unknown path node: ' + id);
      if (result.includes(id)) throw new Error('Cycle in dialogue path: ' + id);
      result.push(id);
      const n = map.get(id);
      if (['gate', 'boundary', 'ending', 'missing'].includes(n.kind)) break;
      const edges = eligibleEdges(book, result);
      if (edges.length !== 1) break;
      id = edges[0].target;
    }
    return result;
  }
  function totals(book, path) {
    const map = nodeMap(book);
    let delta = 0, unknown = 0, measures = [];
    for (const id of path) {
      const n = map.get(id);
      if (!n) continue;
      if (n.delta !== null) delta += n.delta;
      else if (['option', 'measure'].includes(n.kind)) unknown++;
      if (n.action) measures.push({day: n.day, action: n.action});
    }
    return {delta, unknown, measures};
  }
  function view(book, day, round) {
    const map = nodeMap(book);
    const selected = book.nodes.filter(n => n.day === day && (round === 'all' || round === 'summary' || n.round === round));
    const ids = new Set(selected.map(n => n.id));
    const edges = book.edges.filter(e => ids.has(e.source));
    const portals = [...new Set(edges.filter(e => !ids.has(e.target)).map(e => e.target))]
      .map(id => ({...map.get(id), kind: 'portal', originalKind: map.get(id).kind, text: '前往 ' + map.get(id).day + ' / ' + map.get(id).round + '\n' + map.get(id).text}));
    if (round !== 'summary') return {nodes: [...selected, ...portals], edges};
    const groups = new Map(), owner = new Map();
    for (const n of selected) {
      const key = n.kind === 'boundary' ? n.id : `${book.id}:summary:${day}:${n.round}`;
      owner.set(n.id, key);
      if (!groups.has(key)) groups.set(key, []);
      groups.get(key).push(n);
    }
    const nodes = [...groups.entries()].map(([id, members]) => {
      const source = members.find(n => n.row) || members[0];
      const options = members.filter(n => ['option', 'measure'].includes(n.kind));
      return {...source, id, raw: source.round, kind: source.kind === 'boundary' ? 'boundary' : 'summary',
        text: source.kind === 'boundary' ? source.text : `${source.round === '措施' ? '处置' : source.round} · ${source.fields.C || '对话'}\n${members.filter(n => n.row).length} 行原文 · ${options.length} 个选项`,
        memberIds: members.map(n => n.id), delta: null};
    });
    const unique = new Map();
    for (const edge of edges) {
      const source = owner.get(edge.source), target = owner.get(edge.target) || edge.target;
      if (source !== target) {
        const key = `${source}|${target}|${edge.kind}`;
        unique.set(key, {...edge, source, target});
      }
    }
    return {nodes: [...nodes, ...portals], edges: [...unique.values()]};
  }
  function layout(nodes, edges) {
    const ids = new Set(nodes.map(n => n.id));
    const incoming = new Map(nodes.map(n => [n.id, 0]));
    const outgoing = new Map(nodes.map(n => [n.id, []]));
    for (const e of edges) if (ids.has(e.source) && ids.has(e.target)) {
      incoming.set(e.target, incoming.get(e.target) + 1);
      outgoing.get(e.source).push(e.target);
    }
    const ranks = new Map(nodes.map(n => [n.id, 0]));
    const queue = nodes.filter(n => incoming.get(n.id) === 0).map(n => n.id);
    for (let i = 0; i < queue.length; i++) {
      for (const target of outgoing.get(queue[i])) {
        ranks.set(target, Math.max(ranks.get(target), ranks.get(queue[i]) + 1));
        incoming.set(target, incoming.get(target) - 1);
        if (incoming.get(target) === 0) queue.push(target);
      }
    }
    const levels = new Map();
    for (const n of nodes) {
      const r = ranks.get(n.id);
      if (!levels.has(r)) levels.set(r, []);
      levels.get(r).push(n);
    }
    const width = Math.max(1, ...[...levels.values()].map(a => a.length)) * 292;
    return nodes.map(n => {
      const level = levels.get(ranks.get(n.id));
      return {...n, x: (width - level.length * 292) / 2 + level.indexOf(n) * 292 + 20, y: ranks.get(n.id) * 174 + 20};
    });
  }
  return {nodeMap, eligibleEdges, extend, totals, view, layout};
})();
if (typeof module !== 'undefined') module.exports = AtlasCore;
