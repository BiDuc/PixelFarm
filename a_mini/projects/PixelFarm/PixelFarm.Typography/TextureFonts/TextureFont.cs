﻿//MIT, 2016-2017, WinterDev
//----------------------------------- 
using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
using Typography.Rendering;
namespace PixelFarm.DrawingGL
{

    class TextureFontFace : FontFace
    {

        MySimpleFontAtlasBuilder atlasBuilder;
        SimpleFontAtlas fontAtlas;
        FontFace nOpenTypeFontFace;

        public TextureFontFace(FontFace nOpenTypeFontFace, string xmlFontInfo, GlyphImage glyphImg)
        {
            //for msdf font
            //1 font atlas may support mutliple font size 
            atlasBuilder = new MySimpleFontAtlasBuilder();
            fontAtlas = atlasBuilder.LoadFontInfo(xmlFontInfo);
            fontAtlas.TotalGlyph = glyphImg;
            this.nOpenTypeFontFace = nOpenTypeFontFace; 
        }
        public override float GetScale(float pointSize)
        {
            return nOpenTypeFontFace.GetScale(pointSize);
        }
        public override string FontPath
        {
            get { return nOpenTypeFontFace.Name; }
        }
        public override int AscentInDzUnit
        {
            get { return nOpenTypeFontFace.AscentInDzUnit; }
        }
        public override int DescentInDzUnit
        {
            get { return nOpenTypeFontFace.DescentInDzUnit; }
        }
        public override int LineGapInDzUnit
        {
            get { return nOpenTypeFontFace.LineGapInDzUnit; }
        }
        public FontFace InnerFontFace
        {
            get { return nOpenTypeFontFace; }
        }
        public override ActualFont GetFontAtPointsSize(float pointSize)
        {
            TextureFont t = new TextureFont(this, pointSize);
            return t;
        }
        protected override void OnDispose()
        {

        }
        public override string Name
        {
            get { return nOpenTypeFontFace.Name; }
        }
        public SimpleFontAtlas FontAtlas
        {
            get { return fontAtlas; }
        }

    }
    class TextureFont : ActualFont
    {
        SimpleFontAtlas fontAtlas;
        IDisposable glBmp;
        ActualFont actualFont;
        TextureFontFace typeface;

        internal TextureFont(TextureFontFace typeface, float sizeInPoints)
        {
            this.typeface = typeface;
            this.fontAtlas = typeface.FontAtlas;
            actualFont = typeface.InnerFontFace.GetFontAtPointsSize(sizeInPoints);

        }
        public override string FontName
        {
            get { return typeface.Name; }
        }
        public override FontStyle FontStyle
        {
            get { return Drawing.FontStyle.Regular; }
        }
        public override float AscentInPixels
        {
            get
            {
                return actualFont.AscentInPixels;
            }
        }
        public override float DescentInPixels
        {
            get
            {
                return actualFont.DescentInPixels;
            }
        }
        public IDisposable GLBmp
        {
            get { return glBmp; }
            set { glBmp = value; }
        }
        public SimpleFontAtlas FontAtlas
        {
            get { return fontAtlas; }
        }
        public override float SizeInPoints
        {
            get
            {
                return actualFont.SizeInPoints;
            }
        }

        public override float SizeInPixels
        {
            get
            {
                return actualFont.SizeInPixels;
            }
        }

        public override FontFace FontFace
        {
            get
            {
                return actualFont.FontFace;
            }
        }
        public override float LineGapInPixels
        {
            get { return actualFont.LineGapInPixels; }
        }
        public override float RecommendedLineSpacingInPixels
        {
            get { return actualFont.RecommendedLineSpacingInPixels; }
        }


        public override FontGlyph GetGlyph(char c)
        {
            return actualFont.GetGlyph(c);
        }

        public override FontGlyph GetGlyphByIndex(uint glyphIndex)
        {
            return actualFont.GetGlyphByIndex(glyphIndex);
        }
        protected override void OnDispose()
        {
            if (glBmp != null)
            {
                glBmp.Dispose();
                glBmp = null;
            }
        }

        public void AssignToRequestFont(RequestFont r)
        {
            SetCacheActualFont(r, this);
        }
        public static TextureFont GetCacheFontAsTextureFont(RequestFont r)
        {
            return GetCacheActualFont(r) as TextureFont;
        }
    }
}