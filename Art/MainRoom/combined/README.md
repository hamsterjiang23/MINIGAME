# 融合主房间

融合 `Art/MainRoom` 与 `Art/MainRoom2`，源文件保留；新的可编辑房间为 [MainRoomCombined.blend](scene/MainRoomCombined.blend)，交换资源为 [MainRoomCombined.glb](exports/MainRoomCombined.glb)。[verified: reports/composition.json inputs]

Unity 入口：[MainRoomCombined.unity](../../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity)。门、电脑和床已接入现有游戏逻辑，默认体验为小孩 Day1；电脑提供推镜、开机、UI渐显和返回房间过渡，操作与验证见[玩法接入报告](../../../docs/main-room-gameplay-verification.json)。[verified: ../../../docs/main-room-gameplay-verification.json scene/interactions/presentation]

## 组合取舍

- MainRoom：保留房间布局、窗框/窗帘/暖气、CRT与低面数输入设备、原创海报、地毯和散落报纸。[verified: reports/composition.json source_a_retained]
- MainRoom2：采用桌椅、床架/床垫/枕头/被褥、柜子、书本、杯瓶和纸箱；木材纹理也用于地板板块。[verified: reports/composition.json source_b_retained/wood_surface_source]
- 窗外：保留第二版实际的水泥墙与台沿，窗玻璃为轻微积尘的透明表面，外墙与窗保持空间距离。[verified: reports/composition.json window_exterior]
- 摆放：保留电脑屏幕视线；每件引入的室内物件检查墙内范围、地面高度，桌面物件另查支撑高度。外墙单独检查位于窗外。[verified: reports/composition.json source_b_placement_checks/work_surface_adjustment]

第二版带有位置动画；融合时固定当前帧造型并清除这些物件的动画，避免刷新后回到旧位置。床品、家具和海报继续保留独立物件，小型静态碎件整理为命名网格。[verified: reports/composition.json source_b_frozen_object_animations/static_detail_groups]

## 重建

在仓库根运行以下命令，然后在 Unity 使用 `MiniGame/Room Art/Import and open combined room`。[verified: ../scripts/combine_rooms.py main；../../../UnityProject/Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs ImportCombinedAndOpen]

```powershell
blender --background --threads 2 --python Art/MainRoom/scripts/combine_rooms.py
blender --background --threads 2 --python Art/MainRoom/scripts/export_unity.py -- --source Art/MainRoom/combined/exports/MainRoomCombined.glb --output UnityProject/Assets/MiniGame/Art/MainRoom/Combined --evidence-dir Artifacts/Unity/MainRoomCombined
```

最初美术融合结果保留在[导入记录](../../../docs/main-room-combined-verification.json)，后续碰撞与交互验证另见[玩法接入报告](../../../docs/main-room-gameplay-verification.json)。性能与发布构建仍未在本轮验收。[verified: ../../../docs/main-room-gameplay-verification.json limits]
