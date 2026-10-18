# 游戏逻辑分支图

交付文件：[游戏逻辑分支图.html](../../Art/chat_excel/游戏逻辑分支图.html)。单文件内嵌数据和样式，可双击离线打开。

- 全局图整理开场、日循环、三种处置及文档中的结局描述；点节点查看来源与未定项。
- 左侧选角色，再按天、轮次浏览。点击轮次总览节点展开逐句分支；点击对白查看全文及原表行号。
- 路径追踪按源表推进，在选择集合处停下。已知信任增量求和，不假定初值或释放阈值；可撤销选择。
- 搜索覆盖所有角色的原始字段。平移、滚轮缩放、适应画面及原始大小便于浏览长图。

## 数据与边界

源为 `Art/chat_excel` 下非 `~$` 开头的六份 `.xlsx` 工作簿，唯一正文源为“对话流程”。每行保留 A–M 已填字段，并使用工作簿命名空间隔离重复ID。实线对应 K 列；虚线为同日同轮的选择集合；点线为文档规则补充。

`END_DAY` 仅为会谈边界。净化、释放后不返回次日；通用会谈边界只有治疗路径可沿文档连接返场。表内明确的跨日链接原样保留。该工具不调度全部患者，不计算正式KPI，也不替代 `GameSession`。

外交官表的多列同排对白按原列名保留，未猜测H列执行语义。神父表内Day1与设计出场日冲突、Day0净化信任-1与填写说明冲突均显示在待确认区。全局结局及正式数值的待定项以 `docs/open-questions.md` 为准。

## 重新生成

在仓库根目录运行（Python标准库，无安装依赖）：

```powershell
uv run --no-project python Tools/DialogueAtlas/build_atlas.py --smoke
uv run --no-project python Tools/DialogueAtlas/build_atlas.py
node --test Tools/DialogueAtlas/atlas.test.js
```

`build_atlas.py --inspect` 另生成详细源表盘点。页面是快照，不自动读取后续Excel修改。构建覆盖HTML与本工具目录的生成JSON，不修改原始工作簿。

校验记录见 `validation.json` 和 `test-results.json`，检查原始字段/跳转覆盖、图端点/可达性、实际路径与信任累计。浏览器工具不允许打开本地file协议，本轮未完成真实浏览器渲染与点击验收；脚本检查不代表视觉验收。
