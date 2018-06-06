//BSD, 2014-2018, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using PixelFarm.Drawing;
using PixelFarm.Agg.Imaging;
namespace PixelFarm.Agg
{
    public interface IBitmapSrc
    {
        int BitDepth { get; }
        int Width { get; }
        int Height { get; }
        RectInt GetBounds();
        int[] GetInt32Buffer();
        TempMemPtr GetBufferPtr();
        int GetByteBufferOffsetXY(int x, int y);
        int Stride { get; }
        int BytesBetweenPixelsInclusive { get; }
        void ReplaceBuffer(int[] newBuffer);
        Color GetPixel(int x, int y); 
    }


    /// <summary>
    /// access reader /writer to an image buffer
    /// </summary>
    public interface IBitmapBlender : IBitmapSrc
    {
        void SetPixel(int x, int y, Color color);
        PixelBlender32 GetRecieveBlender();
        void SetRecieveBlender(PixelBlender32 value);
        //-------------------------------------------------------------------------------------------
        void BlendHL(int x, int y, int x2, Color sourceColor, byte cover); //**
        void BlendVL(int x, int y1, int y2, Color sourceColor, byte cover);
        //------------------------------------------------------------------------------------------- 

        void CopyFrom(IBitmapBlender sourceImage, RectInt sourceImageRect, int destXOffset, int destYOffset); //not used
        // line stuff
        void CopyHL(int x, int y, int len, Color sourceColor);//not used
        void CopyVL(int x, int y, int len, Color sourceColor);//not used
        // color stuff
        void CopyColorHSpan(int x, int y, int len, Color[] colors, int colorIndex); //**
        void CopyColorVSpan(int x, int y, int len, Color[] colors, int colorIndex); // 
        void BlendSolidHSpan(int x, int y, int len, Color sourceColor, byte[] covers, int coversIndex);// 
        void BlendSolidVSpan(int x, int y, int len, Color sourceColor, byte[] covers, int coversIndex);// 
        void BlendColorHSpan(int x, int y, int len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll);// 
        void BlendColorVSpan(int x, int y, int len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll); //not used
    }
}
