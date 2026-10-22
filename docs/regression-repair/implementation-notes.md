# Deviations

- 保留其他未提交改动，只恢复指定行为；参考提交为 `25a95089`。`[verified: git show 25a95089]`

# Discovered edge cases

- Day0 音频绑定在游戏音源启用开始音乐，新主菜单已有独立音源。`[verified: MiniGameAudioDirector.cs Bind/SynchronizeLoops；MainMenuController.cs Build]`
- 导入器交换了 ED、JN、D0 编号；本轮按要求取消同名 ID 重映射。`[verified: NamedPortraitImport.cs NamedId]`
- 梦后禁用 CharacterController 并直接移动 Transform，单加 Collider 无法阻止穿床。`[verified: ApartmentPlayer.cs UpdateDreamTraversal/TryMoveDreamTraversal]`
- 新导入的四份 WAV 的 meta GUID 与 AudioDefault 的引用不同，Unity 返回空片段；修正引用后音频测试 15/15 通过。`[verified: Artifacts/RegressionRepair20261022/MiniGameAudioPlayModeTests.json summary]`
- 独立审查发现大步斜向移动可穿床，已增加完整线段碰撞检测及旧存档床内位置恢复。`[verified: ApartmentPlayer.cs TryMoveDreamTraversal；DreamTraversalPlayModeTests.json summary]`

# Questions for review

- 两项房间灯光检查未通过：环境反射强度断言，以及灯光渐暗连续性断言；不归入已通过项目。`[verified: Artifacts/RegressionRepair20261022/BedtimeGuidancePlayModeTests.json results]`

偏离数：1，保留既有未提交工作，只修复请求范围。`[verified: 本文件 Deviations]`
可能需要复查：两项灯光断言。`[verified: 本文件 Questions for review]`
边界情况：音频 GUID、立绘编号、梦后碰撞、斜向移动及旧存档位置。`[verified: 本文件 Discovered edge cases]`
针对性检查：120 项通过；额外两项灯光检查失败。`[verified: verification.json targetedTests/additionalChecks]`
后续首先读取：本文件及 verification.json。`[opinion]`

后续五项修复已解决上述两项 Day0 灯光失败；当前三项灯光检查均通过，完整结果见 followup-verification.json。`[verified: followup-verification.json tests.unity.suites filter=BedtimeGuidancePlayModeTests]`
