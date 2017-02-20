﻿//MIT, 2014-2017, WinterDev 
using System;
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.OpenFont.Tables; 
using PixelFarm.Agg;
namespace PixelFarm.Drawing.Fonts
{


    class NOpenTypeFontFace : FontFace
    {
        Typeface ntypeface;
        string name, path;
        PixelFarm.Agg.GlyphPathBuilderVxs glyphPathBuilder;

        public NOpenTypeFontFace(Typeface ntypeface, string fontName, string fontPath)
        {
            this.ntypeface = ntypeface;
            this.name = fontName;
            this.path = fontPath;
            //----
            glyphPathBuilder = new PixelFarm.Agg.GlyphPathBuilderVxs(ntypeface);
        }
        public override string Name
        {
            get { return name; }
        }
        public override string FontPath
        {
            get { return path; }
        }
        protected override void OnDispose() { }
        public override ActualFont GetFontAtPointsSize(float pointSize)
        {
            NOpenTypeActualFont actualFont = new NOpenTypeActualFont(this, pointSize, FontStyle.Regular);
            return actualFont;
        }
        public Typeface Typeface { get { return this.ntypeface; } }

        internal PixelFarm.Agg.GlyphPathBuilderVxs VxsBuilder
        {
            get { return this.glyphPathBuilder; }
        }
        public override float GetScale(float pointSize)
        {
            return ntypeface.CalculateScale(pointSize);
        }
        public override int AscentInDzUnit
        {
            get { return ntypeface.Ascender; }
        }
        public override int DescentInDzUnit
        {
            get { return ntypeface.Descender; }
        }
        public override int LineGapInDzUnit
        {
            get { return ntypeface.LineGap; }
        }

    }
    class NOpenTypeActualFont : ActualFont
    {
        NOpenTypeFontFace ownerFace;
        float sizeInPoints;
        FontStyle style;
        Typeface typeFace;
        float scale;
        Dictionary<uint, VertexStore> glyphVxs = new Dictionary<uint, VertexStore>();
        public NOpenTypeActualFont(NOpenTypeFontFace ownerFace, float sizeInPoints, FontStyle style)
        {
            this.ownerFace = ownerFace;
            this.sizeInPoints = sizeInPoints;
            this.style = style;
            this.typeFace = ownerFace.Typeface;
            //calculate scale *** 
            scale = typeFace.CalculateScale(sizeInPoints);
        }
        public override float SizeInPoints
        {
            get { return this.sizeInPoints; }
        }
        public override float SizeInPixels
        {
            //font height 
            get { return sizeInPoints * scale; }
        }
        public override float AscentInPixels
        {
            get { return typeFace.Ascender * scale; }
        }
        public override float DescentInPixels
        {
            get { return typeFace.Descender * scale; }
        }
        public override float LineGapInPixels
        {
            get { return typeFace.LineGap * scale; }
        }
        public override float RecommendedLineSpacingInPixels
        {
            //from https://www.microsoft.com/typography/OTSpec/recom.htm#tad
            //sTypoAscender, sTypoDescender and sTypoLineGap
            //sTypoAscender is used to determine the optimum offset from the top of a text frame to the first baseline.
            //sTypoDescender is used to determine the optimum offset from the last baseline to the bottom of the text frame. 
            //The value of (sTypoAscender - sTypoDescender) is recommended to equal one em.
            //
            //While the OpenType specification allows for CJK (Chinese, Japanese, and Korean) fonts' sTypoDescender and sTypoAscender 
            //fields to specify metrics different from the HorizAxis.ideo and HorizAxis.idtp baselines in the 'BASE' table,
            //CJK font developers should be aware that existing applications may not read the 'BASE' table at all but simply use 
            //the sTypoDescender and sTypoAscender fields to describe the bottom and top edges of the ideographic em-box. 
            //If developers want their fonts to work correctly with such applications, 
            //they should ensure that any ideographic em-box values in the 'BASE' table describe the same bottom and top edges as the sTypoDescender and
            //sTypoAscender fields. 
            //See the sections “OpenType CJK Font Guidelines“ and ”Ideographic Em-Box“ for more details.

            //For Western fonts, the Ascender and Descender fields in Type 1 fonts' AFM files are a good source of sTypoAscender
            //and sTypoDescender, respectively. 
            //The Minion Pro font family (designed on a 1000-unit em), 
            //for example, sets sTypoAscender = 727 and sTypoDescender = -273.

            //sTypoAscender, sTypoDescender and sTypoLineGap specify the recommended line spacing for single-spaced horizontal text.
            //The baseline-to-baseline value is expressed by:
            //OS/2.sTypoAscender - OS/2.sTypoDescender + OS/2.sTypoLineGap

            //sTypoLineGap will usually be set by the font developer such that the value of the above expression is approximately 120% of the em.
            //The application can use this value as the default horizontal line spacing. 
            //The Minion Pro font family (designed on a 1000-unit em), for example, sets sTypoLineGap = 200.



            get { return (typeFace.Ascender - typeFace.Descender + typeFace.LineGap) * scale; }
        }
        public override FontFace FontFace
        {
            get { return ownerFace; }
        }
        public override string FontName
        {
            get { return typeFace.Name; }
        }
        public override FontStyle FontStyle
        {
            get { return style; }
        }


        public override FontGlyph GetGlyph(char c)
        {
            return GetGlyphByIndex((uint)typeFace.LookupIndex(c));
        }
        public override FontGlyph GetGlyphByIndex(uint glyphIndex)
        {
            //1.  
            FontGlyph fontGlyph = new FontGlyph();
            fontGlyph.flattenVxs = GetGlyphVxs(glyphIndex);
            fontGlyph.horiz_adv_x = typeFace.GetHAdvanceWidthFromGlyphIndex((int)glyphIndex);
            
            return fontGlyph;
        }
        protected override void OnDispose()
        {

        }
        public VertexStore GetGlyphVxs(uint codepoint)
        {
            VertexStore found;
            if (glyphVxs.TryGetValue(codepoint, out found))
            {
                return found;
            }
            //not found
            //then build it
            ownerFace.VxsBuilder.BuildFromGlyphIndex((ushort)codepoint, this.sizeInPoints);


            found = ownerFace.VxsBuilder.GetVxs();
            glyphVxs.Add(codepoint, found);
            return found;
        }
    }


}