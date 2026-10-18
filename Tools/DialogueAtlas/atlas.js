'use strict';
const DATA = JSON.parse(document.getElementById('graph-data').textContent);
const $ = id => document.getElementById(id);
const esc = text => String(text ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const sign = n => n > 0 ? '+' + n : String(n);
const kinds = {dialogue:'角色对白',option:'玩家回答',measure:'玩家处置',gate:'选择集合 · 按轮次推导',boundary:'会谈 / 角色边界',ending:'角色结局',summary:'轮次总览',portal:'后续节点',missing:'目标待补',global:'全局规则'};
const columns = {A:'Day / 状态',B:'轮次',C:'轮次类型',D:'流程段',E:'角色文本（轮首 / 收束）',F:'玩家选项',G:'角色文本（分支回应）',H:'玩家措施（原列名）',I:'角色文本（措施回应）',J:'节点 / 选项 ID',K:'下一节点 / 回应节点',L:'信任值影响',M:'立绘 ID'};
const state = {book:null,day:'Day1',round:'summary',selected:null,path:[],history:[],panel:'detail',view:null,positions:new Map(),zoom:1,x:0,y:0};
const book = () => DATA.books.find(b => b.id === state.book);
const globalNodes = [
 ['start','开始游戏', '进入员工公寓，准备接受工作。', 20,20,'docs/game-overview.md:9,41,55'],
 ['day0','Day0 · 教学审判','引导角色 → 净化 → 回房睡眠与梦境。点击下方按钮阅读完整引导对白。',312,20,'docs/game-overview.md:55；Day0引导角色 对话流程第2–9行'],
 ['morning','Day1–3 · 晨检与任务','醒来、晨检、接收任务，进入电脑终端。正式KPI数值和检查时点待确认。',604,20,'docs/game-overview.md:41–55；docs/open-questions.md:15'],
 ['meeting','仍可出场的病人','按日程与去留安排会谈。五份角色表可从左侧分别打开。神父初见日期存在冲突，不假设每天固定五人。',604,204,'docs/game-overview.md:55,90；docs/open-questions.md:11,40'],
 ['dialogue','回答 → 回应 → 信任变化','各轮分支回应再汇合。特殊会谈门槛、释放阈值未定；表内信任增量逐格保留。',312,204,'docs/game-overview.md:73–82；docs/open-questions.md:21–25'],
 ['treat','治疗','治疗次数增加；未达到终止条件则后续返场。普通角色第三次治疗终止；神父和外交官特例需复核。',20,388,'docs/game-overview.md:66,90–94；docs/open-questions.md:40–42'],
 ['purify','净化','退出审判队列，当日净化配额剩余减一。净化后的命运不能凭按钮补写。',312,388,'docs/game-overview.md:67；docs/open-questions.md:37'],
 ['release','释放','角色退出队列。神父存在次日袭击特例。图展示该分支，正式信任门槛仍待确认。',604,388,'docs/game-overview.md:65,103；docs/open-questions.md:22'],
 ['night','日间工作完成 → 夜晚','先处理当天其余仍可出场的病人，再回房、睡眠、梦境；处置一人不会直接结束全天。',312,572,'docs/game-overview.md:41,49–55'],
 ['check','次日与结局检查','继续到Day2 / Day3，或进入结局。KPI失败去向、检查时点和同时触发顺序待确认。',312,756,'docs/game-overview.md:53,100–107；docs/open-questions.md:15,43'],
 ['taken','正常流程 · 被带走','Day4起床被带走。与其他结局竞争时的优先级待确认。',20,940,'docs/game-overview.md:102,107'],
 ['kpi','KPI失败 · 去向待确认','任意一天未完成跳Day4；永久锁房与画板“常规结局”的口径冲突。',604,940,'docs/game-overview.md:104,107；docs/open-questions.md:15'],
 ['attack','神父释放后 · 遭袭','角色设计描述次日替代监督员并袭击玩家。发生日期随正式出场日程确定。',896,388,'docs/game-overview.md:90,103；docs/open-questions.md:40'],
 ['rebel','反叛值分支 · 纳入范围待定','Day3梦后电脑亮、床不亮，监控出现自己 → 唯一选项“净化” → 超自然场景。反叛值max与阈值、交付三结局与正文四分支仍待裁决。',896,756,'docs/game-overview.md:105–107；docs/open-questions.md:23,44']
].map(([id,text,note,x,y,source]) => ({id,raw:id,text,note,x,y,source,day:'全局',round:'',kind:['taken','kpi','attack','rebel'].includes(id)?'ending':'global',fields:{},delta:null}));
const globalEdges = [
 ['start','day0',''],['day0','morning','教学后'],['morning','meeting',''],['meeting','dialogue',''],
 ['dialogue','treat','治疗'],['dialogue','purify','净化'],['dialogue','release','释放 · 门槛待定'],
 ['treat','night','处理其余病人后'],['purify','night','处理其余病人后'],['release','night','处理其余病人后'],
 ['night','check','睡眠 / 梦境后'],['check','morning','尚未结束 → 次日'],['check','taken','三天流程完成'],
 ['check','kpi','KPI未完成'],['release','attack','仅神父 · 次日'],['night','rebel','Day3 · 反叛值条件待定']
].map(([source,target,label]) => ({source,target,label,kind:'document',evidence:'docs/game-overview.md:41–107；冲突见 docs/open-questions.md'}));

function switchPanel(panel) {
 state.panel=panel;
 for(const key of ['detail','path','issues']) { $(key+'-panel').hidden=key!==panel; $(key+'-tab').classList.toggle('active',key===panel); }
}
function navigate(bookId, day, round='summary', preservePath=false) {
 if(bookId!==state.book && !preservePath){state.path=[];state.history=[];}
 state.book=bookId; state.day=day || Object.keys(book().entries)[0]; state.round=round; state.selected=null;
 render();
}
function openNode(id) {
 const b=book(); const n=b ? b.nodes.find(n=>n.id===id) : globalNodes.find(n=>n.id===id);
 if(!n)return;
 if(b && (n.day!==state.day || (state.round!=='all' && state.round!=='summary' && n.round!==state.round))){state.day=n.day;state.round=n.round;render();}
 state.selected=id; renderDetails(n); switchPanel('detail'); markSelection();
}
function render() {
 const b=book();
 $('overview').classList.toggle('active',!b);
 document.querySelectorAll('[data-book]').forEach(el=>el.classList.toggle('active',el.dataset.book===state.book));
 $('view-title').textContent=b?b.name:'从开场到结局';
 $('view-kicker').textContent=b?'CHARACTER FILE / '+state.day:'THE WHOLE STORY';
 $('start-path').hidden=!b || state.day==='结局';
 $('day-tabs').innerHTML=b?Object.keys(b.entries).map(day=>`<button class="${day===state.day?'active':''}" data-day="${esc(day)}">${esc(day)}</button>`).join(''):'';
 const rounds=b?(b.rounds[state.day]||[]):[];
 $('round-tabs').innerHTML=b?['summary',...rounds,'all'].map(r=>`<button class="${r===state.round?'active':''}" data-round="${esc(r)}">${r==='summary'?'轮次总览':r==='all'?'整天逐句':esc(r)}</button>`).join(''):'';
 $('context-note').textContent=b?(state.round==='summary'?'点击轮次展开。跨日只在治疗后连接；释放和净化结束该角色会谈。':'实线为表内跳转，虚线为选择归组。信任增量来自L列；释放条件未填，浏览不表示已解锁。'):'设计文档总览 · 角色对白从左侧进入 · 结局条件及优先级的冲突已标注';
 state.view=b?AtlasCore.view(b,state.day,state.round):{nodes:globalNodes,edges:globalEdges};
 const positioned=b?AtlasCore.layout(state.view.nodes,state.view.edges):state.view.nodes;
 state.positions=new Map(positioned.map(n=>[n.id,n]));
 drawGraph(); fit(); renderIssues(); renderPath();
 if(b) renderBookIntro(b); else renderGlobalIntro();
}
function renderGlobalIntro(){
 $('detail-panel').innerHTML=`<div class="detail-type">全局流程 · 设计梳理</div><h3 class="intro-title">一次选择，<br>两种后果。</h3><p>会谈内改变回应与信任；会谈后改变人物去留、配额与后续事件。</p><div class="note">先点图中的流程节点查看规则，再从左侧打开某位角色的完整对白。</div><div class="mini-heading">本次读取</div><p>6 份工作簿<br>${DATA.audit.source_rows} 行原始流程<br>Day0 引导 + 5 位角色</p><div class="source">来源：Art/chat_excel/*xlsx「对话流程」；docs/game-overview.md:41–107。表格分支已导入，图不代表这些内容已全部接入Unity。</div>`;
}
function renderBookIntro(b){
 const sources=b.nodes.filter(n=>n.row),opts=sources.filter(n=>['option','measure'].includes(n.kind));
 $('detail-panel').innerHTML=`<div class="detail-type">角色档案 / ${esc(state.day)}</div><h3 class="intro-title">${esc(b.name)}</h3><p>${sources.length} 行流程 · ${opts.length} 个回答与处置选项</p><p class="muted">点击图中节点查看完整对白。轮次总览可展开；“整天逐句”保留本天所有分支。</p><button id="intro-trace">从本天开始追踪</button><div class="note">累计值只表示沿途已填写的信任增量，不是角色当前信任，也不判定释放许可。</div><div class="source">${esc(b.file)}<br>工作表：对话流程<br>SHA-256：${esc(b.sha256)}</div>`;
 $('intro-trace').onclick=startPath;
}
function renderDetails(n) {
 const b=book(); let body=`<div class="detail-type">${esc(kinds[n.kind]||n.kind)} · ${esc(n.day)} ${esc(n.round)}</div><div class="detail-id">${esc(n.raw)}</div>`;
 if(n.kind==='summary'){
   body+=`<h3>${esc(n.text)}</h3><button id="expand-round">展开本轮分支</button>`;
 } else if(!b){
   body+=`<h3>${esc(n.text)}</h3><p>${esc(n.note)}</p><div class="source">[verified: ${esc(n.source)}]</div>`;
   if(n.id==='day0') body+='<button id="open-day0">阅读 Day0 对话</button>';
 } else {
   for(const col of 'EFGHI')if(n.fields[col])body+=`<div class="detail-block"><label>${esc(columns[col])} · ${col}${n.row}</label><p>${esc(n.fields[col])}</p></div>`;
   if(!n.row)body+=`<h3>${esc(n.text)}</h3>`;
   if(n.delta!==null) body+=`<div class="detail-block"><label>本节点信任值影响 · L${n.row}</label><div class="delta ${n.delta<0?'negative':''}">${sign(n.delta)}</div></div>`;
   else if(['option','measure'].includes(n.kind))body+='<div class="note">信任值影响未填写；不按 0 处理。</div>';
   if(n.action){
     const impact={treat:'治疗次数 +1；返场或达到治疗上限。',purify:'从队列移除；当日净化剩余配额 −1。',release:'从队列移除；神父有次日袭击特例。'}[n.action];
     body+=`<div class="note">文档后果：${impact}<br>[verified: docs/game-overview.md:65–67,90–94]<br>正式阈值、日程与特殊终止规则仍待确认。</div>`;
   }
   if(n.note)body+=`<div class="note">${esc(n.note)} [opinion：显示/解析说明]</div>`;
   body+=`<div class="source">${esc(b.file)}<br>${n.row?'对话流程 A'+n.row+':M'+n.row:'显示用补充节点（非 Excel 正文行）'}</div>`;
   if(n.row)body+=`<details class="detail-block"><summary>查看原始字段 A–M</summary>${Object.entries(n.fields).map(([col,val])=>`<p style="font-size:11px"><span class="muted">${col}${n.row} ${esc(columns[col])}</span><br>${esc(val)}</p>`).join('')}</details>`;
 }
 const outgoing=(b?b.edges:globalEdges).filter(e=>e.source===n.id);
 if(outgoing.length){body+='<div class="mini-heading">后续连接</div>';for(const e of outgoing){const target=b?b.nodes.find(x=>x.id===e.target):globalNodes.find(x=>x.id===e.target);body+=`<button class="next-link" data-next="${esc(e.target)}">${esc(target.text.slice(0,65))}<small>${e.kind==='explicit'?'表内跳转':e.kind==='inferred'?'按轮次推导':'文档规则'} · ${esc(e.label||target.raw)}</small></button><div class="source">${esc(e.evidence||'')}</div>`;}}
 $('detail-panel').innerHTML=body;
 if($('expand-round'))$('expand-round').onclick=()=>navigate(b.id,n.day,n.round,true);
 if($('open-day0'))$('open-day0').onclick=()=>navigate(DATA.books[0].id,'Day0','all');
 $('detail-panel').querySelectorAll('[data-next]').forEach(el=>el.onclick=()=>openNode(el.dataset.next));
}
function renderIssues(){
 const common=[['正式数值待定','信任初值、特殊会谈门槛和释放阈值尚未统一；不会用原型20/30或演示3代填。','docs/open-questions.md:21–25'],['其他隐藏值未有统一公式','疯狂、偏离、狂热、反叛值不合并；本图只对Excel明确填写的信任增量求和。','docs/open-questions.md:23'],['全局结局待裁决','KPI失败去向、触发时点、冲突优先级及第四结局是否纳入Mini仍未统一。','docs/open-questions.md:15,43–44']];
 $('issues-panel').innerHTML='<div class="detail-type">来源缺口 / 解释边界</div>'+common.map(([title,text,source])=>`<div class="issue"><strong>${title}</strong>${text}<div class="source">[verified: ${source}]</div></div>`).join('')+(book()?book().issues.map(text=>`<div class="issue">${esc(text)}<div class="source">本工作簿扫描结果 / 解析说明</div></div>`).join(''):'')+'<p class="source">所有源表字段保留；不会为补齐图而改写Excel。虚线只是供阅读的结构推导。</p>';
}
function startPath(){
 const b=book(); if(!b||!b.entries[state.day])return;
 state.history=[];state.path=AtlasCore.extend(b,[],b.entries[state.day]);
 showPathPosition();
}
function showPathPosition(){
 const b=book(),current=b.nodes.find(n=>n.id===state.path.at(-1));
 if(current){state.day=current.day;state.round=current.round==='边界'?'all':current.round;render();state.selected=current.id;markSelection();}
 renderPath();switchPanel('path');
}
function takeEdge(target){
 const b=book();if(!AtlasCore.eligibleEdges(b,state.path).some(e=>e.target===target))return;
 state.history.push([...state.path]);state.path=AtlasCore.extend(b,state.path,target);showPathPosition();
}
function renderPath(){
 $('path-count').textContent=state.path.length;
 const b=book();
 if(!b||!state.path.length){$('path-panel').innerHTML='<div class="detail-type">沿一条路线阅读</div><h3>选择一个角色开始。</h3><p class="muted">在角色页点击“从本天开始追踪”。选择回答后自动经过线性回应，在下一个选择点停下。</p><div class="note">这里只累计沿途信任增量；角色初值、释放门槛与全局结局不自动判定。</div>';return;}
 const map=AtlasCore.nodeMap(b),total=AtlasCore.totals(b,state.path),current=map.get(state.path.at(-1)),edges=AtlasCore.eligibleEdges(b,state.path);
 let body=`<div class="detail-type">${esc(b.name)} · ${esc(current.day)}</div><div class="path-metric"><span class="muted">这条路径的已知信任增量</span><br><strong>${sign(total.delta)}</strong>${total.unknown?'<p>另有 '+total.unknown+' 个选项未填写增量</p>':''}<div class="source">不含未知初始值，不做上下限裁剪，不判定释放门槛。</div></div><div class="path-actions"><button id="undo-path" ${state.history.length?'':'disabled'}>撤销上次选择</button><button id="clear-path">清空路径</button></div>`;
 if(edges.length){body+=`<div class="mini-heading">${current.kind==='boundary'?'会谈后续（依据设计文档）':'选择下一步'}</div>`;for(const e of edges){const target=map.get(e.target);body+=`<button class="path-choice" data-take="${esc(e.target)}">${esc(e.condition?e.label:target.text)}<small>${target.delta!==null?'信任 '+sign(target.delta):e.kind==='document'?'文档条件连接':'继续'}${target.action==='release'?' · 释放许可未判定':''}</small></button>`;}}
 else {const last=total.measures.at(-1);body+=`<div class="note">已到达本条角色路径的终点。${last?.action==='release'&&b.name.includes('神父')?'文档另有次日袭击玩家的全局后果（game-overview.md:103）。':last?.action==='release'||last?.action==='purify'?'该角色不继续进入次日会谈。':'后续全局流程见左侧“全局流程”。'}<br>此处不自动推算KPI或游戏总结束条件。</div>`;}
 if(total.measures.length)body+='<div class="mini-heading">已选处置</div>'+total.measures.map(m=>`<span class="badge">${esc(m.day)} · ${{treat:'治疗',purify:'净化',release:'释放'}[m.action]}</span>`).join('');
 body+='<div class="mini-heading">已经过的节点 · 点击查看全文</div>'+state.path.map((id,i)=>{const n=map.get(id);return `<button class="path-step" data-path-node="${esc(id)}"><small>${i+1} · ${esc(n.day)} / ${esc(n.round)} · ${esc(kinds[n.kind])}${n.delta!==null?' · '+sign(n.delta):''}</small>${esc(n.text.slice(0,90))}</button>`;}).join('');
 $('path-panel').innerHTML=body;
 $('undo-path').onclick=()=>{if(state.history.length){state.path=state.history.pop();showPathPosition();}};
 $('clear-path').onclick=()=>{state.path=[];state.history=[];renderPath();markSelection();};
 $('path-panel').querySelectorAll('[data-take]').forEach(el=>el.onclick=()=>takeEdge(el.dataset.take));
 $('path-panel').querySelectorAll('[data-path-node]').forEach(el=>el.onclick=()=>openNode(el.dataset.pathNode));
}
function svgElement(tag,attrs={},text='') {const el=document.createElementNS('http://www.w3.org/2000/svg',tag);for(const [k,v] of Object.entries(attrs))el.setAttribute(k,v);if(text)el.textContent=text;return el;}
function lines(text,max=18,count=3){const chars=Array.from(text.replace(/\s+/g,' ')),out=[];for(let i=0;i<chars.length&&out.length<count;i+=max)out.push(chars.slice(i,i+max).join(''));if(chars.length>max*count)out[count-1]=out[count-1].slice(0,-1)+'…';return out;}
function drawGraph(){
 const world=$('world');world.replaceChildren();const edgeLayer=svgElement('g');world.append(edgeLayer);
 for(const e of state.view.edges){const a=state.positions.get(e.source),b=state.positions.get(e.target);if(!a||!b)continue;let d,lx,ly;
   if(!book()) {const ax=a.x+126,ay=a.y+118,bx=b.x+126,by=b.y;const middle=(ay+by)/2;d=`M${ax},${ay} C${ax},${middle} ${bx},${middle} ${bx},${by}`;lx=(ax+bx)/2;ly=middle-7; if(b.y<=a.y){const right=Math.max(a.x,b.x)+270;d=`M${a.x+252},${a.y+59} C${right},${a.y+59} ${right},${b.y+59} ${b.x+252},${b.y+59}`;lx=right;ly=(a.y+b.y)/2+59;}}
   else{const ax=a.x+126,ay=a.y+118,bx=b.x+126,by=b.y;const middle=(ay+by)/2;d=`M${ax},${ay} C${ax},${middle} ${bx},${middle} ${bx},${by}`;lx=(ax+bx)/2;ly=middle-5;}
   const path=svgElement('path',{d,class:'edge '+e.kind,'data-source':e.source,'data-target':e.target});edgeLayer.append(path);if(e.label&&(!book()||e.kind==='document'))edgeLayer.append(svgElement('text',{x:lx,y:ly,class:'edge-label','text-anchor':'middle'},e.label));
 }
 for(const n of state.positions.values()){
   const g=svgElement('g',{class:'node '+n.kind,transform:`translate(${n.x},${n.y})`,tabindex:'0',role:'button','aria-label':`${kinds[n.kind]} ${n.text}`,'data-node':n.id});
   g.append(svgElement('rect',{width:252,height:118,rx:5}),svgElement('rect',{width:3,height:94,x:0,y:12,class:'stripe'}));
   g.append(svgElement('text',{x:15,y:21,class:'node-meta'},n.kind==='global'||!book()?'设计文档':n.kind==='summary'?'展开本轮 →':`${kinds[n.kind]}${n.row?' · 第'+n.row+'行':''}`));
   lines(n.text,17,3).forEach((line,i)=>g.append(svgElement('text',{x:15,y:44+i*19,class:'node-title'},line)));
   g.append(svgElement('text',{x:15,y:105,class:'node-id'},n.raw.length>30?n.raw.slice(0,29)+'…':n.raw));
   if(n.delta!==null)g.append(svgElement('text',{x:235,y:21,'text-anchor':'end',class:'node-delta '+(n.delta<0?'negative':'')},'信任 '+sign(n.delta)));
   const select=()=>{if(n.kind==='summary'){navigate(state.book,n.day,n.round,true);return;}if(n.kind==='portal'){openNode(n.id);return;}state.selected=n.id;renderDetails(n);switchPanel('detail');markSelection();};
   g.onclick=select;g.onkeydown=e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();select();}};world.append(g);
 }
 $('graph-stats').textContent=`当前显示 ${state.positions.size} 个节点 · ${state.view.edges.length} 条连接${book()?' · '+book().file:' · 全局图连线均为设计文档关系'}`;
 markSelection();
}
function markSelection(){
 const pathSet=new Set(state.path);const highlighted=new Set();for(const n of state.positions.values()){if(pathSet.has(n.id)||(n.memberIds||[]).some(id=>pathSet.has(id)))highlighted.add(n.id);}
 $('world').querySelectorAll('[data-node]').forEach(el=>{el.classList.toggle('selected',el.dataset.node===state.selected);el.classList.toggle('traced',highlighted.has(el.dataset.node));});
 const pairs=new Set(state.path.slice(1).map((id,i)=>state.path[i]+'|'+id));
 $('world').querySelectorAll('[data-source]').forEach(el=>el.classList.toggle('traced',pairs.has(el.dataset.source+'|'+el.dataset.target)));
}
function transform(){ $('world').setAttribute('transform',`translate(${state.x},${state.y}) scale(${state.zoom})`);$('zoom-label').textContent=Math.round(state.zoom*100)+'%'; }
function fit(){const p=[...state.positions.values()];if(!p.length)return;const box=$('graph').getBoundingClientRect(),minX=Math.min(...p.map(n=>n.x)),minY=Math.min(...p.map(n=>n.y)),w=Math.max(...p.map(n=>n.x+252))-minX,h=Math.max(...p.map(n=>n.y+118))-minY;state.zoom=Math.max(.06,Math.min(1.15,(box.width-70)/w,(box.height-100)/h));state.x=(box.width-w*state.zoom)/2-minX*state.zoom;state.y=25-minY*state.zoom;transform();}
function zoom(factor,x,y){const rect=$('graph').getBoundingClientRect();x??=rect.width/2;y??=rect.height/2;const next=Math.max(.04,Math.min(2.5,state.zoom*factor)),ratio=next/state.zoom;state.x=x-(x-state.x)*ratio;state.y=y-(y-state.y)*ratio;state.zoom=next;transform();}
let drag=null;
$('graph').addEventListener('pointerdown',e=>{if(e.target.closest('.node'))return;drag={x:e.clientX,y:e.clientY,tx:state.x,ty:state.y};$('graph').setPointerCapture(e.pointerId);$('graph').classList.add('dragging');});
$('graph').addEventListener('pointermove',e=>{if(drag){state.x=drag.tx+e.clientX-drag.x;state.y=drag.ty+e.clientY-drag.y;transform();}});
for(const event of ['pointerup','pointercancel'])$('graph').addEventListener(event,()=>{drag=null;$('graph').classList.remove('dragging');});
$('graph').addEventListener('wheel',e=>{e.preventDefault();const r=$('graph').getBoundingClientRect();zoom(Math.exp(-e.deltaY*.0015),e.clientX-r.left,e.clientY-r.top);},{passive:false});
$('zoom-in').onclick=()=>zoom(1.25);$('zoom-out').onclick=()=>zoom(.8);$('fit').onclick=fit;$('actual').onclick=()=>zoom(1/state.zoom);
$('source-count').textContent=`6 份对白表 / ${DATA.audit.source_rows} 行流程`;
$('characters').innerHTML=DATA.books.map((b,i)=>`<button class="nav-item" data-book="${b.id}"><span>${esc(b.name)}</span><small>${i===0?'序章 / 教学':Object.keys(b.entries).filter(x=>x!=='结局').join(' · ')} · ${b.nodes.filter(n=>n.row).length} 行</small></button>`).join('');
$('characters').querySelectorAll('[data-book]').forEach(el=>el.onclick=()=>navigate(el.dataset.book));
$('overview').onclick=()=>{state.book=null;state.selected=null;state.path=[];state.history=[];render();switchPanel('detail');};
$('day-tabs').onclick=e=>{const el=e.target.closest('[data-day]');if(el)navigate(state.book,el.dataset.day,'summary',true);};
$('round-tabs').onclick=e=>{const el=e.target.closest('[data-round]');if(el)navigate(state.book,state.day,el.dataset.round,true);};
for(const key of ['detail','path','issues'])$(key+'-tab').onclick=()=>switchPanel(key);
$('start-path').onclick=startPath;
$('help').onclick=()=>$('help-dialog').showModal();$('close-help').onclick=()=>$('help-dialog').close();
$('search').oninput=()=>{const q=$('search').value.trim().toLowerCase();if(!q){$('search-results').replaceChildren();return;}const results=DATA.books.flatMap(b=>b.nodes.filter(n=>n.row&&JSON.stringify(n.fields).toLowerCase().includes(q)).map(n=>({b,n})));$('search-results').innerHTML=`<div class="search-count">${results.length} 项匹配${results.length>60?'，显示前60项；请缩小关键词':''}</div>`+results.slice(0,60).map(({b,n})=>`<button class="search-result" data-search-book="${b.id}" data-search-node="${esc(n.id)}"><small>${esc(b.name)} / ${esc(n.day)} / 行${n.row}</small>${esc(n.text.slice(0,48))}</button>`).join('');$('search-results').querySelectorAll('[data-search-node]').forEach(el=>el.onclick=()=>{const b=DATA.books.find(b=>b.id===el.dataset.searchBook),n=b.nodes.find(n=>n.id===el.dataset.searchNode);navigate(b.id,n.day,n.round,b.id===state.book);openNode(n.id);});};
let resizeTimer;window.addEventListener('resize',()=>{clearTimeout(resizeTimer);resizeTimer=setTimeout(fit,150);});
render();
