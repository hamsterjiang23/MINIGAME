"""Build an offline dialogue atlas from the source XLSX files (stdlib only)."""

from __future__ import annotations

import argparse
import hashlib
import json
import posixpath
import re
import sys
from collections import Counter
from dataclasses import asdict, dataclass, field
from pathlib import Path
from xml.etree import ElementTree as ET
from zipfile import ZipFile

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Art/chat_excel"
HERE = Path(__file__).resolve().parent
NS = {"s": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
REL_ID = "{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id"


@dataclass
class Node:
    id: str
    raw: str
    day: str
    round: str
    kind: str
    text: str
    row: int = 0
    fields: dict[str, str] = field(default_factory=dict)
    delta: float | None = None
    action: str = ""
    note: str = ""


@dataclass
class Edge:
    source: str
    target: str
    kind: str
    label: str = ""
    condition: str = ""
    evidence: str = ""


@dataclass
class Book:
    id: str
    name: str
    file: str
    sha256: str
    nodes: list[Node]
    edges: list[Edge]
    issues: list[str]
    entries: dict[str, str]
    rounds: dict[str, list[str]]


def build_book(path: Path, index: int) -> Book:
    rows = read_workbook(path)["对话流程"][1:]
    key = f"book{index}"
    name = re.sub(r"v[\d.]+$", "", path.stem.replace("【对话】", ""))
    nodes: list[Node] = []
    edges: list[Edge] = []
    issues: list[str] = []
    by_raw: dict[str, Node] = {}
    groups: dict[tuple[str, str], list[Node]] = {}
    entries: dict[str, str] = {}
    rounds: dict[str, list[str]] = {}
    for row in rows:
        raw = row.get("J", "")
        if not raw or raw in by_raw:
            raise ValueError(f"{path.name} row {row['row']}: missing or duplicate ID {raw}")
        match = re.search(r"(?:^|_)D(\d+)(?:_|$)", raw)
        day = f"Day{match[1]}" if match else "结局"
        segment = row.get("D", "")
        action_match = re.search(r"_(TREAT|PURIFY|RELEASE)$", raw)
        action = action_match[1].lower() if action_match and day != "结局" else ""
        kind = "measure" if action else "option" if segment == "玩家选项" else "dialogue"
        if day == "结局":
            kind = "ending"
        fields = {col: row[col] for col in "ABCDEFGHIJKLM" if col in row}
        text = row.get("F", "") if kind == "option" else (row.get("F") or row.get("H", "")) if action else "\n".join(row[c] for c in "EGI" if row.get(c))
        delta = float(row["L"]) if row.get("L") and re.fullmatch(r"[+-]?\d+(?:\.\d+)?", row["L"]) else None
        node = Node(f"{key}:{raw}", raw, day, row.get("B", ""), kind, text, int(row["row"]), fields, delta, action)
        if not row.get("A"):
            node.note = "Day/状态单元格为空；显示天数由节点ID解析，原单元格未改写。"
        if kind in ("option", "measure") and delta is None:
            issues.append(f"第{node.row}行：选项信任增量未填或不是数值。")
        if row.get("F") and row.get("H"):
            node.note += " 同行同时含F/G/H/I：保留各列原文；H列不另解释为独立处置或第二次选择。"
        nodes.append(node)
        by_raw[raw] = node
        groups.setdefault((day, node.round), []).append(node)
        entries.setdefault(day, node.id)
        if node.round not in rounds.setdefault(day, []):
            rounds[day].append(node.round)

    # Option hubs are reconstructed from the same day/round, never from row adjacency.
    for (day, round_name), group in groups.items():
        options = [n for n in group if n.kind in ("option", "measure")]
        if not options:
            continue
        incoming_targets = {n.fields.get("K", "") for n in group}
        if options[0].action and "_M_" in options[0].raw:
            expected = options[0].raw.split("_M_")[0] + "_MEASURE"
            if any(n.fields.get("K") == expected for n in nodes):
                incoming_targets.add(expected)
        expected_choice = re.sub(r"_O\d+$", "_CHOICE", options[0].raw)
        candidates = [t for t in incoming_targets if t not in by_raw and
                      (t == expected_choice or (options[0].action and t.endswith("_MEASURE")))]
        if len(candidates) > 1:
            raise ValueError(f"Ambiguous choice hub in {path.name}: {candidates}")
        raw = candidates[0] if candidates else f"__{day}_{round_name}_CHOICE"
        gate = Node(f"{key}:{raw}", raw, day, round_name, "gate", "选择处置" if options[0].action else "选择回答",
                    note="显示用选择集合；按同一天、同轮次归组。此节点及到选项的虚线不是源表独立记录。")
        nodes.append(gate)
        by_raw[raw] = gate
        for option in options:
            edges.append(Edge(gate.id, option.id, "inferred", "选择", evidence=f"对话流程 B{option.row}, D{option.row}, J{option.row}；使用方式 C26"))
        for start in group:
            if start.kind == "dialogue" and not start.fields.get("K"):
                edges.append(Edge(start.id, gate.id, "inferred", "按轮次归组", evidence=f"对话流程 B{start.row}, K{start.row}为空"))

    for node in list(nodes):
        target = node.fields.get("K", "")
        if not target:
            continue
        if target not in by_raw:
            terminal = target.startswith("END_")
            boundary = Node(f"{key}:{target}", target, node.day, "边界", "boundary" if terminal else "missing",
                            "角色线结束" if target == "END_CHARACTER" else f"{node.day} 会谈结束" if terminal else f"缺少目标：{target}",
                            note="源表引用的结束标记，非正文行。后续由处置和全局调度决定。" if terminal else "未找到目标，不能自动补写剧情。")
            nodes.append(boundary)
            by_raw[target] = boundary
            if not terminal:
                issues.append(f"第{node.row}行：跳转目标不存在 {target}")
        edges.append(Edge(node.id, by_raw[target].id, "explicit", evidence=f"对话流程 K{node.row} → {target}"))

    # Generic END_DAY boundaries have conditional return edges only after treatment.
    for node in list(nodes):
        if node.kind == "boundary" and re.fullmatch(r"END_DAY[12]", node.raw):
            next_day = f"Day{int(node.raw[-1]) + 1}"
            if next_day in entries:
                edges.append(Edge(node.id, entries[next_day], "document", "仅治疗且仍可返场 → 次日", "treat",
                                  "docs/game-overview.md:55,66；源表次日状态为治疗后"))
    if "外交官" in name:
        issues.append("72行将选项和多段回应放在同一行；H列标题为玩家措施但常含对白。各列并列保留，执行语义待确认。")
    if "神父" in name:
        issues.append("表内有Day1初见；设计文档称Day2出现。日程待确认（docs/open-questions.md:11,40）。")
    if "Day0" in name:
        issues.append("净化选项L7为-1，与使用方式D33写措施信任为0不一致；按实际单元格保留-1。")
    inferred_count = sum(e.kind == "inferred" for e in edges)
    issues.append(f"{inferred_count}条选择归组连接由同日同轮次推导，显示虚线，可在详情查看依据。")
    return Book(key, name, path.name, hashlib.sha256(path.read_bytes()).hexdigest(), nodes, edges, issues, entries, rounds)


def validate(books: list[Book]) -> dict[str, object]:
    summary: list[dict[str, object]] = []
    for book in books:
        ids = {n.id for n in book.nodes}
        assert len(ids) == len(book.nodes)
        assert all(e.source in ids and e.target in ids for e in book.edges)
        source_rows = read_workbook(SOURCE / book.file)["对话流程"][1:]
        originals = {n.row: n for n in book.nodes if n.row}
        assert len(source_rows) == len(originals)
        for row in source_rows:
            node = originals[int(row["row"])]
            assert node.fields == {c: row[c] for c in "ABCDEFGHIJKLM" if c in row}
            if row.get("K"):
                assert any(e.source == node.id and e.target == f"{book.id}:{row['K']}" and e.kind == "explicit" for e in book.edges)
        reached = {book.entries[next(iter(book.entries))]}
        while True:
            updated = reached | {e.target for e in book.edges if e.source in reached}
            if reached == updated:
                break
            reached = updated
        unreachable = [n.raw for n in book.nodes if n.row and n.id not in reached]
        assert not unreachable, (book.name, unreachable)
        summary.append({"file": book.file, "source_rows": len(originals), "nodes": len(ids), "edges": len(book.edges),
                        "explicit_edges": sum(e.kind == "explicit" for e in book.edges),
                        "inferred_edges": sum(e.kind == "inferred" for e in book.edges),
                        "document_edges": sum(e.kind == "document" for e in book.edges),
                        "missing_targets": [n.raw for n in book.nodes if n.kind == "missing"],
                        "unreachable_source_rows": unreachable, "sha256": book.sha256})
    return {"source_rows": sum(x["source_rows"] for x in summary), "books": summary,
            "checks": ["unique_namespaced_ids", "every_source_cell_preserved", "every_K_link_preserved", "all_edge_endpoints_exist", "all_source_rows_reachable"],
            "scope": "Spreadsheet graph validation; inferred connections are not runtime verification."}


def read_workbook(path: Path) -> dict[str, list[dict[str, str]]]:
    """Read stored values; source sheet is authoritative, never recalculate it."""
    result: dict[str, list[dict[str, str]]] = {}
    with ZipFile(path) as archive:
        strings: list[str] = []
        if "xl/sharedStrings.xml" in archive.namelist():
            strings = ["".join(t.text or "" for t in si.findall(".//s:t", NS))
                       for si in ET.fromstring(archive.read("xl/sharedStrings.xml"))]
        relations = {r.attrib["Id"]: r.attrib["Target"] for r in
                     ET.fromstring(archive.read("xl/_rels/workbook.xml.rels"))}
        sheets = ET.fromstring(archive.read("xl/workbook.xml")).find("s:sheets", NS)
        assert sheets is not None
        for sheet in sheets:
            target = relations[sheet.attrib[REL_ID]]
            target = target.lstrip("/") if target.startswith("/") else posixpath.normpath("xl/" + target)
            rows: list[dict[str, str]] = []
            for row in ET.fromstring(archive.read(target)).findall("s:sheetData/s:row", NS):
                values: dict[str, str] = {"row": row.attrib["r"]}
                for cell in row:
                    column = re.sub(r"\d+", "", cell.attrib["r"])
                    value = cell.find("s:v", NS)
                    text = value.text or "" if value is not None else ""
                    if cell.attrib.get("t") == "s" and text:
                        text = strings[int(text)]
                    elif cell.attrib.get("t") == "inlineStr":
                        text = "".join(t.text or "" for t in cell.findall(".//s:t", NS))
                    if text.strip():
                        values[column] = text.strip()
                if len(values) > 1:
                    rows.append(values)
            result[sheet.attrib["name"]] = rows
    return result


def inspect_sources() -> dict[str, object]:
    books: list[dict[str, object]] = []
    for path in sorted(SOURCE.glob("*.xlsx")):
        if path.name.startswith("~$"):
            continue
        sheets = read_workbook(path)
        rows = sheets["对话流程"][1:]
        ids = {r.get("J", "") for r in rows}
        targets = Counter(r.get("K", "") for r in rows if r.get("K"))
        books.append({"file": path.name, "rows": len(rows),
                      "segments": dict(Counter(r.get("D", "") for r in rows)),
                      "external_targets": {k: v for k, v in targets.items() if k not in ids},
                      "missing_id": [r for r in rows if not r.get("J")],
                      "no_next": [r for r in rows if not r.get("K")],
                      "measures": [r for r in rows if r.get("D") == "玩家措施"],
                      "usage": sheets.get("使用方式", [])})
    return {"books": books}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--inspect", action="store_true")
    parser.add_argument("--smoke", action="store_true")
    args = parser.parse_args()
    if args.inspect:
        data = inspect_sources()
        (HERE / "source-inspection.json").write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
        for book in data["books"]:
            print(json.dumps({k: v for k, v in book.items() if k not in ("usage", "no_next")}, ensure_ascii=False))
        return
    paths = sorted(p for p in SOURCE.glob("*.xlsx") if not p.name.startswith("~$"))
    if args.smoke:
        paths = paths[:1]
    books = [build_book(path, index) for index, path in enumerate(paths)]
    report = validate(books)
    print(json.dumps(report, ensure_ascii=False, indent=2))
    if args.smoke:
        return
    data = {"books": [asdict(book) for book in books], "audit": report}
    (HERE / "graph-data.json").write_text(json.dumps(data, ensure_ascii=False, separators=(",", ":")), encoding="utf-8")
    (HERE / "validation.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    template = (HERE / "atlas.template.html").read_text(encoding="utf-8")
    content = template.replace("/*__STYLE__*/", (HERE / "atlas.css").read_text(encoding="utf-8"))
    app = (HERE / "atlas-core.js").read_text(encoding="utf-8") + "\n" + (HERE / "atlas.js").read_text(encoding="utf-8")
    content = content.replace("/*__APP__*/", app)
    payload = json.dumps(data, ensure_ascii=False, separators=(",", ":")).replace("<", "\\u003c")
    content = content.replace("__GRAPH_DATA__", payload)
    output = SOURCE / "游戏逻辑分支图.html"
    output.write_text(content, encoding="utf-8")
    print(f"HTML: {output} ({output.stat().st_size:,} bytes)")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
