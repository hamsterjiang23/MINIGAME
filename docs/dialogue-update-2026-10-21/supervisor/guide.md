# 监督员对白与立绘

原表46句、6种立绘已接入独立的监督员 Yarn 项目；原始来源和导入时SHA256见 [source-manifest.json](source-manifest.json)。该清单是历史导入快照，当前Unity纹理已与原始PNG哈希不同；本轮不重写纹理。`[verified: source-manifest.json sourceRows、images；../../../Artifacts/SupervisorStability20261023/asset-hashes.json identical、matchesManifest；本轮纹理路径git diff为空]`

- Day1、Day2、Day3分别16、12、11句；每天最后两句接现有 Briefing，其余接 MorningInspection。Day4的7句接 TakenAway 结局。`[verified: Resources/MiniGame/Supervisor/Dialogue.json conversations；source-manifest.json stagePolicy]`
- 表格L20缺失立绘，继承上一句JD_03；J40保留原始跨日目标，但当天停止播放，结局从Day4自己的第一句“惊——喜。”开始。`[verified: source-manifest.json adaptations；Dialogue.json conversations[3].lines[0]]`
- 立绘按每句ID切换；沿用门外世界坐标，暂停存档保存当前监督员句子，结局读档恢复镜头位置和FOV。`[verified: reports/supervisor-playmode.json results；reports/doorway-regression.json results]`
- 门口对白等待相机和开门避让回位完成后才显示；正式来访结局使用同一就位条件，旧式直接晨检存档不等待不存在的门口镜头。`[verified: SupervisorDoorwayView.IsReadyForDialogue；MiniGameYarnBridge.PresentLineAsync；EndingDialoguePresenter.Update]`
- 六张立绘按可见脚底定位，统一使用JD01的像素比例；切表情与重新绑定不再沿用不同画布的中心、底边或缩放。定位采用alpha大于16的主体底部10%横向重心及主体最低点，保持完整纹理。`[verified: SupervisorDoorwayView.PortraitPivot、SetPortrait、Bind]`

重新导入：使用Python运行 `Tools/DialogueCampaign/build_supervisor.py`，再让Unity完成资源导入。原始工作簿保存在 `Art/chat_excel/【对话】监督员.xlsx`，RAR和六张图保存在 `Art/Characters/Supervisor/`。`[verified: Tools/DialogueCampaign/build_supervisor.py main]`

原生截图：[拿起仪器](images/supervisor-instrument.png)、[绿灯晨检](images/supervisor-green-check.png)、[红灯结局](images/supervisor-red-ending.png)。`[verified: images 下三个PNG]`

2026-10-23站位修复与验证见[验收记录](stability-verification.md)。`[verified: stability-verification.md「自动验证」「画面检查」]`
