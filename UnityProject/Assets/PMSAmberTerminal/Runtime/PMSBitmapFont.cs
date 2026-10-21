using System;
using System.Collections.Generic;
using UnityEngine;

namespace PMSAmber
{
    /// <summary>Canvas-exported glyph pixels and top-origin metrics; no runtime font substitution.</summary>
    [CreateAssetMenu(menuName="PMS/HTML bitmap font")]
    public sealed class PMSBitmapFont : ScriptableObject
    {
        public Texture2D Atlas;
        public TextAsset Metrics;
        [Serializable] public sealed class Glyph
        { public int code,size,rgb,x,y,w,h,offsetX,offsetY,advance; }
        [Serializable] private sealed class FontData
        { public int atlasWidth,atlasHeight; public Glyph[] glyphs; }
        private Dictionary<ulong,Glyph> index;
        private TextAsset indexedMetrics;
        private readonly HashSet<ulong> missing=new HashSet<ulong>();
        public void InvalidateMetrics() { index=null; indexedMetrics=null; missing.Clear(); }
        private void OnEnable() => InvalidateMetrics();
        private void OnValidate() => InvalidateMetrics();
        private static ulong Key(int code,int size,int rgb) => ((ulong)size<<48)|((ulong)(uint)rgb<<16)|(uint)code;
        private void Load()
        {
            if(index!=null&&indexedMetrics==Metrics)return;
            InvalidateMetrics();index=new Dictionary<ulong,Glyph>();indexedMetrics=Metrics;
            if(!Metrics)return;
            var data=JsonUtility.FromJson<FontData>(Metrics.text);
            if(Atlas&&(Atlas.width!=data.atlasWidth||Atlas.height!=data.atlasHeight))Debug.LogError("PMS: font atlas was resized on import. Set Max Size to the atlas size and Non Power of 2 to None; keep Point filtering and disable mipmaps/compression.");
            foreach(var glyph in data.glyphs)index[Key(glyph.code,glyph.size,glyph.rgb)]=glyph;
        }
        public bool Get(char character,int size,Color32 color,out Glyph glyph)
        {
            Load();int rgb=(color.r<<16)|(color.g<<8)|color.b;ulong key=Key(character,size,rgb);
            if(index.TryGetValue(key,out glyph))return true;
            if(missing.Add(key))Debug.LogWarning("PMS: missing exported glyph U+"+((int)character).ToString("X4")+" at "+size+"px. Regenerate the supplied HTML glyph atlas before shipping new text.");
            return index.TryGetValue(Key('?',size,rgb),out glyph);
        }
        public bool Covers(char character,int size) {Load();return index.ContainsKey(Key(character,size,0xefb83f));}
        public Rect UV(Glyph glyph)
        {return new Rect((float)glyph.x/Atlas.width,1f-(float)(glyph.y+glyph.h)/Atlas.height,(float)glyph.w/Atlas.width,(float)glyph.h/Atlas.height);}
    }
}
