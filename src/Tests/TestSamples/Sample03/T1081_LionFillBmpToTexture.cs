﻿//MIT, 2014-2016,WinterDev

using System;
using PixelFarm.Drawing;
using Mini;
using PixelFarm.DrawingGL;
using PixelFarm.CpuBlit;
namespace OpenTkEssTest
{
    [Info(OrderCode = "108.1")]
    [Info("T1081_LionFillBmpToTexture")]
    public class T1081_LionFillBmpToTexture : DemoBase
    {
        //***
        //software-based bitmap cache
        //this example:
        //we render the lion with Agg (software-based)
        //then copy pixel buffer to gl texture
        //and render the texture the bg surface 
        //***

        //---------------------------

        ActualBitmap aggImage;

        AggPainter aggPainter;
        //---------------------------
        GLRenderSurface _glsx;
        SpriteShape lionShape;
        GLPainter painter;

        GLBitmap glBmp;
        protected override void OnGLSurfaceReady(GLRenderSurface glsx, GLPainter painter)
        {
            this._glsx = glsx;
            this.painter = painter;
        }
        protected override void OnReadyForInitGLShaderProgram()
        {


            var _svgRenderVx = PixelFarm.CpuBlit.SvgRenderVxLoader.CreateSvgRenderVxFromFile("Samples/lion.svg");
            lionShape = new SpriteShape(_svgRenderVx);

            RectD lionBounds = lionShape.Bounds;
            //-------------
            aggImage = new ActualBitmap((int)lionBounds.Width, (int)lionBounds.Height);
            aggPainter = AggPainter.Create(aggImage);


            DrawLion(aggPainter, lionShape);
            //convert affImage to texture 
            glBmp = DemoHelper.LoadTexture(aggImage);
        }
        protected override void DemoClosing()
        {
            _glsx.Dispose();
        }
        static void DrawLion(Painter p, SpriteShape shape)
        {
            shape.Paint(p);

            //int j = shape.NumPaths;
            //int[] pathList = shape.PathIndexList;
            //Color[] colors = shape.Colors;
            //for (int i = 0; i < j; ++i)
            //{
            //    p.FillColor = colors[i];
            //    p.Fill(new VertexStoreSnap(myvxs, pathList[i]));
            //}
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            _glsx.SmoothMode = SmoothMode.Smooth;
            _glsx.StrokeColor = PixelFarm.Drawing.Color.Blue;
            _glsx.ClearColorBuffer();
            //-------------------------------
            _glsx.DrawImage(glBmp, 0, 600);
            SwapBuffers();
        }
    }
}

