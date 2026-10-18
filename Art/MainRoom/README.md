# 主场景美术资源

所有本次制作的资源集中在本文件夹内。打开[完整场景](scene/MainRoom.blend)查看房间；[主视角预览](previews/main-room-preview.png)用于核对构图。[opinion]

当前融合版入口见 [combined](combined/README.md)：结合本版布局、原创图形和窗暖，与第二版家具、床品和生活道具；窗外水泥墙及台沿保留。[verified: combined/reports/composition.json source_a_retained/source_b_retained/window_exterior]

2026-10-18 首版导入预览保留在 [MainRoomPreview.unity](../../UnityProject/Assets/MiniGame/Scenes/MainRoomPreview.unity)，菜单 `MiniGame/Room Art/Import and open preview` 可重建。后续可玩融合版入口是 `MainRoomCombined.unity`，门、电脑与床的接入范围见[玩法报告](../../docs/main-room-gameplay-verification.json)。[verified: ../../docs/main-room-import-verification.json scene；../../docs/main-room-gameplay-verification.json scene/interactions]

## 文件入口

- references：用户提供的两张主场景参考图；第一张核对形体、摆放和材质，第二张核对粗粝、低清的效果。[verified: reports/source-provenance.json references]
- concepts：使用内置图像生成工具制作的书桌、CRT显示器、铁床独立概念图及完整提示词。[verified: reports/source-provenance.json concepts]
- scene/MainRoom.blend：组装后的房间，单位为米，保留独立对象、材质、灯光和多个相机。[verified: reports/scene-validation.json]
- models：可单独使用的Blender部件；另保留Tripo下载原件、减面后的CRT模型。[verified: reports/asset-manifest.json parts；reports/tripo-crt-validation.json]
- exports：各部件及整个房间的GLB交换文件。[verified: reports/asset-manifest.json]
- textures：原创程序化材质贴图，以及Tripo显示器的颜色、法线和粗糙度/金属度贴图。[verified: reports/source-provenance.json textures]
- previews：完整房间、书桌细节、Tripo结果与Blender界面截图。[verified: reports/scene-validation.json previews]
- scripts：材质制作、建模、Tripo整理、导出和灯光预设脚本；reports保存实际制作与核验记录。[opinion]

## 部件与使用

场景按房间结构、人字木地板、书桌、椅子、电脑、铁床、窗户/窗帘、暖气/管线、地毯、小柜、吊灯、纸张和小道具组织。抽屉、键帽、床架、床品、窗格、暖气片等保留独立对象，可在Blender中继续编辑。[verified: reports/scene-validation.json collections]

书桌和铁床按生成概念图在Blender中程序化建模；CRT先生成概念图，再上传Tripo H3.1生成模型，随后在Blender中减面、定尺寸并放入场景。Tripo原件保留，场景使用约2万三角面的版本。[verified: reports/tripo-crt-validation.json objects]

scripts/set_lighting.py 提供日常、夜晚、全黑、床上聚光灯、仅电脑屏幕五种美术预览配置。在Blender文本编辑器打开脚本，修改 PRESET 后运行。预设只调整本场景灯光，不能视为游戏流程已接入。[verified: scripts/set_lighting.py]

完整场景及部件Blender文件打包贴图；贴图副本仍集中保存在 textures。搬运时建议复制整个 MainRoom 文件夹。[opinion]

models 中按编号保存的Blender部件是集合库：在Blender使用 File → Append，进入对应文件的 Collection，选中集合追加到当前工程。完整房间直接打开 scene/MainRoom.blend。[opinion]

## 当前边界

这是依据两张图搭建的可编辑场景第一版。图片没有尺寸和背面信息，房间尺寸、未入镜的入口和遮挡细节按制作需要推定；墙上图形为原创占位，不是已确认叙事文案。[opinion]

GLB以普通UV贴图表达Blender中的盒状投射材质，局部纹理朝向可能有差异；以Blender文件和实际渲染为美术核对依据。美术制作时尚未验证 Unity；后续导入预览已单独记录，碰撞、互动、性能和正式发布构建仍未在本次导入中验证。[verified: ../../docs/main-room-import-verification.json scope/limits] [opinion]

本目录的二进制资源沿用仓库LFS要求；参考图、概念图、Tripo原件、可编辑场景、独立部件、贴图、预览、导出、脚本和验证记录一起纳入版本管理。Blender自动备份和此前会话备份保留在本地，不作为交付资产。[opinion]

## 从GitHub获取完整物料

仓库保持私有，先使用具有访问权限的账号克隆仓库。安装Git LFS后，在仓库目录执行以下命令获取本目录的大文件；不要仅把网页中的LFS指针文件当作模型或图片。[opinion]

    git lfs install
    git lfs pull --include="Art/MainRoom/**" --exclude=""

随后打开 scene/MainRoom.blend。完整原始文件的大小与SHA256见[物料同步清单](reports/publish-inventory.json)，场景和GLB核验见[验证记录](reports/scene-validation.json)；后续 Unity 导入范围见[导入记录](../../docs/main-room-import-verification.json)。[verified: reports/scene-validation.json scope；../../docs/main-room-import-verification.json scope]

重做 Unity 交换资源时，在仓库根运行 `blender --background --threads 2 --python Art/MainRoom/scripts/export_unity.py`，随后在 Unity 执行导入预览菜单。脚本使用现有 GLB，保留原始美术文件；预览灯光为实时近似，未承诺还原 Blender 的 Cycles 渲染。[verified: scripts/export_unity.py main；../../UnityProject/Assets/MiniGame/Editor/MainRoomPreviewBuilder.cs ImportAndOpen] [opinion]
