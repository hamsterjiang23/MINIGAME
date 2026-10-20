# CRT 屏幕贴合与每日电源

物理显示器使用原模型玻璃曲率生成的覆盖网格，替换原先悬浮的四顶点矩形；原屏幕对象、材质和场景交互引用继续使用。正面与左右侧预览见 [front.png](previews/front.png)、[left.png](previews/left.png)、[right.png](previews/right.png)。`[verified: CrtScreenFitPostprocessor.Apply；../../Art/MainRoom/screen-fit/report.json vertices/triangles；verification.json geometry]`

覆盖面有3,191顶点、6,153三角形，玻璃前后深度变化36.87毫米。采样实际模型表面后沿观察轴向前偏移0.65毫米，圆角边界收在玻璃内；四处源网格缺口使用附近有效采样补齐。深度变化表示玻璃曲率，不是覆盖面浮起的距离。`[verified: ../../Art/MainRoom/screen-fit/report.json depthVariationMillimetres/method/scanHoleFallbackCount]`

生成脚本为 [fit_screen.py](../../Art/MainRoom/screen-fit/fit_screen.py)，源模型、资源和脚本SHA256保存在同目录 [report.json](../../Art/MainRoom/screen-fit/report.json)。Unity导入器通过四个参考标记转换坐标，并校验实际显示器包围盒；源模型移动或改变后应重新采样。FBX开启Read/Write，让导入阶段可以原位修改屏幕子网格，保留其文件标识；源FBX几何文件未改写。`[verified: CrtScreenFitPostprocessor.OnPreprocessModel/Apply；report.json sourceSha256/resourceSha256/generatorSha256]`

每天首次交互前黑屏，第一次进入播放开机与音效；目录“返回房间”只退出操作，屏幕保持亮着，再次进入免开机继续。全部当天会谈及反应结束后，才播放Closing session、停止、收线余辉与关机音效并熄屏。电源状态和当天编号写入存档，跨日重置。Day0沿用监管者开场，本人复核保留安全屏引导。`[verified: GameRuntime.TryInteract/CompleteCue/Save/Restore/Accept；PresentationEffects.PowerOn/PowerOff；AmberTerminalPlayerSmoke.Tick]`

起床仍回到安全位置，但保留玩家朝向、俯仰角和视野；海报正文使用完整白色矩形框，底部装饰线与操作提示已移除。海报实际场景预览见 [poster.png](previews/poster.png)。`[verified: ApartmentPlayer.PlaceAtWakePose；PropNarrationView.Build；PropNarrationPlayModeTests.ReadingOwnsInputPagesAndPauseWithoutChangingGameState]`

运行版：[Builds/TerminalScreenFit/MiniGame.exe](../../Builds/TerminalScreenFit/MiniGame.exe)。构建产物需保留整个文件夹。测试结果、证据路径及验证限制见 [verification.json](verification.json)。`[verified: verification.json build]`
