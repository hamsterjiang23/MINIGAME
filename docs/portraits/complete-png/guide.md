# 完整人物PNG接入

本批22张透明PNG均已接入：小孩3、执政者4、外交官4、战场医生4、神父4、教学引导者3。其中11张已存在且源文件完全相同，其余11张新增。导入过程只配置Unity切片与目录引用，不改PNG像素。来源与SHA256见[source-manifest.json](source-manifest.json)。`[verified: source-manifest.json；Artifacts/NamedPortraits/import.txt；EditMode NamedPortraitAssetsTests]`

正式资源位于 `UnityProject/Assets/MiniGame/Art/Characters/NamedPortraits/`；`Assets/Resources/MiniGame/PortraitCatalog.asset`共25个逻辑编号。为兼容旧存档和预览，小孩ML04/ML05指向新ML03，神父JN05指向新JN04，避免回到旧版人物。`[verified: PortraitCatalog.asset entries；NamedPortraitAssetsTests LegacyPortraitIdsResolveToTheNewCharacterSprite]`

## 显示与预览

Unity菜单 **MiniGame → 人物监控室** 可查看人物与表情；**MiniGame → 人物素材 → 更新命名PNG目录** 可重新绑定命名资源。已有切片元数据保留，新PNG按透明边界裁切并保持脚底对齐。`[verified: PatientMonitorWindow.cs MenuItem/CharacterCrop；NamedPortraitImport.cs Run]`

ED01–03、JN01–03使用前景半身构图；ED04为床沿坐姿；JN04站于地面。教学引导者D0_01–03使用房间中景。保留VV04原床位及当前监控颜色和扰动设置。`[verified: PatientMonitorDefault.asset PortraitPlacements；Final/monitor-patient-*.png]` `[opinion]`

第零天开场继续显示居中的NORM标志，点击并拉远后显示新的D0_01教学对象；门外监督员JD系列为独立角色，未替换。D0_02/03已在目录和监控中验证，但没有为未标注表情的对白虚构切换时机。`[verified: TerminalView.cs SetOpeningLogo/Render/RestorePresentation；Final/day0-opening-terminal.png；NamedPortraitAssetsTests]`

## 验证证据

- [医生](../../../Artifacts/NamedPortraits/Final/monitor-patient-battlefield-doctor-ED01.png)、[抱头坐姿](../../../Artifacts/NamedPortraits/Final/monitor-patient-battlefield-doctor-ED04.png)、[神父](../../../Artifacts/NamedPortraits/Final/monitor-patient-priest-JN01.png)、[完整站姿](../../../Artifacts/NamedPortraits/Final/monitor-patient-priest-JN04.png)。`[verified: 本轮实际GPU截图]`
- [引导角色](../../../Artifacts/NamedPortraits/Final/monitor-patient-tutorial-D0_01.png)、[教学界面](../../../Artifacts/NamedPortraits/Final/day0-opening-terminal.png)、[Closing session](../../../Artifacts/NamedPortraits/Final/day0-closing-session.png)。`[verified: 本轮实际GPU截图]`
- 22张源PNG的目录引用、Alpha透明度、3个别名与6个人物的预览上限共32项EditMode检查通过；22张角色逐张真实GPU渲染并验证人物像素贡献，保存独立截图。`[verified: Artifacts/NamedPortraits/editmode-final.xml；PatientMonitorCompositionPlayModeTests；Final目录22张monitor-patient PNG]`

视觉截图已自查并由另一代理只读检查新增姿态；最终美术接受仍以用户反馈为准。当前夜间灯光资产显式关闭床灯，本轮保留该设置；不将效果组件存在误写成床灯已开启。`[verified: monitor_asset_audit报告；RoomLightingNight.asset SceneLightDefaults]` `[opinion]`

## Windows Mono交付

[完整角色包](../../../Builds/Deliverables/NORM-Windows-x64-Mono-CompletePortraits-20261022-181735.zip)，解压后运行MiniGame.exe。构建0错误／0警告，最终Player开场完成教学、一次净化、一次睡觉并进入Day1；ZIP内159个文件逐一核对SHA256。测试范围与历史重试见[verification.json](verification.json)。`[verified: Artifacts/NamedPortraits/windows-build.json；Player/Opening/verification.json；package.json verifiedZipFileEntries/validation]`
