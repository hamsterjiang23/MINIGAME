# 暂停界面与操作手册

## Deviations

- 保留原有动态字体和背景捕获，菜单改为平面琥珀命令条；不放大终端位图字体，也不改动背景方向修复。`[verified: PauseMenuView.Build / PauseMenuBackdrop.CaptureFrame]`
- 手册按当前输入写明海报和物品使用鼠标左键；没有新增鼠标右键操作。`[verified: ApartmentPlayer.Update / InteractSecondaryFocused]`

## Discovered edge cases

- 打开手册时暂停菜单列隐藏并锁定；已排队的菜单回调也被拒绝。关闭手册恢复此前可用的菜单焦点，游戏继续保持暂停。`[verified: PauseMenuView.ShowManual / TryCloseManual / Build]`
- 暂停整体关闭时清除手册页，下次打开回到命令目录；在背景捕获的零透明度帧中，手册仍保持打开。`[verified: PauseMenuView.ResetManual / SetVisible / Update]`

## Questions for review

- 主界面接入 `ShowManual`，Esc 优先调用 `TryCloseManual`，全局存读档快捷键在手册打开时锁定，由主任务统一实现。`[opinion]`
- 静态差异检查已通过；Unity PlayMode、图片和 Player 检查由主任务在隔离工程串行执行。`[verified: git diff --check pause-owned files exit_code=0]`

偏离项：2。`[verified: 本文 Deviations]`
最可能复查：实际视觉中的字号与留白。`[opinion]`
边界项：2。`[verified: 本文 Discovered edge cases]`
已添加覆盖：平面点击、琥珀配色、手册只读、绑定文字、焦点和关闭重开。`[verified: PauseMenuPlayModeTests]`
下次先读：`PauseMenuView.ShowManual` 与 `TryCloseManual`。`[opinion]`
