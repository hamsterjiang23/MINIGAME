# Implementation notes

## Deviations

- 全局总览取自 docs/game-overview.md 的设计梳理；不把旧 PrototypeRules 的阈值套入 Excel。数值路径只累计表内信任增量，不模拟未定的释放许可和全局结局。
- 浏览器工具拒绝 file:// 协议。未尝试改用服务器或其他浏览器绕过；仅完成数据、路径、布局坐标与脚本检查，真实浏览器视觉/点击验收未完成。

## Discovered edge cases

- 多个角色复用 D1_R1_OPEN1 等 ID，图节点必须以工作簿为命名空间。
- 角色文本表和玩家选项表由公式生成，首行缓存可能缺 ID；以“对话流程”为唯一正文来源。
- END_DAYn 是会谈边界，不等同角色死亡或全局结局。跨日必须取决于处置。
- 神父表含 Day1，但设计文档写 Day2 初见；保留表内容并提示冲突。
- 外交官72行同时填写F/G/H/I；各列原文保留，H列不自动解释为新处置。Day0净化L7=-1与填写说明冲突，按实际单元格保留。
- END_PURIFY / END_RELEASE 后缀不能识别为处置选项；测试及review C1发现重复累计和空结局标题后已修复，重建通过。

## Questions for review

- 正式信任初值、释放阈值、额外隐藏值、KPI 与全局结局优先级尚待策划裁决；浏览器不替代游戏规则引擎。

## Handoff

Deviations: 2（全局规则单独说明、浏览器验证受限）。
Most likely to revisit: 在允许的用户浏览器中完成视觉与交互验收。
Edge cases: 6组，含跨表ID、派生公式、结束标记、日程冲突、同行多列、结局后缀。
Validation: 994行原始字段及跳转保留；144路径场景、151图视图通过。
Read first: README.md、validation.json、test-results.json；交付HTML在原始表格目录。
