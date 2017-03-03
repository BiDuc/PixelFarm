﻿//MIT, 2014-2016,WinterDev

using System;
using Mini;
using PixelFarm.DrawingGL;

namespace OpenTkEssTest
{
    [Info(OrderCode = "101")]
    [Info("T101_BlankCanvas")]
    public class T101_BlankCanvas : PrebuiltGLControlDemoBase
    {
        CanvasGL2d canvas2d;
        protected override void OnInitGLProgram(object sender, EventArgs args)
        {
            int max = Math.Max(this.Width, this.Height);
            canvas2d = PixelFarm.Drawing.GLES2.GLES2Platform.CreateCanvasGL2d(max, max);
        }
        protected override void DemoClosing()
        {
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            canvas2d.ClearColorBuffer();
            SwapBuffers();
        }
    }
}
