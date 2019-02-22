﻿//MIT, 2014-present, WinterDev


namespace PixelFarm.Drawing
{

    public abstract class DrawBoard : System.IDisposable
    {

        //------------------------------
        //this class provides canvas interface for drawing
        //with 'screen' coordinate system
        //y axis points down
        //x axis points to the right
        //(0,0) is on left-upper corner
        //-------------------------------
        //who implement this class
        //1. PixelFarm.Drawing.WinGdi.GdiPlusDrawBoard (for win32,legacy) 
        //2. PixelFarm.Drawing.GLES2.MyGLDrawBoard  (for GLES2)

        //------------------------------
        //who use this interface
        //the HtmlRenderer
        //------------------------------

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

        public abstract int OriginX { get; }
        public abstract int OriginY { get; }
        public abstract void SetCanvasOrigin(int x, int y);

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
        public virtual void RenderTo(Image destImg, int srcX, int srcYy, int srcW, int srcH) { }
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
        public abstract void DrawImage(Image image, int x, int y);//draw image unscaled at specific pos
        //---------------------------------------------------------------------------
        //text ,font, strings 
        //TODO: review these funcs
        public abstract RequestFont CurrentFont { get; set; }
        public abstract Color CurrentTextColor { get; set; }
        public abstract void DrawText(char[] buffer, int x, int y);
        public abstract void DrawText(char[] buffer, Rectangle logicalTextBox, int textAlignment);
        public abstract void DrawText(char[] buffer, int startAt, int len, Rectangle logicalTextBox, int textAlignment);
        public abstract void MeasureString(char[] buffer, Rectangle logicalTextBox, out int width, out int height);
        //-------------------------------------------------------
        /// <summary>
        /// create formatted string base on current font,font-size, font style
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public abstract RenderVxFormattedString CreateFormattedString(char[] buffer, int startAt, int len);
        public abstract void DrawRenderVx(RenderVx renderVx, float x, float y);
        public abstract void Dispose();
        //--
        public abstract Painter GetPainter();
        /// <summary>
        /// get software rendering surface drawboard
        /// </summary>
        /// <returns></returns>
        public abstract DrawBoard GetCpuBlitDrawBoard();
        public abstract Backbuffer CreateBackbuffer(int w, int h);
        public abstract void AttachToBackBuffer(Backbuffer backbuffer);
        public abstract void SwitchBackToDefaultBuffer(Backbuffer backbuffer);
        public abstract bool IsGpuDrawBoard { get; }
        public abstract void BlitFrom(DrawBoard src, float srcX, float srcY, float srcW, float srcH, float dstX, float dstY);
        public abstract BitmapBufferProvider GetInternalBitmapProvider();
    }

    public abstract class Backbuffer
    {
        public abstract Image GetImage();
        public bool IsValid { get; set; }
    }



    public enum BitmapBufferFormat
    {
        BGRA, //eg. System.Drawing.Bitmap
        BGR, //eg. Native Windows GDI surface
        RGBA //eg. OpenGL 
    }


    public abstract class BitmapBufferProvider : Image
    {
        public abstract System.IntPtr GetRawBufferHead();
        public abstract void ReleaseBufferHead();

        public abstract bool IsYFlipped { get; }

        /// <summary>
        /// notify the bitmap provider that it can release the local bmp (eg. we have use it, not need anymore)
        /// </summary>
        public abstract void ReleaseLocalBitmapIfRequired();
        public abstract void NotifyUsage();
        public BitmapBufferFormat BitmapFormat { get; set; }
        public override bool IsReferenceImage => true;
        public override int ReferenceX => 0;
        public override int ReferenceY => 0;
    }





    public enum RenderQuality
    {
        HighQuality,
        Fast,
    }

    /// <summary>
    /// image filter
    /// </summary>
    public interface IImageFilter
    {
        //implementation for cpu-based and gpu-based may be different
        void Apply();
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
    public enum RenderSurfaceOrientation
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

    public static class DrawBoardExtensionMethods
    {
        public static void OffsetCanvasOrigin(this DrawBoard drawBoard, int dx, int dy)
        {
            //TODO: review offset function
            drawBoard.SetCanvasOrigin(drawBoard.OriginX + dx, drawBoard.OriginY + dy);
        }
        public static void OffsetCanvasOriginX(this DrawBoard drawBoard, int dx)
        {
            //TODO: review offset function
            drawBoard.OffsetCanvasOrigin(dx, 0);
        }
        public static void OffsetCanvasOriginY(this DrawBoard drawBoard, int dy)
        {
            //TODO: review offset function
            drawBoard.OffsetCanvasOrigin(0, dy);
        }


        //--------------------------------------------------

        public static SmoothingModeState SaveSmoothMode(this DrawBoard drawBoard)
        {
            //TODO: review offset function
            return new SmoothingModeState(drawBoard, drawBoard.SmoothingMode);
        }
        public static SmoothingModeState SetSmoothMode(this DrawBoard drawBoard, SmoothingMode value)
        {
            //TODO: review offset function
            var saveState = new SmoothingModeState(drawBoard, drawBoard.SmoothingMode);
            drawBoard.SmoothingMode = value;
            return saveState;
        }
        public static StrokeState SaveStroke(this DrawBoard drawBoard)
        {
            return new StrokeState(drawBoard);
        }
        public struct SmoothingModeState : System.IDisposable
        {
            readonly DrawBoard _drawBoard;
            readonly SmoothingMode _latestSmoothMode;
            internal SmoothingModeState(DrawBoard drawBoard, SmoothingMode state)
            {
                _latestSmoothMode = state;
                _drawBoard = drawBoard;
            }
            public void Dispose()
            {
                _drawBoard.SmoothingMode = _latestSmoothMode;
            }
        }
        //--------------------------------------------------

        public struct StrokeState : System.IDisposable
        {
            readonly DrawBoard _d;
            readonly Color _stokeColor;
            readonly float _strokeW;
            public StrokeState(DrawBoard d)
            {
                _d = d;
                _stokeColor = d.StrokeColor;
                _strokeW = d.StrokeWidth;
            }
            public void Dispose()
            {
                _d.StrokeColor = _stokeColor;
                _d.StrokeWidth = _strokeW;
            }
        }


    }




}



