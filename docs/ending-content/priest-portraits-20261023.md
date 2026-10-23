# 神父结局立绘替换与位置稳定性

用户提供的 `01a16472-c67d-70e3-a90d-7defdb995935.rar` 内两张原图按现有结局 ID 接入；PNG 字节不变，原资源 GUID 保留。来源哈希与对齐参数见 [generation.json](../../UnityProject/Assets/MiniGame/Art/Characters/EndingPortraits/generation.json)。`[verified: Artifacts/PriestPortraitReplacement/editmode.json data.result.Summary；generation.json images]`

| 压缩包 ID | 游戏资源 ID | Sprite 脚底锚点，像素 |
|---|---|---|
| ED_01 | PE01 | (514.08828, 33)，坐标从图片左下角起。`[verified: generation.json images[0]]` |
| ED_02 | PE02 | (310.96351, 16)，坐标从图片左下角起。`[verified: generation.json images[1]]` |

锚点取 alpha > 16 的可见区域底部 10% 像素横向重心及可见底边。两图统一使用 PE01 全画布高度计算缩放，导入脚本固定 FullRect、100 PPU 及各自锚点，重新导入也保留对齐。切图不更改人物 Transform 或相机；原图的倾身、歪头姿势仍然保留，并非把脸部位置强行重合。`[verified: EndingContentImport.cs ImportPortraits；SupervisorDoorwayView.cs SetPortrait；PriestEndingPortraitAssetsTests；正式 Combined 截图]`

两张神父立绘共同沿门口视角左移 0.4 米，偏移由固定基准位置计算，不逐帧累加；监督员使用原位置。`[verified: SupervisorDoorwayView.cs ApplyDoorOffset]`

## 验证

| 检查 | 结果 |
|---|---|
| EditMode：PriestEndingPortraitAssetsTests | 3/3；原图 SHA-256、独立 alpha 测量脚底锚点、重导入与共同像素比例通过。`[verified: ../../Artifacts/PriestPortraitReplacement/editmode.json data.result.Summary/Results]` |
| PlayMode：SupervisorDoorwayPlayModeTests | 5/5；包括两张神父图连续 30 帧切换的脚底世界锚点、Transform、缩放和相机稳定性，以及原监督员门口回归。`[verified: ../../Artifacts/PriestPortraitReplacement/stability-result.json data.result summary/results]` |
| PlayMode：ReleasedDayTwoPriestUsesEightEndingLinesAndFinalBlackScreen | 1/1；正式 Combined 场景完整八句神父结局、黑屏和返回主菜单通过。`[verified: ../../Artifacts/PriestPortraitReplacement/route-result.json data.result summary/results]` |

正式场景截图：[PE01](../../Artifacts/PriestPortraitReplacement/LeftShift04Captures/20261023T1210028659793Z-efde0fa6/priest-PE01-frame000068.png)、[PE02](../../Artifacts/PriestPortraitReplacement/LeftShift04Captures/20261023T1210028659793Z-efde0fa6/priest-PE02-frame000087.png)。两张使用同一门口构图；实际姿势变化可见。截图与测试输出保存在本地 Artifacts 目录，不纳入 Git。`[verified: 对应截图；git check-ignore Artifacts/PriestPortraitReplacement/editmode.json]`

本次未重新构建 Windows Player，以上结果属于 Unity Editor 与 PlayMode 验收。`[verified: 本次验证命令为 run_tests editmode/playmode]`
