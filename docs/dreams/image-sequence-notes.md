# 每日梦境图片序列与后处理

本轮采用 McQueenMa777 的 `87d9c3ec`（2026-10-21）中 `Art/Dream/Dream1–3` 的 30 张图。用户确认每晚按编号播放，并授权按照世界观加强屏幕后处理。没有数字后缀的 Day1 图作为第 1 张，其余按末尾编号 1–10 排序。[verified: 远端提交；DreamImageSequenceSetup.FrameNumber；source-manifest.json]

## 表现设计

- Day1 鲸滩：红色海水与鲸影保持原画构图，加入低频海面折射、缓慢推进和边缘色差。[opinion]
- Day2 倒悬花海：采用最新素材替换旧长路画面，以缓慢倾斜、镜头呼吸和偏移重影制造重力失常感。[opinion]
- Day3 红黑审视：逐步增强 CRT 扫描线、RGB 分离和局部水平撕裂，在段落后半段加入短暂压暗与重影。[opinion]

每晚保留 20 秒与原有音乐，10 张图均分时长；叠化默认依次为 0.65、0.85、0.4 秒。最后一张保持到结束，不回跳第一张。素材本身已经包含浪、海鸥、花田、拳影与 CRT 痕迹，序列模式不再绘制旧版的额外飞鸟、公路线条或链条。[verified: DreamImageSequenceSetup.Import；DreamAnimationStage.Render；DreamProjectionAnimation.shader 的 memorySequence]

## 调节与预览

**MiniGame → 梦境与结局 → 每日梦境** 可调整顺序画面、时长、叠化、运动强度和梦境后处理强度。后处理强度设为 0 可关闭新增后处理；时间仍正常推进图片序列。预览、暂停和读档共用显式播放时间，所以恢复后画面与转场一致。[verified: DreamSettingsWindow.DrawDailyDreams；DreamAnimationStage.Render；DreamSequenceDirector.RenderStage]

导入入口：**MiniGame → 梦境 → 导入 McQueen 每日图片序列**。视频导出入口：**MiniGame → 梦境 → 导出每日梦境视频预览**，需要 PATH 中有 FFmpeg，或设置 `MINIGAME_FFMPEG`。视频为 Unity 固定时间步渲染，配乐和放映机音轨按当前配置离线混合，不代表实时性能或人工听感验收。[verified: DreamImageSequenceSetup.Import；DreamImageSequencePreviewExporter.Export、AddAudio]

## 查阅的资料

- [Unity Chromatic Aberration](https://docs.unity3d.com/Packages/com.unity.postprocessing@3.4/manual/Chromatic-Aberration.html)：RGB 通道分离及强度控制。[verified: 官方文档 Chromatic Aberration、Properties]
- [Unity Lens Distortion](https://docs.unity3d.com/Packages/com.unity.postprocessing@3.4/manual/Lens-Distortion.html)：镜头畸变参数参考。
- [Cyanilux CRT 拆解](https://cyangamedev.wordpress.com/2020/09/10/retro-crt-shader-breakdown/)：UV 弯曲、扫描线、按行偏移与信号噪声的组合思路。[verified: CRT Warping、Scanlines、Distortion、Static]

本工程继续使用现有 Built-in 渲染与墙面投影，在自己的 Shader 中实现上述处理，没有导入 URP 插件或复制示例源码。[verified: DreamProjectionAnimation.shader；Packages/manifest.json]

## 实现记录

- 新增序列字段，单张 Artwork 保留为回退；仅三个普通梦境使用新序列，门口固定海滩结局的配置独立保留。
- 图像按原始尺寸导入，不做额外 NPOT 缩放；原始 PNG 与 Unity 副本保持字节一致。
- 所有动态效果使用 PlaybackSeconds，避免系统时间让暂停和存档恢复出现不同画面。
