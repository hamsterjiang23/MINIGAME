# 海浪动态修订

用户要求：海滩不再显得生硬，海浪有清楚可见的运动，并参考公开 Shader 实现。目标是在玩家走向床的视角看到浪峰向岸推进、白沫破碎、海水退回及湿沙变化。`[verified: 本轮用户反馈]` `[opinion]`

## 参考与取舍

- [Catlike Coding — Waves](https://catlikecoding.com/unity/tutorials/flow/waves/)：采用 Gerstner 的水平／垂直位移和多波方向叠加思路；原始网格位置用于计算每个波的相位，避免串联变形。代码及资产许可为 [MIT-0](https://catlikecoding.com/unity/tutorials/license/)。`[verified: Waves Gerstner Waves / Multiple Waves；License Code and Assets]`
- [Roystan — Toon Water](https://roystan.net/articles/toon-water/) 及 [实际源码](https://raw.githubusercontent.com/IronWarrior/ToonWaterShader/master/Assets/Shaders/ToonWater.shader)：参考浅水控制泡沫、噪声滚动与扭曲、软化边缘的组合。项目为 [Unlicense](https://raw.githubusercontent.com/IronWarrior/ToonWaterShader/master/LICENSE)。本工程针对固定海滩使用解析沙坡高度，与沙地材质共享岸线函数，而非照搬屏幕深度与法线相机。`[verified: ToonWater.shader L114–150；DreamOcean.cginc]`

## 表现与控制

- 远处：多方向低频涌浪；近处：可辨认的浪峰、破碎白沫与岸边往复冲刷；退水后湿沙渐变。保持现有天空、床和结局流程。`[opinion]`
- 中文配置入口：**MiniGame → 梦境与结局 → 海滩终局 → 海浪与岸边**。波高、周期、波长、冲刷距离、泡沫强度均由梦境配置资产驱动。`[verified: DreamSettingsWindow.cs DrawEnding；DreamSequenceProfile.cs SeaWaveHeight–SeaFoamAmount]`
- 运行时海面和湿沙使用材质副本，共用存档中的 `WorldTime`；暂停冻结时钟，参数仍可预览，销毁时恢复原材质。`[verified: DreamEndingController.cs CreateWaterMaterials/ApplyWaterParameters/RestoreWaterMaterials]`

## 验证范围

修订前画面保留于 `Artifacts/Dreams/beach-walking-fixed.png` 及 [海滩流程录制](../../Art/Dreams/Previews/beach-ending.mp4)。本轮另存了 [12 秒固定相机海浪预览](../../Art/Dreams/Previews/beach-waves-preview.mp4)，可以直接观察冲岸和退回；它是无声的固定步长生产相机录制，不是帧率测试。`[verified: beach-waves-preview.mp4.video-metadata.json capture/audio]`

海滩 6 项 PlayMode、真实跨场景读档 1 项、配置面板 3 项均通过。GPU 海水区域在固定相机下随时间变化，暂停与恢复后误差分别小于百万分之一、十万分之一；Windows 构建成功，两份水／沙 Shader 已打入 Player。详细记录见 [water-verification.json](water-verification.json)。当前为可播放的程序化风格候选，最终美术认可仍由用户判断。`[verified: water-verification.json tests/gpu/build；Unity CLI ShaderHasError=False、packed=True]` `[opinion]`

仓库附带 [海浪循环 GIF](../../Art/Dreams/Previews/beach-waves-preview.gif) 与 [原报告副本索引](evidence/index.json)，预览文件校验值见 [预览清单](../../Art/Dreams/Previews/manifest.json)。无声预览使用固定步长录制；原报告中的 Artifacts 路径保留为历史来源。`[verified: Previews/manifest.json files；evidence/index.json files；beach-waves-preview.gif.video-metadata.json capture/audio]`
