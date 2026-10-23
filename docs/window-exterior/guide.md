# 窗外楼群替换

主场景 `MainRoomCombined` 的窗外已替换为用户提供的 `employee apartment1.0(1).blend` 中七栋楼与地面，保留原始世界坐标、比例和材质贴图。室内、窗框、家具、走廊、灯光配置、碰撞和玩法对象保持原有数据。[verified: reports/unity-import.json exteriorRenderers；reports/scene-preservation.json passed/unchangedOriginalDocuments；reports/corridor-gameplay-preservation.json preserved]

唯一修改的原有数据为15个旧窗外构件的 renderer 开关，以及主相机远裁剪距离从45米改为300米；新增独立根节点 `Window exterior - Employee Apartment 1.0`。新楼群不投射阴影到房间。[verified: reports/scene-preservation.json hiddenBackdrop/cameraFarChanges/exteriorRootAppended；../../UnityProject/Assets/MiniGame/Editor/WindowExteriorSetup.cs renderer.shadowCastingMode]

来源、SHA256、选取物件、原始包围盒和贴图检查见[来源清单](../../Art/Exterior/employee-apartment-window-source.json)。Blender原文件未保存，导出时关闭自动脚本执行；没有导入来源中的室内物件、相机或灯光。[verified: ../../Tools/EmployeeApartment/export_window_exterior.py EXTERIOR_NAMES/export_cameras/export_lights；来源清单 scope]

## 查看与重新安装

直接打开[主场景](../../UnityProject/Assets/MiniGame/Scenes/MainRoomCombined.unity)并运行。重新安装入口为 `MiniGame → Room Art → Replace window exterior with Employee Apartment 1.0`，无需重新生成房间。[verified: ../../UnityProject/Assets/MiniGame/Editor/WindowExteriorSetup.cs MenuItem/ApplyInOpenScene]

不要运行“Replace room with Employee Apartment”来更新窗外；那个工具的职责是替换整个室内美术。[verified: ../../UnityProject/Assets/MiniGame/Editor/EmployeeApartmentImport.cs ApplyInOpenScene]

## 画面与验证

- [替换前](evidence/before-window.png)、[替换后](evidence/after-window.png)、[Play Mode实际相机与后处理](evidence/playmode-window.png)。[verified: 对应PNG]
- Unity脚本编译通过；Play Mode中8个新增renderer有效，远裁剪为300米；无新增材质缺失。[verified: reports/unity-import.json exteriorRenderers/missingMaterials/cameraFarAfter；evidence/playmode-window.png]
- 全部781份原有场景文档逐一比较：764份完全相同、15份只关闭旧窗外renderer、1份只扩大远裁剪、1份只追加新根节点引用；没有删除原有文档。[verified: reports/scene-preservation.json beforeDocuments/unchangedOriginalDocuments/hiddenBackdrop/cameraFarChanges/exteriorRootAppended/unexpectedChanges]
- 走廊、门、玩家、电脑与床等12组保护检查全部通过。[verified: reports/corridor-gameplay-preservation.json checks/preserved]
- 导出脚本在同一个隔离Blender进程中连续导出两次通过，原始物件名称保持不变。[verified: Blender重复导出输出 REPEAT_EXPORT_PASS exports=2 source_object_names_preserved=true]
- 本次没有打包新的Player构建；控制台在替换前已存在Package Manager的 `path argument ... Received undefined` 错误。[verified: 本轮未执行Player构建；替换前get_console_logs输出]

Unity保存时自动改写了两处接近零的预览灯强度、移除了两个旧音频序列化字段；交付前已从本轮修改前备份恢复这三份文档，全量检查按恢复后的磁盘文件执行。[verified: reports/scene-preservation.json passed/unexpectedChanges；implementation-notes.md Discovered edge cases]

## 窗户交互

窗口现复用原阅读物件的交互与UI：3.5米内对准窗户后按E或鼠标左键，黑底白字显示“无论搬到哪里，窗外永远是这幅景色。”，再次点击、空格或Enter关闭，Esc也可关闭。内容每天相同，阅读不修改会谈或日期状态。[verified: ../../UnityProject/Assets/MiniGame/Resources/MiniGame/Readables/WindowObservation.asset text/changesWithDay；reports/window-interaction-playmode.json；ApartmentPlayer.HandleInteractionInput；PropNarrationView.Advance]

[文案来源截图](window-text-source.webp)与[实际UI截图](evidence/window-dialogue.png)已保留。配置入口为 `MiniGame → Main Room → Install window observation`；重新生成参考房间时也会安装。原窗框、玻璃及窗外楼群未修改，交互使用独立透明面接收射线。[verified: ../../UnityProject/Assets/MiniGame/Editor/WindowInteractionSetup.cs ConfigureOpenScene；ReferenceRoomGameplayBuilder.Configure；reports/window-interaction-preservation.json passed]

窗户定向PlayMode测试1/1通过，验证E和左键、距离、描边、阅读光标、原UI、单页关闭和游戏状态不变；新增脚本复审通过。场景修改前782份文档中781份保持一致，仅根节点列表追加引用，另增6份窗户交互文档。[verified: reports/window-interaction-playmode.json data.result.summary；reports/window-interaction-preservation.json；代码复审“No bugs found”]

Package Manager的 `path argument ... undefined` 来自启动环境缺少 `ALLUSERSPROFILE`。Unity随附的 `app.js` 在Windows上直接用该变量计算全局配置目录；给Editor子进程补齐为 `C:\ProgramData` 后，当前包查询成功且错误控制台为空；未修改系统环境或工程依赖。[verified: 本轮环境变量输出；UPM app.js getGlobalConfigRoot；package-list.json success=true；get_console_logs total=0]
