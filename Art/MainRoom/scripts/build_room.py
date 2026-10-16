"""Reference-guided, modular room. Run with Blender --background --python."""
import bpy, math, random, json, sys
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi

ROOT=Path(__file__).resolve().parents[1]
random.seed(161026)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.name="MainRoom_ReferenceReconstruction"
scene.unit_settings.system="METRIC"
scene.unit_settings.scale_length=1.0
ASSETS={}
COL=None

def collection(name,description):
    global COL
    COL=bpy.data.collections.new(name)
    scene.collection.children.link(COL)
    COL["description"]=description
    COL["source"]="Original procedural model from supplied reference / concept"
    ASSETS[name]=COL
    return COL

def link(obj,name,mat=None):
    obj.name=name
    for c in list(obj.users_collection):c.objects.unlink(obj)
    COL.objects.link(obj)
    if mat:obj.data.materials.append(mat)
    return obj

def material(name,color=(.3,.3,.3),rough=.8,metal=0,texture=None,box=False,bump=.035):
    m=bpy.data.materials.new(name);m.use_nodes=True
    m.diffuse_color=(*color,1)
    nt=m.node_tree;bs=nt.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*color,1)
    bs.inputs["Roughness"].default_value=rough
    bs.inputs["Metallic"].default_value=metal
    if texture:
        im=nt.nodes.new("ShaderNodeTexImage")
        im.image=bpy.data.images.load(str(ROOT/"textures"/texture),check_existing=True)
        im.interpolation="Linear"
        if box:
            tc=nt.nodes.new("ShaderNodeTexCoord")
            nt.links.new(tc.outputs["Generated"],im.inputs["Vector"])
            im.projection="BOX";im.projection_blend=.16
        nt.links.new(im.outputs["Color"],bs.inputs["Base Color"])
        if bump:
            b=nt.nodes.new("ShaderNodeBump");b.inputs["Strength"].default_value=.3;b.inputs["Distance"].default_value=bump
            nt.links.new(im.outputs["Color"],b.inputs["Height"]);nt.links.new(b.outputs["Normal"],bs.inputs["Normal"])
    return m

def cube(name,loc,dim,mat,bevel=.008):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=link(bpy.context.object,name,mat);o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        b=o.modifiers.new("Soft worn edges","BEVEL");b.width=bevel;b.segments=2
        b.affect="EDGES"
        o.modifiers.new("Weighted corner normals","WEIGHTED_NORMAL")
    return o

def cyl(name,a,b,r,mat,vertices=12):
    a=Vector(a);b=Vector(b);v=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=r,depth=v.length,location=(a+b)/2)
    o=link(bpy.context.object,name,mat);o.rotation_euler=v.to_track_quat("Z","Y").to_euler()
    for p in o.data.polygons:p.use_smooth=True
    return o

def curve(name,pts,r,mat):
    c=bpy.data.curves.new(name,"CURVE");c.dimensions="3D";c.resolution_u=1
    c.bevel_depth=r;c.bevel_resolution=1
    s=c.splines.new("POLY");s.points.add(len(pts)-1)
    for p,co in zip(s.points,pts):p.co=(*co,1)
    o=bpy.data.objects.new(name,c);COL.objects.link(o);o.data.materials.append(mat)
    return o

def quad(name,verts,mat,uv=((0,0),(1,0),(1,1),(0,1))):
    me=bpy.data.meshes.new(name);me.from_pydata(verts,[],[(0,1,2,3)]);me.update()
    o=bpy.data.objects.new(name,me);COL.objects.link(o);me.materials.append(mat)
    u=me.uv_layers.new(name="UVMap")
    for li,co in zip(me.polygons[0].loop_indices,uv):u.data[li].uv=co
    return o

def group_transform(objs,loc,angle=0):
    c=cos(angle);s=sin(angle)
    for o in objs:
        x,y,z=o.location
        o.location=(loc[0]+c*x-s*y,loc[1]+s*x+c*y,loc[2]+z)
        o.rotation_euler.z+=angle

def part_start():return set(bpy.data.objects)
def part_finish(start,loc,angle=0):group_transform([o for o in bpy.data.objects if o not in start],loc,angle)

wood=material("Timber | worn dark walnut",texture="wood-worn-basecolor.png",box=True,bump=.012)
wall=material("Wallpaper | faded olive stripes",texture="wallpaper-olive-basecolor.png",bump=.012)
plaster=material("Plaster | stained ceiling",texture="plaster-basecolor.png",box=True,bump=.045)
cloth=material("Linen | dingy taupe",texture="cloth-basecolor.png",box=True,bump=.009)
curtainmat=material("Curtain | coarse faded green-grey",texture="curtain-basecolor.png",bump=.006)
paint=material("Paint | aged grey-green",texture="paint-basecolor.png",box=True,bump=.008)
metal=material("Steel | dark oxidized frame",(.07,.095,.085),.73,.6)
rust=material("Rust | worn fasteners",(.17,.10,.055),.95,.25)
black=material("Rubber | cable and dark gaps",(.018,.021,.018),.9)
plastic=material("Plastic | yellowed beige",(.42,.40,.30),.7)
keymat=material("Keys | ivory worn caps",(.5,.49,.37),.78)
brass=material("Hardware | dull iron",(.22,.23,.18),.55,.7)
glass=material("Window | fogged blue-grey glass",(.15,.19,.21),.38,.15)
bs=glass.node_tree.nodes.get("Principled BSDF")
bs.inputs["Emission Color"].default_value=(.12,.16,.20,1);bs.inputs["Emission Strength"].default_value=.35
screenmat=material("CRT | desaturated green phosphor",texture="crt-screen-basecolor.png",bump=0,rough=.3)
bs=screenmat.node_tree.nodes.get("Principled BSDF")
bs.inputs["Emission Color"].default_value=(.32,.52,.40,1);bs.inputs["Emission Strength"].default_value=.7
rugmat=material("Carpet | faded geometric weave",texture="rug-woven-basecolor.png",bump=.007)
paper=material("Paper | old bulletin",texture="newspaper-basecolor.png",bump=0)
posters=[material("Poster | fictional institution "+str(i),texture=f"poster-{i}-basecolor.png",bump=0) for i in (1,2,3)]

# Architectural shell. The front is a separate removable module.
collection("01_RoomShell","Room 5.6 x 6.4 x 3.0 m; front access door is inferred beyond reference view.")
cube("Floor substrate", (0,-.2,-.05),(5.6,6.4,.1),black,0)
quad("Left wallpaper", [(-2.8,-3.4,0),(-2.8,3,0),(-2.8,3,3),(-2.8,-3.4,3)],wall,[(0,0),(9.6,0),(9.6,4.5),(0,4.5)])
quad("Right wallpaper",[(2.8,3,0),(2.8,-3.4,0),(2.8,-3.4,3),(2.8,3,3)],wall,[(0,0),(9.6,0),(9.6,4.5),(0,4.5)])
# Back wall has a real window opening x .45..1.65, z1.30..2.55.
for name,x0,x1,z0,z1 in [("Back left",-2.8,.4,0,3),("Back right",1.8,2.8,0,3),("Below window",.4,1.8,0,1.25),("Above window",.4,1.8,2.65,3)]:
    quad(name,[(x1,3,z0),(x0,3,z0),(x0,3,z1),(x1,3,z1)],wall,[(x1*1.5,z0*1.5),(x0*1.5,z0*1.5),(x0*1.5,z1*1.5),(x1*1.5,z1*1.5)])
cube("Ceiling",(0,-.2,3.04),(5.75,6.55,.08),plaster,0)
for x in [-2.78,2.78]:
    cube("Skirting side",(x,-.2,.065),(.035,6.4,.13),wood,.003)
cube("Skirting back",(0,2.98,.065),(5.6,.035,.13),wood,.003)
for x in [-2.75,2.75]:
    cube("Ceiling trim",(x,-.2,2.91),(.08,6.4,.12),plaster,.01)
cube("Ceiling trim back",(0,2.95,2.91),(5.6,.1,.12),plaster,.01)

collection("02_ParquetFloor","Individual parquet polygons with seams and varied timber tones.")
floor_mats=[]
for i in range(8):
    m=wood.copy();m.name=f"Parquet | variation {i+1}"
    nt=m.node_tree;im=next(n for n in nt.nodes if n.type=="TEX_IMAGE")
    for l in list(nt.links):
        if l.to_node==im:nt.links.remove(l)
    im.projection="FLAT"
    mix=nt.nodes.new("ShaderNodeMixRGB");mix.blend_type="MULTIPLY";mix.inputs[0].default_value=.7
    mix.inputs[2].default_value=(*([.65+i*.045]*3),1)
    nt.links.new(im.outputs["Color"],mix.inputs[1]);nt.links.new(mix.outputs[0],nt.nodes.get("Principled BSDF").inputs["Base Color"])
    floor_mats.append(m)
verts=[];faces=[];uvs=[];mis=[]
def clip(poly,axis,limit,less):
    out=[]
    for p,q in zip(poly,poly[1:]+poly[:1]):
        pin=(p[axis]<=limit) if less else (p[axis]>=limit)
        qin=(q[axis]<=limit) if less else (q[axis]>=limit)
        if pin:out.append(p)
        if pin != qin:
            t=(limit-p[axis])/(q[axis]-p[axis])
            out.append((p[0]+t*(q[0]-p[0]),p[1]+t*(q[1]-p[1])))
    return out
step=.145;n=5
for i in range(-12,13):
    for j in range(-38,39):
        x=(n*i+j)*step;y=(n*i-j)*step
        for vertical in [False,True]:
            xx=x+(n*step if vertical else 0);yy=y
            dx=step if vertical else n*step;dy=n*step if vertical else step
            p=[(xx+.0015,yy+.0015),(xx+dx-.0015,yy+.0015),(xx+dx-.0015,yy+dy-.0015),(xx+.0015,yy+dy-.0015)]
            # Rotate the herringbone lattice through 45 degrees.
            p=[((a-b)*.7071,(a+b)*.7071) for a,b in p]
            for axis,lim,less in [(0,-2.8,False),(0,2.8,True),(1,-3.4,False),(1,3,True)]:
                if p:p=clip(p,axis,lim,less)
            if len(p)<3:continue
            idx=len(verts);verts.extend((a,b,.004) for a,b in p);faces.append(tuple(range(idx,idx+len(p))))
            ox=random.random();oy=random.random()
            uvs.extend(((a-xx)/max(dx,.01)+ox,(b-yy)/max(dy,.01)+oy) for a,b in p);mis.append(random.randrange(8))
me=bpy.data.meshes.new("Herringbone parquet mesh");me.from_pydata(verts,[],faces);me.update()
o=bpy.data.objects.new("Parquet | individual boards",me);COL.objects.link(o)
for m in floor_mats:me.materials.append(m)
uv=me.uv_layers.new()
for p,mi in zip(me.polygons,mis):
    p.material_index=mi
    for li in p.loop_indices:uv.data[li].uv=uvs[me.loops[li].vertex_index]

# Desk is built in its local coordinates, front towards local -Y.
collection("03_WritingDesk","Based on generated desk concept; drawers remain separate objects.")
s=part_start()
cube("Desk top",(0,0,.755),(1.58,.65,.055),wood,.012)
cube("Desk pedestal",(-.535,.015,.37),(.46,.58,.72),wood)
cube("Desk right support",(.735,.01,.37),(.045,.58,.72),wood)
cube("Desk rear modesty panel",(.22,.255,.47),(.95,.032,.42),wood,.003)
for i in range(4):
    z=.115+i*.15
    cube(f"Drawer {i+1}",(-.535,-.298,z),(.422,.033,.141),wood,.006)
    cube(f"Drawer {i+1} handle",(-.535,-.323,z+.018),(.09,.021,.019),brass,.004)
cube("Desk shallow drawer",(.225,-.296,.66),(.96,.038,.128),wood,.006)
cube("Shallow drawer pull",(.225,-.325,.66),(.085,.025,.018),brass,.004)
part_finish(s,(-2.43,-.7,0),pi/2)

collection("04_WoodenChair","A separate worn timber chair with curved slat back.")
s=part_start()
cube("Chair seat",(0,0,.45),(.40,.40,.055),wood,.015)
for x in [-.15,.15]:
    for y in [-.15,.15]:
        cyl("Chair leg",(x*1.14,y*1.14,.015),(x,y,.43),.025,wood,8)
for x in [-.15,.15]:
    cyl("Back upright",(x,.15,.45),(x,.21,.99),.023,wood,8)
for z in [.65,.79,.94]:
    cube("Back cross slat",(0,.19,z),(.34,.035,.065),wood,.014)
for x in [-.15,.15]:cyl("Chair side stretcher",(x,-.15,.19),(x,.15,.19),.013,wood,8)
part_finish(s,(-1.58,-.72,0),pi/2+.10)

collection("05_Computer","CRT concept / Tripo mesh imported later; keyboard, mouse, system unit and wiring are individual objects.")
s=part_start()
cube("Computer base unit",(-.37,.12,.055),(.43,.37,.11),plastic,.012)
cube("Diskette slot",(-.37,-.071,.06),(.19,.006,.014),black,.001)
for x in [-.54,-.51,-.48]:cube("Case status LED",(x,-.075,.041),(.012,.006,.006),metal,.001)
# A procedural monitor is kept as a fallback until the real Tripo asset is checked.
cube("CRT fallback rear housing",(.15,.14,.30),(.40,.31,.32),plastic,.035)
cube("CRT fallback face bezel",(.15,-.035,.30),(.43,.07,.34),plastic,.025)
cube("CRT fallback dark inset",(.15,-.074,.31),(.36,.008,.262),black,.015)
quad("CRT luminous screen", [(-.018,-.080,.19),(.318,-.080,.19),(.318,-.080,.432),(-.018,-.080,.432)],screenmat)
cube("CRT fallback foot",(.15,.07,.055),(.23,.22,.043),plastic,.018)
cube("CRT fallback support",(.15,.06,.11),(.13,.12,.12),plastic,.015)
cube("Keyboard chassis",(.14,-.26,.023),(.43,.17,.038),plastic,.01)
for row in range(5):
    for col in range(15):
        cube("Keyboard key",(-.055+col*.027,-.321+row*.027,.048),(.022,.021,.012),keymat,.002)
cube("Space bar",(.13,-.335,.052),(.14,.022,.011),keymat,.002)
cube("Mouse",(.49,-.245,.027),(.07,.11,.045),plastic,.025)
curve("Mouse cord",[(.49,-.19,.03),(.54,-.06,.025),(.42,.06,.025),(.29,.08,.035)],.004,black)
curve("Keyboard cord",[(.02,-.18,.02),(-.02,-.08,.02),(-.13,.04,.02),(-.14,.18,.06)],.004,black)
part_finish(s,(-2.43,-.7,.79),pi/2)

collection("06_IronBed","Tubular frame; mattress, pillow and crumpled blanket are modeled separately.")
s=part_start()
for x in [-1.0,1.0]:
    pts=[(x,-.405,.06),(x,-.405,.83),(x,-.395,.91),(x,-.35,.95),(x,.35,.95),(x,.395,.91),(x,.405,.83),(x,.405,.06)]
    curve("Bed rounded end frame",pts,.023,metal)
    cyl("Bed end cross rail",(x,-.4,.64),(x,.4,.64),.018,metal)
    for y in [-.24,0,.24]:cyl("Bed vertical end bar",(x,y,.51),(x,y,.94),.012,metal)
for y in [-.40,.40]:
    cube("Bed side rail",(0,y,.41),(2.02,.045,.10),metal,.006)
for x in [-.95,-.6,-.2,.2,.6,.95]:
    cyl("Mattress support",(x,-.40,.44),(x,.40,.44),.012,metal)
cube("Thin mattress",(0,0,.52),(1.94,.78,.15),cloth,.052)
for y in [-.386,.386]:
    curve("Mattress stitched piping",[(-.91,y,.55),(-.45,y,.56),(0,y,.555),(.45,y,.56),(.91,y,.55)],.003,cloth)
def pillow(name,center,size):
    nx,ny=28,20;vs=[];fs=[]
    for j in range(ny+1):
        v=j/ny;yy=(v-.5)*size[1]
        for i in range(nx+1):
            u=i/nx;xx=(u-.5)*size[0]
            dome=(sin(pi*u)*sin(pi*v))**.48
            z=.018+size[2]*dome+.007*sin(u*50+v*17)*sin(pi*u)*sin(pi*v)
            vs.append((center[0]+xx,center[1]+yy,center[2]+z))
    for j in range(ny):
        for i in range(nx):
            a=j*(nx+1)+i;fs.append((a,a+1,a+nx+2,a+nx+1))
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update()
    o=bpy.data.objects.new(name,me);COL.objects.link(o);me.materials.append(cloth)
    sol=o.modifiers.new("Pillow thickness","SOLIDIFY");sol.thickness=.05
    for p in me.polygons:p.use_smooth=True
pillow("Flattened pillow",(-.68,0,.59),(.49,.59,.10))
nx,ny=65,32;vs=[];fs=[]
for j in range(ny+1):
    v=j/ny;yy=(v-.5)*.90
    for i in range(nx+1):
        u=i/nx;xx=-.35+u*1.35
        fold=.033*sin(u*21+v*13)+.018*sin(u*47-v*17)
        z=.625+fold*max(.2,sin(pi*u))
        if abs(yy)>.35:z-=((abs(yy)-.35)/.1)*.12
        xx+=.025*sin(v*13)*sin(pi*u)
        vs.append((xx,yy,z))
for j in range(ny):
    for i in range(nx):
        a=j*(nx+1)+i;fs.append((a,a+1,a+nx+2,a+nx+1))
me=bpy.data.meshes.new("Blanket folds");me.from_pydata(vs,[],fs);me.update()
o=bpy.data.objects.new("Rumpled blanket",me);COL.objects.link(o);me.materials.append(cloth)
for p in me.polygons:p.use_smooth=True
o.modifiers.new("Blanket thickness","SOLIDIFY").thickness=.008
part_finish(s,(-1.61,2.15,0),0)

collection("07_WindowAndCurtain","Four pane dirty window, wall recess, sill, one pulled-aside hanging curtain.")
for x in [.42,1.78]:
    cube("Window jamb",(x,3.015,1.95),(.085,.18,1.48),paint,.006)
for z in [1.27,2.63]:
    cube("Window lintel",(1.1,3.015,z),(1.44,.18,.085),paint,.006)
cube("Window central mullion",(1.1,2.96,1.95),(.065,.08,1.32),paint,.005)
cube("Window transom",(1.1,2.955,1.99),(1.29,.08,.065),paint,.005)
for x0,x1 in [(.47,1.065),(1.135,1.73)]:
    for z0,z1 in [(1.32,1.95),(2.03,2.58)]:
        quad("Fogged window pane",[(x0,3.04,z0),(x1,3.04,z0),(x1,3.04,z1),(x0,3.04,z1)],glass)
cube("Wide chipped sill",(1.1,2.90,1.235),(1.55,.35,.065),paint,.007)
cyl("Curtain rod",(-.05,2.73,2.72),(1.98,2.73,2.72),.016,metal)
vs=[];fs=[];nx=40;nz=40
for j in range(nz+1):
    t=j/nz;z=2.70-t*1.65
    for i in range(nx+1):
        u=i/nx
        x=-.04+u*(.48+.08*t)
        y=2.73-.045*sin(u*12*pi)-.03*t
        zz=z+(.03*sin(u*17) if j==nz else 0)
        vs.append((x,y,zz))
for j in range(nz):
    for i in range(nx):
        a=j*(nx+1)+i;fs.append((a,a+1,a+nx+2,a+nx+1))
me=bpy.data.meshes.new("Curtain pleated fabric");me.from_pydata(vs,[],fs);me.update()
o=bpy.data.objects.new("Hanging curtain",me);COL.objects.link(o);me.materials.append(curtainmat)
uv=me.uv_layers.new()
for p in me.polygons:
    p.use_smooth=True
    for li in p.loop_indices:
        vi=me.loops[li].vertex_index;uv.data[li].uv=(vi%(nx+1)/nx,vi//(nx+1)/nz)
o.modifiers.new("Fabric thickness","SOLIDIFY").thickness=.004

collection("08_RadiatorAndPipes","Twelve distinct cast iron fins, wall brackets, valve and exposed supply pipe.")
for i in range(12):
    x=.55+i*.101
    # A rounded loop gives each radiator fin an open centre.
    pts=[(x,2.79,.32),(x-.025,2.79,.33),(x-.034,2.79,.40),(x-.034,2.79,.93),(x-.023,2.79,.99),(x+.024,2.79,.99),(x+.034,2.79,.93),(x+.034,2.79,.40),(x+.024,2.79,.33),(x,2.79,.32)]
    curve("Cast iron radiator fin",pts,.027,paint)
    cyl("Radiator depth rib",(x,2.79,.42),(x,2.91,.42),.032,paint)
    cyl("Radiator depth rib",(x,2.79,.90),(x,2.91,.90),.032,paint)
for z in [.40,.90]:cyl("Radiator manifold",(.46,2.87,z),(1.75,2.87,z),.031,paint)
curve("Exposed heating pipe",[(1.75,2.87,.90),(2.42,2.87,.90),(2.48,2.87,.94),(2.48,2.87,2.93)],.019,metal)
cyl("Radiator valve",(.43,2.87,.90),(.33,2.87,.90),.04,brass)

collection("09_Rug","Original faded geometric carpet and separate ragged fringe.")
o=quad("Worn carpet",[(-1.45,-2.05,.014),(1.32,-2.05,.014),(1.32,1.30,.014),(-1.45,1.30,.014)],rugmat)
o.modifiers.new("Carpet edge","SOLIDIFY").thickness=.006
for yy in [-2.05,1.30]:
    for i in range(130):
        x=-1.43+i*.021
        curve("Carpet fringe",[(x,yy,.015),(x+random.uniform(-.005,.005),yy+(-1 if yy<0 else 1)*random.uniform(.017,.045),.009)],.0012,cloth)

collection("10_SideCabinet","Low rough wooden cabinet on the right; stacked books are separate.")
cube("Side cabinet body",(2.46,.84,.39),(.55,.58,.78),wood,.01)
cube("Side cabinet top",(2.46,.84,.80),(.60,.62,.045),wood,.009)
cube("Side cabinet door",(2.455,.535,.40),(.50,.028,.65),wood,.005)
cube("Cabinet inset panel",(2.455,.514,.40),(.40,.018,.52),wood,.004)
cube("Cabinet small handle",(2.62,.495,.49),(.025,.02,.055),brass,.004)
bookmats=[material("Book | olive cover",(.14,.19,.12)),material("Book | brown cover",(.16,.12,.08)),material("Book | grey cover",(.23,.24,.21))]
for i in range(3):
    z=.85+i*.038
    o=cube("Stacked book",(2.42,.82,z),(.36-i*.018,.23,.033),bookmats[i],.003);o.rotation_euler.z=.08*(i-1)
    cube("Book page block",(2.42,.805,z+.002),(.335-i*.018,.215,.022),cloth,.001)

collection("11_PendantLight","Blackened green enamel hanging shade, white interior and exposed cord.")
cx,cy=.0,-.05
curve("Pendant cable",[(cx,cy,3.0),(cx+.014,cy,2.85),(cx,cy,2.72)],.008,black)
vs=[];fs=[];nr=32
profile=[(.025,2.75),(.055,2.71),(.11,2.65),(.17,2.58),(.205,2.55)]
for r,z in profile:
    for j in range(nr):vs.append((cx+r*cos(j*2*pi/nr),cy+r*sin(j*2*pi/nr),z))
for k in range(len(profile)-1):
    for j in range(nr):fs.append((k*nr+j,k*nr+(j+1)%nr,(k+1)*nr+(j+1)%nr,(k+1)*nr+j))
me=bpy.data.meshes.new("Enamel shade");me.from_pydata(vs,[],fs);me.update()
o=bpy.data.objects.new("Pendant shade",me);COL.objects.link(o);me.materials.append(metal)
o.modifiers.new("Shade thickness","SOLIDIFY").thickness=.004
bulbmat=material("Bulb | pale green white",(.8,.85,.72),.3)
bs=bulbmat.node_tree.nodes.get("Principled BSDF");bs.inputs["Emission Color"].default_value=(.75,.89,.76,1);bs.inputs["Emission Strength"].default_value=4
bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=8,radius=.045,location=(cx,cy,2.56));link(bpy.context.object,"Visible lightbulb",bulbmat)

collection("12_PostersAndPaper","Original fictional graphics follow the reference's placement. Text is decorative, not final narrative.")
for name,y,z,w,h,mat in [("Large desk poster",-.5,1.67,.44,.65,posters[0]),("Narrow desk poster",-.03,1.51,.27,.47,posters[1]),("Desk bulletin",-1.0,1.58,.27,.48,posters[2]),("Small wall sticker",-2.35,1.3,.12,.20,posters[1]),("Bed notice",1.8,1.30,.25,.40,posters[2])]:
    quad(name,[(-2.783,y-w/2,z-h/2),(-2.783,y+w/2,z-h/2),(-2.783,y+w/2,z+h/2),(-2.783,y-w/2,z+h/2)],mat)
for i,(x,y,ang) in enumerate([(-1.12,-2.25,.3),(-1.77,-1.87,-.2),(.20,1.9,.4)]):
    s=part_start();quad("Loose newspaper", [(-.15,-.11,.012),(.15,-.11,.012),(.15,.11,.012),(-.15,.11,.012)],paper);part_finish(s,(x,y,0),ang)

collection("13_SmallProps","Bottles, enamel mug, storage cases, loose cans and wiring.")
green=material("Bottle | dark olive glass",(.055,.12,.065),.31,.12)
label=material("Bottle | worn paper label",(.43,.43,.28),.9)
def bottle(name,loc):
    s=part_start()
    cyl(name+" body",(0,0,.025),(0,0,.18),.042,green,16)
    cyl(name+" neck",(0,0,.18),(0,0,.29),.017,green,12)
    bpy.ops.mesh.primitive_cone_add(vertices=16,radius1=.042,radius2=.017,depth=.045,location=(0,0,.195));link(bpy.context.object,name+" shoulder",green)
    cyl(name+" label",(0,0,.07),(0,0,.13),.0426,label,16)
    cyl(name+" cap",(0,0,.287),(0,0,.30),.019,metal,12)
    part_finish(s,loc)
bottle("Desk bottle",(-2.11,-1.22,.79))
bottle("Cabinet bottle",(2.62,.96,.84))
cupmat=material("Mug | grey enamel",(.23,.28,.27),.4,.2)
cyl("Mug body",(-2.25,-.12,.80),(-2.25,-.12,.895),.043,cupmat,20)
cyl("Mug dark opening",(-2.25,-.12,.895),(-2.25,-.12,.899),.036,black,20)
curve("Mug handle",[(-2.20,-.12,.88),(-2.17,-.12,.89),(-2.15,-.12,.86),(-2.17,-.12,.82),(-2.20,-.12,.83)],.008,cupmat)
for i,(loc,dim) in enumerate([((2.53,.30,.20),(.35,.32,.40)),((2.25,-.15,.12),(.28,.25,.24)),((1.83,-.6,.08),(.22,.20,.16)),((-2.03,2.1,.15),(.5,.43,.29))]):
    cube("Storage box",loc,dim,wood,.008)
    cube("Storage box lid",(loc[0],loc[1],loc[2]+dim[2]/2), (dim[0]+.01,dim[1]+.01,.025),wood,.003)
canmat=material("Can | tarnished tin",(.22,.28,.27),.45,.65)
cyl("Discarded tin can",(-.45,1.48,.04),(-.31,1.51,.04),.035,canmat,16)
curve("Computer power cable",[(-2.72,-.5,.82),(-2.74,-.5,.4),(-2.74,-.46,.03),(-2.5,-.1,.02),(-2.7,.2,.02)],.007,black)
cube("Wall outlet",(-2.77,.21,.19),(.025,.095,.12),plastic,.008)
for z in [.16,.21]:cyl("Outlet socket",(-2.75,.21,z),(-2.74,.21,z),.01,black)

collection("14_FrontWallAndDoor","Inferred off-camera closure, removable to inspect the room. Not visible in reference.")
cube("Front left wall",(-1.85,-3.47,1.5),(1.9,.12,3),plaster,0)
cube("Front right wall",(1.45,-3.47,1.5),(2.7,.12,3),plaster,0)
cube("Front above door",(-.4,-3.47,2.65),(1.0,.12,.7),plaster,0)
cube("Entrance door",(-.4,-3.45,1.12),(.92,.06,2.24),wood,.008)
cube("Entrance frame left",(-.91,-3.40,1.15),(.07,.12,2.3),paint,.004)
cube("Entrance frame right",(.11,-3.40,1.15),(.07,.12,2.3),paint,.004)
cube("Entrance frame top",(-.4,-3.40,2.30),(1.09,.12,.07),paint,.004)
cube("Entrance handle",(-.09,-3.39,1.1),(.12,.05,.025),brass,.006)

collection("15_LightingAndCameras","Five switchable reference-discussed lighting states; artistic, not gameplay implementation.")
def light(name,typ,loc,color,power,size=1,target=None):
    d=bpy.data.lights.new(name,typ);d.color=color;d.energy=power
    if typ=="AREA":d.shape="DISK";d.size=size
    if typ=="POINT":d.shadow_soft_size=size
    o=bpy.data.objects.new(name,d);COL.objects.link(o);o.location=loc
    if target:o.rotation_euler=(Vector(target)-o.location).to_track_quat("-Z","Y").to_euler()
    return o
light("Daily | ceiling practical","AREA",(cx,cy,2.51),(.70,.82,.69),105,1.3,(0,0,0))
light("Daily | soft window","AREA",(1.05,2.82,2.05),(.47,.58,.72),95,1.25,(-1,-1,.6))
light("Daily | monitor spill","AREA",(-2.10,-.60,1.1),(.48,.76,.61),8,.4,(-1,-.6,.8))
light("Daily | photographic fill","AREA",(1.3,-2.8,2.2),(.66,.69,.60),24,3.0,(-1,1,1))
spot=light("Dream | bed spotlight","SPOT",(-1.6,2.1,2.85),(.70,.80,.83),450,.1,(-1.6,2.1,.3))
spot.data.spot_size=.58;spot.data.spot_blend=.35;spot.hide_render=True;spot.hide_viewport=True
def camera(name,loc,target,lens=23):
    d=bpy.data.cameras.new(name);d.lens=lens;d.clip_start=.03;d.clip_end=120
    o=bpy.data.objects.new(name,d);COL.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat("-Z","Y").to_euler()
    return o
cam=camera("Camera | reference composition",(2.48,-3.05,1.78),(-.55,1.25,1.34),22)
camera("Camera | desk detail",(-.55,-1.8,1.55),(-2.2,-.65,1.05),42)
camera("Camera | bed and window",(1.5,-.4,1.65),(-.6,2.45,1.15),32)
camera("Camera | overhead layout",(0,-.2,9),(0,-.2,0),35)
scene.camera=cam
world=bpy.data.worlds.new("Room ambient");scene.world=world;world.use_nodes=True
world.node_tree.nodes["Background"].inputs[0].default_value=(.11,.14,.14,1)
world.node_tree.nodes["Background"].inputs[1].default_value=.12
scene.render.engine="CYCLES";scene.cycles.samples=36;scene.cycles.use_denoising=True
scene.cycles.max_bounces=6;scene.cycles.diffuse_bounces=3
scene.render.resolution_x=1536;scene.render.resolution_y=864;scene.render.resolution_percentage=75
scene.render.image_settings.file_format="PNG"
scene.view_settings.view_transform="AgX"
scene.view_settings.exposure=.25
scene.render.film_transparent=False
scene["reference_images"]="../references/main-room-material-reference.png; ../references/main-room-style-reference.png"
scene["fidelity_notes"]="Visible layout follows supplied references. Dimensions and off-camera entrance are inferred. Original fictional paper graphics."
scene["lighting_presets"]="daily, night, blackout, bed_spot, screen_only; run scripts/set_lighting.py in this file"
for area in bpy.context.screen.areas:
    if area.type=="VIEW_3D":
        area.spaces.active.region_3d.view_perspective="CAMERA"
        area.spaces.active.shading.type="MATERIAL"
# Package source textures inside the .blend; retain sidecars for interchange.
for im in bpy.data.images:
    if im.source=="FILE":
        im.pack()
        im.filepath="//../textures/"+Path(im.filepath).name
out=ROOT/"scene"/"MainRoom.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(out))
print("SAVED_ROOM",out,flush=True)
scene.render.filepath=str(ROOT/"previews"/"main-room-preview.png")
bpy.ops.render.render(write_still=True)
print("RENDER_COMPLETE",flush=True)
