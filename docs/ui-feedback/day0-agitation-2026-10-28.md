# Day0 教学角色开场最狂躁波形（2026-10-28）

## 当前修改

- Day0 无名男子使用单独的初始信任 **-2**；正式五位患者继续使用默认0。转换器支持 `patients[*].initialTrust`，包的教学 `profile.initialTrust` 与 Campaign 患者初值保持一致，来源审计记录更新后的配置哈希。[verified: Tools/DialogueCampaign/conversion-config.json patients[id=tutorial].initialTrust；Tools/DialogueCampaign/build_campaign.py rules/tutorial_trust；UnityProject/Assets/MiniGame/Content/Campaign/campaign.minigame.json profile.initialTrust/campaign.patients；provenance.json configHash]
- 当前教学参考门槛为3；信任-2得到相对读数-5/3，波形坐标 `clamp(0.65 + (-5/3) × 0.45, 0, 1) = 0`，达到最狂躁档位：幅度2.45、频率5.4、扰动0.945、噪声速度64、躁动1。-1对应坐标0.05，继续低于-2不会增强波形；这是显示映射的最低档，不是给核心信任新增数值下限。[verified: src/MiniGame.Core/Contracts/GameResult.cs SocialAdaptationReading；src/MiniGame.Presentation/SocialAdaptationSignal.cs CoordinateFor/Parameters]
- 原实现只对含释放处置的会谈提供社会适应度读数，Day0只有净化，所以仅降低初值无法显示波形。核心现在也为Day0提供权威信任读数；教学仍只允许净化，净化配额和处置副作用保持原有规则。[verified: src/MiniGame.Core/Runtime/GameSession.cs GetPlayerView；src/MiniGame.Presentation.Tests/CampaignPackageTests.cs TutorialStartsAtMaximumAgitationAndRetainsItsPurifyOnlyFlowAcrossRestore]

## 内容与活动绑定

执行转换器 smoke、生成、`--check`，监督员与结局 `--check`，以及对白工具全部测试。使用现有隔离工程执行 `EndingContentImport.Run` 导入，新包目录为 `8c631cfad43eb406f04cd5275564d8ea`。[verified: Tools/DialogueCampaign/build_campaign.py；Artifacts/Day0Agitation20261028/import-editor.log MINIGAME_DIALOGUE_PACKAGE_IMPORTED/MINIGAME_ENDINGS_IMPORTED]

另行核对源工程实际活动资产：`ActiveCampaignPackage.asset` 的 PackageSource GUID 为 `4616077b0354a1745983cdc87ff8c1ae`，对应同目录的源工程导入包；其 JSON 与生成包一致，SourceHash 为 `ef11bfdb41409b44ef0d60e64841a04302286ddac2a46ce69efa1dd83bbc0025`。源工程打开的 Editor 日志也记录了该包的导入；隔离工程自己的资产 GUID 不覆盖源工程 GUID。[verified: UnityProject/Assets/Resources/MiniGame/ActiveCampaignPackage.asset packageSource/sourceHash；UnityProject/Assets/MiniGame/ImportedDialogue/inspection-campaign/8c631cfad43eb406f04cd5275564d8ea/Package.minigame.json.meta；Artifacts/DesktopLauncher/editor.log MINIGAME_DIALOGUE_PACKAGE_IMPORTED；src/MiniGame.Presentation.Tests/CampaignPackageTests.cs ActiveCampaignBindsTheValidatedRebalancedPackageAndRuntimeYarn]

## 本次验证

- Python 对白工具测试 **33/33通过**；生成包、监督员与结局来源检查通过。[verified: 2026-10-28执行 python -m unittest discover -s Tools/DialogueCampaign -p test_*.py；Tools/DialogueCampaign/test_campaign.py test_tutorial_initial_trust_override_keeps_formal_patients_at_default]
- 固定 SDK **8.0.424**，按顺序锁定 restore、Release build、Release test；构建0警告0错误，核心 **154/154**、宿主／内容 **320/320**通过，总计 **474**。覆盖教学初始读数、开场与处置后存档恢复、仅净化许可、配额，以及既有结局路线。[verified: Artifacts/Day0Agitation20261028/dotnet-restore.log；dotnet-build.log；dotnet-test-final.log；day0-final_net8.0_20261028112220.trx；day0-final_net8.0_20261028112227.trx]
- Unity **3/3 PlayMode通过**：新增Day0真实活动包开场、即时最大躁动与读档验证；既有回答、暂停、选择命中和神父释放回应波形检查继续通过。[verified: Artifacts/Day0Agitation20261028/playmode.xml test-run result=Passed/total=3/passed=3/failed=0]
- 已查看实际 Unity UI 渲染纹理中的Day0开场波形；这属于测试会谈截图，不替代完整 Campaign 游玩与人工视觉验收。[verified: Artifacts/Day0Agitation20261028/Visuals/day0-maximum-agitation.png]

Unity验证沿用已有 `Artifacts/InteractionVisualValidation/UnityProject`，Editor实际为2022.3.23f1，与该隔离工程一致；未改源工程的2022.3.23f1c1版本标记，未重建主场景或生成新Player。[verified: Artifacts/Day0Agitation20261028/playmode-editor.log Initialize engine version；UnityProject/ProjectSettings/ProjectVersion.txt；git diff]

本次日志、TRX、XML与预览保存在本地忽略目录 [Artifacts/Day0Agitation20261028](../../Artifacts/Day0Agitation20261028/)。新初值用于新开局，已有活动会话需重新开始Day0加载新规则。[verified: src/MiniGame.Core/Runtime/GameSession.cs constructor；src/MiniGame.Presentation/GameRuntime.cs constructor；Artifacts/Day0Agitation20261028]
