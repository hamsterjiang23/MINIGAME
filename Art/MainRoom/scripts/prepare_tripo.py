"""Keep the downloaded original; create a manageable, textured Blender asset."""
import bpy, json, math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(ROOT/"models"/"crt-monitor-tripo-original.glb"))
meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
report={"source":"https://studio.tripo3d.ai/workspace/generate/42a8bc2c-ded5-4055-a2d0-b0b5664b3865","objects":[]}
for o in meshes:
    bpy.ops.object.select_all(action="DESELECT");o.select_set(True);bpy.context.view_layer.objects.active=o
    before=len(o.data.polygons)
    if before>24000:
        m=o.modifiers.new("Scene mesh reduction","DECIMATE");m.ratio=20000/before;m.use_collapse_triangulate=True
        bpy.ops.object.modifier_apply(modifier=m.name)
    o.name="CRT_Monitor_Tripo"
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    points=[o.matrix_world@Vector(c) for c in o.bound_box]
    mn=[min(p[i] for p in points) for i in range(3)];mx=[max(p[i] for p in points) for i in range(3)]
    factor=.405/(mx[0]-mn[0])
    for v in o.data.vertices:
        v.co.x=(v.co.x-(mn[0]+mx[0])/2)*factor
        v.co.y=(v.co.y-(mn[1]+mx[1])/2)*factor
        v.co.z=(v.co.z-mn[2])*factor
    o.location=(0,0,0)
    o["source"]="Tripo H3.1 from generated CRT concept; original retained"
    report["objects"].append({"name":o.name,"triangles_before":before,"polygons_after":len(o.data.polygons),"original_bbox":[mn,mx],"dimensions_m":[(mx[i]-mn[i])*factor for i in range(3)]})
for im in bpy.data.images:
    if im.source=="FILE":
        name="tripo-crt-"+im.name.replace("/","-")
        if not name.lower().endswith((".png",".jpg",".jpeg")):name+=".png"
        im.filepath_raw=str(ROOT/"textures"/name);im.file_format="PNG";im.save();im.pack()
for m in bpy.data.materials:
    if m.use_nodes:
        for n in m.node_tree.nodes:
            if n.type=="BSDF_PRINCIPLED":n.inputs["Roughness"].default_value=.7
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/"models"/"CRT-Monitor.blend"))
bpy.ops.object.select_all(action="DESELECT")
for o in meshes:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(ROOT/"models"/"CRT-Monitor.glb"),export_format="GLB",use_selection=True,export_yup=True)
(ROOT/"reports"/"tripo-crt-validation.json").write_text(json.dumps(report,ensure_ascii=False,indent=2))
print(json.dumps(report),flush=True)
