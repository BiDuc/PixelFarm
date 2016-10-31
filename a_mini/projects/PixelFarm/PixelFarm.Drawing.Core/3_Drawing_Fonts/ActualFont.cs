﻿//MIT, 2014-2016, WinterDev

using System;
namespace PixelFarm.Drawing.Fonts
{

    /// <summary>
    /// specific fontface + size + style
    /// </summary>
    public abstract class ActualFont : IDisposable
    {

        public abstract float SizeInPoints { get; }
        public abstract float SizeInPixels { get; } 
        public void Dispose()
        {
            OnDispose();
        }
#if DEBUG
        static int dbugTotalId = 0;
        public readonly int dbugId = dbugTotalId++;
        public ActualFont()
        {

        }
#endif
        protected abstract void OnDispose();
        //---------------------
        public abstract FontGlyph GetGlyphByIndex(uint glyphIndex);
        public abstract FontGlyph GetGlyph(char c);
        public abstract FontFace FontFace { get; }
        public abstract FontStyle FontStyle { get; }
        public abstract string FontName { get; }
       
        public abstract float GetAdvanceForCharacter(char c);
        public abstract float GetAdvanceForCharacter(char c, char next_c);
        public abstract float AscentInPixels { get; }
        public abstract float DescentInPixels { get; }

        ~ActualFont()
        {
            Dispose();
        }
        //---------------------
    }



}