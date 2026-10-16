"""Run in Blender's Text Editor after opening MainRoom.blend.
Change PRESET to daily, night, blackout, bed_spot, or screen_only.
These are art-preview presets, not an implementation of game progression.
"""
import bpy
PRESET="daily"

def set_lighting(preset):
    if preset not in {"daily","night","blackout","bed_spot","screen_only"}:raise ValueError(preset)
    scene=bpy.context.scene
    for o in scene.objects:
        if o.type!="LIGHT":continue
        visible=False
        if preset=="daily":visible=o.name.startswith("Daily")
        elif preset=="night":visible=o.name.startswith("Daily") and "window" not in o.name
        elif preset=="bed_spot":visible=o.name.startswith("Dream")
        elif preset=="screen_only":visible="monitor spill" in o.name
        o.hide_render=not visible
        o.hide_viewport=not visible
    scene.world.node_tree.nodes["Background"].inputs[1].default_value=.07 if preset=="daily" else (.02 if preset=="night" else 0)
    glass=bpy.data.materials.get("Window | fogged blue-grey glass")
    if glass and glass.use_nodes:
        for node in glass.node_tree.nodes:
            if node.type=="EMISSION":node.inputs[1].default_value=.7 if preset=="daily" else 0
    for m in bpy.data.materials:
        if "Bulb |" in m.name and m.use_nodes:
            m.node_tree.nodes.get("Principled BSDF").inputs["Emission Strength"].default_value=4 if preset in {"daily","night"} else 0
    screen=bpy.data.objects.get("CRT screen emission")
    if screen:
        screen.hide_render=preset in {"blackout","bed_spot"}
        screen.hide_viewport=screen.hide_render
    scene["active_lighting_preset"]=preset

if __name__=="__main__":set_lighting(PRESET)
