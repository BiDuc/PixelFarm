﻿//2014 Apache2, WinterDev


namespace LayoutFarm.Drawing
{
    public abstract class Canvas
    {
#if DEBUG
        public static int dbug_canvasCount = 0;
        public int debug_resetCount = 0;
        public int debug_releaseCount = 0;
        public int debug_canvas_id = 0;
#endif
        const int CANVAS_UNUSED = 1 << (1 - 1);
        const int CANVAS_DIMEN_CHANGED = 1 << (2 - 1);
        public Canvas()
        {

        }
        public abstract GraphicsPlatform Platform { get; }
        public abstract SmoothingMode SmoothingMode { get; set; }
        //---------------------------------------------------------------------
        public abstract float StrokeWidth { get; set; }
        public abstract Color FillSolidColor { get; set; }
        //states
        public abstract void Invalidate(Rect rect);
        public abstract IGraphics GetIGraphics();
        public abstract Rect InvalidateArea { get; }
        public bool IsContentReady { get; set; }
        //---------------------------------------------------------------------
        // canvas dimension, canvas origin
        public abstract int Top { get; }
        public abstract int Left { get; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract int Bottom { get; }
        public abstract int Right { get; }
        public abstract Rectangle Rect { get; }
        public abstract void OffsetCanvasOrigin(int dx, int dy);
        public abstract void OffsetCanvasOriginX(int dx);
        public abstract void OffsetCanvasOriginY(int dy);
        public abstract bool IntersectsWith(Rect clientRect);
        //---------------------------------------------------------------------
        //clip area
        public abstract bool PushClipAreaForNativeScrollableElement(Rect updateArea);
        public abstract bool PushClipArea(int width, int height, Rect updateArea);
        public abstract void DisableClipArea();
        public abstract void EnableClipArea();
        public abstract void SetClip(RectangleF clip, CombineMode combineMode);

        public abstract Rectangle CurrentClipRect { get; }
        public abstract bool PushClipArea(int x, int y, int width, int height);
        public abstract void PopClipArea();
        //---------------------------------------
        //buffer
        public abstract void ClearSurface();
        public abstract void CopyFrom(Canvas sourceCanvas, int logicalSrcX, int logicalSrcY, Rectangle destArea);
        public abstract void RenderTo(System.IntPtr destHdc, int sourceX, int sourceY, Rectangle destArea);
        //-------------------------------------------------------

        //region object
        public abstract RectangleF GetBound(Region rgn);
        public abstract void FillRegion(Region rgn);
        //---------------------------------------


        //text ,font, strings
        public const int SAME_FONT_SAME_TEXT_COLOR = 0;
        public const int SAME_FONT_DIFF_TEXT_COLOR = 1;
        public const int DIFF_FONT_SAME_TEXT_COLOR = 2;
        public const int DIFF_FONT_DIFF_TEXT_COLOR = 3;
        public abstract int EvaluateFontAndTextColor(FontInfo FontInfo, Color color);
        public abstract FontInfo CurrentFont { get; set; }
        public abstract Color CurrentTextColor { get; set; }
        public abstract float GetFontHeight(Font f);
 

        public abstract void DrawText(char[] buffer, int x, int y);
        public abstract void DrawText(char[] buffer, Rectangle logicalTextBox, int textAlignment);
        //-------------------------------------------------------
        //lines

        public abstract void DrawLine(Color c, int x1, int y1, int x2, int y2);
        public abstract void DrawLine(Color c, float x1, float y1, float x2, float y2);

        public abstract void DrawLine(Color color, Point p1, Point p2);
        public abstract void DrawLine(Color color, Point p1, Point p2, DashStyle lineDashStyle);
        public abstract void DrawLines(Color color, Point[] points);
        //-------------------------------------------------------
        //rects
        public abstract void FillRectangle(Color color, Rectangle rect);
        public abstract void FillRectangle(Color color, RectangleF rectf);
        public abstract void FillRectangle(Color color, int left, int top, int right, int bottom);
        public abstract void DrawRectangle(Color color, int left, int top, int width, int height);
        public abstract void DrawRectangle(Color color, float left, float top, float width, float height);
        public abstract void DrawRectangle(Color color, Rectangle rect);
        //------------------------------------------------------- 
        //path,  polygons,ellipse spline,contour,  
        public abstract void FillPath(GraphicsPath gfxPath, Color solidColor);


        public abstract void DrawPolygon(PointF[] points);

        public abstract void FillPolygon(PointF[] points);
        public abstract void FillEllipse(Point[] points);
        public abstract void FillEllipse(Color color, Rectangle rect);
        public abstract void FillEllipse(Color color, int x, int y, int width, int height);

        public abstract void DrawRoundRect(int x, int y, int w, int h, Size cornerSize);
        public abstract void DrawBezire(Point[] points);

        public abstract void DrawPath(GraphicsPath gfxPath);
        public abstract void DrawPath(GraphicsPath gfxPath, Color color);
        public abstract void DrawPath(GraphicsPath gfxPath, Pen pen);
        //------------------------------------------------------- 

        //images
        public abstract void DrawImage(Image image, RectangleF dest, RectangleF src);
        public abstract void DrawImage(Image image, RectangleF rect);
        public abstract void DrawImageUnScaled(Bitmap image, int x, int y);
        //---------------------------------------------------------------------------
#if DEBUG
        public abstract void dbug_DrawRuler(int x);
        public abstract void dbug_DrawCrossRect(Color color, Rectangle rect);
#endif

    }
}
