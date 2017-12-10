﻿//MIT, 2014-2017, WinterDev


namespace PixelFarm.Drawing
{
    public abstract class Canvas
    {

#if DEBUG
        public static int dbug_canvasCount = 0;
        public int debug_resetCount = 0;
        public int debug_releaseCount = 0;
        public int debug_canvas_id = 0;
        public abstract void dbug_DrawRuler(int x);
        public abstract void dbug_DrawCrossRect(Color color, Rectangle rect);
#endif

        public abstract void CloseCanvas();

        ////////////////////////////////////////////////////////////////////////////
        //drawing properties
        public abstract SmoothingMode SmoothingMode { get; set; }
        public abstract float StrokeWidth { get; set; }
        public abstract Color StrokeColor { get; set; }


        ////////////////////////////////////////////////////////////////////////////
        //states
        public abstract void ResetInvalidateArea();
        public abstract void Invalidate(Rectangle rect);
        public abstract Rectangle InvalidateArea { get; }


        ////////////////////////////////////////////////////////////////////////////
        // canvas dimension, canvas origin
        public abstract int Top { get; }
        public abstract int Left { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract int Bottom { get; }
        public abstract int Right { get; }

        public abstract Rectangle Rect { get; }

        public abstract int CanvasOriginX { get; }
        public abstract int CanvasOriginY { get; }
        public abstract void SetCanvasOrigin(int x, int y);
        public void OffsetCanvasOrigin(int dx, int dy)
        {
            this.SetCanvasOrigin(this.CanvasOriginX + dx, this.CanvasOriginY + dy);
        }
        public void OffsetCanvasOriginX(int dx)
        {
            this.OffsetCanvasOrigin(dx, 0);
        }
        public void OffsetCanvasOriginY(int dy)
        {
            this.OffsetCanvasOrigin(0, dy);
        }
        //---------------------------------------------------------------------
        //clip area
        public abstract bool PushClipAreaRect(int width, int height, ref Rectangle updateArea);
        public abstract void PopClipAreaRect();
        public abstract void SetClipRect(Rectangle clip, CombineMode combineMode = CombineMode.Replace);
        public abstract Rectangle CurrentClipRect { get; }
        //------------------------------------------------------
        //buffer
        public abstract void Clear(Color c);
        public abstract void RenderTo(System.IntPtr destHdc, int sourceX, int sourceY, Rectangle destArea);
        //------------------------------------------------------- 

        //lines         
        public abstract void DrawLine(float x1, float y1, float x2, float y2);
        //-------------------------------------------------------
        //rects 
        public abstract void FillRectangle(Color color, float left, float top, float width, float height);
        public abstract void FillRectangle(Brush brush, float left, float top, float width, float height);
        public abstract void DrawRectangle(Color color, float left, float top, float width, float height);
        //------------------------------------------------------- 
        //path,  polygons,ellipse spline,contour,   
        public abstract void FillPath(Color color, GraphicsPath gfxPath);
        public abstract void FillPath(Brush brush, GraphicsPath gfxPath);
        public abstract void DrawPath(GraphicsPath gfxPath);
        public abstract void FillPolygon(Brush brush, PointF[] points);
        public abstract void FillPolygon(Color color, PointF[] points);
        //-------------------------------------------------------  
        //images
        public abstract void DrawImage(Image image, RectangleF dest, RectangleF src);
        public abstract void DrawImage(Image image, RectangleF dest);
        public abstract void DrawImages(Image image, RectangleF[] destAndSrcPairs);
        //---------------------------------------------------------------------------
        //text ,font, strings 
        //TODO: review these funcs
        public abstract RequestFont CurrentFont { get; set; }
        public abstract Color CurrentTextColor { get; set; }
        public abstract void DrawText(char[] buffer, int x, int y);
        public abstract void DrawText(char[] buffer, Rectangle logicalTextBox, int textAlignment);
        public abstract void DrawText(char[] buffer, int startAt, int len, Rectangle logicalTextBox, int textAlignment);
        //-------------------------------------------------------
    }

    public enum SmoothingMode
    {
        AntiAlias = 4,
        Default = 0,
        HighQuality = 2,
        HighSpeed = 1,
        Invalid = -1,
        None = 3
    }
    public enum CanvasOrientation
    {
        LeftTop,
        LeftBottom,
    }
    public enum CanvasBackEnd
    {
        Software,
        Hardware,
        HardwareWithSoftwareFallback
    }
    public delegate void CanvasInvalidateDelegate(Rectangle paintArea);
}
