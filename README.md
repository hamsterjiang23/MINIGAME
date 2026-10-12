# MINIGAME

第一人称、2D/3D混合的心理恐怖叙事游戏资料库。玩家扮演检定员，在员工公寓中通过电脑与病人交流，并在治疗、净化和释放之间作出选择。当前Mini Game内容目标为 **3天、5位病人、3种结局**，研究技术基线为 **Unity 2022.3 LTS**。

当前仓库包含设计来源、技术研究、参考图片、第三方参考工程和纯C#核心逻辑。核心代码位于`src`；尚未建立本游戏的Unity工程或验证可玩的游戏应用。

## 阅读入口

- [核心逻辑源码](src/README.md)：五人三天的规则、状态、事件、结局和存档。
- [实现说明](src/architecture-and-rationale.md)：逐文件职责、设计理由、整体结构和借鉴来源。
- [游戏总览](docs/game-overview.md)：游戏定位、玩法、角色、日循环与交付范围。
- [设计文档入口](docs/README.md)：逐篇飞书资料、来源索引与图片。
- [源码借鉴研究](research/README.md)：15个参考仓库的实际能力与改造方式。
- [需求借鉴矩阵](research/reuse-matrix.md)：25项需求的来源与自研部分。
- [Unity 2022路线图](research/unity-2022-roadmap.md)：验证切片与后续开发顺序。
- [设计待定项](docs/open-questions.md)及[研究风险](research/risks-and-gaps.md)：规则、版本、许可与验证状态。

## 目录

| 目录 | 内容 |
| --- | --- |
| `src/` | 纯C#核心逻辑、自动化测试与实现说明；Unity／Yarn接入待验证 |
| `docs/` | 中文游戏设计整理、来源记录、参考图片和原始读取数据 |
| `research/` | 19份源码借鉴文档：15份逐库研究与4份综合文档 |
| `Dependency/` | 原有Markdown、Word、PDF研究报告与CSV索引／任务表 |
| `references/` | 15个第三方参考仓库，以固定提交的Git子模块保存 |

## 获取全部资料与参考工程

先安装Git和Git LFS，再递归克隆：

```bash
git lfs install
git clone --recurse-submodules https://github.com/hamsterjiang23/MINIGAME.git
cd MINIGAME
```

如果已经普通克隆过主仓库：

```bash
git submodule update --init --recursive
```

若此前跳过了LFS下载，补取各参考工程当前版本的大文件：

```bash
git submodule foreach --recursive 'git lfs pull'
```

主仓库直接保存文档、图片、原始设计读取数据和研究材料。参考仓库通过`.gitmodules`及gitlink关联各自上游，并固定到[版本记录](docs/source-lock.md)和[source-lock.json](docs/source-lock.json)中的提交；其完整历史和LFS资源从相应上游获取。不要使用`git submodule update --remote`自动切换研究版本。

参考工程合计约4.4GB，当前版本包含355个LFS文件。获取完成不代表各工程已通过编译或运行；运行状态与许可边界在对应研究文档中单独记录。代码、第三方依赖和资产遵循各自许可，主仓库不重新授予参考工程或素材的权利。
