# 房间阅读物件

桌面拆封家书、报刊栏日报和墙上录用通知书使用 Word 中的文案。日报按实际游戏 Day1、Day2、Day3切换；Day0及未配置日期不可阅读。文案资源位于 `Assets/MiniGame/Resources/MiniGame/Readables/`，可在 Inspector 修改。`[verified: RoomReadableDocument.GetText；source-manifest.json documents]`

准星指向距离内的物件后使用鼠标左键阅读；左键、空格或回车翻页，Esc关闭。全部阅读面板（包括原海报）使用黑底无框白字。`[verified: ApartmentPlayer.InteractSecondaryFocused；MiniGameScreen.Update；PropNarrationView]`

每页首次显示时选择避开物件的位置，右侧保留1.5%边距。转动视角时保持该页在视口中的位置和大小；物件进入阅读区域时面板原位暂隐，移开后恢复同一页。翻页或视口宽高比变化时重新选择位置；初始空间不足时暂隐并等待可用空间。`[verified: PropNarrationView.cs L130-L188、L239-L240]`

非末页显示“点击继续阅读”，末页隐藏该提示。海滩海报文案按最新截图更正为“海报里的小镇是我的故乡...但我从没去过海边”，替代此前海报用语。`[verified: PropNarrationView.cs L214-L218、L272；ReferenceRoomGameplayBuilder.cs L90；本轮用户截图红框]`

三张外观图使用用户最后提供的原图，保留透明通道和原始像素。场景作者工具从 `Art/RoomReadables/Textures/` 导入，不再重建旧的占位印刷图；重装入口为 `MiniGame > Main Room > Install readable letter, newspaper and notice`。`[verified: ../../Art/RoomReadables/Textures/source-manifest.json；RoomReadablePropsSetup.PrintedTexture]`

修正版本入口：[MiniGame.exe](../../Builds/ReadingStable/MiniGame.exe)。11项PlayMode检查、Windows构建及独立Player启动已通过，详见[阅读稳定性验证记录](reading-stability-verification.json)。此前的[验证记录](verification.json)保留三张物件外观接入时的基线。`[verified: reading-stability-verification.json tests、build、playerStartup；verification.json tests]`
