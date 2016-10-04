﻿//MIT, 2014-2016, WinterDev   

using System;
using System.Collections.Generic;
using PixelFarm.Drawing.Fonts;
namespace PixelFarm.Drawing.WinGdi
{
    //*** this class need System.Drawing  
    class WinGdiPlusFont : ActualFont
    {
        System.Drawing.Font myFont;
        System.IntPtr hFont;
        float emSize;
        float emSizeInPixels;
        float ascendInPixels;
        float descentInPixels;

        static BasicGdi32FontHelper basGdi32FontHelper = new BasicGdi32FontHelper();

        int[] charWidths;
        Win32.NativeTextWin32.FontABC[] charAbcWidths;

        FontGlyph[] fontGlyphs;
        public WinGdiPlusFont(System.Drawing.Font f)
        {
            this.myFont = f;
            this.hFont = f.ToHfont();

            this.emSize = f.SizeInPoints;
            this.emSizeInPixels = Font.ConvEmSizeInPointsToPixels(this.emSize);
            //
            //build font matrix
            basGdi32FontHelper.MeasureCharWidths(hFont, out charWidths, out charAbcWidths);

            //--------------
            //we build font glyph, this is just win32 glyph
            //
            int j = charAbcWidths.Length;
            fontGlyphs = new FontGlyph[j];
            for (int i = 0; i < j; ++i)
            {
                FontGlyph glyph = new FontGlyph();
                glyph.horiz_adv_x = charWidths[i] << 6;
                fontGlyphs[i] = glyph;
            }


        }


        public System.IntPtr ToHfont()
        {   /// <summary>
            /// Set a resource (e.g. a font) for the specified device context.
            /// WARNING: Calling Font.ToHfont() many times without releasing the font handle crashes the app.
            /// </summary>
            return this.hFont;
        }
        public override float EmSize
        {
            get { return emSize; }
        }
        public override float EmSizeInPixels
        {
            get { return emSizeInPixels; }
        }
        protected override void OnDispose()
        {
            if (myFont != null)
            {
                myFont.Dispose();
                myFont = null;
            }
        }
        public override FontGlyph GetGlyphByIndex(uint glyphIndex)
        {

            throw new NotImplementedException();
        }

        public override FontGlyph GetGlyph(char c)
        {
            //convert c to glyph index
            //temp fix 

            throw new NotImplementedException();
        }

        public override void GetGlyphPos(char[] buffer, int start, int len, ProperGlyph[] properGlyphs)
        {
            //get gyph pos
            throw new NotImplementedException();
        }
        public override float GetAdvanceForCharacter(char c)
        {
            throw new NotImplementedException();
        }

        public override float GetAdvanceForCharacter(char c, char next_c)
        {
            throw new NotImplementedException();
        }

        public System.Drawing.Font InnerFont
        {
            get { return this.myFont; }
        }

        public override FontFace FontFace
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override float AscentInPixels
        {
            get
            {
                return ascendInPixels;
            }
        }

        public override float DescentInPixels
        {
            get
            {
                return descentInPixels;
            }
        }



    }
}