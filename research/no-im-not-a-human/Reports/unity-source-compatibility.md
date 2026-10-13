# Unity 导出源码格式与语法检查

## 已确认结果

主调查使用 ILSpy 的 CSharp9_0 输出模式生成了独立目录 DecompiledCode/UnityCompatible/Assembly-CSharp（587 个 .cs）与 Assembly-CSharp-firstpass（42 个 .cs）。这两目录合计 629 个源文件实际通过了 Roslyn ParseText(LanguageVersion.CSharp9) 的解析诊断检查：0 error、0 warning，未包含 bin/obj 文件。[verified: Analysis/Environment/csharp9-export-provenance.json arguments_supplied / observed_command_exit_code=0 / observed_output][verified: 两兼容导出 csproj L8 LangVersion=9.0][verified: Analysis/syntax-csharp9-verification.json input_paths / inspected_source_files=629 / syntax_error_count=0 / syntax_warning_count=0][verified: 文件清单检查 main_files=587 / firstpass_files=42 / included_bin_obj=0]

这里没有完成全部语言特性门禁、符号绑定、引用解析、Unity 编译或工程导入。不能将上述结果称作完整 C# 9 特性兼容证明，也不能称作可重编译、可导入或可玩的工程。[verified: Analysis/syntax-csharp9-verification.json performed_comprehensive_language_feature_compatibility_validation=false / performed_symbol_binding=false / performed_compilation_or_emit=false / performed_unity_import=false]

## 为什么重新导出

游戏引擎版本为 6000.3.10f1。Unity 6000.3 的官方手册说明该版本使用 Roslyn 和 C# 9.0；该手册还列出 Unity 对部分 C# 9 特性的限制。因此语言版本相同也不是 Unity 编译支持的充分证明。[verified: Analysis/Environment/engine-and-recovery-summary.json unity_version=6000.3.10f1][verified: Unity 官方手册 L125-L133、L142-L154][opinion] [Unity 官方说明](https://docs.unity3d.com/6000.3/Documentation/Manual/csharp-compiler.html)

原始分析用导出中有文件范围命名空间：DecompiledCode/Assembly-CSharp-firstpass/AYellowpaper/SerializedCollections/SerializedDictionarySample.cs L3 为 namespace AYellowpaper.SerializedCollections;。独立 UnityCompatible 输出的对应 L3 改为花括号命名空间形式。原始分析目录用于保留已有代码证据位置；本检查没有修改这两个版本的游戏源码。[verified: 对应两份 SerializedDictionarySample.cs L3 的 rg 实读][verified: Analysis/Tools/CSharpSyntaxInspector/Program.cs 仅 File.ReadAllBytes / File.ReadAllText]

## 关键负对照：解析诊断并非全部语言门禁

另外只读解析了上述原始文件作为已知格式对照。即便设置 CSharp9，SyntaxTree.GetDiagnostics 仍返回 0 error，但 AST 明确存在 1 个 FileScopedNamespaceDeclaration 节点。该真实对照保存在 Analysis/Tools/CSharpSyntaxInspector/original-language-control.json，直接说明这套解析 API 的零诊断不能代替全部语言版本特性检查。[verified: original-language-control.json inspected_source_files=1 / syntax_error_count=0 / file_scoped_namespace_count=1]

兼容输出的 629 文件另做了以下 AST 观察，全部计数为零：FileScopedNamespaceDeclaration、global using、RecordStructDeclaration、ListPattern、CollectionExpression。SDK 的后三个 SyntaxKind 名字均实际存在，因此它们的零计数不是因为该 SDK 不认识这些节点。[verified: Analysis/syntax-csharp9-verification.json file_scoped_namespace_count=0 / global_using_count=0 / observed_feature_node_counts / sdk_supports_observed_feature_kind_names]

这只是指定语法结构的观察清单，不覆盖全部新版特性、Unity 专属限制或未激活预处理分支。没有设置 Unity 的平台预处理符号，未激活条件分支不能据此声称全部检查过。[verified: Analysis/syntax-csharp9-verification.json preprocessor_symbols=[] / limitations]

## 工具、执行顺序与证据

检查工具位于 Analysis/Tools/CSharpSyntaxInspector，使用 .NET 10 与已存在的 SDK Roslyn 5.3.0.0 库。先读取了本机两个 DLL 的公开 ParseText、CSharpParseOptions、GetDiagnostics 签名，再写解析工具；API 清单已保存 roslyn-api-inventory.json。PowerShell 中直接 LoadFrom 曾遇到程序集版本冲突 0x80131040，隔离 .NET 10 探针成功取得签名。[verified: Analysis/Tools/CSharpSyntaxInspector/roslyn-api-inventory.json common_assembly / csharp_assembly / parse_text_signatures / parse_options_constructors / get_diagnostics_signatures / initial_powershell_probe]

先做单文件 smoke（1 文件，0 error、0 warning），再运行两目录完整检查，并保存每个源文件的绝对路径、字节数、SHA-256 和解析诊断。使用的是 SDK Roslyn，不是从目标 Unity Editor 提取的编译器。[verified: Analysis/Tools/CSharpSyntaxInspector/smoke-csharp9-verification.json mode=single_file_smoke / inspected_source_files=1][verified: Analysis/syntax-csharp9-verification.json files / roslyn_assembly / limitations]

本检查未加载或执行游戏程序集，未调用游戏函数。所有读取对象是已导出的文本和 SDK Roslyn 库；没有对原 Steam 游戏目录或恢复资产进行写入。[verified: Analysis/Tools/CSharpSyntaxInspector/Program.cs L48-L67、L89-L90][verified: Analysis/syntax-csharp9-verification.json loaded_or_executed_game_assemblies=false / executed_game_functions=false]

## 仍阻碍编译的条件

- 原样分析导出的 net40 项目编译失败：83 个错误、4 个警告，已读例子包括缺少 ExtensionAttribute 与 IReadOnlyList。真实 CIL 恢复不能据此改判为失败；失败的是该导出项目当前重编译配置/环境。[verified: Analysis/Logs/decompiled-project-build.log L3-L13、L91、L180-L181][opinion]
- 改以 .NET Framework 4.8 做诊断时，MSB3644 显示本机缺少该目标的 reference assemblies；1 个错误。[verified: Analysis/Logs/decompiled-project-net48-diagnostics.txt L1-L5]
- 指定范围内实际定位的是 2019.4.33f1 和 2022.3.23f1 系列 Editor，E:/Unity/Editors 当前只有 2022.3.23f1。未定位目标 6000.3.10f1 Editor，也没有完成它的 Unity 导入/编译验证。该“未定位”限定于已检查路径，不是全盘不存在的证明。[verified: Analysis/Environment/tool-inventory.json unity_editors / limitations][verified: 当前 E:/Unity/Editors 目录实读][verified: Analysis/syntax-csharp9-verification.json performed_unity_import=false]

这些限制仍需要合适的 Editor、目标参考程序集及实际 Unity 导入/编译才能进一步消解。本任务没有通过修改真实逻辑来制造编译成功。[opinion][verified: Analysis/syntax-csharp9-verification.json performed_compilation_or_emit=false]

Verification: CSharpSyntaxInspector Release 构建 0 warning/0 error；单文件 smoke 后完整读取 629 文件，解析诊断 0 error/0 warning；指定 5 类现代语法 AST 观察均为零；原始命名空间负对照返回 0 解析错误但明确检出 1 个文件范围命名空间。符号绑定、编译和 Unity 导入均未执行。[verified: build 输出 Build succeeded / 0 Warning(s) / 0 Error(s)][verified: Analysis/syntax-csharp9-verification.json][verified: original-language-control.json]


