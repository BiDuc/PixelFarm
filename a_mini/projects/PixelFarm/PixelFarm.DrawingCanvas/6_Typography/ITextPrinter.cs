﻿//MIT, 2014-2017, WinterDev



namespace PixelFarm.Drawing
{

    //implement this interface to handler font measurement/ glyph layout position

    public interface IFonts
    {
        //review this name again
        float MeasureWhitespace(RequestFont f);
        float MeasureBlankLineHeight(RequestFont f);
        Size MeasureString(char[] str, int startAt, int len, RequestFont font); 
        void CalculateGlyphAdvancePos(char[] str, int startAt,
            int len, RequestFont font, int[] glyphXAdvances);
    }


    /// <summary>
    /// for printing a string to target canvas
    /// </summary>
    public interface ITextPrinter
    {

        void DrawString(char[] text, int startAt, int len, double x, double y);
        /// <summary>
        /// render from RenderVxFormattedString object to specific pos
        /// </summary>
        /// <param name="renderVx"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void DrawString(RenderVxFormattedString renderVx, double x, double y);
        //-------------
        void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len);
        void ChangeFont(RequestFont font);
        //-------------
        void ChangeFillColor(Color fillColor);
        void ChangeStrokeColor(Color strokColor);
    }

    public static class ITextPrinterExtensions
    {
        public static void DrawString(this ITextPrinter textPrinter, string text, double x, double y)
        {
            char[] textBuffer = text.ToCharArray();
            textPrinter.DrawString(textBuffer, 0, textBuffer.Length, x, y);
        }
    }

}