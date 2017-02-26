﻿//MIT, 2016-2017, WinterDev
using System;
using PixelFarm.Agg;
using PixelFarm.Drawing;
using PixelFarm.DrawingGL;

namespace PixelFarm.Drawing.Fonts
{

    class AggFontPrinter : ITextPrinter
    {
        ActualImage actualImage;
        ImageGraphics2D imgGfx2d;
        AggCanvasPainter aggPainter;
        TextPrinter textPrinter;
        int bmpWidth;
        int bmpHeight;
        CanvasGL2d canvas;
        GLCanvasPainter canvasPainter;

        public AggFontPrinter(GLCanvasPainter canvasPainter, int w, int h)
        {
            //TODO: review here
            this.canvasPainter = canvasPainter;
            this.canvas = canvasPainter.Canvas;
            bmpWidth = w;
            bmpHeight = h;
            actualImage = new ActualImage(bmpWidth, bmpHeight, PixelFormat.ARGB32);

            imgGfx2d = new ImageGraphics2D(actualImage);
            aggPainter = new AggCanvasPainter(imgGfx2d);
            aggPainter.FillColor = Color.Black;
            aggPainter.StrokeColor = Color.Black;

            //set default1
            aggPainter.CurrentFont = canvasPainter.CurrentFont;
            textPrinter = new TextPrinter(aggPainter);
            aggPainter.TextPrinter = textPrinter;
        }

        public void DrawString(string text, double x, double y)
        {

            aggPainter.Clear(Drawing.Color.White);
            //draw text 
            textPrinter.DrawString(text, 0, 18);

            byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(actualImage);
            //------------------------------------------------------
            GLBitmap glBmp = new GLBitmap(bmpWidth, bmpHeight, buffer, true);

            bool isYFliped = canvas.FlipY;
            if (isYFliped)
            {
                canvas.DrawImage(glBmp, (float)x, (float)y);
            }
            else
            {
                canvas.FlipY = true;
                canvas.DrawImage(glBmp, (float)x, (float)y);
                canvas.FlipY = false;
            }
            glBmp.Dispose();
        }

        public void ChangeFont(RequestFont font)
        {
            aggPainter.CurrentFont = font;
        }

        public void ChangeFontColor(Color fontColor)
        {
            aggPainter.FillColor = fontColor;
        }
    }
    /// <summary>
    /// this use win gdi only
    /// </summary>
    class WinGdiFontPrinter : ITextPrinter, IDisposable
    {

        int _width;
        int _height;
        Win32.NativeWin32MemoryDc memdc;
        IntPtr hfont;
        int bmpWidth = 200;
        int bmpHeight = 50;
        CanvasGL2d canvas;
        public WinGdiFontPrinter(CanvasGL2d canvas, int w, int h)
        {
            this.canvas = canvas;
            _width = w;
            _height = h;
            bmpWidth = w;
            bmpHeight = h;

            memdc = new Win32.NativeWin32MemoryDc(bmpWidth, bmpHeight);
            //TODO: review here
            //use default font from current platform
            InitFont("tahoma", 14);
            memdc.SetTextColor(0);
        }
        public void ChangeFont(RequestFont font)
        {

        }
        public void ChangeFontColor(Color fontColor)
        {

        }
        public void Dispose()
        {
            //TODO: review here 
            Win32.MyWin32.DeleteObject(hfont);
            hfont = IntPtr.Zero;
            memdc.Dispose();
        }
        void InitFont(string fontName, int emHeight)
        {
            Win32.MyWin32.LOGFONT logFont = new Win32.MyWin32.LOGFONT();
            Win32.MyWin32.SetFontName(ref logFont, fontName);
            logFont.lfHeight = emHeight;
            logFont.lfCharSet = 1;//default
            logFont.lfQuality = 0;//default
            hfont = Win32.MyWin32.CreateFontIndirect(ref logFont);
            Win32.MyWin32.SelectObject(memdc.DC, hfont);
        }
        public void DrawString(string text, double x, double y)
        {
            //TODO: review performan
            char[] textBuffer = text.ToCharArray();
            Win32.MyWin32.PatBlt(memdc.DC, 0, 0, bmpWidth, bmpHeight, Win32.MyWin32.WHITENESS);
            Win32.NativeTextWin32.TextOut(memdc.DC, 0, 0, textBuffer, textBuffer.Length);
            // Win32.Win32Utils.BitBlt(hdc, 0, 0, bmpWidth, 50, memHdc, 0, 0, Win32.MyWin32.SRCCOPY);
            //---------------
            int stride = 4 * ((bmpWidth * 32 + 31) / 32);

            //Bitmap newBmp = new Bitmap(bmpWidth, 50, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //var bmpData = newBmp.LockBits(new Rectangle(0, 0, bmpWidth, 50), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] tmp1 = new byte[stride * 50];
            System.Runtime.InteropServices.Marshal.Copy(memdc.PPVBits, tmp1, 0, tmp1.Length);
            //---------------
            int pos = 3;
            for (int r = 0; r < 50; ++r)
            {
                for (int c = 0; c < stride; ++c)
                {
                    tmp1[pos] = 255;
                    pos += 4;
                    c += 4;
                }
            }

            Win32.NativeTextWin32.WIN32SIZE win32Size;
            unsafe
            {
                fixed (char* bufferHead = &textBuffer[0])
                {
                    Win32.NativeTextWin32.GetTextExtentPoint32Char(memdc.DC, bufferHead, textBuffer.Length, out win32Size);
                }
            }
            bmpWidth = win32Size.Width;
            bmpHeight = win32Size.Height;

            var actualImg = new Agg.ActualImage(bmpWidth, bmpHeight, Agg.PixelFormat.ARGB32);
            //------------------------------------------------------
            //copy bmp from specific bmp area 
            //and convert to GLBmp   
            byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(actualImg);
            unsafe
            {
                byte* header = (byte*)memdc.PPVBits;
                fixed (byte* dest0 = &buffer[0])
                {
                    byte* dest = dest0;
                    byte* rowHead = header;
                    int rowLen = bmpWidth * 4;
                    for (int h = 0; h < bmpHeight; ++h)
                    {

                        header = rowHead;
                        for (int n = 0; n < rowLen;)
                        {
                            //move next
                            *(dest + 0) = *(header + 0);
                            *(dest + 1) = *(header + 1);
                            *(dest + 2) = *(header + 2);
                            //*(dest + 3) = *(header + 3);
                            *(dest + 3) = 255;
                            header += 4;
                            dest += 4;
                            n += 4;
                        }
                        //finish one row
                        rowHead += stride;
                    }
                }
            }

            //------------------------------------------------------
            GLBitmap glBmp = new GLBitmap(bmpWidth, bmpHeight, buffer, false);
            canvas.DrawImage(glBmp, (float)x, (float)y);
            glBmp.Dispose();
        }
    }
}