# 全局房门交互开关

Unity 菜单 **MiniGame → 全局交互 → 监督员对话后锁门** 是全局勾选开关，默认开启。也可选择 **打开配置**，在 Inspector 修改 `LockDoorAfterSupervisor`。[verified: RoomInteractionSettingsMenu.cs 的 ToggleDoorLock、OpenSettings；RoomInteractionSettings.asset 的 LockDoorAfterSupervisor]

- 开启：Day1 起，当天监督员对话完成后，关闭的门不可再打开；门口提示“门已锁”。[verified: GameRuntime.IsDoorLockedAfterSupervisor、WorldInteractions；ApartmentPlayer.RefreshFocus]
- 关闭：仍可开门观察走廊；不会重新触发当天已完成的监督员对白。[verified: GameRuntime.CompleteCue 的 DoorMotion 分支]
- 对话结束时已经在门外，仍可返回并关闭门；次日晨检阶段重新允许开门。[verified: GameRuntime.ExitDoorView、ConfigureDoorLockAfterSupervisor、IsDoorLockedAfterSupervisor]
- 播放时切换立即作用于当前运行实例；读取存档后重新应用当前全局配置，不把此选项保存在每份游戏进度里。[verified: MiniGameBootstrap.Update、InstallRuntime；GameRuntime.ConfigureDoorLockAfterSupervisor]

配置资产：[RoomInteractionSettings.asset](../../UnityProject/Assets/MiniGame/Resources/MiniGame/RoomInteractionSettings.asset)。状态仍由 GameSession 的日期与阶段决定，没有另存一份“监督员完成”标记。[verified: GameRuntime.IsDoorLockedAfterSupervisor]

## 开门与关门音效

门开始打开时播放 McQueen 的 `DoorOpen.wav`，开始关闭时播放 `DoorShut.wav`。在 **MiniGame → 音频设置** 的“开门”“关门”条目可分别调整片段和音量，统一经过场景音效通道。[verified: HingedDoor.MotionStarted；MiniGameAudioDirector.PlayDoorMotion；AudioDefault.asset 的 Id 21、22]

锁门拒绝、初始定位和读档不播放门声；每个门动作只播放一次，暂停恢复不重播。需要重新导入源文件时可用 **MiniGame → 音频 → 应用开门与关门音效**。[verified: DoorAudioPlayModeTests；MiniGameAudioSetup.ApplyDoorSounds]
