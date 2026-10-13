# 尚未解决的问题与后续入口

## 原工程和运行验证

匹配本构建的Unity6000.3.10f1 Editor未在已检查位置找到，因此Editor导入、组件绑定、脚本编译、材质渲染、输入和可玩性均未验证。net40重编译83错误与net48缺少reference assemblies已经有真实日志；不能把语法解析零错误改写为编译成功。[verified: dependencies.md；unity-source-compatibility.md；Analysis/Logs/decompiled-project-build.log；decompiled-project-net48-diagnostics.txt]

入口是恢复工程的ProjectSettings/ProjectVersion.txt、Packages/manifest.json与正确版本Editor；先处理编译及项目引用，再逐场景验证。此后工作须继续使用恢复副本，不改原Steam安装。[opinion]

## 序列化角色/灯光/玩家配置

首次IL2CPP路线有73个CharacterSOData、49个UniversalAdditionalLightData及1个PlayerSettings读取长度/布局错误。备份Managed覆盖不能独立解决这些错误，具体后续读取结果与对象清单见资源报告。未验证字段不能填成零、空数组或猜测身份日程。[verified: unity-recovery.md与AssetRipper原始日志][opinion]

入口是原始Data资产、副本Managed DLL真实字段、Unity版本对应序列化布局、失败对象pathID以及已有替代读取smoke。正确Editor导入是后续检查数据和组件兼容性的一个入口，不能保证自动修复读取失败对象。[opinion]

UnityPy替代探针实际取得Alkonost原始1388字节（sharedassets0.assets PathID2615、文件偏移85736720），独立SHA校验一致；真实类型树遇到SerializeReference引用类型缺失，精确类型回退后仍解析失败，没有完整字段/精确字节消费证明。原始对象及异常均已保存，73个角色配置仍未恢复。[verified: Analysis/Assets/unitypy-status.json source/raw/error；Alkonost.raw.sha256-verification.json matches=true]

## Shader和编辑器资源

免费Shader导出使用占位策略，不能声称恢复原ShaderGraph/HLSL或所有材质效果。GUID检查只确认目标文件存在，不等于local fileID、组件布局或导入正确。两个未解析GUID的源为Resources/dialogs/Project Import Data.asset和Resources/FolderColorSettings.asset；其名称暗示编辑器用途，但运行时影响仍需真实类型/使用路径核对。[verified: Analysis/Assets/recovery-summary.json limits；unresolved-references.json][opinion]

入口为Shader/Material导出文件、引用JSONL及原始二进制；不要以通用shader替代后宣称与原效果相同。[opinion]

## 原生机器码和泛型

已恢复4999个非零静态指针条目，未恢复Ghidra反汇编/CFG、完整函数边界、内联和泛型实例表。856个NULL slot中存在有CIL的泛型方法，不代表实现缺失；3416个独立地址的重复指针成因未作机器码确认。[verified: native-map-verification.json limitations；cil-cpp-map-summary.json limits；code-recovery.md]

入口为native-method-map.csv的RVA、对应CIL/C++和REA支持的Ghidra12.1.x/JDK依赖。运行时ASLR基址、真实接收者和行为等价，需要额外证据而不是此静态表的计数。[opinion]

## 剧情、概率、动态调用和存档

编译Yarn已恢复3255节点、276变量及56585指令；仍有1868条依赖栈/选项的JUMP记录未静态确定，1个常量目标未在当前程序中找到：Wheelchair_Entrance_AcceptBribe的instruction12指向Wheelchair_Entrance_Occupied。这是当前编译程序范围内的未解析目标，不是已经观察到的运行故障。原作者Yarn源文件/注释和所有运行时可达路径不能由字节码索引完全证明。[verified: Analysis/Gameplay/summary.json；yarn-unresolved-control.json；yarn-disassembly.txt L50120][opinion]

静态调用图没有解析全部Unity序列化回调、反射和运行时虚分派；未验证全局随机种子或实际长期概率分布，也未读写用户存档或观测云同步。[verified: Analysis/Managed/Assembly-CSharp/coverage.json limitations；npc-identity-checks.md「后续入口与未知」]

入口为Yarn节点/控制边、本地化line ID、GameEventConditions、各服务SaveData以及成功恢复的实际SO配置。若以后需要运行时观察，先隔离存档/云写入影响并继续使用合法Steam环境，不能采用DRM/反作弊绕过。[opinion]

Verification: 每个剩余项均对应现有失败日志、覆盖声明或未执行检查，未生成猜测游戏实现。[verified: 本报告引用的日志及coverage/verification字段]
