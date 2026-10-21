using UnityEngine;
using UnityEngine.UI;

namespace PMSAmber
{
    [AddComponentMenu(""), RequireComponent(typeof(CanvasRenderer))]
    public sealed class PMSFixedText : MaskableGraphic
    {
        public PMSBitmapFont Glyphs;
        [TextArea] public string Content="";
        public int PixelSize=14,LinePitch=20,MaxHeight=0;
        public bool Wrap;
        public override Texture mainTexture => Glyphs && Glyphs.Atlas ? Glyphs.Atlas : Texture2D.whiteTexture;
        public static float Cell(char c,int size) => c>255?size:Mathf.Ceil(size*.6f);
        public static float Width(string text,int size)
        {float value=0;foreach(char c in text??"")value+=Cell(c,size);return value;}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();if(!Glyphs||!Glyphs.Atlas)return;
            float x=0,y=0,max=rectTransform.rect.width;
            foreach(char ch in Content??"")
            {
                if(ch=='\r')continue;
                float cell=Cell(ch,PixelSize);
                if(ch=='\n'||Wrap&&x+cell>max&&x>0){x=0;y+=LinePitch;if(ch=='\n')continue;}
                if(!Wrap&&x+cell>max)break;
                if(MaxHeight>0&&y+PixelSize>MaxHeight)break;
                PMSBitmapFont.Glyph c;
                if(Glyphs.Get(ch,PixelSize,(Color32)color,out c)&&c.w>0&&c.h>0)
                {
                    Rect uv=Glyphs.UV(c);float left=x+c.offsetX,top=-y-c.offsetY;
                    int i=mesh.currentVertCount;
                    var tint=new Color32(255,255,255,((Color32)color).a);
                    mesh.AddVert(new Vector3(left,top-c.h),tint,new Vector2(uv.xMin,uv.yMin));
                    mesh.AddVert(new Vector3(left,top),tint,new Vector2(uv.xMin,uv.yMax));
                    mesh.AddVert(new Vector3(left+c.w,top),tint,new Vector2(uv.xMax,uv.yMax));
                    mesh.AddVert(new Vector3(left+c.w,top-c.h),tint,new Vector2(uv.xMax,uv.yMin));
                    mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
                }
                x+=cell;
            }
        }
    }
}
