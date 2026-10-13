# GameAssembly原生静态地址

## 实际恢复

直接读取游戏副本的x64 PE32+字节，找到唯一的Assembly-CSharp.dll CodeGenModule前缀：模块名指针、5855方法指针数量、475个adjustor thunk数量与生成CodeGen.c一致。模块首三个字段的布局参照[Cpp2IL真实读取实现](https://raw.githubusercontent.com/SamboyCoding/Cpp2IL/development/LibCpp2IL/BinaryStructures/Il2CppCodeGenModule.cs) L48-L50，再以本地文件中的计数、字符串及地址映射验证，未运行GameAssembly。[verified: Analysis/Tools/native_method_map.py；Analysis/Inputs/GameCopy/NoImNotAHuman_BackUpThisFolder_ButDontShipItWithYourGame/il2cppOutput/Assembly-CSharp_CodeGen.c L18420-L18439；native-map-verification.json]

5855个指针slot全部读取；4999个非零条目都有file-backed executable section中的地址，856个NULL位置逐项匹配生成C++的NULL模式。非零条目只有3416个不同地址；重复指针的具体成因未经机器码分析确认，不按名字推断函数折叠或运行时调用。[verified: Analysis/Native/native-map-verification.json slots/nonzero_static_addresses/null_slots/unique_nonzero_addresses/null_mask_matches_generated_cpp/all_nonzero_targets_file_backed_executable][opinion]

GameAssembly SHA256=de2c120d90e757916d74c298f264040df0cda1ddaa8c412e438aeee72eb4dc3f，PE首选ImageBase=0x180000000。输出为每个MethodDef token连接CIL签名、C++符号、指针slot文件偏移、首选映像VA、RVA、代码文件偏移、section及16字节局部证据。16字节只是局部读取，不是完整函数体或函数边界。[verified: Analysis/Native/native-map-verification.json；native-method-map.json/csv][opinion]

## 证据入口

- native-method-map.json/csv：5855条slot关系，包括NULL和共享地址，不删除重复。[verified: Analysis/Tools/native_method_map.py main；Analysis/Native/native-method-map.json/csv]
- gameplay-entrypoints.csv：访客随机选择、LetIn、GenerateStatus、ShowSign的实际静态地址入口。[verified: Analysis/Native/gameplay-entrypoints.csv]
- native-map-verification.json：输入SHA、结构候选、完整计数、所有非零地址映射与限制。[verified: Analysis/Native/native-map-verification.json]
- code-recovery.md和cil-cpp-map.csv：以token继续查CIL、生成C++源码范围和实际签名。[verified: Reports/code-recovery.md；Analysis/Managed/Assembly-CSharp/cil-cpp-map.csv]

## 未完成的边界

这些地址使用PE首选基址，不是正在运行进程的ASLR地址；实时加载基址未知。行顺序和NULL模式对应是静态结构关联，未证明所有CIL与机器码逐条或行为等价。未运行Ghidra、未恢复原生CFG/内联边界、未分析泛型实例表或观测游戏执行。[verified: native-map-verification.json limitations/target_execution=false；Analysis/Evidence/rea-initial-session.json Ghidra provider unavailable]

后续若在研究副本上配置REA支持的Ghidra/JDK，可从RVA加Ghidra当前ImageBase定位这些入口，再与对应生成C++及CIL核对；不得把当前指针表当成已完成的反汇编。[opinion]

Verification: 唯一模块前缀、5855行索引、NULL模式和4999非零指针的可执行文件映射全部断言通过，独立复检亦通过。[verified: native-map-verification.json slots=5855/nonzero_static_addresses=4999；Reports/native-map-review.md「No bugs found」；工具输出 NativeMapExit=0]
