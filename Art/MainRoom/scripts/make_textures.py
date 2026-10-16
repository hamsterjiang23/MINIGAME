"""Original, deterministic surface textures for the reference-guided room."""
from pathlib import Path
import random
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "textures"
OUT.mkdir(exist_ok=True)
rng = np.random.default_rng(161026)
random.seed(161026)

def noise(w, h, scales=(8, 24, 80, 220)):
    a = np.zeros((h,w), dtype=np.float32)
    for k, s in enumerate(scales):
        small = Image.fromarray(rng.integers(0,256,(max(2,h//s),max(2,w//s)),dtype=np.uint8))
        a += (np.asarray(small.resize((w,h),Image.Resampling.BICUBIC),dtype=float)/255-.5)/(k+1)
    return a

def weather(im, strength=.34):
    a=np.asarray(im).astype(float)
    h,w=a.shape[:2]
    n=noise(w,h)
    a *= np.clip(1+strength*n, .38,1.35)[...,None]
    a += rng.normal(0,2.0,(h,w,1))
    return Image.fromarray(np.uint8(np.clip(a,0,255)))

def save(im,name):
    im.save(OUT/name)

# Wallpaper: a repeating period stripe and tiny stylized floral diamond.
w=h=1024
im=Image.new("RGB",(w,h),(126,133,119)); d=ImageDraw.Draw(im)
for x in range(0,w,128):
    d.rectangle((x,0,x+11,h),fill=(114,124,108))
    d.line((x+17,0,x+17,h),fill=(155,155,128),width=2)
    d.line((x+109,0,x+109,h),fill=(151,151,126),width=2)
    for y in range(-30,h,128):
        xx=x+67; yy=y+40
        c=(116,123,97)
        d.polygon([(xx,yy-19),(xx+6,yy-5),(xx+17,yy),(xx+6,yy+5),(xx,yy+19),(xx-6,yy+5),(xx-17,yy),(xx-6,yy-5)],fill=c)
        d.line((xx,yy-27,xx,yy+27),fill=c,width=2)
for _ in range(230):
    x=random.randrange(w);y=random.randrange(h)
    d.line([(x,y),(x+random.randrange(-8,8),y+random.randrange(5,50))], fill=(100,109,90),width=1)
save(weather(im,.23),"wallpaper-olive-basecolor.png")

# Directional old timber, coarse enough for a retro first-person environment.
w,h=1024,512
x,y=np.meshgrid(np.arange(w),np.arange(h))
grain=np.sin(y*.32+7*noise(w,h,(70,220)))+ .45*np.sin(y*1.27+3*np.sin(x*.006))
n=noise(w,h)
a=np.array([89,72,51])[None,None,:]+grain[...,None]*np.array([5,4,3])+n[...,None]*36
im=Image.fromarray(np.uint8(np.clip(a,0,255)));d=ImageDraw.Draw(im)
for _ in range(160):
    xx=random.randrange(w);yy=random.randrange(h)
    d.line((xx,yy,min(w,xx+random.randrange(12,240)),yy+random.choice([-1,0,1])), fill=random.choice([(125,110,85),(52,45,34),(101,87,63)]),width=1)
save(weather(im,.18),"wood-worn-basecolor.png")

# Worn plaster and unglazed woven cloth.
for name,base in [("plaster",(109,112,101)),("cloth",(140,135,111)),("curtain",(122,126,113)),("paper",(178,172,142)),("paint",(116,125,117))]:
    n=noise(1024,1024)
    a=np.array(base)[None,None,:]*(1+n[...,None]*.55)
    if name in ("cloth","curtain"):
        yy,xx=np.indices((1024,1024))
        a+=(((xx%4==0)|(yy%4==0))*-8)[...,None]
    save(Image.fromarray(np.uint8(np.clip(a,0,255))),name+"-basecolor.png")

# Rug: original geometric borders, worn medallion and small repeating knots.
w,h=1024,1536
im=Image.new("RGB",(w,h),(79,91,88));d=ImageDraw.Draw(im)
palette=[(118,91,76),(133,129,104),(62,81,80),(99,91,75)]
for inset,col,width in [(15,(111,105,86),10),(34,(54,71,71),10),(58,(125,101,82),37),(100,(139,129,102),8),(121,(55,75,75),12),(144,(119,102,83),7)]:
    d.rectangle((inset,inset,w-inset,h-inset),outline=col,width=width)
def diamond(cx,cy,rx,ry,c,outline=None):
    d.polygon([(cx,cy-ry),(cx+rx,cy),(cx,cy+ry),(cx-rx,cy)],fill=c,outline=outline)
for yy in range(190,h-160,80):
    for xx in range(180,w-160,80):
        diamond(xx,yy,11,17,palette[(xx//80+yy//80)%4])
        d.line((xx-18,yy,xx+18,yy),fill=(130,125,100),width=2)
for xx in range(80,w-60,70):
    for yy in [78,h-78]:
        diamond(xx,yy,21,29,(73,87,81))
        diamond(xx,yy,8,12,(142,126,96))
for yy in range(150,h-110,70):
    for xx in [78,w-78]:
        diamond(xx,yy,29,21,(73,87,81))
        diamond(xx,yy,12,8,(142,126,96))
for rx,ry,c in [(236,414,(126,101,82)),(206,377,(68,84,80)),(167,318,(143,128,100)),(147,292,(105,82,67)),(110,234,(74,89,84)),(78,152,(144,130,99)),(47,89,(103,82,65))]:
    diamond(w//2,h//2,rx,ry,c)
for yy in [330,h-330]:
    diamond(w//2,yy,50,78,(134,117,91))
    diamond(w//2,yy,25,48,(69,83,79))
im=weather(im,.42)
a=np.asarray(im).astype(float);n=noise(w,h,(100,240,400))
wear=np.clip((n+.13)*.75,0,.53)
a=a*(1-wear[...,None])+np.array([133,133,114])*wear[...,None]
save(Image.fromarray(np.uint8(np.clip(a,0,255))),"rug-woven-basecolor.png")

# Fictional reference-guided paper graphics; no copied slogans or political symbols.
fontpath="/System/Library/Fonts/Supplemental/Courier New Bold.ttf"
def font(sz):
    try:return ImageFont.truetype(fontpath,sz)
    except OSError:return ImageFont.load_default()
for idx in range(3):
    im=Image.new("RGB",(512,768),(164,159,128));d=ImageDraw.Draw(im)
    d.rectangle((14,14,498,754),outline=(43,49,43),width=7)
    if idx==0:
        d.rectangle((28,28,484,590),fill=(44,56,53))
        for j in range(5):
            bx=40+j*89;by=160+random.randrange(80)
            d.rectangle((bx,by,bx+70,570),fill=(112,126,114))
            for yy in range(by+16,560,28):
                for xx in range(bx+10,bx+65,17):
                    d.rectangle((xx,yy,xx+7,yy+12),fill=(47,61,57))
        d.text((41,612),"CARE / ORDER",font=font(47),fill=(42,50,45))
        d.text((46,678),"PUBLIC HEALTH",font=font(30),fill=(42,50,45))
    elif idx==1:
        d.rectangle((28,28,484,740),fill=(48,59,56))
        d.ellipse((145,155,355,365),fill=(145,147,121))
        d.polygon([(250,340),(110,570),(395,570)],fill=(132,139,120))
        d.rectangle((70,610,444,698),fill=(159,157,127))
        d.text((82,632),"KEEP ROUTINE",font=font(39),fill=(42,50,45))
        d.text((70,66),"REGULAR REVIEW",font=font(36),fill=(166,164,136))
    else:
        d.text((45,40),"DAILY NOTICE",font=font(44),fill=(49,54,45))
        for yy in range(120,720,13):
            for xx in [45,262]:
                d.line((xx,yy,xx+random.randrange(120,191),yy),fill=(83,86,69),width=random.choice([1,2,2,3]))
    save(weather(im,.4),f"poster-{idx+1}-basecolor.png")

im=Image.new("RGB",(512,512),(167,161,131));d=ImageDraw.Draw(im)
d.text((22,20),"BULLETIN",font=font(43),fill=(67,70,57))
for xx in (25,185,345):
    for yy in range(90,490,10):
        d.line((xx,yy,min(xx+random.randrange(80,140),499),yy),fill=(94,94,73),width=2)
save(weather(im,.3),"newspaper-basecolor.png")
im=Image.new("RGB",(512,384),(132,157,139));d=ImageDraw.Draw(im)
for y in range(0,384,3):d.line((0,y,512,y),fill=(115,144,126))
d.rectangle((15,15,497,369),outline=(98,126,109),width=2)
save(im,"crt-screen-basecolor.png")
print("Created",len(list(OUT.glob("*.png"))),"original textures in",OUT)
