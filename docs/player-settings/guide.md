# 玩家画面设置

进入游戏后按 **Esc → 游戏设置 → 像素风强度**。高／中／低分别为像素尺寸 **8／4／2**，首次启动默认低档 **2**。切换立即生效，自动保存；按 Esc 先返回暂停菜单，再按一次恢复游戏。[verified: 用户确认；PlayerGraphicsSettings；PauseMenuView.ShowSettings/TryCloseSettings；MiniGameBootstrap.HandleEscape]

设置页显示实时场景，三档均可通过鼠标或上下键选择。选择保存在本机 PlayerPrefs，重新进入场景、读档或重启游戏后继续使用，不写入故事存档或白天／夜晚美术配置。选项只调整场景像素尺度，UI 保持清晰；梦境专用处理仍由演出配置控制。[verified: PlayerGraphicsSettings.SetDetail/Reload；PauseMenuView；RetroScreenEffect.PlayerPixelSettingsActive/Render]

本轮按用户要求提交推送，未生成新 EXE 包。此前已完成 3 项冒烟测试与 27 项 PlayMode 回归；目标 Player 的打包与重启验收留待后续构建。[verified: 用户要求“不用打包测试直接push”；Artifacts/PlayerPixelSettings20261022/smoke.xml、playmode.xml]
