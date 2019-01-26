﻿//MIT, 2014-present, WinterDev
using System;
using PixelFarm.DrawingGL;


namespace Mini
{

    public delegate void SetupPainterDel(GLPainter painter);

    public class GLDemoContext
    {
        Mini.DemoBase _demo;
        int _w, _h;
        SetupPainterDel _getTextPrinterDel;

        public GLDemoContext(int w, int h)
        {
            _w = w;
            _h = h;
        }
        public void SetTextPrinter(SetupPainterDel del)
        {
            _getTextPrinterDel = del;
        }
        public void Close()
        {
            _demo.CloseDemo();
        }
        public void LoadDemo(Mini.DemoBase demo)
        {
            _demo = demo;
            demo.Init();

            int max = Math.Max(_w, _h);

            demo.Width = _w;
            demo.Height = _h;
            GLPainterContext pcx = null;
            GLPainter canvasPainter = null;

            //if demo not create canvas and painter
            //the we create for it
            //int max = Math.Max(w, h);
            //canvas2d = PixelFarm.Drawing.GLES2.GLES2Platform.CreateCanvasGL2d(max, max);
            //canvasPainter = new GLCanvasPainter(canvas2d, max, max);

            //canvas2d = PixelFarm.Drawing.GLES2.GLES2Platform.CreateCanvasGL2d(w, h);
            pcx = GLPainterContext.Create(max, max, _w, _h, true);
            pcx.OriginKind = PixelFarm.Drawing.RenderSurfaceOrientation.LeftBottom;
            canvasPainter = new GLPainter();
            canvasPainter.BindToPainterContext(pcx);

            //create text printer for opengl 
            //----------------------
            //1. win gdi based
            //var printer = new WinGdiFontPrinter(canvas2d, w, h);
            //canvasPainter.TextPrinter = printer;
            //----------------------
            //2. raw vxs
            //var printer = new PixelFarm.Drawing.Fonts.VxsTextPrinter(canvasPainter);
            //canvasPainter.TextPrinter = printer;
            //----------------------
            //3. agg texture based font texture
            //var printer = new AggFontPrinter(canvasPainter, w, h);
            //canvasPainter.TextPrinter = printer;
            //----------------------
            //4. texture atlas based font texture 
            //------------
            //resolve request font 
            //var printer = new GLBmpGlyphTextPrinter(canvasPainter, YourImplementation.BootStrapWinGdi.myFontLoader);
            //canvasPainter.TextPrinter = printer;

            if (_getTextPrinterDel != null)
            {
                _getTextPrinterDel(canvasPainter);
            }



            demo.SetEssentialGLHandlers(
                () => { },
                () => IntPtr.Zero,
                () => IntPtr.Zero);

            DemoBase.InvokeGLPainterReady(demo, pcx, canvasPainter);
            DemoBase.InvokePainterReady(demo, canvasPainter);
        }
        public void Render()
        {
            _demo.InvokeGLPaint();
        }
    }



    //public delegate PixelFarm.CpuBlit.MemBitmap LoadImageDelegate(string filename);

    public static class DemoHelper
    {
         
        public static GLBitmap LoadTexture(string imgFileName)
        {
            return LoadTexture(PixelFarm.CpuBlit.MemBitmapExtensions.LoadImageFromFile(imgFileName));
        }
        public static GLBitmap LoadTexture(PixelFarm.CpuBlit.MemBitmap memBmp)
        {
            return new GLBitmap(memBmp) { IsBigEndianPixel = memBmp.IsBigEndian };
        }

        public static GLBitmap LoadTexture(PixelFarm.Drawing.Image bmp)
        {
            return null;

            //var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
            //    System.Drawing.Imaging.ImageLockMode.ReadOnly,
            //    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //int stride = bmpdata.Stride;
            //byte[] buffer = new byte[stride * bmp.Height];
            //System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, buffer, 0, buffer.Length);
            //bmp.UnlockBits(bmpdata);
            ////---------------------------
            ////if we are on Little-endian  machine,
            ////
            ////---------------------------
            //return new PixelFarm.DrawingGL.GLBitmap(bmp.Width, bmp.Height, buffer, false);
        }
    }

}