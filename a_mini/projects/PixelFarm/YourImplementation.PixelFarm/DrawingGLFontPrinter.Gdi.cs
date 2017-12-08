﻿//MIT, 2016-2017, WinterDev

using System;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;

namespace PixelFarm.DrawingGL
{
    /// <summary>
    /// this use win gdi only
    /// </summary>
    public class WinGdiFontPrinter : ITextPrinter, IDisposable
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
        public void ChangeFillColor(Color fillColor)
        {

        }
        public void ChangeStrokeColor(Color strokeColor)
        {

        }
        public void Dispose()
        {
            //TODO: review here             
            _defautInitFont.Dispose();
            _defautInitFont = null;

            hfont = IntPtr.Zero;
            memdc.Dispose();
        }

        Win32.Win32Font _defautInitFont;
        void InitFont(string fontName, int emHeight)
        {
            Win32.Win32Font font = Win32.FontHelper.CreateWin32Font(fontName, emHeight, false, false);
            memdc.SetFont(font.GetHFont());
            _defautInitFont = font;
        }


        public void DrawString(char[] textBuffer, int startAt, int len, double x, double y)
        {
            //TODO: review performance              
            memdc.PatBlt(Win32.NativeWin32MemoryDc.PatBltColor.White, 0, 0, bmpWidth, bmpHeight);
            memdc.TextOut(textBuffer);
            //memdc.BitBltTo(destHdc);
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


            memdc.MeasureTextSize(textBuffer, out bmpWidth, out bmpHeight);
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

        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            throw new NotImplementedException();
        }

        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] buffer, int startAt, int len)
        {
            throw new NotImplementedException();
        }
    }

}