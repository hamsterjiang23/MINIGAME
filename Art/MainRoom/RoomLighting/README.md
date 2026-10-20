# 参考房间地毯资产

地毯主体与 260 组流苏来自现有 `../exports/09_Rug.glb`，合并为一个 mesh、一个材质槽，共 3664 顶点、3132 三角形；未修改原始 GLB。`[verified: rug-mesh-report.json sourceMeshCount/vertices/triangles/submeshes]`

生成后的模型位于 `../../../UnityProject/Assets/MiniGame/Art/MainRoom/RoomLighting/WornReferenceRug.obj`。模型中心在 XZ 原点、底部 Y=0、平面尺寸 1×1，保留约 8mm 厚度；默认 Transform 尺寸为 `(3.4, 1, 2)`。`[verified: rug-mesh-report.json normalisedSize/defaultScale]`

原始 OBJ 可用以下命令重建：`[verified: build_rug_mesh.py main]`

```powershell
uv run --no-project python Art/MainRoom/RoomLighting/build_rug_mesh.py
```

`WornTraditionalRugAlbedo.png` 由内置 imagegen 生成，使用用户房间参考图作为配色与风格参考，采用原创棕红、赭黄、旧花纹图案。贴图是无透视、无场景投影的完整地毯 UV 底色；完整生成提示保存在 `rug-albedo-prompt.txt`。`[verified: 本轮 imagegen 输出 exec-99777375-bb57-4eac-ab9f-1e8987a88cd2.png]`

`RoomRugSetup.Ensure(scene, parent, roomBase)` 在已有场景添加或更新单一地毯 Renderer，接收阴影、不投射阴影、没有 Collider。首次接入按 `B_Floor.bounds.max.y + .008` 放置；已有 Transform 不重置，供房间灯光配置控制位置和尺寸。`[verified: ../../../UnityProject/Assets/MiniGame/Editor/RoomRugSetup.cs Ensure]`
