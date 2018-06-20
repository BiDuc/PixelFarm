﻿//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using System.IO;

using PixelFarm.CpuBlit;

using Typography.Contours;
using Typography.OpenFont;
using Typography.Rendering;
using Typography.TextLayout;
using Typography.TextServices;

namespace PixelFarm.Drawing.Fonts
{

    public class VxsTextPrinter : DevTextPrinterBase, ITextPrinter
    {
        /// <summary>
        /// target canvas
        /// </summary>
        Painter _painter;
        IFontLoader _fontLoader;
        RequestFont _reqFont;
        //----------------------------------------------------------- 
        GlyphLayout _glyphLayout = new GlyphLayout();
        UnscaledGlyphPlanList _outputUnscaledGlyphPlans = new UnscaledGlyphPlanList();
        PxScaledGlyphPlanList _outputPxScaledGlyphPlans = new PxScaledGlyphPlanList();

        Typeface _currentTypeface;
        PixelScaleLayoutEngine _pxScaleEngine;
        GlyphMeshStore _glyphMeshStore;
        //-----------------------------------------------------------
        Dictionary<InstalledFont, Typeface> _cachedTypefaces = new Dictionary<InstalledFont, Typeface>();
        //-----------------------------------------------------------

        float _currentFontSizePxScale;

        public VxsTextPrinter(Painter painter, IFontLoader fontLoader)
        {
            StartDrawOnLeftTop = true;
            //
            this._painter = painter;
            this._fontLoader = fontLoader;

            _glyphMeshStore = new GlyphMeshStore();
            _glyphMeshStore.FlipGlyphUpward = true;

            //
            _pxScaleEngine = new PixelScaleLayoutEngine();
            _pxScaleEngine.HintedFontStore = _glyphMeshStore;//share _glyphMeshStore with pixel-scale-layout-engine
            //
            //_glyphLayout.PxScaleLayout = _pxScaleEngine; //assign the pxscale-layout-engine to main glyphLayout engine
            this.PositionTechnique = PositionTechnique.OpenFont;

        }
        /// <summary>
        /// start draw on 'left-top' of a given area box
        /// </summary>
        public bool StartDrawOnLeftTop { get; set; }

        public Painter TargetCanvasPainter
        {
            get
            {
                return _painter;
            }
            set
            {
                _painter = value;
            }
        }
        bool TryGetTypeface(InstalledFont instFont, out Typeface found)
        {
            return _cachedTypefaces.TryGetValue(instFont, out found);
        }
        void RegisterTypeface(InstalledFont instFont, Typeface typeface)
        {
            _cachedTypefaces[instFont] = typeface;
        }
        /// <summary>
        /// for layout that use with our  lcd subpixel rendering technique 
        /// </summary>
        public bool UseWithLcdSubPixelRenderingTechnique
        {
            get { return _pxScaleEngine.UseWithLcdSubPixelRenderingTechnique; }
            set
            {
                _pxScaleEngine.UseWithLcdSubPixelRenderingTechnique = value;
            }
        }
        public void ChangeFont(RequestFont font)
        {
            //1.  resolve actual font file
            this._reqFont = font;
            InstalledFont installedFont = _fontLoader.GetFont(font.Name, font.Style.ConvToInstalledFontStyle());
            Typeface foundTypeface;

            if (!TryGetTypeface(installedFont, out foundTypeface))
            {
                //if not found then create a new one
                //if not found
                //create the new one
                using (FileStream fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    foundTypeface = reader.Read(fs);
                    foundTypeface.Filename = installedFont.FontPath;
                }
                RegisterTypeface(installedFont, foundTypeface);
            }

            this.Typeface = foundTypeface;
            this.FontSizeInPoints = font.SizeInPoints;

        }
        public void ChangeFillColor(Color fontColor)
        {
            //change font color

#if DEBUG
            Console.Write("please impl change font color");
#endif
        }
        public void ChangeStrokeColor(Color strokeColor)
        {

        }

        protected override void OnFontSizeChanged()
        {
            //update some font metrics property   
            Typeface currentTypeface = _currentTypeface;
            if (currentTypeface != null)
            {
                float pointToPixelScale = currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }

        }
        public override GlyphLayout GlyphLayoutMan
        {
            get
            {
                return _glyphLayout;
            }
        }

        public override Typeface Typeface
        {
            get
            {
                return _currentTypeface;
            }
            set
            {

                if (_currentTypeface == value) return;
                //
                _currentTypeface = value;
                OnFontSizeChanged();
            }
        }


        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len)
        {

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            Typeface typeface = _currentTypeface;// _glyphPathBuilder.Typeface;            
            _glyphLayout.Typeface = typeface;
            _glyphLayout.Layout(text, startAt, len);
            //
            //3. scale from design unit to specific font size
            _outputPxScaledGlyphPlans.Clear();
            _pxScaleEngine.Layout(_glyphLayout.ResultUnscaledGlyphPositions, _outputPxScaledGlyphPlans);
            TextPrinterHelper.CopyGlyphPlans(renderVx, _outputUnscaledGlyphPlans, this._currentFontSizePxScale);
        }

        public override void DrawCaret(float x, float y)
        {

            Painter p = this.TargetCanvasPainter;
            PixelFarm.Drawing.Color prevColor = p.StrokeColor;
            p.StrokeColor = PixelFarm.Drawing.Color.Red;
            p.DrawLine(x, y, x, y + this.FontAscendingPx);
            p.StrokeColor = prevColor;

        }

        public void UpdateGlyphLayoutSettings()
        {
            if (this._reqFont == null)
            {
                //this.ScriptLang = canvasPainter.CurrentFont.GetOpenFontScriptLang();
                ChangeFont(_painter.CurrentFont);
            }

            //2.1              
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            //2.2
            _glyphLayout.Typeface = this.Typeface;
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;

            _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);

            //2.3
            if (_pxScaleEngine != null)
            {
                _pxScaleEngine.SetFont(this.Typeface, this.FontSizeInPoints);
            }
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?




            //3.
            //color...
        }

        /// <summary>
        /// draw specfic glyph with current settings, at specific position
        /// </summary>
        /// <param name="glyph"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawGlyph(Glyph glyph, double x, double y)
        {
            //TODO...
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            //TODO: review here
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            _glyphMeshStore.SetFont(_currentTypeface, this.FontSizeInPoints);
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel? 
            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);
            RenderVxGlyphPlan[] glyphPlans = renderVx.glyphList;
            int j = glyphPlans.Length;
            //---------------------------------------------------
            //consider use cached glyph, to increase performance 

            //GlyphPosPixelSnapKind x_snap = this.GlyphPosPixelSnapX;
            //GlyphPosPixelSnapKind y_snap = this.GlyphPosPixelSnapY;
            float g_x = 0;
            float g_y = 0;
            float baseY = (int)y;

            for (int i = 0; i < j; ++i)
            {
                RenderVxGlyphPlan glyphPlan = glyphPlans[i];
                //-----------------------------------
                //TODO: review here ***
                //PERFORMANCE revisit here 
                //if we have create a vxs we can cache it for later use?
                //-----------------------------------  
                VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                g_x = (float)(glyphPlan.x * scale + x);
                g_y = (float)glyphPlan.y * scale;

                _painter.SetOrigin(g_x, g_y);
                _painter.Fill(vxs);
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }


        public override void DrawFromGlyphPlans(PxScaledGlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        {
            Painter painter = this.TargetCanvasPainter;
            if (StartDrawOnLeftTop)
            {
                //version 2
                //offset y down 
                y += this.FontLineSpacingPx;
            }


            //Typeface typeface = _glyphPathBuilder.Typeface;
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?

            float fontSizePoint = this.FontSizeInPoints;
            //float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);
            float scale = 1;

            //4. render each glyph 
            float ox = painter.OriginX;
            float oy = painter.OriginY;
            int endBefore = startAt + len;

            Typography.OpenFont.Tables.COLR colrTable = _currentTypeface.COLRTable;
            Typography.OpenFont.Tables.CPAL cpalTable = _currentTypeface.CPALTable;
            bool hasColorGlyphs = (colrTable != null) && (cpalTable != null);

            //--------------------------------------------------- 
            _glyphMeshStore.SetFont(_currentTypeface, fontSizePoint);
            //---------------------------------------------------

            float g_x = 0;
            float g_y = 0;
            float baseY = (int)y;
            if (!hasColorGlyphs)
            {

                bool savedUseLcdMode = painter.UseSubPixelLcdEffect; //save,restore later
                RenderQualtity savedRederQuality = painter.RenderQuality;
                painter.RenderQuality = RenderQualtity.HighQuality;
                painter.UseSubPixelLcdEffect = true;

                CpuBlit.VertexProcessing.Affine flipY = CpuBlit.VertexProcessing.Affine.NewMatix(
                    CpuBlit.VertexProcessing.AffinePlan.Scale(1, -1)); //flip Y

                VertexStore reusableVxs = new VertexStore();

                float acc_x = 0; //acummulate x
                float acc_y = 0; //acummulate y

                for (int i = startAt; i < endBefore; ++i)
                {   //-----------------------------------
                    //TODO: review here ***
                    //PERFORMANCE revisit here 
                    //if we have create a vxs we can cache it for later use?
                    //-----------------------------------   
                    PxScaledGlyphPlan glyphPlan = glyphPlanList[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);


                    g_x = ngx;
                    g_y = ngy;
                    painter.SetOrigin(g_x, g_y);

                    //-----------------------------------  

                    //invert each glyph 
                    //version 3:

                    reusableVxs.Clear();
                    VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                    PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(flipY, vxs, reusableVxs);
                    painter.Fill(reusableVxs);


                    //version2; 
                    //VertexStore vsx = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                    //_vxs1 = _invertY.TransformToVxs(vsx, _vxs1);
                    //painter.Fill(_vxs1);
                    //_vxs1.Clear();

                    //version1
                    //painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                }
                //restore
                painter.RenderQuality = savedRederQuality;
                painter.UseSubPixelLcdEffect = savedUseLcdMode;

            }
            else
            {
                //-------------    
                //this glyph has color information
                //-------------
                Color originalFillColor = painter.FillColor;

                float acc_x = 0;
                float acc_y = 0;
                for (int i = startAt; i < endBefore; ++i)
                {
                    PxScaledGlyphPlan glyphPlan = glyphPlanList[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    g_x = ngx;
                    g_y = ngy;

                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);
                    painter.SetOrigin(g_x, g_y);


                    //-----------------------------------  
                    ushort colorLayerStart;
                    if (colrTable.LayerIndices.TryGetValue(glyphPlan.glyphIndex, out colorLayerStart))
                    {
                        //TODO: optimize this                        
                        //we found color info for this glyph 
                        ushort colorLayerCount = colrTable.LayerCounts[glyphPlan.glyphIndex];
                        byte r, g, b, a;
                        for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                        {
                            ushort gIndex = colrTable.GlyphLayers[c];

                            int palette = 0; // FIXME: assume palette 0 for now 
                            cpalTable.GetColor(
                                cpalTable.Palettes[palette] + colrTable.GlyphPalettes[c], //index
                                out r, out g, out b, out a);
                            //-----------  
                            painter.FillColor = new Color(r, g, b);//? a component
                            painter.Fill(_glyphMeshStore.GetGlyphMesh(gIndex));
                        }
                    }
                    else
                    {
                        //-----------------------------------
                        //TODO: review here ***
                        //PERFORMANCE revisit here 
                        //if we have create a vxs we can cache it for later use?
                        //----------------------------------- 
                        painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                    }
                }
                painter.FillColor = originalFillColor; //restore color
            }
            //restore prev origin
            painter.SetOrigin(ox, oy);
        }
        public override void DrawFromGlyphPlans(UnscaledGlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        {
            Painter painter = this.TargetCanvasPainter;
            if (StartDrawOnLeftTop)
            {
                //version 2
                //offset y down 
                y += this.FontLineSpacingPx;
            }


            //Typeface typeface = _glyphPathBuilder.Typeface;
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?

            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);


            //4. render each glyph 
            float ox = painter.OriginX;
            float oy = painter.OriginY;
            int endBefore = startAt + len;

            Typography.OpenFont.Tables.COLR colrTable = _currentTypeface.COLRTable;
            Typography.OpenFont.Tables.CPAL cpalTable = _currentTypeface.CPALTable;
            bool hasColorGlyphs = (colrTable != null) && (cpalTable != null);

            //--------------------------------------------------- 
            _glyphMeshStore.SetFont(_currentTypeface, fontSizePoint);
            //---------------------------------------------------

            float g_x = 0;
            float g_y = 0;
            float baseY = (int)y;
            if (!hasColorGlyphs)
            {

                bool savedUseLcdMode = painter.UseSubPixelLcdEffect; //save,restore later
                RenderQualtity savedRederQuality = painter.RenderQuality;
                painter.RenderQuality = RenderQualtity.HighQuality;
                painter.UseSubPixelLcdEffect = true;

                CpuBlit.VertexProcessing.Affine flipY = CpuBlit.VertexProcessing.Affine.NewMatix(
                    CpuBlit.VertexProcessing.AffinePlan.Scale(1, -1)); //flip Y
                VertexStore reusableVxs = new VertexStore();

                float acc_x = 0; //acummulate x
                float acc_y = 0; //acummulate y

                for (int i = startAt; i < endBefore; ++i)
                {   //-----------------------------------
                    //TODO: review here ***
                    //PERFORMANCE revisit here 
                    //if we have create a vxs we can cache it for later use?
                    //-----------------------------------   
                    UnscaledGlyphPlan glyphPlan = glyphPlanList[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);


                    g_x = ngx;
                    g_y = ngy;
                    painter.SetOrigin(g_x, g_y);

                    //-----------------------------------  

                    //invert each glyph 
                    //version 3:

                    reusableVxs.Clear();
                    VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                    PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(flipY, vxs, reusableVxs);
                    painter.Fill(reusableVxs);


                    //version2; 
                    //VertexStore vsx = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                    //_vxs1 = _invertY.TransformToVxs(vsx, _vxs1);
                    //painter.Fill(_vxs1);
                    //_vxs1.Clear();

                    //version1
                    //painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                }
                //restore
                painter.RenderQuality = savedRederQuality;
                painter.UseSubPixelLcdEffect = savedUseLcdMode;

            }
            else
            {
                //-------------    
                //this glyph has color information
                //-------------
                Color originalFillColor = painter.FillColor;

                float acc_x = 0;
                float acc_y = 0;
                for (int i = startAt; i < endBefore; ++i)
                {
                    UnscaledGlyphPlan glyphPlan = glyphPlanList[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    g_x = ngx;
                    g_y = ngy;

                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);
                    painter.SetOrigin(g_x, g_y);


                    //-----------------------------------  
                    ushort colorLayerStart;
                    if (colrTable.LayerIndices.TryGetValue(glyphPlan.glyphIndex, out colorLayerStart))
                    {
                        //TODO: optimize this                        
                        //we found color info for this glyph 
                        ushort colorLayerCount = colrTable.LayerCounts[glyphPlan.glyphIndex];
                        byte r, g, b, a;
                        for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                        {
                            ushort gIndex = colrTable.GlyphLayers[c];

                            int palette = 0; // FIXME: assume palette 0 for now 
                            cpalTable.GetColor(
                                cpalTable.Palettes[palette] + colrTable.GlyphPalettes[c], //index
                                out r, out g, out b, out a);
                            //-----------  
                            painter.FillColor = new Color(r, g, b);//? a component
                            painter.Fill(_glyphMeshStore.GetGlyphMesh(gIndex));
                        }
                    }
                    else
                    {
                        //-----------------------------------
                        //TODO: review here ***
                        //PERFORMANCE revisit here 
                        //if we have create a vxs we can cache it for later use?
                        //----------------------------------- 
                        painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                    }
                }
                painter.FillColor = originalFillColor; //restore color
            }
            //restore prev origin
            painter.SetOrigin(ox, oy);
        }

        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            InternalDrawString(text, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            InternalDrawString(textBuffer, startAt, len, x, y);
        }


        void InternalDrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            UpdateGlyphLayoutSettings();
            //unscale layout, with design unit scale
            _glyphLayout.Layout(textBuffer, startAt, len);
            //
            //

            //
            if (this._pxScaleEngine != null)
            {
                _outputPxScaledGlyphPlans.Clear();
                //scale to specific font size 
                _pxScaleEngine.Layout(_glyphLayout.ResultUnscaledGlyphPositions, _outputPxScaledGlyphPlans);
                DrawFromGlyphPlans(_outputPxScaledGlyphPlans, x, y);
            }
            else
            {
                _outputUnscaledGlyphPlans.Clear();
                //no custom engine
                //then use default scale  
                GlyphLayoutExtensions.GenerateGlyphPlans(
                    _glyphLayout.ResultUnscaledGlyphPositions,
                    _currentFontSizePxScale,
                    false,
                    _outputPxScaledGlyphPlans);
                DrawFromGlyphPlans(_outputUnscaledGlyphPlans, x, y);

            }


        }

    }

    public static class TextPrinterHelper
    {
        public static void CopyGlyphPlans(RenderVxFormattedString renderVx, PxScaledGlyphPlanList glyphPlans)
        {
            int n = glyphPlans.Count;
            //copy 
            var renderVxGlyphPlans = new RenderVxGlyphPlan[n];
            float acc_x = 0;
            float acc_y = 0;
            float x = 0;
            float y = 0;
            float g_x = 0;
            float g_y = 0;

            for (int i = 0; i < n; ++i)
            {
                PxScaledGlyphPlan glyphPlan = glyphPlans[i];


                float ngx = acc_x + glyphPlan.OffsetX;
                float ngy = acc_y + glyphPlan.OffsetY;
                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------
                g_x = (float)(x + ngx); //ideal x
                g_y = (float)(y + ngy);


                float g_w = glyphPlan.AdvanceX;
                acc_x += g_w;

                //g_x = (float)Math.Round(g_x);
                g_y = (float)Math.Floor(g_y);


                renderVxGlyphPlans[i] = new RenderVxGlyphPlan(
                    glyphPlan.glyphIndex,
                    g_x,
                    g_y,
                    g_w
                    );
            }
            renderVx.glyphList = renderVxGlyphPlans;
        }
        public static void CopyGlyphPlans(RenderVxFormattedString renderVx, UnscaledGlyphPlanList glyphPlans, float scale)
        {
            int n = glyphPlans.Count;
            //copy 
            var renderVxGlyphPlans = new RenderVxGlyphPlan[n];
            float acc_x = 0;
            float acc_y = 0;
            float x = 0;
            float y = 0;
            float g_x = 0;
            float g_y = 0;

            for (int i = 0; i < n; ++i)
            {
                UnscaledGlyphPlan glyphPlan = glyphPlans[i];


                float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);
                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------
                g_x = (float)(x + ngx); //ideal x
                g_y = (float)(y + ngy);


                float g_w = (float)Math.Round(glyphPlan.AdvanceX * scale);
                acc_x += g_w;

                //g_x = (float)Math.Round(g_x);
                g_y = (float)Math.Floor(g_y);


                renderVxGlyphPlans[i] = new RenderVxGlyphPlan(
                    glyphPlan.glyphIndex,
                    g_x,
                    g_y,
                    g_w
                    );
            }
            renderVx.glyphList = renderVxGlyphPlans;
        }
    }


}
