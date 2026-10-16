import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/"scene"/"MainRoom.blend"))
scene=bpy.context.scene
cam=scene.camera
cam.location=(-.1,-3.9469,1.2546)
yaw=-.1784;pitch=-.05288
f=Vector((math.sin(yaw)*math.cos(pitch),math.cos(yaw)*math.cos(pitch),math.sin(pitch)))
cam.rotation_euler=f.to_track_quat("-Z","Y").to_euler();cam.data.lens=18.1924
front=bpy.data.collections["14_FrontWallAndDoor"]
front.hide_render=True
front.hide_viewport=True
front["preview_note"]="Hidden from reference camera, which sits beyond the inferred front wall. Toggle for enclosed-room inspection."
for o in bpy.data.collections["11_PendantLight"].objects:
    o.location.x+=.35;o.location.y-=.8
o=bpy.data.objects["Daily | ceiling practical"];o.location.x+=.35;o.location.y-=.8
o.rotation_euler=(Vector((-.5,-.8,0))-o.location).to_track_quat("-Z","Y").to_euler()
crt=bpy.data.objects["CRT_Monitor_Tripo"]
crt.rotation_mode="XYZ";crt.rotation_euler=(0,0,math.pi/2)
nt=bpy.data.materials["Window | fogged blue-grey glass"].node_tree
nt.nodes.clear()
e=nt.nodes.new("ShaderNodeEmission");e.inputs[0].default_value=(.07,.085,.105,1);e.inputs[1].default_value=.7
out=nt.nodes.new("ShaderNodeOutputMaterial");nt.links.new(e.outputs[0],out.inputs["Surface"])
col=bpy.data.collections["05_Computer"]
verts=[(-2.311,-1.248,.902),(-2.311,-.952,.902),(-2.311,-.952,1.106),(-2.311,-1.248,1.106)]
me=bpy.data.meshes.new("CRT phosphor surface");me.from_pydata(verts,[],[(0,1,2,3)]);me.update()
screen=bpy.data.objects.new("CRT screen emission",me);col.objects.link(screen)
me.materials.append(bpy.data.materials["CRT | desaturated green phosphor"])
uv=me.uv_layers.new()
for i,co in enumerate([(0,0),(1,0),(1,1),(0,1)]):uv.data[i].uv=co
scene["active_lighting_preset"]="daily"
scene["reference_camera_note"]="18.2mm reference matching view; front closure excluded from preview only."
for area in bpy.context.screen.areas:
    if area.type=="VIEW_3D":
        area.spaces.active.region_3d.view_perspective="CAMERA"
        area.spaces.active.shading.type="MATERIAL"
for im in list(bpy.data.images):
    if im.users==0:bpy.data.images.remove(im);continue
    if im.source=="FILE" and not im.packed_file:im.pack()
scene.render.resolution_x=1536;scene.render.resolution_y=864;scene.render.resolution_percentage=100
scene.render.filepath=str(ROOT/"previews"/"main-room-preview.png")
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/"scene"/"MainRoom.blend"))
bpy.ops.render.render(write_still=True)
maincam=scene.camera
detail=bpy.data.objects["Camera | desk detail"]
detail.location=(-.7,-2.8,1.6);detail.rotation_euler=(Vector((-2.38,-1.15,1.05))-detail.location).to_track_quat("-Z","Y").to_euler()
scene.camera=detail;scene.render.resolution_x=1024;scene.render.resolution_y=768
scene.render.filepath=str(ROOT/"previews"/"desk-detail.png");bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects["Camera | bed and window"]
scene.render.filepath=str(ROOT/"previews"/"bed-window-detail.png");bpy.ops.render.render(write_still=True)
scene.camera=maincam
print("FINAL_PREVIEWS_COMPLETE",flush=True)
