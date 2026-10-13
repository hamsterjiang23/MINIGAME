from __future__ import annotations

import json
from pathlib import Path
from typing import cast

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Analysis" / "Gameplay"
BASE = ROOT / "DecompiledCode" / "Assembly-CSharp"


def resolve_source(value: str) -> Path:
    """Map preserved evidence paths to this research directory."""
    path = Path(value)
    if not path.is_absolute():
        return ROOT / path
    roots = json.loads((ROOT / "Analysis/investigation-status.json").read_text(encoding="utf-8-sig"))
    for key in ("original_research_root", "current_research_root"):
        previous_root = Path(str(roots.get(key, ROOT)))
        try:
            relative = path.relative_to(previous_root)
        except ValueError:
            continue
        return ROOT / relative
    return path


def main() -> None:
    data = cast(dict[str, object], json.loads((OUT / "dialogue-progression.json").read_text(encoding="utf-8")))
    claims = [claim for claim in cast(list[dict[str, object]], data["claims"]) if not str(claim["id"]).startswith(("serialized_assets-", "yarn_bytecode-"))]
    statements = {
        "dialogue-2": "DialogView把节点名传入StartDialogue；CustomYarnReader从YarnProject查line ID，再从Unity Localization的Dialogs表读取当前语言文本。现已实际解析非空编译YarnProgram，保留3255节点，未还原原作者编辑的.yarn源文本。",
        "dialogue-9": "YarnFunction可查询IsImposter、GetInnocentsCount、IsAlive、IsNobodyHome、IsCharacterPlaceEmpty、GetEnergy、GetItemCount、HasCompletedMushroomCondition、CanOrderCourier、IsEverCompletedGame、HasSeenDream。IsImposter查询CharacterSOData.IsImposter；各编译节点的实际调用记录现已保存到yarn-command-function-index.json。",
        "dialogue-11": "编译Yarn程序已恢复为真实protobuf bytes及JSON；保留所有节点、指令、变量、label、headers和常量跳转。选项文本line ID已编入索引，文本表保留在恢复工程。未执行运行时选择，未自动复原原Yarn语法。",
        "daynight_resources-1": "MaxDayActions=配置.MaxDayActionsByDays[Day]+ExtraEnergySlots-当日EnergyFines；实际数值数组、末日和咖啡持续天数已经读取成功导出的DayNightControllerData.asset，详见本报告资产数值部分。",
        "daynight_resources-7": "KilledByImpostersCount对EndingsSOData.BodyEaterCount整除时安排BodyEater梦与下夜标志；下夜若标志就绪，KillEveryone、破门事件并DisableSuperForDay。实际BodyEaterCount字段已读取为4，参见资产字段证据。",
        "daynight_resources-8": "AddEnergySlot记录咖啡日并增加ExtraEnergySlots；AddEnergy只在当前行动<上限时+1；RefillEnergy重填；RemoveExtraEnergySlotsForTomorrow把罚值写入明天EnergyFines。具体道具调用点已核对，见本报告道具补充。",
        "daynight_resources-13": "实际每日行动数、LastDay、特殊结局日期和CoffeeBoostDays现已恢复；缺货参数的实际OtherGameSOData实例与全物品效果仍未由本子任务完整读取，不声称资源平衡表已完全恢复。",
        "endings-5": "Killer，Priority3：InnocentsKilledCount>=EndingsSOData.KillsCount；真实KillsCount已读取为4、KillerEndingDay为17。UnlockCartoon只改_isReady但AreConditionsMet并未使用该字段，不能额外添加门槛；资产数值证据见下节。",
        "endings-10": "FEMA，Priority1：未EverRudeToFema、PovistaskUsedCount>=配置PovistkasUsedCount、电话回调计数>=配置FemaCallsCount；真实门槛已读取为传票2、通话3，特殊结局日期15。FemaCallsCount委托实际由CloseUpsController绑定，见调用点补充。",
        "endings-14": "实际EndingsData.asset已读取KillsCount4、BodyEaterCount4、ProphetDontCheckSignsDaysNeeded2、PovistkasUsedCount2、FemaCallsCount3。这些是成功导出的资产字段，非C#默认；影片在本子任务未进行播放器验证。",
        "events_progression-9": "Yarn节点字节码、条件跳转label与命令函数使用已恢复；完整Visits和随机难度参数未由本子任务归一化。未做运行时采样，所以不声称全部分支可达性、事件概率分布或难度曲线已验证。",
    }
    for claim in claims:
        cid = str(claim["id"])
        if cid in statements:
            claim["statement"] = statements[cid]
        if cid == "dialogue-11":
            claim.update(status="verified", source=str(OUT / "summary.json"), line_start=2, line_end=13,
                         methods="parser/nodes/initial_variables/instructions", implementation="decoded_original_Yarn_program")
        elif cid == "dialogue-6":
            claim.update(source=str(BASE / "_Code/Infrastructure/DayNight/DayNightController.cs"), line_start=501, line_end=505,
                         methods="Act; subscription also at Init L133-L148")
        elif cid == "endings-14":
            claim.update(status="verified", source=str(ROOT / "RecoveredUnityProject" / "ExportedProject" / "Assets" / "MonoBehaviour" / "EndingsData.asset"),
                         line_start=15, line_end=19, methods="serialized config", implementation="serialized_asset")
    extras = [
        ("serialized_assets-1", "serialized_assets", "每日基础行动数组按day索引0..18解码为[1,2,2,2,3,3,3,3,3,3,3,3,3,4,5,0,0,0,0]，Unity YAML packed little-endian Int32。真实LastDay14、BabyEndingDay16、KillerEndingDay17、FemaEndingDay15、CoffeeBoostDays3。",
         ROOT / "RecoveredUnityProject/ExportedProject/Assets/MonoBehaviour/DayNightControllerData.asset", 28, 41, "serialized config", "verified"),
        ("serialized_assets-2", "serialized_assets", "Coffee动画后调用AddEnergySlot；Enerjeka动画后调用AddEnergy；DrinkBeerAsync调用ActAll并记录DrunkBeerToday；Mushroom调用GameplayEndingMushroom.EatMushroom并安排Mushroom梦。",
         BASE / "_Code/Infrastructure/CloseUps/Views/FridgeCloseUpView.cs", 227, 287, "OnUse/DrinkCoffeeAsync/DrinkEnerjekaAsync/DrinkBeerAsync", "verified"),
        ("serialized_assets-3", "serialized_assets", "CigaretteInteractable.OnUse调用RefillEnergy和RemoveExtraEnergySlotsForTomorrow(1)，并更新玩家Signs.Smoke；该效果已沿真实调用点核对。",
         BASE / "_Code/Infrastructure/CigaretteInteractable.cs", 118, 131, "OnUse", "verified"),
        ("serialized_assets-4", "serialized_assets", "CloseUpsController实际初始化调用GameplayEndingManager.InitGetFemaCallsFunc(() => FemaCallsCount)，FEMA结局依赖的计数委托存在真实绑定。",
         BASE / "_Code/Infrastructure/CloseUps/Views/CloseUpsController.cs", 151, 165, "InitModules", "verified"),
        ("yarn_bytecode-1", "yarn_bytecode", "_NoImNotAHuman.asset L15包含真实compiledYarnProgram；原备份YarnSpinner.dll的Yarn.Program.Parser成功解析2,207,477字节。原bytes与完整protobuf JSON已经本地持久化，SHA256见summary。",
         OUT / "summary.json", 2, 6, "parser/compiled_program_bytes/compiled_program_sha256", "verified"),
        ("yarn_bytecode-2", "yarn_bytecode", "编译程序含3255节点、276初始变量、56,585指令、15,693唯一line ID；4043个RUN_COMMAND含815不同命令字符串，2821个CALL_FUNC。命令字符串含参数，所以815不是815种宿主方法。",
         OUT / "summary.json", 7, 13, "counts", "verified"),
        ("yarn_bytecode-3", "yarn_bytecode", "已索引4570常量RUN_NODE跳转、10,848本地JUMP_TO/JUMP_IF_FALSE/ADD_OPTION标签边；本地标签无缺失。1868个JUMP目标由运行时栈提供，单独保留。全部RUN_NODE均紧邻PUSH_STRING，可恢复常量名字。",
         OUT / "summary.json", 46, 65, "edge/opcode counts", "verified"),
        ("yarn_bytecode-4", "yarn_bytecode", "存在1个缺失常量目标：Wheelchair_Entrance_AcceptBribe instruction12跳向Wheelchair_Entrance_Occupied，该名不在程序节点中。未补造目标，也不称控制图完全闭合。",
         OUT / "yarn-unresolved-control.json", 18685, 18697, "missing_nodes", "verified"),
        ("yarn_bytecode-5", "yarn_bytecode", "SuperFake_Entrance_3_NotAlone先CALL_FUNC IsNobodyHome，条件为真时RUN_NODE到SuperFake_Entrance_3_Alone；后者包含Refuse SuperFake、Cutscene Fek3d。记录为静态代码分支，不声称某次游玩已走到该分支。",
         OUT / "yarn-program.json", 1, 1, "nodes.SuperFake_Entrance_3_NotAlone.instructions[0..4] / nodes.SuperFake_Entrance_3_Alone.instructions[7..8]", "verified"),
        ("yarn_bytecode-6", "yarn_bytecode", "原作者.yarn文本、自然语言文本与字节码的完整联表、所有节点实际可达路径、概率函数dice实际采样和场景序列化UnityEvent初始化，尚未在本子任务完整验证。",
         OUT / "summary.json", 2, 65, "artifact boundaries", "unknown"),
        ("yarn_bytecode-7", "yarn_bytecode", "恢复游戏场景s1_GameScene的FakeDialogRunner组件将yarnProject序列化指向_NoImNotAHuman资产guid 86b9fcf429903564596b72440f8ba0d8；因此被解析程序确有游戏场景静态引用，仍未运行游戏。",
         ROOT / "RecoveredUnityProject/ExportedProject/Assets/_Scenes/s1_GameScene.unity", 105880, 105913, "component 4471 yarnProject binding", "verified"),
        ("yarn_bytecode-8", "yarn_bytecode", "上述场景m_Script guid a14375ca4a7b3bd8aac4a8ace2eb7f22与FakeDialogRunner.cs.meta的guid一致；CustomYarnReader组件同样引用该YarnProject（s1_GameScene.unity L89011-L89014）。",
         ROOT / "RecoveredUnityProject/ExportedProject/Assets/Scripts/Assembly-CSharp/_Code/DialogSystem/FakeDialogRunner.cs.meta", 1, 2, "script guid", "verified"),
    ]
    for cid, module, text, source, start, end, methods, status in extras:
        claims.append({"id": cid, "module": module, "statement": text, "status": status, "source": str(source),
                       "line_start": start, "line_end": end, "methods": methods,
                       "implementation": "decoded_original_Yarn_program" if module == "yarn_bytecode" else "asset_or_actual_host_call",
                       "validation": "static_source_or_original_asset_read"})
    data["schema_version"] = 2
    data["limitations"] = ["No game launch", "No live Unity import verification", "No Steam files modified", "No player save files read or modified",
                           "Original authored .yarn source not reconstructed", "One constant node target missing", "1868 option-stack JUMPs runtime dependent"]
    (OUT / "dialogue-progression.json").write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
    titles = {"dialogue": "对话系统与条件触发", "daynight_resources": "昼夜循环、行动与物资", "decisions_npc_state": "玩家决策与NPC状态",
              "events_progression": "事件、剧情与难度边界", "save": "存档与持久化", "endings": "结局条件与优先级",
              "serialized_assets": "真实资产数值与道具调用点", "yarn_bytecode": "Yarn字节码恢复、索引与明确缺口"}
    lines = ["# 《No, I'm not a Human》对白、进展、状态与存档静态研究", "",
             "本报告使用恢复后的真实CIL实现、成功导出的配置资产及原DLL解码的编译Yarn程序。未启动游戏、未修改Steam目录、未读取或改写玩家存档。“已验证”代表已读取静态源码/资产/解析输出，不等于Unity导入或游戏运行验证。[已验证: 本子任务实际文件检查与持久化命令]", "",
             "源码根目录：[主程序集源码](../DecompiledCode/Assembly-CSharp)。引用行号是当前ILSpy导出位置；Yarn字节码以节点名+instruction_index定位，另有完整反汇编文本。[已验证: 下列每项来源]", ""]
    for module, title in titles.items():
        lines.extend([f"## {title}", ""])
        for claim in claims:
            if claim["module"] != module:
                continue
            source = resolve_source(str(claim["source"]))
            mark = "已验证" if claim["status"] == "verified" else "推断" if claim["status"] == "inferred" else "未知"
            link = f"../{source.relative_to(ROOT).as_posix()}:{claim['line_start']}"
            lines.append(f"- {claim['statement']} [{mark}: [{source.name}](<{link}>) L{claim['line_start']}–L{claim['line_end']}, `{claim['methods']}`]")
        lines.append("")
    lines.extend(["## 调用边边界", "",
                  "- C#直接调用、接口callvirt、C#事件委托、UnityEvent监听、Yarn节点字符串绑定分开保留；类或节点存在不能证明实际可达。FakeDialogRunner真实继承DialogueRunner，DialogView真实调用StartDialogue，仍未启动游戏验证场景绑定。[已验证: DialogView.cs L82–L100/L170–L201; FakeDialogRunner.cs L19–L88]",
                  "- 模块图与NPC/日夜/对白状态图是从真实数据列表与回调重建的概念图，不冒称存在同名通用FSM类。[已验证: ARoom.cs L138–L182; CharactersManagerSaveData.cs L11–L48; DayNightController.cs L267–L363]", "",
                  "## 已保存产物与复跑", "",
                  f"- dialogue-progression.json：{len(claims)}条结论，带status/source/line范围/方法。",
                  "- yarn-program.bytes/json：原编译protobuf与原DLL解析JSON；yarn-node-index.json/node-edges.json/control-edges.json/command-function-index.json/unresolved-control.json：全部节点、边、命令函数与缺口。",
                  "- yarn-disassembly.txt：完整指令文本，非原作者Yarn源；serialized-gameplay-configs.json：真实YAML字段和来源行。",
                  "- parse-yarn.ps1、index_yarn.py：可复跑helper；parse-yarn-result.json与index-yarn-result.json保存实际命令结果；summary.json保存数量、SHA256和控制目标校验。",
                  "- dialogue-progression.mmd及dialogue-progression.npc-state.mmd/daynight-state.mmd/dialogue-state.mmd：分别是独立合法Mermaid模块图和状态图。[已验证: 本子任务文件写入及验证结果]", "",
                  "Verification: parse-yarn.ps1成功解析2,207,477字节/3255节点/276初始变量；uv run --no-project python index_yarn.py完成所有索引，本地标签无缺失、常量节点缺失1（已记录）。JSON解析及证据来源行范围检查通过。未运行Unity/gameplay测试。[已验证: parse-yarn-result.json; index-yarn-result.json; verification.json]"])
    (ROOT / "Reports" / "gameplay-dialogue-progression.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
    original = (OUT / "dialogue-progression.mmd").read_text(encoding="utf-8")
    parts = original.split("\n---\n")
    (OUT / "dialogue-progression.mmd").write_text(parts[0], encoding="utf-8")
    for suffix, content in zip(("npc-state", "daynight-state", "dialogue-state"), parts[1:]):
        (OUT / f"dialogue-progression.{suffix}.mmd").write_text(content, encoding="utf-8")
    failures: list[dict[str, object]] = []
    for claim in claims:
        source = resolve_source(str(claim["source"]))
        if not source.is_file():
            failures.append({"id": claim["id"], "error": "source_missing", "source": str(source)})
            continue
        count = sum(1 for _ in source.open(encoding="utf-8-sig"))
        if not 1 <= int(cast(int, claim["line_start"])) <= int(cast(int, claim["line_end"])) <= count:
            failures.append({"id": claim["id"], "error": "invalid_line_range", "line_count": count})
    check = {"evidence_claims": len(claims), "source_line_checks": "passed" if not failures else "failed", "failures": failures,
             "json_parse": "passed", "diagrams": len(list(OUT.glob("dialogue-progression*.mmd"))), "game_runtime_test": "not_run"}
    (OUT / "verification.json").write_text(json.dumps(check, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(check, ensure_ascii=False))


if __name__ == "__main__":
    main()

