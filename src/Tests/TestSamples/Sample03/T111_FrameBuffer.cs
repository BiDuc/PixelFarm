﻿//MIT, 2014-present,WinterDev
//creadit : http://learningwebgl.com/lessons/lesson16/index.html

using System;
using Mini;
using PixelFarm.DrawingGL;
using OpenTK.Graphics.ES20;
namespace OpenTkEssTest
{
    [Info(OrderCode = "111")]
    [Info("T111_FrameBuffer")]
    public class T111_FrameBuffer : DemoBase
    {
        GLRenderSurface _glsx;
        GLPainter painter;
        Framebuffer frameBuffer;
        bool isInit;
        protected override void OnGLSurfaceReady(GLRenderSurface glsx, GLPainter painter)
        {
            this._glsx = glsx;
            this.painter = painter;
        }
        protected override void OnReadyForInitGLShaderProgram()
        {
            int max = Math.Max(this.Width, this.Height);
            frameBuffer = _glsx.CreateFramebuffer(max, max);
        }
        protected override void DemoClosing()
        {
            _glsx.Dispose();
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            _glsx.SmoothMode = SmoothMode.Smooth;
            _glsx.StrokeColor = PixelFarm.Drawing.Color.Blue;
            _glsx.Clear();
            //-------------------------------
            if (!isInit)
            {
                isInit = true;
            }
            if (frameBuffer.FrameBufferId > 0)
            {
                //------------------------------------------------------------------------------------
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer.FrameBufferId);
                //--------
                //do draw to frame buffer here
                GL.ClearColor(OpenTK.Graphics.Color4.Red); //clear with red color
                GL.Clear(ClearBufferMask.ColorBufferBit);
                //------------------------------------------------------------------------------------


                //------------------------------------------------------------------------------------ 
                GL.BindTexture(TextureTarget.Texture2D, frameBuffer.TextureId);
                GL.GenerateMipmap(TextureTarget.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);//switch to default framewbuffer
                //------------------------------------------------------------------------------------

                //create gl bmp from framebuffer 
                GLBitmap bmp = new GLBitmap(frameBuffer.TextureId, frameBuffer.Width, frameBuffer.Height);
                bmp.IsBigEndianPixel = true;//since this is created from FrameBuffer so set BigEndianPixel = true


                _glsx.DrawImage(bmp, 15, 0);

                //
                GL.ClearColor(OpenTK.Graphics.Color4.White); //clear with red color
            }
            else
            {
                _glsx.Clear(PixelFarm.Drawing.Color.Blue);
            }
            //-------------------------------
            SwapBuffers();
        }
    }
}

