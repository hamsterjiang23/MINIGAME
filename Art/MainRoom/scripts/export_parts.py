"""Create independent editable Blender modules and textured GLB interchange files."""
import bpy, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/"scene"/"MainRoom.blend"))
main=bpy.context.scene
out=ROOT/"exports";out.mkdir(exist_ok=True)
report={"blender_version":bpy.app.version_string,"units":"metres","parts":[],"limits":["GLB uses ordinary UV image mappings for box-projected Blender materials.","Unity import, colliders, gameplay and target-platform performance are not verified."]}
groups=[c for c in main.collection.children if c.name[:2].isdigit() and not c.name.startswith("15_")]

def copies(collections,name,recenter):
    sc=bpy.data.scenes.new(name)
    pts=[o.matrix_world@Vector(corner) for co in collections for o in co.objects if o.type in {"MESH","CURVE"} for corner in o.bound_box]
    mn=Vector([min(p[i] for p in pts) for i in range(3)])
    mx=Vector([max(p[i] for p in pts) for i in range(3)])
    origin=Vector(((mn.x+mx.x)/2,(mn.y+mx.y)/2,mn.z)) if recenter else Vector((0,0,0))
    objs=[]
    for co in collections:
        dest=bpy.data.collections.new(co.name+" | export");sc.collection.children.link(dest)
        for old in co.objects:
            if old.type not in {"MESH","CURVE"} or old.hide_render:continue
            o=old.copy();o.data=old.data.copy();dest.objects.link(o)
            o.matrix_world=old.matrix_world.copy();o.location-=origin
            objs.append(o)
    return sc,objs,mn,mx,origin

def export(sc,objs,path):
    bpy.context.window.scene=sc
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:o.select_set(True)
    bpy.context.view_layer.objects.active=objs[0]
    bpy.ops.object.convert(target="MESH")
    objs=list(sc.objects)
    cache={}
    for o in objs:
        for slot in o.material_slots:
            old=slot.material
            if not old or not old.use_nodes:continue
            if old.name not in cache:
                m=old.copy();m.name=old.name+" | UV interchange"
                nt=m.node_tree;bs=next((n for n in nt.nodes if n.type=="BSDF_PRINCIPLED"),None)
                ims=[n for n in nt.nodes if n.type=="TEX_IMAGE"]
                if ims and bs and ("tripo" not in old.name.lower()):
                    im=ims[0];im.projection="FLAT"
                    for l in list(nt.links):
                        if l.to_node==im:nt.links.remove(l)
                    nt.links.new(im.outputs["Color"],bs.inputs["Base Color"])
                cache[old.name]=m
            slot.material=cache[old.name]
    bpy.ops.export_scene.gltf(filepath=str(path),export_format="GLB",use_selection=False,use_active_scene=True,export_cameras=False,export_lights=False,export_apply=True)
    return sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objs if o.type=="MESH")

for co in groups:
    sc,objs,mn,mx,origin=copies([co],co.name,True)
    blend=ROOT/"models"/(co.name+".blend")
    # Store an appendable collection library. Blender 5.2 scene-ID partial
    # writes can fail while copying a newly created scene's view layers.
    bpy.data.libraries.write(str(blend),set(sc.collection.children),path_remap="RELATIVE_ALL",fake_user=True,compress=True)
    path=out/(co.name+".glb")
    tris=export(sc,objs,path)
    report["parts"].append({"name":co.name,"description":co.get("description",""),"blend":str(blend.relative_to(ROOT)),"glb":str(path.relative_to(ROOT)),"triangles":tris,"dimensions_m":list(mx-mn),"origin_in_room_m":list(origin),"bytes":path.stat().st_size})
    bpy.context.window.scene=main
    for o in list(sc.objects):bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.scenes.remove(sc)
sc,objs,mn,mx,origin=copies(groups,"MainRoom_Interchange",False)
report["combined_triangles"]=export(sc,objs,out/"MainRoom.glb")
report["combined_glb_bytes"]=(out/"MainRoom.glb").stat().st_size
(ROOT/"reports"/"asset-manifest.json").write_text(json.dumps(report,ensure_ascii=False,indent=2))
print("EXPORTED",len(report["parts"]),"modules;",report["combined_triangles"],"triangles",flush=True)
