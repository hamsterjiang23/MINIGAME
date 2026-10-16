import bpy
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/"scene"/"MainRoom.blend"))
scene=bpy.context.scene;scene.render.engine="CYCLES";scene.cycles.samples=48;scene.cycles.use_denoising=True
maincam=bpy.data.objects["Camera | reference composition"];scene.camera=maincam
scene.render.resolution_x=1536;scene.render.resolution_y=864;scene.render.resolution_percentage=100
scene.render.filepath=str(ROOT/"previews"/"main-room-preview.png");bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects["Camera | desk detail"]
scene.camera.location=(-.7,-2.8,1.6);scene.camera.rotation_euler=(Vector((-2.38,-1.15,1.05))-scene.camera.location).to_track_quat("-Z","Y").to_euler()
scene.render.resolution_x=1024;scene.render.resolution_y=768
scene.render.filepath=str(ROOT/"previews"/"desk-detail.png");bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects["Camera | bed and window"]
scene.render.filepath=str(ROOT/"previews"/"bed-window-detail.png");bpy.ops.render.render(write_still=True)
print("ALL_PREVIEWS_RENDERED",flush=True)
