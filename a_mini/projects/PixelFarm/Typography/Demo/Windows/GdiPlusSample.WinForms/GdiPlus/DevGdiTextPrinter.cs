﻿//MIT, 2016-2017, WinterDev
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
//
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;
using System;
using Typography.FontManagement;

namespace SampleWinForms
{


    /// <summary>
    /// developer's version, Gdi+ text printer
    /// </summary>
    class DevGdiTextPrinter : DevTextPrinterBase
    {
        Typeface _currentTypeface;
        GlyphPathBuilder _currentGlyphPathBuilder;
        GlyphTranslatorToGdiPath _txToGdiPath;
        GlyphLayout _glyphLayout = new GlyphLayout();
        SolidBrush _fillBrush = new SolidBrush(Color.Black);
        Pen _outlinePen = new Pen(Color.Green);
        GlyphMeshCollection<GraphicsPath> _glyphMeshCollections = new GlyphMeshCollection<GraphicsPath>();


        FontRequest _currentFontRequest;
        FontStreamSource _currentFontStreamSource;

        public DevGdiTextPrinter()
        {
            FillBackground = true;
            FillColor = Color.Black;
            OutlineColor = Color.Green;
        }
        public override FontRequest FontRequest
        {
            get { return _currentFontRequest; }
            set
            {
                _currentFontRequest = value;
                //resolve the request font

            }
        }
        public override FontStreamSource FontStreamSource
        {
            get { return _currentFontStreamSource; }
            set
            {
                if (value == this._currentFontStreamSource)
                {
                    return;
                }

                //--------------------------------
                //reset 
                _currentTypeface = null;
                _currentGlyphPathBuilder = null;
                _currentFontStreamSource = value;
                //load new typeface 
                if (value == null)
                {
                    return;
                }
                //--------------------------------
                //1. read typeface from font file
                //TODO: review how to read font data again ***
                using (Stream fontstream = value.ReadFontStream())
                {
                    var reader = new OpenFontReader();
                    _currentTypeface = reader.Read(fontstream);
                }
                //2. glyph builder
                _currentGlyphPathBuilder = new GlyphPathBuilder(_currentTypeface);
                //for gdi path***
                //3. glyph reader,output as Gdi+ GraphicsPath
                _txToGdiPath = new GlyphTranslatorToGdiPath();
                //4.
                OnFontSizeChanged();
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
        }
        protected override void OnFontSizeChanged()
        {
            //update some font matrix property  
            if (_currentTypeface != null)
            {
                float pointToPixelScale = _currentTypeface.CalculateToPixelScaleFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = _currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = _currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = _currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }

        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics TargetGraphics { get; set; }

        public override void DrawCaret(float xpos, float ypos)
        {
            this.TargetGraphics.DrawLine(Pens.Red, xpos, ypos, xpos, ypos + this.FontAscendingPx);
        }

        List<GlyphPlan> _outputGlyphPlans = new List<GlyphPlan>();//for internal use
        public override void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos)
        {
            UpdateGlyphLayoutSettings();
            _outputGlyphPlans.Clear();
            this._glyphLayout.GenerateGlyphPlans(textBuffer, startAt, len, _outputGlyphPlans, null);

            DrawFromGlyphPlans(_outputGlyphPlans, xpos, ypos);
        }
        void UpdateGlyphLayoutSettings()
        {
            _glyphLayout.Typeface = this.Typeface;
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;
        }
        void UpdateVisualOutputSettings()
        {
            _currentGlyphPathBuilder.SetHintTechnique(this.HintTechnique);
            _fillBrush.Color = this.FillColor;
            _outlinePen.Color = this.OutlineColor;
        }
        public override void DrawFromGlyphPlans(List<GlyphPlan> glyphPlanList, int startAt, int len, float x, float y)
        {
            UpdateVisualOutputSettings();

            //draw data in glyph plan 
            //3. render each glyph 
            System.Drawing.Drawing2D.Matrix scaleMat = null;
            float sizeInPoints = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateToPixelScaleFromPointSize(sizeInPoints);
            //
            _glyphMeshCollections.SetCacheInfo(this.Typeface, sizeInPoints, this.HintTechnique);


            //this draw a single line text span***
            int endBefore = startAt + len;

            Graphics g = this.TargetGraphics;
            for (int i = startAt; i < endBefore; ++i)
            {
                GlyphPlan glyphPlan = glyphPlanList[i];

                //check if we have a cache of this glyph
                //if not -> create it

                GraphicsPath foundPath;
                if (!_glyphMeshCollections.TryGetCacheGlyph(glyphPlan.glyphIndex, out foundPath))
                {
                    //if not found then create a new one
                    _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                    _txToGdiPath.Reset();
                    _currentGlyphPathBuilder.ReadShapes(_txToGdiPath);
                    foundPath = _txToGdiPath.ResultGraphicsPath;

                    //register
                    _glyphMeshCollections.RegisterCachedGlyph(glyphPlan.glyphIndex, foundPath);
                }
                //------
                //then move pen point to the position we want to draw a glyph
                float tx = x + glyphPlan.x * scale;
                float ty = y + glyphPlan.y * scale;
                g.TranslateTransform(tx, ty);

                if (FillBackground)
                {
                    g.FillPath(_fillBrush, foundPath);
                }
                if (DrawOutline)
                {
                    g.DrawPath(_outlinePen, foundPath);
                }
                //and then we reset back ***
                g.TranslateTransform(-tx, -ty);
            }
        }
    }
}
