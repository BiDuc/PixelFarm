﻿//MIT, 2014-2017, WinterDev

using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES20;
using Tesselate;

namespace PixelFarm.DrawingGL
{

    public sealed class CanvasGL2d
    {
        SmoothLineShader smoothLineShader;
        InvertAlphaLineSmoothShader invertAlphaFragmentShader;
        BasicFillShader basicFillShader;
        RectFillShader rectFillShader;
        GdiImageTextureShader gdiImgTextureShader;
        GdiImageTextureWithWhiteTransparentShader gdiImgTextureWithWhiteTransparentShader;
        ImageTextureWithSubPixelRenderingShader textureSubPixRendering;
        OpenGLESTextureShader glesTextureShader;
        BlurShader blurShader;
        Conv3x3TextureShader conv3x3TextureShader;
        MultiChannelSdf msdfShader;
        MultiChannelSubPixelRenderingSdf msdfSubPixelRenderingShader;
        SingleChannelSdf sdfShader;
        //-----------------------------------------------------------
        CanvasToShaderSharedResource shaderRes;

        //tools---------------------------------

        int canvasOriginX = 0;
        int canvasOriginY = 0;
        int canvasW;
        int canvasH;

        MyMat4 orthoView;
        MyMat4 flipVerticalView;
        MyMat4 orthoAndFlip;

        TessTool tessTool;
        FrameBuffer _currentFrameBuffer;//default = null, system provide frame buffer 
        SmoothBorderBuilder smoothBorderBuilder = new SmoothBorderBuilder();

        internal CanvasGL2d(int canvasW, int canvasH)
        {
            this.canvasW = canvasW;
            this.canvasH = canvasH;
            ////setup viewport size
            int max = Math.Max(canvasW, canvasH);
            ////square viewport 
            orthoView = MyMat4.ortho(0, max, 0, max, 0, 1);
            flipVerticalView = MyMat4.scale(1, -1) * MyMat4.translate(new OpenTK.Vector3(0, -max, 0));
            orthoAndFlip = orthoView * flipVerticalView;
            //-----------------------------------------------------------------------
            shaderRes = new CanvasToShaderSharedResource();
            shaderRes.OrthoView = orthoView;
            //----------------------------------------------------------------------- 
            basicFillShader = new BasicFillShader(shaderRes);
            smoothLineShader = new SmoothLineShader(shaderRes);
            rectFillShader = new RectFillShader(shaderRes);
            gdiImgTextureShader = new GdiImageTextureShader(shaderRes);
            gdiImgTextureWithWhiteTransparentShader = new GdiImageTextureWithWhiteTransparentShader(shaderRes);
            textureSubPixRendering = new ImageTextureWithSubPixelRenderingShader(shaderRes);
            blurShader = new BlurShader(shaderRes);
            glesTextureShader = new OpenGLESTextureShader(shaderRes);
            invertAlphaFragmentShader = new InvertAlphaLineSmoothShader(shaderRes); //used with stencil  ***
            // tessListener.Connect(tess,          
            //Tesselate.Tesselator.WindingRuleType.Odd, true);
            conv3x3TextureShader = new Conv3x3TextureShader(shaderRes);
            msdfShader = new DrawingGL.MultiChannelSdf(shaderRes);


            msdfSubPixelRenderingShader = new DrawingGL.MultiChannelSubPixelRenderingSdf(shaderRes);
            sdfShader = new DrawingGL.SingleChannelSdf(shaderRes);
            //-----------------------------------------------------------------------

            tessTool = new TessTool();
            //-----------------------------------------------------------------------


            //GL.Enable(EnableCap.CullFace);
            //GL.FrontFace(FrontFaceDirection.Cw);
            //GL.CullFace(CullFaceMode.Back); 

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);//original **

            //GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.One);// not apply alpha to src
            //GL.BlendFuncSeparate(BlendingFactorSrc.SrcColor, BlendingFactorDest.OneMinusSrcAlpha,
            //                     BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            //GL.BlendFuncSeparate(BlendingFactorSrc.SrcColor, BlendingFactorDest.OneMinusSrcColor, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);

            GL.ClearColor(1, 1, 1, 1);
            //-------------------------------------------------------------------------------
            GL.Viewport(0, 0, canvasW, canvasH);
        }

        bool _flipY;
        public bool FlipY
        {
            get
            {
                return this._flipY;
            }
            set
            {
                if (this._flipY = value)
                {
                    shaderRes.OrthoView = orthoAndFlip;
                }
                else
                {
                    shaderRes.OrthoView = orthoView;
                }
            }
        }

        public void Dispose()
        {
        }

        public CanvasSmoothMode SmoothMode
        {
            get;
            set;
        }

        public FrameBuffer CreateFrameBuffer(int w, int h)
        {
            return new FrameBuffer(w, h);
        }

        public FrameBuffer CurrentFrameBuffer
        {
            get { return this._currentFrameBuffer; }
        }
        public void AttachFrameBuffer(FrameBuffer frameBuffer)
        {
            DetachFrameBuffer(true);
            if (frameBuffer != null)
            {
                this._currentFrameBuffer = frameBuffer;
                frameBuffer.MakeCurrent();
            }
        }
        public void DetachFrameBuffer(bool updateTextureResult = true)
        {
            if (_currentFrameBuffer != null)
            {
                if (updateTextureResult)
                {
                    _currentFrameBuffer.UpdateTexture();
                }
                _currentFrameBuffer.ReleaseCurrent();
            }
            _currentFrameBuffer = null;
        }
        public void Clear(PixelFarm.Drawing.Color c)
        {
            GL.ClearColor(
               (float)c.R / 255f,
               (float)c.G / 255f,
               (float)c.B / 255f,
               (float)c.A / 255f);
            GL.ClearStencil(0);
            //actual clear here !
            GL.Clear(ClearBufferMask.ColorBufferBit |
                ClearBufferMask.DepthBufferBit |
                ClearBufferMask.StencilBufferBit);
        }

        public void ClearColorBuffer()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }
        public float StrokeWidth
        {
            get { return shaderRes._strokeWidth; }
            set
            {
                shaderRes._strokeWidth = value;
            }
        }
        public Drawing.Color StrokeColor
        {
            get { return shaderRes.StrokeColor; }
            set { shaderRes.StrokeColor = value; }
        }
        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            switch (this.SmoothMode)
            {
                case CanvasSmoothMode.Smooth:
                    {
                        this.smoothLineShader.DrawLine(x1, y1, x2, y2);
                    }
                    break;
                default:
                    {
                        if (StrokeWidth == 1)
                        {
                            this.basicFillShader.DrawLine(x1, y1, x2, y2, StrokeColor);
                        }
                        else
                        {
                            //TODO: review stroke with for smooth line shader again
                            shaderRes._strokeWidth = this.StrokeWidth;
                            this.smoothLineShader.DrawLine(x1, y1, x2, y2);
                        }
                    }
                    break;
            }
        }
        public void DrawFrameBuffer(FrameBuffer frameBuffer, float x, float y)
        {
            //draw frame buffer into specific position
            glesTextureShader.Render(frameBuffer.TextureId, x, y, frameBuffer.Width, frameBuffer.Height);
        }
        public void DrawImage(GLBitmap bmp, float x, float y)
        {
            DrawImage(bmp,
                   new Drawing.RectangleF(0, 0, bmp.Width, bmp.Height),
                   x, y, bmp.Width, bmp.Height);
        }
        public void DrawImage(GLBitmap bmp, float x, float y, float w, float h)
        {
            DrawImage(bmp,
                new Drawing.RectangleF(0, 0, bmp.Width, bmp.Height),
                x, y, w, h);
        }
        public void DrawSubImage(GLBitmap bmp, float srcLeft, float srcTop, float srcW, float srcH, float targetLeft, float targetTop)
        {
            if (bmp.IsBigEndianPixel)
            {
                glesTextureShader.RenderSubImage(bmp, srcLeft, srcTop, srcW, srcH, targetLeft, targetTop);
            }
            else
            {
                gdiImgTextureShader.RenderSubImage(bmp, srcLeft, srcTop, srcW, srcH, targetLeft, targetTop);
            }
        }
        public void DrawSubImage(GLBitmap bmp, ref PixelFarm.Drawing.Rectangle r, float targetLeft, float targetTop)
        {
            if (bmp.IsBigEndianPixel)
            {
                glesTextureShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
            else
            {
                gdiImgTextureShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
        }
        public void DrawSubImage(GLBitmap bmp, ref PixelFarm.Drawing.Rectangle r, float targetLeft, float targetTop, float scale)
        {
            if (bmp.IsBigEndianPixel)
            {
                glesTextureShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
            else
            {
                gdiImgTextureShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
        }
        public void DrawSubImageWithMsdf(GLBitmap bmp, ref PixelFarm.Drawing.Rectangle r, float targetLeft, float targetTop)
        {
            if (bmp.IsBigEndianPixel)
            {
                msdfShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
            else
            {
                msdfShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
        }
        public void DrawSubImageWithMsdf(GLBitmap bmp, ref PixelFarm.Drawing.Rectangle r, float targetLeft, float targetTop, float scale)
        {
            if (bmp.IsBigEndianPixel)
            {
                msdfShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop, scale);
            }
            else
            {
                msdfShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop, scale);
            }
        }
        public void DrawSubImageWithMsdf(GLBitmap bmp, float[] coords, float scale)
        {
            if (bmp.IsBigEndianPixel)
            {
                msdfShader.RenderSubImages(bmp, coords, scale);
            }
            else
            {
                msdfShader.RenderSubImages(bmp, coords, scale);
            }
        }
        public void DrawImage(GLBitmap bmp,
            Drawing.RectangleF srcRect,
            float x, float y, float w, float h)
        {
            if (bmp.IsBigEndianPixel)
            {
                glesTextureShader.Render(bmp, x, y, w, h);
            }
            else
            {
                gdiImgTextureShader.Render(bmp, x, y, w, h);
            }
        }
        /// <summary>
        /// draw glyph image with transparent
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawGlyphImage(GLBitmap bmp, float x, float y)
        {
            this.gdiImgTextureWithWhiteTransparentShader.Render(bmp, x, y, bmp.Width, bmp.Height);
        }
        /// <summary>
        /// draw glyph image with transparent
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawGlyphImageWithSubPixelRenderingTechnique(GLBitmap bmp, float x, float y)
        {
            PixelFarm.Drawing.Rectangle r = new Drawing.Rectangle(0, bmp.Height, bmp.Width, bmp.Height);
            DrawGlyphImageWithSubPixelRenderingTechnique(bmp, ref r, x, y, 1);
        }
        public PixelFarm.Drawing.Color FontFillColor { get; set; }
        public void DrawGlyphImageWithSubPixelRenderingTechnique(
            GLBitmap bmp,
            ref PixelFarm.Drawing.Rectangle r,
            float targetLeft,
            float targetTop,
            float scale)
        {

            if (bmp.IsBigEndianPixel)
            {
                throw new NotSupportedException();
            }
            else
            {
                textureSubPixRendering.LoadGLBitmap(bmp);
                textureSubPixRendering.IsBigEndian = bmp.IsBigEndianPixel;
                textureSubPixRendering.SetColor(this.FontFillColor);
                textureSubPixRendering.SetIntensity(1.15f);
                //-------------------------
                //draw a serie of image***
                //-------------------------

                //1. B , cyan result
                GL.ColorMask(false, false, true, false);
                textureSubPixRendering.SetCompo(0);
                textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
                //float subpixel_shift = 1 / 9f;
                //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft - subpixel_shift, targetTop); //TODO: review this option
                //---------------------------------------------------
                //2. G , magenta result
                GL.ColorMask(false, true, false, false);
                textureSubPixRendering.SetCompo(1);
                textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
                //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop); //TODO: review this option
                //1. R , yellow result 
                textureSubPixRendering.SetCompo(2);
                GL.ColorMask(true, false, false, false);//             
                textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
                //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft + subpixel_shift, targetTop); //TODO: review this option
                //enable all color component
                GL.ColorMask(true, true, true, true);
            }

        }
        public void DrawImage(GLBitmapReference bmp, float x, float y)
        {
            this.DrawImage(bmp.OwnerBitmap,
                 bmp.GetRectF(),
                 x, y, bmp.Width, bmp.Height);
        }
        //-------------------------------------------------------------------------------
        public void DrawImageWithBlurY(GLBitmap bmp, float x, float y)
        {
            //TODO: review here
            //not complete
            blurShader.IsBigEndian = bmp.IsBigEndianPixel;
            blurShader.IsHorizontal = false;
            blurShader.Render(bmp, x, y, bmp.Width, bmp.Height);
        }
        public void DrawImageWithBlurX(GLBitmap bmp, float x, float y)
        {
            //TODO: review here
            //not complete
            blurShader.IsBigEndian = bmp.IsBigEndianPixel;
            blurShader.IsHorizontal = true;
            blurShader.Render(bmp, x, y, bmp.Width, bmp.Height);
        }
        public void DrawImageWithConv3x3(GLBitmap bmp, float[] kernel3x3, float x, float y)
        {
            conv3x3TextureShader.IsBigEndian = bmp.IsBigEndianPixel;
            conv3x3TextureShader.SetBitmapSize(bmp.Width, bmp.Height);
            conv3x3TextureShader.SetConvolutionKernel(kernel3x3);
            conv3x3TextureShader.Render(bmp, x, y, bmp.Width, bmp.Height);
        }
        public void DrawImageWithMsdf(GLBitmap bmp, float x, float y)
        {

            msdfShader.ForegroundColor = PixelFarm.Drawing.Color.Black;
            msdfShader.Render(bmp, x, y, bmp.Width, bmp.Height);
        }
        public void DrawImageWithMsdf(GLBitmap bmp, float x, float y, float scale)
        {
            msdfShader.ForegroundColor = PixelFarm.Drawing.Color.Black;

            msdfShader.Render(bmp, x, y, bmp.Width * scale, bmp.Height * scale);
        }
        public void DrawImageWithSubPixelRenderingMsdf(GLBitmap bmp, float x, float y)
        {

            msdfSubPixelRenderingShader.ForegroundColor = PixelFarm.Drawing.Color.Black;
            //msdfSubPixelRenderingShader.BackgroundColor = PixelFarm.Drawing.Color.Blue;//blue is suite for transparent bg
            msdfSubPixelRenderingShader.BackgroundColor = PixelFarm.Drawing.Color.White;//opaque white
            msdfSubPixelRenderingShader.Render(bmp, x, y, bmp.Width, bmp.Height);
        }
        public void DrawImageWithSubPixelRenderingMsdf(GLBitmap bmp, float x, float y, float scale)
        {
            msdfSubPixelRenderingShader.ForegroundColor = PixelFarm.Drawing.Color.Black;
            //msdfSubPixelRenderingShader.BackgroundColor = PixelFarm.Drawing.Color.Blue;//blue is suite for transparent bg
            msdfSubPixelRenderingShader.BackgroundColor = PixelFarm.Drawing.Color.White;//opaque white
            msdfSubPixelRenderingShader.Render(bmp, x, y, bmp.Width * scale, bmp.Height * scale);
        }





        public void DrawImageWithSdf(GLBitmap bmp, float x, float y, float scale)
        {
            sdfShader.ForegroundColor = PixelFarm.Drawing.Color.Black;
            sdfShader.Render(bmp, x, y, bmp.Width * scale, bmp.Height * scale);
        }

        //-------------------------------------------------------------------------------
        public void FillTriangleStrip(Drawing.Color color, float[] coords, int n)
        {
            basicFillShader.FillTriangleStripWithVertexBuffer(coords, n, color);
        }
        public void FillTriangleFan(Drawing.Color color, float[] coords, int n)
        {
            unsafe
            {
                fixed (float* head = &coords[0])
                {
                    basicFillShader.FillTriangleFan(head, n, color);
                }
            }
        }
        //-------------------------------------------------------------------------------
        //RenderVx
        public void FillRenderVx(Drawing.Brush brush, Drawing.RenderVx renderVx)
        {
            GLRenderVx glRenderVx = (GLRenderVx)renderVx;
            FillGfxPath(brush, glRenderVx.gxpth);
        }
        public void FillRenderVx(Drawing.Color color, Drawing.RenderVx renderVx)
        {
            GLRenderVx glRenderVx = (GLRenderVx)renderVx;
            FillGfxPath(color, glRenderVx.gxpth);
        }
        public void DrawRenderVx(Drawing.Color color, Drawing.RenderVx renderVx)
        {
            GLRenderVx glRenderVx = (GLRenderVx)renderVx;
            DrawGfxPath(color, glRenderVx.gxpth);
        }
        //-------------------------------------------------------------------------------
        public void FillGfxPath(Drawing.Color color, InternalGraphicsPath igpth)
        {
            switch (SmoothMode)
            {
                case CanvasSmoothMode.No:
                    {
                        List<Figure> figures = igpth.figures;
                        int subPathCount = figures.Count;
                        for (int i = 0; i < subPathCount; ++i)
                        {
                            Figure f = figures[i];
                            if (f.SupportVertexBuffer)
                            {
                                f.InitVertexBufferIfNeed(this.tessTool);
                                basicFillShader.FillTriangles(f.VBOArea, f.TessAreaTriangleCount, color);
                            }
                            else
                            {
                                float[] tessArea = f.GetAreaTess(this.tessTool);
                                if (tessArea != null)
                                {
                                    this.basicFillShader.FillTriangles(tessArea, f.TessAreaTriangleCount, color);
                                }
                            }

                        }
                    }
                    break;
                case CanvasSmoothMode.Smooth:
                    {

                        List<Figure> figures = igpth.figures;
                        int subPathCount = figures.Count;
                        float saved_Width = StrokeWidth;
                        Drawing.Color saved_Color = StrokeColor;

                        //temp set stroke width to 2 amd stroke color
                        //to the same as bg color (for smooth border).
                        //and it will be set back later.
                        //

                        StrokeColor = color;
                        StrokeWidth = 2f; //TODO: review this ***
                        //
                        for (int i = 0; i < subPathCount; ++i)
                        {
                            //draw each sub-path
                            //
                            Figure f = figures[i];
                            if (f.SupportVertexBuffer)
                            {

                                f.InitVertexBufferIfNeed(this.tessTool);
                                //draw area
                                basicFillShader.FillTriangles(f.VBOArea, f.TessAreaTriangleCount, color);
                                //draw smooth border
                                smoothLineShader.DrawTriangleStrips(f.GetSmoothBorders(smoothBorderBuilder), f.BorderTriangleStripCount);
                            }
                            else
                            {
                                float[] tessArea = f.GetAreaTess(this.tessTool);
                                if (tessArea != null)
                                {
                                    basicFillShader.FillTriangles(tessArea, f.TessAreaTriangleCount, color);
                                    smoothLineShader.DrawTriangleStrips(f.GetSmoothBorders(smoothBorderBuilder), f.BorderTriangleStripCount);
                                }
                            }
                        }
                        //restore stroke width and color
                        StrokeWidth = saved_Width; //restore back
                        StrokeColor = saved_Color;
                    }
                    break;
            }
        }
        public void FillGfxPath(Drawing.Brush brush, InternalGraphicsPath igpth)
        {
            switch (brush.BrushKind)
            {
                case Drawing.BrushKind.Solid:
                    {
                        var solidBrush = brush as PixelFarm.Drawing.SolidBrush;
                        FillGfxPath(solidBrush.Color, igpth);
                    }
                    break;
                case Drawing.BrushKind.LinearGradient:
                case Drawing.BrushKind.Texture:
                    {
                        List<Figure> figures = igpth.figures;
                        int m = figures.Count;
                        for (int b = 0; b < m; ++b)
                        {
                            Figure fig = figures[b];
                            GL.ClearStencil(0); //set value for clearing stencil buffer 
                            //actual clear here
                            GL.Clear(ClearBufferMask.StencilBufferBit);
                            //-------------------
                            //disable rendering to color buffer
                            GL.ColorMask(false, false, false, false);
                            //start using stencil
                            GL.Enable(EnableCap.StencilTest);
                            //place a 1 where rendered
                            GL.StencilFunc(StencilFunction.Always, 1, 1);
                            //replace where rendered
                            GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                            //render  to stencill buffer
                            //-----------------

                            float[] tessArea = fig.GetAreaTess(this.tessTool);
                            //-------------------------------------   
                            if (tessArea != null)
                            {
                                this.basicFillShader.FillTriangles(tessArea, fig.TessAreaTriangleCount, PixelFarm.Drawing.Color.Black);
                            }
                            //-------------------------------------- 
                            //render color
                            //--------------------------------------  
                            //reenable color buffer 
                            GL.ColorMask(true, true, true, true);
                            //where a 1 was not rendered
                            GL.StencilFunc(StencilFunction.Equal, 1, 1);
                            //freeze stencill buffer
                            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
                            //------------------------------------------
                            //we already have valid ps from stencil step
                            //------------------------------------------

                            //-------------------------------------------------------------------------------------
                            //1.  we draw only alpha chanel of this black color to destination color
                            //so we use  BlendFuncSeparate  as follow ... 
                            //-------------------------------------------------------------------------------------
                            //1.  we draw only alpha channel of this black color to destination color
                            //so we use  BlendFuncSeparate  as follow ... 
                            GL.ColorMask(false, false, false, true);
                            //GL.BlendFuncSeparate(
                            //     BlendingFactorSrc.DstColor, BlendingFactorDest.DstColor, //the same
                            //     BlendingFactorSrc.One, BlendingFactorDest.Zero);

                            //use alpha chanel from source***
                            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
                            float[] smoothBorder = fig.GetSmoothBorders(smoothBorderBuilder);
                            invertAlphaFragmentShader.DrawTriangleStrips(smoothBorder, fig.BorderTriangleStripCount);
                            //at this point alpha component is fill in to destination 
                            //-------------------------------------------------------------------------------------
                            //2. then fill again!, 
                            //we use alpha information from dest, 
                            //so we set blend func to ... GL.BlendFunc(BlendingFactorSrc.DstAlpha, BlendingFactorDest.OneMinusDstAlpha)    
                            GL.ColorMask(true, true, true, true);
                            GL.BlendFunc(BlendingFactorSrc.DstAlpha, BlendingFactorDest.OneMinusDstAlpha);
                            {
                                //draw box*** of gradient color
                                switch (brush.BrushKind)
                                {
                                    case Drawing.BrushKind.LinearGradient:
                                        {
                                            var linearGradientBrush = brush as PixelFarm.Drawing.LinearGradientBrush;
                                            var colors = linearGradientBrush.GetColors();
                                            var points = linearGradientBrush.GetStopPoints();
                                            float[] v2f, color4f;
                                            GLGradientColorProvider.CalculateLinearGradientVxs2(
                                                 points[0].X, points[0].Y,
                                                 points[1].X, points[1].Y,
                                                 colors[0],
                                                 colors[1], out v2f, out color4f);
                                            rectFillShader.Render(v2f, color4f);
                                        }
                                        break;
                                    case Drawing.BrushKind.Texture:
                                        {
                                            //draw texture image ***
                                            PixelFarm.Drawing.TextureBrush tbrush = (PixelFarm.Drawing.TextureBrush)brush;
                                            GLBitmap bmpTexture = PixelFarm.Drawing.Image.GetCacheInnerImage(tbrush.TextureImage) as GLBitmap;
                                            //TODO: review here 
                                            //where text start?
                                            this.DrawImage(bmpTexture, 0, 300);
                                        }
                                        break;
                                }
                            }
                            //restore back 
                            //3. switch to normal blending mode 
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            GL.Disable(EnableCap.StencilTest);
                        }
                    }
                    break;
            }
        }

        public void DrawGfxPath(Drawing.Color color, InternalGraphicsPath igpth)
        {
            switch (SmoothMode)
            {
                case CanvasSmoothMode.No:
                    {
                        List<Figure> figures = igpth.figures;
                        int subPathCount = figures.Count;
                        for (int i = 0; i < subPathCount; ++i)
                        {
                            Figure f = figures[i];
                            float[] coordXYs = f.coordXYs;
                            unsafe
                            {
                                fixed (float* head = &coordXYs[0])
                                {
                                    basicFillShader.DrawLineLoopWithVertexBuffer(head, coordXYs.Length / 2, StrokeColor);
                                }
                            }
                        }
                    }
                    break;
                case CanvasSmoothMode.Smooth:
                    {
                        StrokeColor = color;
                        StrokeWidth = 1f;
                        List<Figure> figures = igpth.figures;
                        int subPathCount = figures.Count;
                        for (int i = 0; i < subPathCount; ++i)
                        {
                            Figure f = figures[i];
                            smoothLineShader.DrawTriangleStrips(f.GetSmoothBorders(smoothBorderBuilder), f.BorderTriangleStripCount);
                        }
                    }
                    break;
            }
        }
        //-------------------------------------------------------------------------------

        public void DrawRect(float x, float y, float w, float h)
        {
            switch (this.SmoothMode)
            {
                case CanvasSmoothMode.Smooth:
                    {
                        int borderTriAngleCount;

                        float[] triangles = smoothBorderBuilder.BuildSmoothBorders( 
                            CreatePolyLineRectCoords(x, y, w, h), out borderTriAngleCount);
                        smoothLineShader.DrawTriangleStrips(triangles, borderTriAngleCount);
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
        }

        public int CanvasOriginX
        {
            get { return this.canvasOriginX; }
        }
        public int CanvasOriginY
        {
            get { return this.canvasOriginY; }
        }

        public void SetCanvasOrigin(int x, int y)
        {
            //int originalW = 800;
            //set new viewport
            GL.Viewport(x, y, canvasW, canvasH);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadIdentity();
            //GL.Ortho(0, originalW, 0, originalW, 0.0, 100.0);
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadIdentity();
        }
        public void EnableClipRect()
        {
            GL.Enable(EnableCap.ScissorTest);
        }
        public void DisableClipRect()
        {
            GL.Disable(EnableCap.ScissorTest);
        }
        public void SetClipRect(int x, int y, int w, int h)
        {
            GL.Scissor(x, y, w, h);
        }

        static float[] CreatePolyLineRectCoords(
               float x, float y, float w, float h)
        {
            return new float[]
            {
                x,y,
                x+w,y,
                x+w,y+h,
                x,y+h
            };
        }




    }
}