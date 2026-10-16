"""Inspect the saved Blender scene and reimport the actual combined GLB."""
import bpy, json, math, hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/"scene"/"MainRoom.blend"))
sc=bpy.context.scene
report={"blender_version":bpy.app.version_string,"scene":sc.name,"unit_system":sc.unit_settings.system,"unit_scale":sc.unit_settings.scale_length,"objects":len(sc.objects),"collections":[],"images":[],"errors":[],"previews":[str(p.relative_to(ROOT)) for p in (ROOT/"previews").glob("*.png")],"scope":"Art asset generation, Blender rendering and GLB roundtrip only; no Unity/gameplay verification."}
for c in sc.collection.children:
    report["collections"].append({"name":c.name,"objects":len(c.objects),"description":c.get("description","")})
for im in bpy.data.images:
    if im.source!="FILE" or im.users==0:continue
    exists=Path(bpy.path.abspath(im.filepath)).exists()
    packed=bool(im.packed_file)
    report["images"].append({"name":im.name,"path":im.filepath,"packed":packed,"sidecar_exists":exists,"size":list(im.size)})
    if not (packed or exists):report["errors"].append("Missing image: "+im.name)
    if min(im.size)<=0:report["errors"].append("Empty image: "+im.name)
for o in sc.objects:
    if not all(math.isfinite(v) for row in o.matrix_world for v in row):report["errors"].append("Nonfinite transform: "+o.name)
report["lighting_presets"]=["daily","night","blackout","bed_spot","screen_only"]
before_file=str(bpy.data.filepath)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(ROOT/"exports"/"MainRoom.glb"))
meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
report["glb_roundtrip"]={"objects":len(bpy.context.scene.objects),"meshes":len(meshes),"polygons":sum(len(o.data.polygons) for o in meshes),"materials":len(bpy.data.materials),"images":len(bpy.data.images)}
if not meshes:report["errors"].append("GLB contains no mesh")
manifest=json.loads((ROOT/"reports"/"asset-manifest.json").read_text())
expected=manifest["combined_triangles"]
if report["glb_roundtrip"]["polygons"]!=expected:report["errors"].append("GLB triangle count does not match export manifest")
report["passed"]=not report["errors"]
(ROOT/"reports"/"scene-validation.json").write_text(json.dumps(report,ensure_ascii=False,indent=2))
print(json.dumps({"passed":report["passed"],"objects":report["objects"],"images":len(report["images"]),"glb":report["glb_roundtrip"],"errors":report["errors"]}),flush=True)
