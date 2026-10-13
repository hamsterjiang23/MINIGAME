# 托管代码与 CIL/C++ 关系恢复

## 真正恢复了什么

游戏副本中的备份 Assembly-CSharp.dll 包含可读取的真实 CIL。它具有 943 个 TypeDef、5,855 个 MethodDef、5,291 个 FieldDef；其中 5,126 个方法有真实 IL 方法体，729 个没有 RVA，抽象方法数量也是 729。没有无法读取的 RVA 方法体。[verified: Analysis/Managed/Assembly-CSharp/summary.json type_def_count / method_def_count / field_def_count / methods_with_readable_il_body / methods_with_no_rva / abstract_methods / unreadable_rva_bodies]

文件 SHA-256 为 c674f171c61b644e6e746e0ced079ffa365abee2336d04ea9e67cbbc93e88213，MVID 为 78d03d53-c34f-423e-aad0-70e27d2b7bcc；与 REA 的目标身份结果一致。[verified: Analysis/Managed/Assembly-CSharp/smoke-summary.json sha256 / mvid][verified: Analysis/Evidence/rea-managed-artifact.json]

5,126 个 IL 方法体合计 264,858 字节。其中 49 个只含单字节 ret，标为真实 CIL 的空方法候选；其余 5,077 个标为 real_cil_body。不能仅因一个方法为空就认定整个程序集是反编译生成的空壳。[verified: Analysis/Managed/Assembly-CSharp/verification.json body_classification / validated_il_bytes][opinion]

主调查已导出 DecompiledCode/Assembly-CSharp；当前观察到 587 个 C# 文件及 Assembly-CSharp.csproj。它们是反编译表达，不等于作者原始源码、注释或原始工程组织。[verified: Get-ChildItem 观察输出 source_cs_files=587 / project_files Assembly-CSharp.csproj][opinion]

导出的原样项目编译测试失败，产生 83 个错误、4 个警告；已读例子包括 ExtensionAttribute 和 IReadOnlyList 缺失。尝试 .NET Framework 4.8 时又因本机缺少相应 reference assemblies 失败。这证明当前导出项目尚不能直接重编译，不否定上面的真实 CIL 方法体恢复结果；未修改原样反编译代码来掩盖该限制。[verified: Analysis/Logs/decompiled-project-build.log L3-L13、L91、L180-L181][verified: Analysis/Logs/decompiled-project-net48-diagnostics.txt "error MSB3644"、"1 Error(s)"][opinion]

## 执行方法与完整性

本次另外创建了 Analysis/Tools/ManagedInspector/ManagedInspector.csproj 与 Program.cs。探针仅用 .NET 10 的 System.Reflection.Metadata/PEReader 读取文件；没有反射加载执行目标程序集，也没有启动游戏。[verified: Analysis/Tools/ManagedInspector/Program.cs L51、L83][verified: Analysis/Managed/Assembly-CSharp/coverage.json target_assembly_loaded_via_reflection=false / read_target_execution=false]

单程序集 smoke 首先核对了 REA 元数据计数、SHA-256、MVID与真实 IL 方法体数量；随后才导出完整索引及静态调用边。[verified: Analysis/Managed/Assembly-CSharp/smoke-summary.json][verified: Analysis/Managed/Assembly-CSharp/summary.json][verified: Analysis/Managed/Assembly-CSharp/coverage.json]

REA inspect_managed_members 的成员视图失败不能算作完成。这里的成员/方法体索引来自本地 .NET 探针的实际读取结果，而不是 REA 失败预览或推断类型补全。[verified: Analysis/Evidence/rea-managed-members.json][verified: Analysis/Tools/ManagedInspector/Program.cs]

## 可检索的索引

完整机器索引位于 Analysis/Managed/Assembly-CSharp。每行保留程序集内元数据 token；类型索引保留命名空间、嵌套类型父关系、基类、接口和泛型参数，方法索引保留返回值、参数类型和名字、泛型参数、RVA、方法体分类、IL长度/散列、局部变量与异常区域；字段索引保留字段类型、属性、RVA及常量。[verified: Analysis/Tools/ManagedInspector/Program.cs L98、L113、L135]

- types.json / types.csv：943 个真实 TypeDef，包括编译器生成类型和模块元数据。[verified: Analysis/Managed/Assembly-CSharp/coverage.json type_rows=943][verified: Analysis/Managed/Assembly-CSharp/types.json token=0x02000001 full_name=<Module>]
- methods.json / methods.csv：5,855 个真实 MethodDef。[verified: Analysis/Managed/Assembly-CSharp/coverage.json method_rows=5855]
- fields.json / fields.csv：5,291 个真实 FieldDef。[verified: Analysis/Managed/Assembly-CSharp/coverage.json field_rows=5291]
- method-il.jsonl / method-il.txt：5,126 个方法的原始 IL 字节、指令偏移、操作码、操作数 token、解析的引用/字符串。[verified: Analysis/Managed/Assembly-CSharp/verification.json parsed_body_count=5126 / decoded_instruction_count=96630 / operand_resolution_errors=0]
- assemblies-inventory.json / assemblies-inventory.csv：108 个备份 Managed DLL 的真实元数据和方法体计数，没有读取异常。[verified: Analysis/Managed/Assembly-CSharp/assemblies-inventory.json][verified: Analysis/Managed/Assembly-CSharp/verification.json assemblies_inventory_count=108]

108 个程序集合计 18,593 个 TypeDef、136,685 个 MethodDef、127,033 个可读取 IL 方法体。插件只建立了元数据/方法体数量清单，没有宣称把所有插件反编译为 C#。[verified: assemblies-inventory.json 聚合检查 total_types=18593 / total_methods=136685 / total_readable_bodies=127033][verified: Analysis/Tools/ManagedInspector/Program.cs L35-L38]

## 静态调用与依赖关系

call-edges.json/csv 保留 19,045 条逐指令引用：8,913 条 call、7,190 条 callvirt、1,864 条 newobj、898 条 ldftn、180 条 ldvirtftn。每条保留调用方 token/签名、IL 偏移、目标 token/签名、目标程序集、关系种类及分派边界。[verified: Analysis/Managed/Assembly-CSharp/verification.json call_opcodes / recorded_call_edges][verified: Analysis/Managed/Assembly-CSharp/call-edges.json]

其中 7,119 条直接解析到当前程序集 MethodDef；11,926 条是外部或尚未解析到 MethodDef 的 MemberRef/泛型引用。它们全部保留在调用索引；后者不能被误报为当前程序集的直接 MethodDef 调用。[verified: Analysis/Managed/Assembly-CSharp/verification.json scope_classification]

经独立审查修正后，TypeSpec 的目标程序集沿真实泛型声明根 TypeDef/TypeRef 解码；原先遗漏的 2,476 条边、870 个 operand 均恢复了 callee_assembly，当前遗漏为零。泛型实参的程序集不会替代泛型声明的程序集，例如 Func<UniTask> 仍属于 mscorlib。[verified: Analysis/Tools/ManagedInspector/Program.cs RootType / GenericRootProvider.GetGenericInstantiation][verified: Analysis/Managed/Assembly-CSharp/verification.json previously_missing_assembly_edges_recovered=2476 / previously_missing_assembly_distinct_operands=870 / remaining_missing_callee_assembly_edges=0][verified: call-edges.json caller_token=0x06000007 il_offset=7 callee_assembly=mscorlib]

同程序集泛型 MemberRef 只在元数据根 TypeDef 内找到唯一、完全相同的声明签名时绑定 MethodDef，额外解析了 268 条声明引用；当前同程序集未绑定声明边为零。这不等于 callvirt 的运行时接收者已解析。[verified: Analysis/Tools/ManagedInspector/Program.cs SameSignature / Resolve 的 root TypeDefinition 分支][verified: Analysis/Managed/Assembly-CSharp/verification.json newly_bound_local_generic_declaration_edges=268 / remaining_local_unresolved_declaration_edges=0]

call-edges.csv 已补齐 JSON 的全部 14 个字段，包括 callee_operand_token、relation、scope，并逐行逐字段比对通过。cil-cpp-map.csv 已补齐 pointer源行、方法体起止行、完整 native 声明及 body_sha256_utf8；源码范围与散列字段同 JSON 逐行相符。[verified: Analysis/Tools/ManagedInspector/Program.cs L202][verified: Analysis/Tools/ManagedInspector/map_native.py L181][verified: Analysis/Managed/Assembly-CSharp/verification.json call_csv_all_json_fields_parity=true / native_csv_source_range_and_hash_parity=true]

7,370 条 callvirt/ldvirtftn 只确定声明目标，未完成运行时接收者/接口实现的 points-to 分析。ldftn/ldvirtftn 是函数指针引用，不能证明函数运行。Unity 序列化事件、反射、原生回调和动态加载也不能由该 IL 图自动补全。[verified: Analysis/Managed/Assembly-CSharp/verification.json dispatch_classification][verified: Analysis/Managed/Assembly-CSharp/coverage.json limitations]

- method-call-graph.dot：完整方法图，节点用 token 标识，边标记操作码及 IL 偏移。[verified: Analysis/Tools/ManagedInspector/map_native.py L114-L137]
- type-call-edges.json / type-call-graph.dot：当前程序集已解析 MethodDef 引用聚合为 2,026 条类型关系。[verified: Analysis/Managed/Assembly-CSharp/coverage.json type_call_edges=2026]
- assembly-dependencies.json / assembly-dependencies.dot / assembly-dependencies.mmd：35 个真实 AssemblyRef，包含版本和 public key/token。[verified: Analysis/Managed/Assembly-CSharp/coverage.json assembly_references=35][verified: Analysis/Managed/Assembly-CSharp/assembly-dependencies.json]

关键依赖包括 YarnSpinner/YarnSpinner.Unity 2.4.2.0、Unity.InputSystem 1.18.0.0、Newtonsoft.Json 13.0.0.0、UniTask、DOTween、Zenject、Steamworks.NET、Unity.Localization、Unity.Cinemachine 和 Universal/Core Render Pipeline。这里的版本是 AssemblyRef 元数据值，不能把所有值当作 Package Manager 完整包版本。[verified: Analysis/Managed/Assembly-CSharp/assembly-dependencies.json target_assembly/version][opinion]

## CIL 到生成 C++ 的映射

副本 il2cppOutput/Assembly-CSharp_CodeGen.c 的 L5010 声明 s_methodPointers[5855]；L18423-L18425 将 Assembly-CSharp.dll、5855 和 s_methodPointers 注册进 CodeGenModule。前几个 RID 与 DoorTrigger 的已读方法顺序一致。[verified: Analysis/Inputs/GameCopy/NoImNotAHuman_BackUpThisFolder_ButDontShipItWithYourGame/il2cppOutput/Assembly-CSharp_CodeGen.c L5010-L5030、L18423-L18425][verified: Analysis/Managed/Assembly-CSharp/methods.json token=0x06000001 至 0x06000008]

据此按 MethodDef RID-1 对应数组位置，建立 cil-cpp-map.json/csv。扫描 10 个 Assembly-CSharp*.cpp 文件，全部 4,999 个非 NULL slot 都找到了真实 C++ 函数定义。每条保存 native symbol、完整 native 声明、函数签名行、方法体起止行和文本散列。[verified: Analysis/Managed/Assembly-CSharp/cil-cpp-map-summary.json slots_with_cpp_definition=4999 / nonnull_slots_without_scanned_cpp_definition=0 / scanned_cpp_files=10][verified: Analysis/Tools/ManagedInspector/map_native.py]

856 个 slot 为 NULL：729 个对应没有 RVA 的方法，另 127 个仍有真实 CIL 方法体。NULL 不能据此解释为逻辑不存在；泛型共享/实例化以及 adjustor thunk 尚未独立还原。[verified: cil-cpp-map.json 与 methods.json token 关联检查 null_slots_no_rva=729 / null_slots_with_real_cil_body=127][verified: Analysis/Managed/Assembly-CSharp/cil-cpp-map-summary.json limits]

该CIL/C++映射恢复的是备份程序集与生成 C++ 的结构关系，本索引本身没有GameAssembly机器码RVA/VA。根调查随后独立读取了实际GameAssembly的CodeGenModule指针表，静态地址另保存在[原生地址报告](native-address-map.md)与Analysis/Native；该补充也未证明CIL与机器码在所有方法上逐条或行为等价，未观测运行时执行。[verified: Analysis/Managed/Assembly-CSharp/cil-cpp-map-summary.json limits；Analysis/Native/native-map-verification.json limitations/target_execution=false]

## 仍未恢复的边界

- 原始 C# 注释、原始项目文件与编辑器工程组织：不包含在当前 IL 索引中。[verified: Analysis/Tools/ManagedInspector/Program.cs 索引字段][opinion]
- GameAssembly.dll运行时ASLR地址、已内联机器码和泛型共享分派：未包含在CIL/C++文本映射中；首选映像静态指针地址见独立Analysis/Native结果。[verified: Analysis/Managed/Assembly-CSharp/cil-cpp-map-summary.json limits；Analysis/Native/native-map-verification.json limitations]
- Unity 事件、反射调用、运行时接口分派和资源到 MonoBehaviour 的完整引用绑定：调用图未解析。[verified: Analysis/Managed/Assembly-CSharp/coverage.json limitations]
- C# 导出工程：主调查的原样编译测试失败；可玩 Unity 工程不能由本索引验证。[verified: Analysis/Logs/decompiled-project-build.log L91、L181][verified: Analysis/Managed/Assembly-CSharp/verification.json check_scope]

## 复用入口

后续研究可从 methods.csv 的类型/方法名找到 MethodDef token，在 method-il.txt 查实际指令，再通过 cil-cpp-map.csv 找生成 C++ 的函数签名及方法体位置；call-edges.csv 可查调用方与调用目标。此链路保留真实代码证据与未解析的动态边界。[verified: Analysis/Managed/Assembly-CSharp/methods.csv / method-il.txt / cil-cpp-map.csv / call-edges.csv][opinion]

Verification: ManagedInspector 的 dotnet build Release 通过（0 警告、0 错误）；单文件 smoke 与 REA 身份/计数一致；重新生成索引后 verify_indices.py 检查 5,126 方法体、96,630 IL 指令、264,858 IL 字节、19,045 引用边、5,855 token/native位置、2,476 泛型程序集补全、JSON/CSV逐字段一致性，全部通过且操作数解析错误为 0。原样反编译游戏项目的编译失败已单独记录。[verified: dotnet build 输出 Build succeeded / 0 Warning(s) / 0 Error(s)][verified: Analysis/Managed/Assembly-CSharp/verification.json passed=true][verified: Analysis/Logs/decompiled-project-build.log L91、L181]



