﻿//MIT, 2014-present,WinterDev

using System;
using PixelFarm.Drawing; 
using Mini;
using PixelFarm.DrawingGL;
namespace OpenTkEssTest
{
    [Info(OrderCode = "106")]
    [Info("T106_SampleBrushes")]
    public class T106_SampleBrushes : DemoBase
    {
        GLRenderSurface _glsx;
        GLPainter painter;
        RenderVx polygon1;
        RenderVx polygon2;
        RenderVx polygon3;

        protected override void OnGLSurfaceReady(GLRenderSurface glsx, GLPainter painter)
        {
            _glsx = glsx;
            this.painter = painter;
        }
        protected override void OnReadyForInitGLShaderProgram()
        {   
            polygon1 = painter.CreatePolygonRenderVx(new float[]
                {
                    0,50,
                    50,50,
                    10,100
                });
            polygon2 = painter.CreatePolygonRenderVx(new float[]
              {
                   200, 50,
                   250, 50,
                   210, 100
              });
            polygon3 = painter.CreatePolygonRenderVx(new float[]
              {
                 400, 50,
                 450, 50,
                 410, 100
              });
        }
        protected override void DemoClosing()
        {
            _glsx.Dispose();
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            _glsx.SmoothMode = SmoothMode.Smooth;
            _glsx.StrokeColor = PixelFarm.Drawing.Color.Blue;
            _glsx.ClearColorBuffer();
            painter.FillColor = PixelFarm.Drawing.Color.Black;
            painter.FillRect(0, 0, 150, 150);
            GLBitmap glBmp = DemoHelper.LoadTexture(RootDemoPath.Path + @"\logo-dark.jpg");
            var textureBrush = new TextureBrush(glBmp);
            painter.FillRenderVx(textureBrush, polygon1);
            //------------------------------------------------------------------------- 
            var linearGrBrush2 = new LinearGradientBrush(
              new PointF(0, 50), Color.Red,
              new PointF(0, 100), Color.White);
            //fill polygon with gradient brush  
            painter.FillColor = Color.Yellow;
            painter.FillRect(200, 0, 150, 150);
            painter.FillRenderVx(linearGrBrush2, polygon2);
            painter.FillColor = Color.Black;
            painter.FillRect(400, 0, 150, 150);
            //-------------------------------------------------------------------------  
            //another  ...                 
            painter.FillRenderVx(linearGrBrush2, polygon3);
            //------------------------------------------------------------------------- 


            SwapBuffers();
        }
    }
}

