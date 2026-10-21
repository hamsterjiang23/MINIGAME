# Deviations

- 原表未指定宿主演出阶段：沿用已有晨检/工作简报，最后两句分到简报；Day4接已有TakenAway结局，不新增可玩日。若策划调整节奏，修改导入器stagePolicy及对应测试。`[verified: source-manifest.json stagePolicy]`
- L20继承上一句JD_03；J40跨日目标保留为来源记录，演出在日期边界停止，Day4从自身根节点开始。`[verified: source-manifest.json adaptations]`

# Discovered edge cases

- 立绘ID表A2是Excel字面量数组，使用现有静态解析恢复JD_01，不执行公式。`[verified: source-manifest.json adaptations kind=literal-array-recovered]`
- 病人与监督员原始节点ID重叠：监督员Yarn元数据加supervisor前缀，并使用独立Yarn项目。`[verified: Tools/DialogueCampaign/build_supervisor.py yarn生成逻辑]`
- 审查发现结局相机FOV可能被后绑定的投影覆盖；已将投影绑定放在监督员绑定之前，并增加结局存档镜头测试。`[verified: MiniGameBootstrap.cs InstallRuntime；reports/supervisor-playmode.json TakenAwayCheckpointKeepsCameraPoseAndZoom]`
- 初版测试夹具没有实际每日工作，无法合法睡觉；改为每天一名病人的完整命令流程，生产规则未为测试放宽。`[verified: SupervisorDialoguePlayModeTests.cs AdvanceNight]`

# Questions for review

无阻塞项；对白分配到晨检/简报的节奏可根据实际游玩继续调整。`[opinion]`

偏差：2项（阶段映射、缺失/跨日单元格）。`[verified: 上方Deviations]`
最可能调整：晨检与简报的句数分配。`[opinion]`
边界情况：Excel数组、ID重叠、结局FOV、测试夹具。`[verified: 上方Discovered edge cases]`
后续先读：source-manifest.json与guide.md。`[opinion]`
验证入口：verification.json。`[opinion]`
