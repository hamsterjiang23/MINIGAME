# 监控人物重复与落点修正

同一人物前后重复的根因是多个监控Feed使用同一世界位置、同一渲染层，镜头会拍到其他Feed的立绘。修复前GPU测试中，新增第二路监控使首路画面改变43,379个像素。修复后同时显示第二路不会改变首路像素。`[verified: Artifacts/MonitorComposition/before.xml；final.xml AnotherFeedCannotContributePixelsToAnExistingMonitor]`

每路舞台沿Z轴独立分配位置，分配时检查现存舞台，包含禁用对象；相机远裁剪面保持原值。原病房图片、人物图片、表情目录及监控颜色／扰动参数均保留。`[verified: PatientSurveillanceFeed.cs Build/ReleaseStage；本轮生产差异]`

实时监控可用时关闭备用的前／当前立绘Image；关闭监控或不可用时恢复单张立绘。配置变化无需等待下一条对白即可切换；重新绑定时清理旧目标与舞台。`[verified: TerminalView.cs UpdatePortrait与VisibilityChanged订阅；TerminalPortraitPresenter.cs Paint；final.xml RealTerminalUsesOnePortraitSourceAndRestoresFallbackWhenMonitoringIsDisabled]`

## 人物构图

沿用已有VV04床边落点(.625,.15)、大小倍率.74。医生ED01–04使用(.625,.26)、.74；牧师JN01–05使用(.625,.28)、.70。小孩、执政者与外交官的半身立绘底边放到画外(-.035)，避免裁切边缘悬在地面上。这些为构图选择，可继续调整。`[verified: PatientMonitorDefault.asset PortraitPlacements]` `[opinion]`

在Unity的 **MiniGame → 人物监控室** 中调整“独立立绘落点”；对应配置为 `Assets/MiniGame/Resources/MiniGame/PatientMonitorDefault.asset`。`[verified: PatientMonitorWindow.cs MenuItem与PortraitPlacements字段]`

- [戴眼镜人物的终端画面](../../Artifacts/MonitorComposition/Final/monitor-vv04-terminal.png)。`[verified: 本轮实际GPU截图]`
- [小孩](../../Artifacts/MonitorComposition/Final/monitor-patient-child-ML01.png)、[医生](../../Artifacts/MonitorComposition/Final/monitor-patient-battlefield-doctor-ED01.png)、[外交官](../../Artifacts/MonitorComposition/Final/monitor-patient-diplomat-AD01.png)、[牧师](../../Artifacts/MonitorComposition/Final/monitor-patient-priest-JN01.png)。`[verified: 本轮实际GPU截图]`

本轮15项PlayMode检查覆盖监控隔离、五人真实立绘像素贡献、暂停、配置变化、立绘回退、销毁释放和终端交互；检查在隔离工程运行，未关闭或重启主编辑器。视觉截图已自查，最终美术接受以用户反馈为准。`[verified: Artifacts/MonitorComposition/final.xml；本轮进程启动命令]` `[opinion]`

## Windows Mono

[修复包](../../Builds/Deliverables/NORM-Windows-x64-Mono-MonitorComposition-20261022-164207.zip)，解压后运行MiniGame.exe。构建0错误／0警告，独立Player终端检查通过，ZIP中158个文件逐一核对SHA256。完整记录见[verification.json](verification.json)。`[verified: Artifacts/MonitorComposition/windows-build.json；Player/verification.json；package.json verifiedZipFileEntries/validation]`
