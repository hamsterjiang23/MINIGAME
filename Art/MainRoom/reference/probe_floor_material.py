"""Inspect the reference floor's nested node group without changing its source."""
from __future__ import annotations

from pathlib import Path
import bpy


def dump(tree: bpy.types.NodeTree, level: int = 0) -> None:
    for node in tree.nodes:
        print(" " * level, node.name, node.type,
              str(getattr(node, "image", None)),
              str(getattr(node, "operation", "")),
              [(socket.name, str(socket.default_value)) for socket in node.inputs if not socket.is_linked and hasattr(socket, "default_value")])
        for output in node.outputs:
            for link in output.links:
                print(" " * level, "LINK", node.name, output.name, "->", link.to_node.name, link.to_socket.name)
        if node.type == "GROUP" and node.node_tree:
            dump(node.node_tree, level + 2)


def main() -> None:
    root = Path(__file__).resolve().parents[3]
    bpy.ops.wm.open_mainfile(filepath=str(root / "Art/MainRoom2/minigame员工宿舍.blend"))
    dump(bpy.data.materials["木地板014"].node_tree)


if __name__ == "__main__":
    main()
