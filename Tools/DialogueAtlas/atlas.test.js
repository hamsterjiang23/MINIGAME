'use strict';
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const core = require('./atlas-core.js');
const data = JSON.parse(fs.readFileSync(path.join(__dirname,'graph-data.json'),'utf8'));
let routes=0, views=0;
for(const book of data.books){
 const map=core.nodeMap(book);
 for(const day of Object.keys(book.entries)){
   for(const round of ['summary','all',...(book.rounds[day]||[])]){
     const view=core.view(book,day,round),ids=new Set(view.nodes.map(n=>n.id));
     assert.equal(ids.size,view.nodes.length,'duplicate displayed nodes');
     assert.ok(view.edges.every(e=>ids.has(e.source)&&ids.has(e.target)),'invisible edge endpoint');
     const laid=core.layout(view.nodes,view.edges);
     assert.ok(laid.every(n=>Number.isFinite(n.x)&&Number.isFinite(n.y)));
     assert.equal(new Set(laid.map(n=>`${n.x}:${n.y}`)).size,laid.length,'node overlap');
     views++;
   }
   if(day==='结局')continue;
   for(const desired of ['treat','purify','release'])for(const variation of [0,1,2]){
     let route=core.extend(book,[],book.entries[day]);
     let checkpoints=0;
     for(;checkpoints<40;checkpoints++){
       const edges=core.eligibleEdges(book,route);
       if(!edges.length)break;
       const actions=edges.filter(e=>map.get(e.target).action);
       const next=actions.length?(actions.find(e=>map.get(e.target).action===desired)||actions[0]):edges[variation%edges.length];
       const old=[...route];
       route=core.extend(book,route,next.target);
       assert.deepEqual(route.slice(0,old.length),old);
     }
     assert.ok(checkpoints<40,'path did not terminate');
     for(let i=1;i<route.length;i++)assert.ok(book.edges.some(e=>e.source===route[i-1]&&e.target===route[i]));
     const total=core.totals(book,route);
     const expected=route.reduce((sum,id)=>sum+(map.get(id).fields.L===undefined?0:Number(map.get(id).fields.L)),0);
     assert.equal(total.delta,expected,'trust delta differs from original L cells');
     const measures=route.map(id=>map.get(id)).filter(n=>n.action);
     const terminal=measures.find(n=>n.action!=='treat');
     if(terminal){
       assert.equal(measures.at(-1).id,terminal.id,'terminal disposition followed by another disposition');
       assert.ok(route.slice(route.indexOf(terminal.id)+1).every(id=>map.get(id).day===terminal.day||map.get(id).day==='结局'),'released or purified character returned');
     }
     if(desired==='treat'&&day==='Day1'&&!book.name.includes('Day0'))assert.equal(measures.length,3,'treatment route must reach third treatment');
     routes++;
   }
 }
}
const book=data.books.find(b=>b.name.includes('小孩'));
const option=book.nodes.find(n=>n.kind==='option');
const fixture=structuredClone(book);fixture.nodes.find(n=>n.id===option.id).delta=null;
assert.equal(core.totals(fixture,[option.id]).unknown,1,'missing increment is unknown');
assert.throws(()=>core.extend(book,[], 'not-a-node'),/Unknown path node/);
const first=data.books[0];const day0=core.extend(first,[],first.entries.Day0);
assert.equal(core.nodeMap(first).get(day0.at(-1)).kind,'gate','one-option tutorial must still stop at a choice');
const tutorial=core.extend(first,day0,core.eligibleEdges(first,day0)[0].target);
assert.equal(core.totals(first,tutorial).delta,-1,'Day0 raw -1 must be preserved');
const html=fs.readFileSync(path.join(__dirname,'../../Art/chat_excel/游戏逻辑分支图.html'),'utf8');
assert.ok(!html.includes('__GRAPH_DATA__')&&!html.includes('/*__APP__*/'));
assert.ok(!/<script[^>]+src=|<link[^>]+href=/i.test(html),'offline deliverable must not load external code');
const embedded=JSON.parse(html.match(/<script type="application\/json" id="graph-data">([\s\S]*?)<\/script>/)[1]);
assert.deepEqual(embedded,data,'embedded snapshot differs');
const result={route_scenarios:routes,display_views:views,source_rows:data.audit.source_rows,checks:['terminal_dispositions_do_not_return','three_treatments_reach_termination','path_delta_equals_source_L_cells','missing_delta_stays_unknown','tutorial_single_choice_is_explicit','display_edges_and_layout_valid','embedded_snapshot_equal','no_external_scripts_or_styles'],browser_visual_verification:false};
fs.writeFileSync(path.join(__dirname,'test-results.json'),JSON.stringify(result,null,2)+'\n');
console.log(JSON.stringify(result,null,2));
