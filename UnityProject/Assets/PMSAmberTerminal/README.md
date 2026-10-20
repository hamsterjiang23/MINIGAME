# PMS 琥珀终端资源包 · v0.8.12

本仓库接入版移除了 F2 设置与 F3 查阅模式。正式场景使用 MiniGame 宿主适配器及动态人物监控，独立演示仍使用本包的演示驱动。

对应确认的 HTML v0.8.12：800×600 琥珀字符界面、暖灰病房监控、16:9 监控画面、编号开机自检、仅开机后播放的分区加载、柔光关机。

## 导入与预览

1. 解压 ZIP，将其中 `Assets/PMSAmberTerminal` 整个目录复制到 Unity 工程的 `Assets` 下。一起复制 `.meta` 文件以保留预制体、字体和图片引用。不要把 ZIP 本身拖入 Project 窗口。
2. 使用 Unity 2022.3 LTS，等待脚本和字体导入完成。项目应有 **Unity UI / com.unity.ugui 1.0.0**。无需 WebView、浏览器插件、Yarn 或 TextMesh Pro。
3. 打开 `Assets/PMSAmberTerminal/Scenes/PMSAmberDemo.unity`，按 Play。也可以从菜单 **PMS Amber Terminal → Open demo scene** 打开。
4. 看完编号自检后，目录会按区块加载。上下键选择、Enter 进入；对话中 Enter 继续，数字键选择应答。名册也支持双击进入。
5. 演示应答后可点击处置按钮，预览关机。熄屏后点击画面或按 Enter 重新开机。

此演示只展示 UI 交互、外观和开关机，不代表正式角色对白或处置规则。完整 HTML 对照版在 `Reference/PMS_v0.8.12.html`。

## 加入自己的场景

将 `Prefabs/PMSAmberTerminal.prefab` 拖入场景，按 Play。这个预制体只有显示组件，不自动执行任何处置。

也可以使用 **PMS Amber Terminal → Create terminal in current scene**。界面在运行时生成命名明确的 uGUI 层级，包含 `Logical 800x600`、目录分区、监控、对话、处置、应答及命令栏。

默认输出一个保持 4:3 的全屏显示层，黑边由宿主场景背景决定。`CrtMode`：0 关闭后处理、1 标准 CRT、2 老化 CRT。`Ward` 可替换为其他 16:9 图片。`QuotaDisplay` 仅是宿主提供的显示文字，不保存或计算净化配额。

内置源摄像机使用 Layer 27，只渲染包内 UI。若已有对象使用该层，可在预制体上更改 `SourceLayer`，无需更改项目的层名称。`ShowOverlay` 控制默认全屏输出；`ReadKeyboardAndMouse` 控制包自带输入。正式游戏可关闭自带输入，统一交给宿主处理。

## 与当前项目连接

当前项目的核心仍应由 `GameSession` 裁决；本包只显示宿主投影并发出请求，不保存第二份信任、次数、配额、人物去留或跨日状态。现有 `TerminalView` 不会被此包覆盖。

组件接口：

| 接口 | 用途 |
| --- | --- |
| `SetRoster(PMSRosterEntry[])` | 显示最多五行角色名册；数据由宿主提供 |
| `ShowDirectory()` | 立即切换到名册，不播放加载 |
| `ShowReview(PMSReviewData)` / `SetReview(...)` | 显示或更新审查投影，不播放加载 |
| `EnterRequested(string id)` | 请求宿主进入角色会谈 |
| `ContinueRequested` | 请求宿主推进或补全当前对白 |
| `ReplyRequested(string id)` | 请求宿主提交应答 |
| `DispositionRequested(string type)` | 请求宿主提交处置，type 为“释放”“治疗”“净化” |
| `NotifyAllCharactersCompletedAfterDialogue()` | 宿主确认所有当日角色完成处置且最后反应对白已结束后，触发关机 |
| `ShutdownCompleted` | 熄屏完成；宿主可据此回到房间或推进演出 |
| `Boot()` | 重新播放开机及分区加载，不重置宿主游戏数据 |
| `HandleKey(string)` | 接收统一输入，支持 Enter / Space / Up / Down / 1–4 |
| `PointerUV(uv, down, up)` | 从 3D 屏幕射线传入 UV；UV 原点在左下，内部校正 CRT 曲面坐标 |

`PMSReviewData` 的 `Lines`、`Replies`、`CanContinue`、`CanRelease`、`CanTreat`、`CanPurify` 都应从当前游戏投影生成。处置是否可执行以及提交后的反应推进由宿主负责。不要在按钮回调中同时执行处置和关机；应等宿主确认最后反应完成。

`PMSPreviewDriver` 仅挂在独立演示场景。接正式玩法时只使用 `PMSTerminalView`，不要挂演示驱动。

如需接入 3D 电脑屏幕，关闭默认 `ShowOverlay` 与输入，使用 `ScreenTexture` 接到宿主的屏幕材质；若需要同样的 CRT 后处理，可用 `ScreenMaterial` 将 `ScreenTexture` 后处理到宿主自己的 RenderTexture。3D 交互射线再调用 `PointerUV`。该部分需接入现有场景，不会因解压资源包自动替换游戏里的电脑。

## 包内目录

- `Assets/PMSAmberTerminal/Art`：原病房照片、暖灰处理后的 464×261 图片、透明荧光线与光点、可复用 UI 图片、CRT 材质。
- `Fonts`：完整中文字体 Noto Sans Mono CJK SC 和 SIL OFL 许可。使用可随资源包分发的字体替代 HTML 调用的系统字体；字体轮廓与原 Windows 宋体会有差异。
- `Runtime`：原生 uGUI 呈现、固定字格文本、显示数据及独立演示驱动。
- `Shaders`：扫描线、曲面、暗角、轻微扩散的 CRT 着色器。
- `Prefabs` / `Scenes`：可拖入场景的终端预制体、独立演示场景。
- `Documentation`：色板、坐标、来源和验证记录。
- `Preview`：标明来源的 HTML 对照图与 Unity 实际捕获图。
- `Reference`：自包含 HTML 与对白参考数据，不参与 Unity 运行，也不覆盖正式对话数据。

本包保持确认的布局与色调；它是可导入的显示层资源与接入组件，不是整个 Unity 游戏工程。导入与运行验证结果以 `验证记录.json` 为准。未验证的目标平台或渲染管线不能据此视为已通过。
