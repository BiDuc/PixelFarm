﻿//MIT, 2016-2017, WinterDev

using System;
using System.Collections.Generic;
//
using PixelFarm.Agg;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
using Typography.TextLayout;
#if GL_ENABLE

namespace PixelFarm.DrawingGL
{



    public class AggTextSpanPrinter : ITextPrinter
    {
        ActualImage actualImage;
        ImageGraphics2D imgGfx2d;
        AggCanvasPainter _aggPainter;
        VxsTextPrinter textPrinter;
        int bmpWidth;
        int bmpHeight;
        CanvasGL2d canvas;
        GLCanvasPainter canvasPainter;

        public AggTextSpanPrinter(GLCanvasPainter canvasPainter, int w, int h)
        {
            //this class print long text into agg canvas
            //then copy pixel buffer from aff canvas to gl-bmp
            //then draw the  gl-bmp into target gl canvas


            //TODO: review here
            this.canvasPainter = canvasPainter;
            this.canvas = canvasPainter.Canvas;
            bmpWidth = w;
            bmpHeight = h;
            actualImage = new ActualImage(bmpWidth, bmpHeight, PixelFormat.ARGB32);

            imgGfx2d = new ImageGraphics2D(actualImage);
            _aggPainter = new AggCanvasPainter(imgGfx2d);
            _aggPainter.FillColor = Color.Black;
            _aggPainter.StrokeColor = Color.Black;

            //set default1
            _aggPainter.CurrentFont = canvasPainter.CurrentFont;
            var openFontStore = new PixelFarm.Drawing.Fonts.OpenFontStore();
            textPrinter = new VxsTextPrinter(_aggPainter, openFontStore);
            _aggPainter.TextPrinter = textPrinter;
        }
        public Typography.Contours.HintTechnique HintTechnique
        {
            get { return textPrinter.HintTechnique; }
            set { textPrinter.HintTechnique = value; }
        }
        public bool UseSubPixelRendering
        {
            get { return _aggPainter.UseSubPixelRendering; }
            set
            {
                _aggPainter.UseSubPixelRendering = value;
            }
        }
        public void ChangeFont(RequestFont font)
        {

            _aggPainter.CurrentFont = font;
        }
        public void ChangeFillColor(Color fillColor)
        {
            //we use agg canvas to draw a font glyph
            //so we must set fill color for this
            _aggPainter.FillColor = fillColor;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //we use agg canvas to draw a font glyph
            //so we must set fill color for this
            _aggPainter.StrokeColor = strokeColor;
        }
        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {


            if (this.UseSubPixelRendering)
            {
                //1. clear prev drawing result
                _aggPainter.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
                //aggPainter.Clear(Drawing.Color.White);
                //aggPainter.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
                //2. print text span into Agg Canvas
                textPrinter.DrawString(text, startAt, len, 0, 0);
                //3.copy to gl bitmap
                byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(actualImage);
                //------------------------------------------------------
                GLBitmap glBmp = new GLBitmap(bmpWidth, bmpHeight, buffer, true);
                glBmp.IsInvert = false;
                //TODO: review font height
                canvas.DrawGlyphImageWithSubPixelRenderingTechnique(glBmp, (float)x, (float)y + 40);
                glBmp.Dispose();
            }
            else
            {

                //1. clear prev drawing result
                _aggPainter.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
                //2. print text span into Agg Canvas
                textPrinter.DrawString(text, startAt, len, 0, 0);
                //3.copy to gl bitmap
                byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(actualImage);
                //------------------------------------------------------
                GLBitmap glBmp = new GLBitmap(bmpWidth, bmpHeight, buffer, true);
                glBmp.IsInvert = false;
                //TODO: review font height
                canvas.DrawGlyphImage(glBmp, (float)x, (float)y + 40);
                glBmp.Dispose();
            }

        }
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int start, int len)
        {
            throw new NotImplementedException();
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            throw new NotImplementedException();
        }
    }


    delegate GLBitmap LoadNewGLBitmapDel<T>(T src);

    class GLBitmapCache<T> : IDisposable
    {
        Dictionary<T, GLBitmap> _loadedGLBmps = new Dictionary<T, GLBitmap>();
        LoadNewGLBitmapDel<T> _loadNewGLBmpDel;
        public GLBitmapCache(LoadNewGLBitmapDel<T> loadNewGLBmpDel)
        {
            _loadNewGLBmpDel = loadNewGLBmpDel;
        }
        public GLBitmap GetOrCreateNewOne(T key)
        {
            GLBitmap found;
            if (!_loadedGLBmps.TryGetValue(key, out found))
            {

                return _loadedGLBmps[key] = _loadNewGLBmpDel(key);
            }
            return found;
        }
        public void Dispose()
        {
            Clear();
        }
        public void Clear()
        {
            foreach (GLBitmap glbmp in _loadedGLBmps.Values)
            {
                glbmp.Dispose();
            }
            _loadedGLBmps.Clear();
        }
        public void Delete(T key)
        {
            GLBitmap found;
            if (_loadedGLBmps.TryGetValue(key, out found))
            {
                found.Dispose();
                _loadedGLBmps.Remove(key);
            }
        }
    }


    public class GLBmpGlyphTextPrinter : ITextPrinter, IDisposable
    {

        GLBitmapCache<SimpleFontAtlas> _loadedGlyphs;

        //--------
        GlyphLayout _glyphLayout = new GlyphLayout();
        CanvasGL2d canvas2d;
        GLCanvasPainter painter;
        SimpleFontAtlas simpleFontAtlas;
        IFontLoader _fontLoader;
        GLBitmap _glBmp;
        RequestFont font;


        ScriptLang _defaultScriptLang = ScriptLangs.Latin;//review here again
        public GLBmpGlyphTextPrinter(GLCanvasPainter painter, IFontLoader fontLoader)
        {
            //create text printer for use with canvas painter 
            this.painter = painter;
            this.canvas2d = painter.Canvas;
            //GlyphPosPixelSnapX = GlyphPosPixelSnapKind.Integer;
            //GlyphPosPixelSnapY = GlyphPosPixelSnapKind.Integer;

            _fontLoader = fontLoader;
            ChangeFont(painter.CurrentFont);
            this._glyphLayout.ScriptLang = ScriptLangConv.GetOpenFontScriptLang(_defaultScriptLang.shortname);

            _loadedGlyphs = new GLBitmapCache<SimpleFontAtlas>(atlas =>
            {
                //create new one
                Typography.Rendering.GlyphImage totalGlyphImg = atlas.TotalGlyph;
                //load to glbmp 
                GLBitmap found = new GLBitmap(totalGlyphImg.Width, totalGlyphImg.Height, totalGlyphImg.GetImageBuffer(), false);
                found.IsInvert = false;
                return found;
            });

        }
        public void ChangeFillColor(Color color)
        {
            //called by owner painter   
            canvas2d.FontFillColor = color;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //TODO: implementation here

        }

        public void ChangeFont(RequestFont font)
        {
            //from request font
            //we resolve it to actual font

            this.font = font;
            this._glyphLayout.ScriptLang = ScriptLangConv.GetOpenFontScriptLang(_defaultScriptLang.shortname);

            SimpleFontAtlas foundFontAtlas;
            ActualFont fontImp = ActiveFontAtlasService.GetTextureFontAtlasOrCreateNew(_fontLoader, font, out foundFontAtlas);
            if (foundFontAtlas != this.simpleFontAtlas)
            {
                //change to another font atlas
                _glBmp = null;
                this.simpleFontAtlas = foundFontAtlas;
            }

            _typeface = (Typography.OpenFont.Typeface)fontImp.FontFace.GetInternalTypeface();
            float srcTextureScale = _typeface.CalculateToPixelScaleFromPointSize(simpleFontAtlas.OriginalFontSizePts);
            //scale at request
            float targetTextureScale = _typeface.CalculateToPixelScaleFromPointSize(font.SizeInPoints);
            _finalTextureScale = targetTextureScale / srcTextureScale;
        }
        public void Dispose()
        {
            _loadedGlyphs.Clear();

            if (_glBmp != null)
            {
                _glBmp.Dispose();
                _glBmp = null;
            }
        }
        static PixelFarm.Drawing.Rectangle ConvToRect(Typography.Contours.Rectangle r)
        {
            //TODO: review here
            return PixelFarm.Drawing.Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
        }


        //-----------
        List<GlyphPlan> glyphPlans = new List<GlyphPlan>();
        Typography.OpenFont.Typeface _typeface;
        float _finalTextureScale = 1;
        //-----------

        void EnsureLoadGLBmp()
        {
            if (_glBmp == null)
            {
                _glBmp = _loadedGlyphs.GetOrCreateNewOne(simpleFontAtlas);
            }
        }

        public void DrawString(char[] buffer, int startAt, int len, double x, double y)
        {


            int j = buffer.Length;
            //resolve font from painter?  
            glyphPlans.Clear();
            _glyphLayout.Layout(_typeface, buffer, startAt, len, glyphPlans);
            float scale = _typeface.CalculateToPixelScaleFromPointSize(font.SizeInPoints);

            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again
            y -= (_typeface.Ascender - _typeface.Descender + _typeface.LineGap) * scale;

            int n = glyphPlans.Count;
            EnsureLoadGLBmp();
            // 
            float scaleFromTexture = _finalTextureScale;
            Typography.Rendering.TextureKind textureKind = simpleFontAtlas.TextureKind;
            //--------------------------

            //TODO: review render steps 
            //NOTE:
            // -glyphData.TextureXOffset => restore to original pos
            // -glyphData.TextureYOffset => restore to original pos
            // ideal_x = (float)(x + (glyph.x * scale - glyphData.TextureXOffset) * scaleFromTexture);
            // ideal_y = (float)(y + (glyph.y * scale - glyphData.TextureYOffset + srcRect.Height) * scaleFromTexture);
            //--------------------------

            float g_x = 0;
            float g_y = 0;
            int baseY = (int)Math.Round(y);
            for (int i = 0; i < n; ++i)
            {
                GlyphPlan glyph = glyphPlans[i];
                Typography.Rendering.TextureFontGlyphData glyphData;
                if (!simpleFontAtlas.TryGetGlyphDataByCodePoint(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //-------------------------------------- 
                PixelFarm.Drawing.Rectangle srcRect = ConvToRect(glyphData.Rect);


                switch (textureKind)
                {
                    case Typography.Rendering.TextureKind.Msdf:

                        canvas2d.DrawSubImageWithMsdf(_glBmp,
                            ref srcRect,
                            g_x,
                            g_y,
                            scaleFromTexture);

                        break;
                    case Typography.Rendering.TextureKind.AggGrayScale:

                        canvas2d.DrawSubImage(_glBmp,
                         ref srcRect,
                            g_x,
                            g_y,
                            scaleFromTexture);

                        break;
                    case Typography.Rendering.TextureKind.AggSubPixel:

                        canvas2d.DrawGlyphImageWithSubPixelRenderingTechnique(_glBmp,
                             ref srcRect,
                             g_x,
                             g_y,
                             scaleFromTexture);

                        break;
                }
            }
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            RenderVxGlyphPlan[] glyphPlans = renderVx.glyphList;
            int n = glyphPlans.Length;
            EnsureLoadGLBmp();

            //PERF:
            //TODO: review here, can we cache the glbmp for later use
            //not to create it every time 

            float scaleFromTexture = _finalTextureScale;

            Typography.Rendering.TextureKind textureKind = simpleFontAtlas.TextureKind;
            float g_x = 0;
            float g_y = 0;
            int baseY = (int)Math.Round(y);
            float scale = 1;

            for (int i = 0; i < n; ++i)
            {
                //PERF:
                //TODO: 
                //render a set of glyph instead of one glyph per time ***
                RenderVxGlyphPlan glyph = glyphPlans[i];
                Typography.Rendering.TextureFontGlyphData glyphData;
                if (!simpleFontAtlas.TryGetGlyphDataByCodePoint(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***

                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //-------------------------------------- 
                PixelFarm.Drawing.Rectangle srcRect = ConvToRect(glyphData.Rect);
                //--------------------------
                g_x = (float)(x + (glyph.x * scale - glyphData.TextureXOffset) * scaleFromTexture); //ideal x
                g_y = (float)((glyph.y * scale - glyphData.TextureYOffset + srcRect.Height) * scaleFromTexture);

                switch (textureKind)
                {
                    case Typography.Rendering.TextureKind.Msdf:

                        canvas2d.DrawSubImageWithMsdf(_glBmp,
                            ref srcRect,
                            g_x,
                            g_y,
                            scaleFromTexture);

                        break;
                    case Typography.Rendering.TextureKind.AggGrayScale:

                        canvas2d.DrawSubImage(_glBmp,
                         ref srcRect,
                            g_x,
                            g_y,
                            scaleFromTexture);

                        break;
                    case Typography.Rendering.TextureKind.AggSubPixel:
                        canvas2d.DrawGlyphImageWithSubPixelRenderingTechnique(_glBmp,
                                ref srcRect,
                                g_x,
                                g_y,
                                scaleFromTexture);
                        break;
                }

            }
        }

        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] buffer, int startAt, int len)
        {
            glyphPlans.Clear();
            _glyphLayout.Layout(_typeface, buffer, startAt, len, glyphPlans);

            TextPrinterHelper.CopyGlyphPlans(renderVx, glyphPlans, _typeface.CalculateToPixelScaleFromPointSize(font.SizeInPoints));
        }
    }

}


#endif