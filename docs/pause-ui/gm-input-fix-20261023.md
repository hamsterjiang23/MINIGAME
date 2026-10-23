# GM 四键同时按住修复

## 行为与已确认问题

- 保持唯一键盘规则：游戏内暂停主页面检测到 N、O、R、M 四键同一帧均处于按住状态，就打开 GM；依次输入 NORM 不触发。`[verified: 用户本轮确认；PauseMenuView.UpdateGmChord；GmChordDoesNotAcceptSequentialLettersOrThreeHeldKeys]`
- 旧实现先记录“四键已按住”，再检查页面是否允许进入。如果第一次检测发生在隐藏、未绑定或子页面状态，返回可用的暂停主页后持续按住不会打开。新增测试在旧实现上失败，消息为 `Opening pause must not require a new key-down edge`。`[verified: ../../Artifacts/GmInputFix/before-result.json results[0].Message]`
- 新实现仅在成功打开 GM 时消耗本次四键状态。由 GM 返回暂停主页面时，继续按住不会自动重开；松开至少一个键后可以再次触发。`[verified: PauseMenuView.cs UpdateGmChord、TryCloseGm；after-result.json GmChordRequiresAllKeysAndConsumesOnlySuccessfulOpening=Passed]`
- 以上确认了代码边界问题；用户原始失败当时的四键状态未被记录，不能认定原始现场必然由该边界造成。`[opinion]`

## 验证

- Unity Play Mode 暂停测试 **11/11 通过**，覆盖四键完整状态、三键/顺序输入不触发、进入可用主页后触发、返回不反复打开，以及原暂停菜单、标题快捷入口和输入隔离。`[verified: ../../Artifacts/GmInputFix/after-result.json summary、results]`
- 临时 Development Player 在正式 Combined 场景中使用独立存档目录。仅由观察组件自动打开暂停；GM 打开使用系统注入的原生键盘事件，经过实际 `Input.GetKey`，没有调用 GM 打开方法。`[verified: ../../Artifacts/GmInputFix/GmInputPlayerProbe.cs Prepare、Update；本轮 Computer Use press_key 调用]`
- 原生输入记录两次 `mask=15`（四键均按住）后 GM 可见；三个键 `mask=7` 不打开；ESC 返回后再次四键按住可重新打开。一次极短注入只观察到 `7 → 8 → 0`、没有同一帧四键状态，因此没有打开。这符合“同一时刻同时按住”规则。`[verified: ../../Artifacts/GmInputFix/Native/input.jsonl]`
- [GM 实机画面](../../Artifacts/GmInputFix/Native/gm-open.png) 已人工查看，四个结局入口及返回暂停菜单均可见。本轮没有点击任何结局入口；不替代四条结局完整游玩回归。`[verified: gm-open.png；本轮仅 NORM、NOR、ESC、Alt+F4 输入记录]`
- 临时观察脚本已从 Assets 移除。正式修复包为非 Development 构建，构建成功、0 错误；保留一条未配置 RuntimePipelineManager 的 Pipeline 工具提示。正式包仅另做启动日志检查，原生按键与画面验证来自前述临时 Player。`[verified: ../../Artifacts/Packages/NORM-Windows-x64-Mono-GmFix-20261023-194239/build-summary.json；launch-smoke.json scope、passed]`

## 交付

- [Windows 修复包](../../Builds/Deliverables/NORM-Windows-x64-Mono-GmFix-20261023-194239.zip)；校验与启动结果见 [delivery.json](../../Artifacts/Packages/NORM-Windows-x64-Mono-GmFix-20261023-194239/delivery.json)。运行此目录的 `MiniGame.exe` 才使用本次修改；已经运行的旧包不会自动更新。`[verified: build-summary.json outputPath；launch-smoke.json executable]`
- 包由当前共享工作区构建，包含同期档案 UI 源码；本报告只验收 GM 输入，档案 UI 画面验收由对应聊天继续完成。`[verified: 本轮共享 Editor 交接消息]`
