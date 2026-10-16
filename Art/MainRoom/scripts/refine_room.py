import bpy, math, json, shutil
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/"scene"/"MainRoom.blend"))
scene=bpy.context.scene
# Reload the authored sidecars, then pack them again.
for im in bpy.data.images:
    p=ROOT/"textures"/Path(im.filepath).name
    if p.exists():
        if im.packed_file:im.unpack(method="REMOVE")
        im.filepath=str(p);im.reload()
for o in bpy.data.collections["01_RoomShell"].objects:
    if o.type=="MESH" and o.data.uv_layers and ("wallpaper" in o.name or o.name.startswith(("Back ","Below window","Above window"))):
        for d in o.data.uv_layers.active.data:d.uv/=3
for name in ["Wallpaper | faded olive stripes","Plaster | stained ceiling","Paint | aged grey-green"]:
    m=bpy.data.materials[name]
    for n in m.node_tree.nodes:
        if n.type=="BUMP":n.inputs["Distance"].default_value=.003
# Tame the ceiling colour without baking directional room lighting into it.
m=bpy.data.materials["Plaster | stained ceiling"];nt=m.node_tree
bs=nt.nodes.get("Principled BSDF");im=next(n for n in nt.nodes if n.type=="TEX_IMAGE")
mix=nt.nodes.new("ShaderNodeMixRGB");mix.blend_type="MULTIPLY";mix.inputs[0].default_value=1
mix.inputs[2].default_value=(.30,.33,.30,1)
nt.links.new(im.outputs["Color"],mix.inputs[1]);nt.links.new(mix.outputs[0],bs.inputs["Base Color"])
for cn in ["03_WritingDesk","04_WoodenChair","05_Computer"]:
    for o in bpy.data.collections[cn].objects:o.location.y-=.55
for o in bpy.data.collections["12_PostersAndPaper"].objects:
    if o.name.startswith(("Large desk","Narrow desk","Desk bulletin")):o.location.y-=.55
for o in bpy.data.collections["13_SmallProps"].objects:
    if o.name.startswith(("Desk bottle","Mug","Computer power")):o.location.y-=.55
# Better frame the full carpet and keep it out of the foreground.
for o in bpy.data.collections["09_Rug"].objects:
    if o.type=="MESH":
        for v in o.data.vertices:
            v.co.x*=.93;v.co.y=v.co.y*.80+.52
    elif o.type=="CURVE":
        for s in o.data.splines:
            for p in s.points:p.co.x*=.93;p.co.y=p.co.y*.80+.52
glass=bpy.data.materials["Window | fogged blue-grey glass"].node_tree.nodes.get("Principled BSDF")
glass.inputs["Roughness"].default_value=.96;glass.inputs["Metallic"].default_value=0
glass.inputs["Emission Strength"].default_value=.18
for o in scene.objects:
    if o.type=="LIGHT" and o.name.startswith("Daily"):
        o.data.energy*=.53
        if "photographic fill" in o.name:o.data.energy*=.4
scene.world.node_tree.nodes["Background"].inputs[1].default_value=.07
scene.view_settings.exposure=.05
cam=scene.camera;cam.location=(2.48,-3.12,1.70)
cam.rotation_euler=(Vector((-.95,1.20,1.30))-cam.location).to_track_quat("-Z","Y").to_euler()
cam.data.lens=23
for o in list(bpy.data.collections["05_Computer"].objects):
    if o.name.startswith("CRT fallback") or o.name=="CRT luminous screen":bpy.data.objects.remove(o,do_unlink=True)
with bpy.data.libraries.load(str(ROOT/"models"/"CRT-Monitor.blend"),link=False) as (src,dst):
    dst.objects=[n for n in src.objects if n.startswith("CRT_Monitor")]
crt=dst.objects[0];bpy.data.collections["05_Computer"].objects.link(crt)
crt.location=(-2.51,-1.10,.79);crt.rotation_euler.z=math.pi/2
crt["dimensions_m"]="0.405 x 0.380 x 0.365"
for m in crt.data.materials:
    if m and m.use_nodes:
        # Generated normal detail is not suitable for extreme decimation.
        # Preserve the albedo and coarse normal but reduce micro-normal strength.
        for n in m.node_tree.nodes:
            if n.type=="NORMAL_MAP":n.inputs["Strength"].default_value=.35
# Save with all image data packed and correctly named sidecars.
for im in list(bpy.data.images):
    if im.source=="FILE":
        fname=Path(im.filepath).stem+".png"
        if "tripo" in im.name.lower():
            src=Path(bpy.path.abspath(im.filepath))
            if not src.exists():src=ROOT/"textures"/("tripo-crt-"+im.name)
            dst=ROOT/"textures"/src.with_suffix(".png").name
            if src.resolve()!=dst.resolve():shutil.copyfile(src,dst)
            replacement=bpy.data.images.load(str(dst),check_existing=True)
            replacement.colorspace_settings.name=im.colorspace_settings.name
            im.user_remap(replacement)
            replacement.pack();replacement.filepath="//../textures/"+dst.name
            continue
        im.pack();im.filepath="//../textures/"+fname
scene.render.resolution_percentage=100;scene.cycles.samples=48
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/"scene"/"MainRoom.blend"))
scene.render.filepath=str(ROOT/"previews"/"main-room-preview.png");bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects["Camera | desk detail"]
scene.camera.location=(-.7,-2.8,1.6)
scene.camera.rotation_euler=(Vector((-2.38,-1.15,1.05))-scene.camera.location).to_track_quat("-Z","Y").to_euler()
scene.render.resolution_x=1024;scene.render.resolution_y=768
scene.render.filepath=str(ROOT/"previews"/"desk-detail.png");bpy.ops.render.render(write_still=True)
print("REFINEMENT_AND_CRT_IMPORT_COMPLETE",flush=True)
