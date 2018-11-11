﻿//MIT, 2014-present, WinterDev

using System;
using OpenTK.Graphics.ES20;

namespace PixelFarm.DrawingGL
{

    /// <summary>
    /// GLES2 render surface, This is not intended to be used directly from your code
    /// </summary>
    public sealed class GLRenderSurface
    {
        SmoothLineShader smoothLineShader;
        InvertAlphaLineSmoothShader invertAlphaFragmentShader;
        BasicFillShader basicFillShader;
        RectFillShader rectFillShader;
        GlyphImageStecilShader glyphStencilShader;
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
        ShaderSharedResource _shareRes;
        //tools---------------------------------

        int canvasOriginX = 0;
        int canvasOriginY = 0;
        int _width;
        int _height;

        MyMat4 orthoView;
        MyMat4 flipVerticalView;
        MyMat4 orthoAndFlip;

        TessTool tessTool;
        FrameBuffer _currentFrameBuffer;//default = null, system provide frame buffer 
        SmoothBorderBuilder smoothBorderBuilder = new SmoothBorderBuilder();


        internal GLRenderSurface(int width, int height, int viewportW, int viewportH)
        {
            //-------------
            //y axis points upward (like other OpenGL)
            //x axis points to right.
            //please NOTE: left lower corner of the canvas is (0,0)
            //-------------

            this._width = width;
            this._height = height;
            _vwWidth = viewportW;
            _vwHeight = viewportH;


            //setup viewport size,
            //we need W:H ratio= 1:1 , square viewport
            int max = Math.Max(width, height);
            orthoView = MyMat4.ortho(0, max, 0, max, 0, 1); //this make our viewport W:H =1:1

            flipVerticalView = MyMat4.scale(1, -1) * MyMat4.translate(new OpenTK.Vector3(0, -viewportH, 0));
            orthoAndFlip = orthoView * flipVerticalView;
            //-----------------------------------------------------------------------
            _shareRes = new ShaderSharedResource();
            _shareRes.OrthoView = orthoView;
            //----------------------------------------------------------------------- 
            basicFillShader = new BasicFillShader(_shareRes);
            smoothLineShader = new SmoothLineShader(_shareRes);
            rectFillShader = new RectFillShader(_shareRes);
            gdiImgTextureShader = new GdiImageTextureShader(_shareRes);
            gdiImgTextureWithWhiteTransparentShader = new GdiImageTextureWithWhiteTransparentShader(_shareRes);
            glyphStencilShader = new GlyphImageStecilShader(_shareRes);
            textureSubPixRendering = new ImageTextureWithSubPixelRenderingShader(_shareRes);
            blurShader = new BlurShader(_shareRes);
            glesTextureShader = new OpenGLESTextureShader(_shareRes);
            invertAlphaFragmentShader = new InvertAlphaLineSmoothShader(_shareRes); //used with stencil  ***

            conv3x3TextureShader = new Conv3x3TextureShader(_shareRes);
            msdfShader = new DrawingGL.MultiChannelSdf(_shareRes);
            msdfSubPixelRenderingShader = new DrawingGL.MultiChannelSubPixelRenderingSdf(_shareRes);
            sdfShader = new DrawingGL.SingleChannelSdf(_shareRes);
            //-----------------------------------------------------------------------
            //tools

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
            GL.Viewport(0, 0, width, height);
            FlipY = true;
        }


        int _vwWidth = 0;
        int _vwHeight = 0;

        public void SetViewport(int width, int height)
        {
            //when change, need to recalcate?
            _vwWidth = width;
            _vwHeight = height;
        }
        public int ViewportWidth
        {
            get { return _vwWidth; }
        }
        public int ViewportHeight
        {
            get { return _vwHeight; }
        }

        public int CanvasWidth
        {
            get { return _width; }
        }
        public int CanvasHeight
        {
            get { return _height; }
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
                    _shareRes.OrthoView = orthoAndFlip;
                }
                else
                {
                    _shareRes.OrthoView = orthoView;
                }
            }
        }

        public void Dispose()
        {
        }
        public void DetachCurrentShader()
        {
            _shareRes._currentShader = null;
        }
        public SmoothMode SmoothMode
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
            get { return _shareRes._strokeWidth; }
            set
            {
                _shareRes._strokeWidth = value;
            }
        }
        public Drawing.Color StrokeColor
        {
            get { return _shareRes.StrokeColor; }
            set { _shareRes.StrokeColor = value; }
        }
        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            switch (this.SmoothMode)
            {
                case SmoothMode.Smooth:
                    {
                        if (y1 == y2)
                        {
                            this.basicFillShader.DrawLine(x1, y1, x2, y2, StrokeColor);
                        }
                        else
                        {
                            this.smoothLineShader.DrawLine(x1, y1, x2, y2);
                        }
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
                            _shareRes._strokeWidth = this.StrokeWidth;
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
        public void DrawSubImage(GLBitmap bmp, ref PixelFarm.Drawing.Rectangle srcRect, float targetLeft, float targetTop)
        {
            if (bmp.IsBigEndianPixel)
            {
                glesTextureShader.RenderSubImage(bmp, srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
            }
            else
            {
                gdiImgTextureShader.RenderSubImage(bmp, srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
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
        public void DrawGlyphImageWithSubPixelRenderingTechnique(GLBitmap bmp, float x, float y)
        {
            PixelFarm.Drawing.Rectangle r = new Drawing.Rectangle(0, bmp.Height, bmp.Width, bmp.Height);
            DrawGlyphImageWithSubPixelRenderingTechnique(bmp, ref r, x, y, 1);
        }
        public PixelFarm.Drawing.Color FontFillColor { get; set; }


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
        public void DrawGlyphImageWithStecil(GLBitmap bmp, ref PixelFarm.Drawing.Rectangle r, float targetLeft, float targetTop, float scale)
        {
            glyphStencilShader.SetColor(this.FontFillColor);
            if (bmp.IsBigEndianPixel)
            {

                glyphStencilShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
            else
            {
                glyphStencilShader.RenderSubImage(bmp, r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop);
            }
        }

        public void LoadTexture1(GLBitmap bmp)
        {
            textureSubPixRendering.LoadGLBitmap(bmp);
            textureSubPixRendering.IsBigEndian = bmp.IsBigEndianPixel;
            textureSubPixRendering.SetColor(this.FontFillColor);
            textureSubPixRendering.SetIntensity(1f);
        }
        public void SetAssociatedTextureInfo(GLBitmap bmp)
        {
            textureSubPixRendering.SetAssociatedTextureInfo(bmp);
        }
        public void DrawGlyphImageWithSubPixelRenderingTechnique(
           ref PixelFarm.Drawing.Rectangle srcRect,
           float targetLeft,
           float targetTop,
           float scale)
        {
            //TODO: review performance here *** 
            //1. B , cyan result
            GL.ColorMask(false, false, true, false);
            textureSubPixRendering.SetCompo(0);
            textureSubPixRendering.DrawSubImage(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
            //float subpixel_shift = 1 / 9f;
            //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft - subpixel_shift, targetTop); //TODO: review this option
            //---------------------------------------------------
            //2. G , magenta result
            GL.ColorMask(false, true, false, false);
            textureSubPixRendering.SetCompo(1);
            textureSubPixRendering.DrawSubImage(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
            //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop); //TODO: review this option
            //1. R , yellow result 
            textureSubPixRendering.SetCompo(2);
            GL.ColorMask(true, false, false, false);//             
            textureSubPixRendering.DrawSubImage(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
            //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft + subpixel_shift, targetTop); //TODO: review this option
            //enable all color component
            GL.ColorMask(true, true, true, true);
        }
        public void DrawGlyphImageWithSubPixelRenderingTechnique2(
          ref PixelFarm.Drawing.Rectangle srcRect,
          float targetLeft,
          float targetTop,
          float scale)
        {
            textureSubPixRendering.NewDrawSubImage(srcRect.Left,
                srcRect.Top,
                srcRect.Width,
                srcRect.Height, targetLeft, targetTop);

        }
        public void WriteVboToList(
           System.Collections.Generic.List<float> buffer,
           System.Collections.Generic.List<ushort> indexList,
           ref PixelFarm.Drawing.Rectangle srcRect,
           float targetLeft,
           float targetTop,
           float scale)
        {
            // https://developer.apple.com/library/content/documentation/3DDrawing/Conceptual/OpenGLES_ProgrammingGuide/TechniquesforWorkingwithVertexData/TechniquesforWorkingwithVertexData.html

            ushort indexCount = (ushort)indexList.Count;

            if (indexCount > 0)
            {

                //add degenerative triangle
                float prev_5 = buffer[buffer.Count - 5];
                float prev_4 = buffer[buffer.Count - 4];
                float prev_3 = buffer[buffer.Count - 3];
                float prev_2 = buffer[buffer.Count - 2];
                float prev_1 = buffer[buffer.Count - 1];

                buffer.Add(prev_5); buffer.Add(prev_4); buffer.Add(prev_3);
                buffer.Add(prev_2); buffer.Add(prev_1);


                indexList.Add((ushort)(indexCount));
                indexList.Add((ushort)(indexCount + 1));

                indexCount += 2;
            }

            //version 3            
            textureSubPixRendering.WriteVboStream(buffer, indexCount > 0, srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);

            indexList.Add(indexCount);
            indexList.Add((ushort)(indexCount + 1));
            indexList.Add((ushort)(indexCount + 2));
            indexList.Add((ushort)(indexCount + 3));
            //---
            //add degenerate rect

        }
        public void DrawGlyphImageWithSubPixelRenderingTechnique3(
             float[] buffer,
             ushort[] indexList)
        {

            //version 3            
            textureSubPixRendering.NewDrawSubImage3(buffer, indexList);
            //textureSubPixRendering.WriteVboStream(buffer, srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
        }
        public void DrawGlyphImageWithSubPixelRenderingTechnique4(int count, float x, float y)
        {
            //x = 100;
            //y = 400;
            //this.SetCanvasOrigin((int)x, (int)y);

            textureSubPixRendering.NewDrawSubImage4FromCurrentLoadedVBO(count, x, y);
        }
        public void DrawGlyphImageWithSubPixelRenderingTechnique(
            GLBitmap bmp,
            ref PixelFarm.Drawing.Rectangle srcRect,
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
                textureSubPixRendering.SetIntensity(1f);
                //-------------------------
                //draw a serie of image***
                //-------------------------

                //TODO: review performance here ***

                //1. B , cyan result
                GL.ColorMask(false, false, true, false);
                textureSubPixRendering.SetCompo(0);
                textureSubPixRendering.DrawSubImage(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
                //float subpixel_shift = 1 / 9f;
                //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft - subpixel_shift, targetTop); //TODO: review this option
                //---------------------------------------------------
                //2. G , magenta result
                GL.ColorMask(false, true, false, false);
                textureSubPixRendering.SetCompo(1);
                textureSubPixRendering.DrawSubImage(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
                //textureSubPixRendering.DrawSubImage(r.Left, r.Top, r.Width, r.Height, targetLeft, targetTop); //TODO: review this option
                //1. R , yellow result 
                textureSubPixRendering.SetCompo(2);
                GL.ColorMask(true, false, false, false);//             
                textureSubPixRendering.DrawSubImage(srcRect.Left, srcRect.Top, srcRect.Width, srcRect.Height, targetLeft, targetTop);
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
            GLRenderVx glRenderVx = renderVx as GLRenderVx;
            if (glRenderVx == null) return;
            //
            FillGfxPath(brush, glRenderVx.gxpth);
        }
        public void FillRenderVx(Drawing.Color color, Drawing.RenderVx renderVx)
        {
            GLRenderVx glRenderVx = renderVx as GLRenderVx;
            if (glRenderVx == null) return;
            //
            if (glRenderVx.multipartTessResult != null)
            {
                FillGfxPath(color, glRenderVx.multipartTessResult);
            }
            else
            {
                FillGfxPath(color, glRenderVx.gxpth);
            }
        }
        public void FillRenderVx(Drawing.Color color, MultiPartTessResult multiPartTessResult, int index)
        {

            FillGfxPath(color, multiPartTessResult, index);

        }
        public void DrawRenderVx(Drawing.Color color, Drawing.RenderVx renderVx)
        {
            GLRenderVx glRenderVx = renderVx as GLRenderVx;
            if (glRenderVx == null) return;

            DrawGfxPath(color, glRenderVx.gxpth);
        }
        //------------------------------------------------------------------------------- 
        void FillGfxPath(Drawing.Color color, MultiPartTessResult multipartTessResult)
        {
            switch (SmoothMode)
            {
                case SmoothMode.No:
                    {

                        float saved_Width = StrokeWidth;
                        Drawing.Color saved_Color = StrokeColor;
                        //temp set stroke width to 2 amd stroke color
                        //to the same as bg color (for smooth border).
                        //and it will be set back later.
                        // 
                        StrokeColor = color;
                        StrokeWidth = 1.2f; //TODO: review this *** 

                        basicFillShader.FillTriangles(multipartTessResult, color);

                        //restore stroke width and color
                        StrokeWidth = saved_Width; //restore back
                        StrokeColor = saved_Color;
                    }
                    break;
                case SmoothMode.Smooth:
                    {

                        float saved_Width = StrokeWidth;
                        Drawing.Color saved_Color = StrokeColor;
                        //temp set stroke width to 2 amd stroke color
                        //to the same as bg color (for smooth border).
                        //and it will be set back later.
                        // 
                        StrokeColor = color;
                        StrokeWidth = 1.2f; //TODO: review this *** 

                        basicFillShader.FillTriangles(multipartTessResult, color);

                        //add smooth border
                        smoothLineShader.DrawTriangleStrips(multipartTessResult);

                        //restore stroke width and color
                        StrokeWidth = saved_Width; //restore back
                        StrokeColor = saved_Color;
                    }
                    break;
            }
        }
        void FillGfxPath(Drawing.Color color, MultiPartTessResult multipartTessResult, int index)
        {
            switch (SmoothMode)
            {
                case SmoothMode.No:
                    {

                        float saved_Width = StrokeWidth;
                        Drawing.Color saved_Color = StrokeColor;
                        //temp set stroke width to 2 amd stroke color
                        //to the same as bg color (for smooth border).
                        //and it will be set back later.
                        // 
                        StrokeColor = color;
                        StrokeWidth = 1.2f; //TODO: review this *** 

                        basicFillShader.FillTriangles(multipartTessResult, index, color);

                        //restore stroke width and color
                        StrokeWidth = saved_Width; //restore back
                        StrokeColor = saved_Color;
                    }
                    break;
                case SmoothMode.Smooth:
                    {

                        float saved_Width = StrokeWidth;
                        Drawing.Color saved_Color = StrokeColor;
                        //temp set stroke width to 2 amd stroke color
                        //to the same as bg color (for smooth border).
                        //and it will be set back later.
                        // 
                        StrokeColor = color;
                        StrokeWidth = 1.2f; //TODO: review this *** 

                        basicFillShader.FillTriangles(multipartTessResult, index, color);

                        //add smooth border
                        smoothLineShader.DrawTriangleStrips(multipartTessResult, index, color);

                        //restore stroke width and color
                        StrokeWidth = saved_Width; //restore back
                        StrokeColor = saved_Color;
                    }
                    break;
            }
        }
        public void FillGfxPath(Drawing.Color color, InternalGraphicsPath igpth)
        {
            switch (SmoothMode)
            {
                case SmoothMode.No:
                    {
                        int subPathCount = igpth.FigCount;

                        for (int i = 0; i < subPathCount; ++i)
                        {
                            Figure f = igpth.GetFig(i);
                            if (f.SupportVertexBuffer)
                            {
                                basicFillShader.FillTriangles(
                                    f.GetAreaTessAsVBO(tessTool),
                                    f.TessAreaVertexCount,
                                    color);
                            }
                            else
                            {
                                float[] tessArea = f.GetAreaTess(this.tessTool);
                                if (tessArea != null)
                                {
                                    this.basicFillShader.FillTriangles(tessArea, f.TessAreaVertexCount, color);
                                }
                            }
                        }
                    }
                    break;
                case SmoothMode.Smooth:
                    {


                        int subPathCount = igpth.FigCount;
                        float saved_Width = StrokeWidth;
                        Drawing.Color saved_Color = StrokeColor;
                        //temp set stroke width to 2 amd stroke color
                        //to the same as bg color (for smooth border).
                        //and it will be set back later.
                        // 
                        StrokeColor = color;
                        StrokeWidth = 1.5f; //TODO: review this ***
                        //
                        float[] tessArea;
                        for (int i = 0; i < subPathCount; ++i)
                        {
                            //draw each sub-path 
                            Figure f = igpth.GetFig(i);
                            if (f.SupportVertexBuffer)
                            {
                                //TODO: review here again
                                //draw area
                                basicFillShader.FillTriangles(
                                    f.GetAreaTessAsVBO(tessTool),
                                    f.TessAreaVertexCount,
                                    color);
                                //draw smooth border
                                smoothLineShader.DrawTriangleStrips(
                                    f.GetSmoothBorders(smoothBorderBuilder),
                                    f.BorderTriangleStripCount);
                            }
                            else
                            {
                                if ((tessArea = f.GetAreaTess(this.tessTool)) != null)
                                {
                                    //draw area
                                    basicFillShader.FillTriangles(tessArea, f.TessAreaVertexCount, color);
                                    //draw smooth border
                                    smoothLineShader.DrawTriangleStrips(
                                        f.GetSmoothBorders(smoothBorderBuilder),
                                        f.BorderTriangleStripCount);
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

                        int m = igpth.FigCount;
                        for (int b = 0; b < m; ++b)
                        {
                            Figure fig = igpth.GetFig(b);
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
                                this.basicFillShader.FillTriangles(tessArea, fig.TessAreaVertexCount, PixelFarm.Drawing.Color.Black);
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
                                            Drawing.LinearGradientPair firstPair = linearGradientBrush.GetFirstPair();

                                            float[] v2f, color4f;
                                            GLGradientColorProvider.CalculateLinearGradientVxs2(
                                                firstPair.x1, firstPair.y1,
                                                firstPair.x2, firstPair.y2,
                                                firstPair.c1,
                                                firstPair.c2,
                                                out v2f, out color4f);
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
                case SmoothMode.No:
                    {

                        int subPathCount = igpth.FigCount;
                        for (int i = 0; i < subPathCount; ++i)
                        {
                            Figure f = igpth.GetFig(i);
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
                case SmoothMode.Smooth:
                    {

                        StrokeColor = color;

                        float prevStrokeW = StrokeWidth;
                        //Drawing.Color prevColor = color;

                        if (prevStrokeW < 1.5f)
                        {
                            StrokeWidth = 1.5f;
                            //StrokeColor = Drawing.Color.FromArgb(200, color);
                        }

                        int subPathCount = igpth.FigCount;
                        for (int i = 0; i < subPathCount; ++i)
                        {
                            Figure f = igpth.GetFig(i);
                            smoothLineShader.DrawTriangleStrips(
                                f.GetSmoothBorders(smoothBorderBuilder),
                                f.BorderTriangleStripCount);
                        }
                        StrokeWidth = prevStrokeW;
                        //StrokeColor = prevColor;
                        //restore back 
                    }
                    break;
            }
        }
        //-------------------------------------------------------------------------------

        /// <summary>
        /// reusable rect coord
        /// </summary>
        float[] _rectCoords = new float[8];
        /// <summary>
        /// draw rect in OpenGL coord 
        /// </summary>
        /// <param name="x">left</param>
        /// <param name="y">bottom</param>
        /// <param name="w">width</param>
        /// <param name="h">height</param>
        public void DrawRect(float x, float y, float w, float h)
        {
            switch (this.SmoothMode)
            {
                case SmoothMode.Smooth:
                    {
                        int borderTriAngleCount;
                        CreatePolyLineRectCoords(x, y, w, h, _rectCoords);
                        float[] triangles = smoothBorderBuilder.BuildSmoothBorders(
                            _rectCoords,
                            true,
                            out borderTriAngleCount);

                        smoothLineShader.DrawTriangleStrips(triangles, borderTriAngleCount);
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
        }

        public int OriginX
        {
            get { return this.canvasOriginX; }
        }
        public int OriginY
        {
            get { return this.canvasOriginY; }
        }

        public void SetCanvasOrigin(int x, int y)
        {
            //int originalW = 800;
            //set new viewport
            GL.Viewport(x, y, _width, _height);
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

        static void CreatePolyLineRectCoords(
               float x, float y, float w, float h, float[] output8)
        {
            //GL coordinate
            //(0,0) is on left-lower corner

            output8[0] = x; output8[1] = y; //left, bottom
            output8[2] = x + w; output8[3] = y; //right, bottom
            output8[4] = x + w; output8[5] = y + h; //right, top
            output8[6] = x; output8[7] = y + h;//left,top

        }

        internal TessTool GetTessTool() { return tessTool; }
        internal SmoothBorderBuilder GetSmoothBorderBuilder() { return smoothBorderBuilder; }
    }
}