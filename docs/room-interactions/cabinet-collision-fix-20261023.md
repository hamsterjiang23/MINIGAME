# 门边柜子碰撞修复

## 复现与修改

- 正式 `MainRoomCombined` 中，门旁的 `B_立方体.023` 使用原模型网格碰撞时，39 条程序化靠近/后退路径中有 23 条出现后退位移为零。原始位置与轨迹保存在 [before-scan.json](../../Artifacts/CabinetCollision/before-scan.json)。`[verified: before-scan.json data.result.result[].escaped]`
- 为该柜子增加不可见的 `Cabinet solid collision` 实体盒形碰撞，每侧留出 1 cm 局部空间余量，阻止玩家进入柜板边缘。原模型及其交互射线碰撞保留；新增实体放在 `Ignore Raycast` 层，仍与玩家发生实体碰撞。`[verified: UnityProject/Assets/MiniGame/Editor/ReferenceRoomGameplayBuilder.cs L174–195；tests-final.json SolidCabinetStillAllowsInteractionRayToFamilyLetter=Passed]`
- 正式场景已保存该子物体；房间生成脚本在配置碰撞时调用同一方法，重复执行会复用已有子物体。`[verified: scene-repair.json data.result.result.sceneSaved=true；ReferenceRoomGameplayBuilder.cs L57、L179–180]`

## 验证范围

- Unity Play Mode：4/4 通过，覆盖正面及柜角 13 条路径、靠门侧 13 条路径、3 个旧卡住坐标的脱离，以及柜上家信的交互射线。测试加载正式场景，使用实际 `CharacterController` 和玩家的移动速度/重力规则。`[verified: UnityProject/Assets/MiniGame/Tests/PlayMode/CabinetCollisionPlayModeTests.cs LoadProductionRoom、Walk；Artifacts/CabinetCollision/tests-final.json summary]`
- 正式场景相机渲染记录中，靠门侧接触位置为 `(-6.2942,-0.7029,0.1828)`，后退后为 `(-7.3797,-0.7029,0.1828)`，位移约 1.0855 m，地面高度保持。截图为 [接触柜子](../../Artifacts/CabinetCollision/cabinet-contact.png) 与 [退离柜子](../../Artifacts/CabinetCollision/cabinet-retreated.png)。`[verified: Artifacts/CabinetCollision/capture-report.json data.result.result]`
- 上述路线由程序驱动真实角色控制器，并非人工键盘验收；本轮 Windows Player 仅完成启动与日志检查，未进行完整人工游玩，也未构建 macOS 包。`[verified: CabinetCollisionPlayModeTests.cs Walk；launch-smoke.json scope]`

## 修复包

- [Windows x64 Mono 修复包](../../Builds/Deliverables/NORM-Windows-x64-Mono-CabinetFix-20261023-192530.zip)，非 Development 构建，346,198,772 字节。`[verified: 构建前 Editor 状态 development=false；build.cs options=DetailedBuildReport；package-verification.json zipBytes]`
- 构建成功，0 个错误；保留一条 Pipeline 工具提示：场景未配置 `RuntimePipelineManager`，Player 中关闭该工具的运行时连接。`[verified: Artifacts/Packages/NORM-Windows-x64-Mono-CabinetFix-20261023-192530/build-summary.json result、totalErrors、warnings]`
- 启动 8 秒后进程仍响应，日志错误模式检查为空；ZIP 中 159 个文件全部回读并与构建目录逐文件核对长度和 SHA-256。`[verified: launch-smoke.json passed=true；package-verification.json verifiedFiles=159、passed=true]`
- ZIP SHA-256：`5384e77d41cb86e878f040dd44261c595b06ab0d51e3b4ef3de5c0c00ae6ee47`。完整构建、启动及包校验记录见 [delivery.json](../../Artifacts/Packages/NORM-Windows-x64-Mono-CabinetFix-20261023-192530/delivery.json)。`[verified: package-verification.json zipSha256]`
