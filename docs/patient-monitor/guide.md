# 人物监控室

运行 **MainRoomCombined**，进入电脑并审查人物，监控使用独立动态隔离室。当前[琥珀终端](../terminal-amber/guide.md)采用16:9固定全景、暖灰色调、低帧率、扫描线与CAM编号；近景在人物监控室编辑器预览中调整。`[verified: ../terminal-amber/verification.json ui/monitor；../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs UpdatePortrait]`

**MiniGame → 人物监控室** 打开中文配置面板。选择人物与表情编号后，点击 **独立监控预览**；无需开始正式剧情，支持查看表情近景。可调项包括画面分辨率、监控帧率、明暗、色彩、噪点、轻微呼吸，以及各人物房间颜色、顶灯颜色、污旧程度、人物高度和位置、相机位置与视角。支持 Undo／Redo；点 **保存配置** 保存配置资产。`[verified: ../../UnityProject/Assets/MiniGame/Editor/PatientMonitorWindow.cs OnGUI/StartPreview/TickPreview; ../../UnityProject/Assets/MiniGame/Runtime/PatientMonitorProfile.cs]`

六位人物使用六间分别创建的监控室，包含软包墙、床、钢门、风扇、桌椅、洗手盆和对讲机；只渲染当前人物的房间。房间与隔离摄像机独立于玩家房间和自我审查镜头。暂停／查看记录时冻结监控动画，归档和离开电脑后隐藏普通患者监控。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientMonitorRoom.cs Build; ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs SelectRoom/Advance/OnDisable; ../../UnityProject/Assets/MiniGame/Runtime/TerminalView.cs Render/UpdatePortrait]`

新立绘共 **6 张透明图集、27 个表情**，保留原有 ML、VV、ED、AD、JN、D0 表情 ID。原素材仍在 `Assets/MiniGame/Art/Characters/`，新图集位于其 `Surveillance/` 子目录；使用 **导入重绘立绘** 可重新切片。后续 Campaign 导入优先使用新图集。图集只通过 Sprite 元数据裁框和注册脚底，PNG 原图不变。生成提示词与来源见 [art-generation.json](art-generation.json)。`[verified: ../../UnityProject/Assets/MiniGame/Editor/PatientMonitorWindow.cs ImportArtwork/CharacterCrop; ../../UnityProject/Assets/MiniGame/Editor/CampaignBuilder.cs ImportPortraits; art-generation.json stableIds/artA/artB]`

坐姿已移到床沿前方，并取消朝高位相机倾斜整个立绘的处理。当前配置下，27 个表情的全景与近景完成逐画面 GPU 家具遮挡检查；这些结果不覆盖策划今后任意移动人物或家具后的新位置。验证记录见 [verification.json](verification.json)。`[verified: ../../UnityProject/Assets/MiniGame/Runtime/PatientMonitorRoom.cs ActorAnchor; ../../UnityProject/Assets/MiniGame/Runtime/PatientSurveillanceFeed.cs RenderFrame; verification.json occlusion]`
