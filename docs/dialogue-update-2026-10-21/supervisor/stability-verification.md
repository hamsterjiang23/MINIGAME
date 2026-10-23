# 监督员对白站位修复验收（2026-10-23）

首句出现时，相机已经进入Viewing、门已经停止，监督员仍在一帧内横移0.036293745米；回归在修复前失败。六张现用纹理的画布和透明留白不同，直接使用整图底部中心还会让换表情产生跳位。`[verified: ../../../Artifacts/SupervisorStability20261023/dialogue-baseline.json results[0].Message；SupervisorDoorwayView原GetPortrait使用(.5,0)]`

本轮将晨检和正式来访结局对白延后到门口演员回位结束，保留旧式不经过DoorView的存档；六张立绘以可见脚底注册，使用JD01的固定像素比例，绑定时先恢复JD01再定位。当前PNG文件无Git差异。`[verified: SupervisorDoorwayView.IsReadyForDialogue、PortraitPivot、SetPortrait、Bind；MiniGameYarnBridge.PresentLineAsync；EndingDialoguePresenter.Update；git diff --numstat纹理路径输出为空]`

## 自动验证

| 验证 | 结果 |
|---|---|
| Unity编译 | completed，failed=false，errors为空。`[verified: unity command recompile_status输出]` |
| PlayMode最新结果按测试名汇总 | 22/22通过。`[verified: ../../../Artifacts/SupervisorStability20261023/verification.json total、passed、failed]` |
| 连续点击与表情变化 | 从首句开始逐帧校验演员与相机位置、旋转及FOV；覆盖Day1全部对白。`[verified: supervisor-final-playmode.json DoorwayDialogueClicksAndPortraitChangesKeepActorAndCameraPoseStable]` |
| 六张图与重新绑定 | GPU读取纹理alpha，独立计算脚部注册点；校验像素比例及重新绑定前后的姿态、尺寸。`[verified: supervisor-final-playmode.json AllSixPortraitFeetAndPixelScaleRemainRegisteredAcrossCheckpointRebinds]` |
| 兼容与互动 | 46句完整分页、暂停读档、旧晨检检查点、扫描音效去重、鼠标/空格防重复、结束对白回电脑均通过。`[verified: verification.json results]` |
| 结局 | 正常监督员、配额不足监督员及神父的正式门口结局通过；旧TakenAway读档镜头通过。`[verified: verification.json results]` |

扩展测试中发现的旧预期已同步：46句测试累积实际分页后核对完整原文；监督员卡片按当前白字和柔边框验证；真实场景输入夹具显式启动Campaign并配置结局目录。原始失败报告保留，`verification.json`使用每个测试的最新结果；未重复运行已通过且未受后续测试修改影响的用例。`[verified: SupervisorDialoguePlayModeTests.EveryAuthoredLineAndExpressionPlaysOnceAcrossThreeMorningsAndTakenAway；NormWorldPresentationPlayModeTests.MorningUsesSupervisorWithWhiteTextAndSoftBorderWithoutDeveloperLabelsOrWorldInstructions；WorldDialogueInputPlayModeTests.OpenMorning、RestoreFirstMorning；verification.json results[].report]`

## 画面检查

真实MainRoomCombined场景夹具的[默认表情](../../../Artifacts/SupervisorStability20261023/images/supervisor-stable-JD_01.png)与[笑脸](../../../Artifacts/SupervisorStability20261023/images/supervisor-stable-JD_03.png)已逐张查看，帽徽及肩线保持同一位置；同时保留仪器、绿灯及红灯结局截图。位置稳定另由逐帧断言验证。`[verified: 上述两张截图的view_image输出；supervisor-final-playmode.json对应点击回归]`

编辑器最终停在MainRoomCombined，playing=false、compiling=false、dirty=false。本轮是Unity表现修复，没有重新执行.NET核心测试或目标平台构建。`[verified: 收尾unity command eval输出；本轮执行命令记录]`
