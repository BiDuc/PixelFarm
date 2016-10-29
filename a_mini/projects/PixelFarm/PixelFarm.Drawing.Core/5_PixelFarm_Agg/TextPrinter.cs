﻿//MIT, 2014-2016, WinterDev
//-----------------------------------
//use FreeType and HarfBuzz wrapper
//native dll lib
//plan?: port  them to C#  :)
//-----------------------------------

using PixelFarm.Agg;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
using PixelFarm.Drawing.Text;
namespace PixelFarm.Agg
{
    public enum Justification { Left, Center, Right }
    public enum Baseline
    {
        BoundsTop,
        BoundsCenter,
        TextCenter,
        Text,
        BoundsBottom
    }

    public class VxsTextPrinter
    {
        Drawing.RequestFont currentFont;
        Drawing.Fonts.ActualFont actualFont;
        IFonts ifonts;
        public VxsTextPrinter()
        {
            this.ifonts = null;
        }
        public Drawing.RequestFont CurrentFont
        {
            get { return this.currentFont; }
            set { this.currentFont = value; }
        }
        public Drawing.Fonts.ActualFont CurrentActualFont
        {
            get { return this.actualFont; }
            set { this.actualFont = value; }
        }
        public VertexStore CreateVxs(char[] buffer, double x = 0, double y = 0)
        {

            throw new System.NotSupportedException();
            //int j = buffer.Length;
            //int buffsize = j * 2;
            ////get kerning list 

            //ProperGlyph[] properGlyphs = new ProperGlyph[buffsize];
            //ActualFont implFont = ifonts.ResolveActualFont(currentFont);
            //implFont.GetGlyphPos(buffer, 0, buffsize, properGlyphs);
            ////-----------------------------------------------------------
            //VertexStore resultVxs = new VertexStore();
            //double xpos = x;
            //for (int i = 0; i < buffsize; ++i)
            //{
            //    uint codepoint = properGlyphs[i].codepoint;
            //    if (codepoint == 0)
            //    {
            //        break;
            //    }
            //    //-------------------------------------------------------------
            //    FontGlyph glyph = implFont.GetGlyphByIndex(codepoint);
            //    //var left = glyph.glyphMatrix.img_horiBearingX;
            //    //-------------------------------------------------------- 
            //    VertexStore vxs1 = Agg.Transform.Affine.TranslateToVxs(
            //        glyph.flattenVxs,
            //        (float)(xpos),
            //        (float)(y));
            //    //-------------------------------------------------------- 
            //    resultVxs.AddSubVertices(vxs1);
            //    int w = (glyph.glyphMatrix.advanceX) >> 6;
            //    xpos += (w);
            //    //-------------------------------------------------------------                
            //}
            //return resultVxs;
        }
        public void Print(CanvasPainter painter, string t, double x, double y)
        {
            Print(painter, t.ToCharArray(), x, y);
        }
        public void Print(CanvasPainter painter, char[] buffer, double x, double y)
        {
            int j = buffer.Length;
            int buffsize = j * 2;
            //get kerning list 

            ProperGlyph[] properGlyphs = new ProperGlyph[buffsize];
            ActualFont implFont = actualFont;
            TextShapingService.GetGlyphPos(actualFont, buffer, 0, buffsize, properGlyphs);
            double xpos = x;
            for (int i = 0; i < buffsize; ++i)
            {
                uint codepoint = properGlyphs[i].codepoint;
                if (codepoint == 0)
                {
                    break;
                }
                //-------------------------------------------------------------
                FontGlyph glyph = implFont.GetGlyphByIndex(codepoint);
                //var left = glyph.glyphMatrix.img_horiBearingX;
                //--------------------------------------------------------
                //render with vector
                //var mat = Agg.Transform.Affine.NewMatix(
                //Agg.Transform.AffinePlan.Scale(0.30),
                //Agg.Transform.AffinePlan.Translate(xpos, y));
                //var vxs1 = mat.TransformToVxs(glyph.flattenVxs); 
                VertexStore vxs1 = Agg.Transform.Affine.TranslateToVxs(
                    glyph.flattenVxs,
                    (float)(xpos),
                    (float)(y));
                painter.Fill(vxs1);
                //--------------------------------------------------------
                ////render with bitmap
                //if (glyph.glyphImage32 != null)
                //{
                //    if (glyph.glyphImage32.Width > 0 && glyph.glyphImage32.Height > 0)
                //    {
                //        painter.DrawImage(glyph.glyphImage32,
                //            (float)(xpos + (left >> 6)),
                //            (float)(y + (glyph.exportGlyph.bboxYmin >> 6)));
                //    }
                //}
                int w = (glyph.glyphMatrix.advanceX) >> 6;
                xpos += (w);
                //-------------------------------------------------------------                
            }
        }
    }
}