# 两相机中点的海滩全景

当前梦境天空盒来自 `C:/Users/admin/Downloads/sea(1).blend` 的实时未保存场景。房屋使用当前 [MainRoomReference.blend](../../../Art/MainRoom/reference/MainRoomReference.blend)，出生点读自其 [layout.json](../../../Art/MainRoom/reference/layout.json) 的 `markers.Spawn`，拍摄时隐藏房屋。[verified: ../../../Art/DreamEnding/SeaMidpoint/capture-settings.json]

Blender 中 `Camera` 与 `Camera2` 的世界位置中点为 `(-12.956236, -47.507217, 4.944919)`，直接作为第一人称拍摄眼位，不再叠加 1.56 米。角色脚底参考为 `(-12.956236, -47.507217, 3.384919)`，房屋随之降低，保留玩家自身 1.56 米眼高。房屋导出时水平两轴翻转，工作副本用额外 180 度旋转对齐，拍摄眼位误差为 0 米。[verified: ../../../Art/DreamEnding/SeaMidpoint/capture-settings.json camera_midpoint_blender、spawn_blender、eye_blender、capture_eye_alignment_error_m]

海景中的原床 `SM_bed_1` 已从 Blender 工作文件移除，新全景与海面遮罩均从降低后的眼位重拍。[verified: ../../../Art/DreamEnding/SeaMidpoint/capture-settings.json removed_source_objects、capture_revision]

天空在工作副本中改为半径 700 米的内向半球，替换两片平面天空和原暗红背景立方体；仅采样既有环境 JPG 的天空区域，避开下方的草地与材质展示球。[verified: ../../../Art/DreamEnding/SeaMidpoint/hemisphere-sky.json]

- [可编辑 Blender 工作副本](../../../Art/DreamEnding/SeaMidpoint/SeaMidpointAligned.blend)：活动场景为 `Dream sea midpoint capture`，房屋参考以线框显示，渲染隐藏。[verified: MCP verify-final-scene]
- [完整十字展开图](../../../Art/DreamEnding/SeaMidpoint/cubemap-cross.png)：4096×3072，六个面各 1024×1024。[verified: ../../../Tools/SeaDreamCapture/package_cubemap.py contact_sheet]
- [六面预览](../../../Art/DreamEnding/SeaMidpoint/faces-contact.png)与 [Unity 原生 Cubemap](../../../UnityProject/Assets/MiniGame/Art/DreamEnding/SeaMidpointPanorama.asset)。[verified: ../../../Art/DreamEnding/SeaMidpoint/cubemap-verification.json]

重拍脚本位于 [Tools/SeaDreamCapture](../../../Tools/SeaDreamCapture/)：新打开原海滩后运行 `prepare_capture.py`、`hemisphere_sky.py`，再运行 `render_faces.py`；通过 Blender MCP 的 `args` 传入 `smoke=true` 可先试拍，`gpu=true` 可临时启用 CUDA。正式六面完成后用 `uv run --with pillow Tools/SeaDreamCapture/package_cubemap.py` 打包并更新梦境配置。[verified: ../../../Tools/SeaDreamCapture/prepare_capture.py；../../../Tools/SeaDreamCapture/hemisphere_sky.py；../../../Tools/SeaDreamCapture/render_faces.py；../../../Tools/SeaDreamCapture/package_cubemap.py]

Unity 2022.3.23f1c1 独立验证项目已通过六面像素存储和 GPU 天空盒渲染检查；当前六面渲染对比的 RGB 平均绝对误差为 0.36–3.18/255。最新去床、降低眼位的重拍还通过动态海浪、复古效果和 3 项针对性 Play Mode 测试，见[重拍验证](retake-verification.json)。该验证没有试跑完整 Campaign，没有重新打包 Player。[verified: ../../../Art/DreamEnding/SeaMidpoint/cubemap-verification.json unity_verification、full_face_gpu_comparisons；retake-verification.json]

动态海浪和可调复古效果已在 Unity 2022 接入，参数入口与本次运行验证另见[运行与调参说明](runtime-guide.md)。重拍场景后还需运行 `render_water_mask.py` 和 `package_water_mask.py` 更新水面遮罩。[verified: DreamSettingsWindow.OpenEndingPanel；Tools/SeaDreamCapture/render_water_mask.py；Tools/SeaDreamCapture/package_water_mask.py]
