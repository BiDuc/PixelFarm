﻿//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;

using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;

using Typography.OpenFont;

namespace Typography.Contours
{
    public struct GlyphControlParameters
    {
        public float avgXOffsetToFit;
        public short minX;
        public short minY;
        public short maxX;
        public short maxY;
    }
    class GlyphMeshStore
    {

        class GlyphMeshData
        {
            public GlyphDynamicOutline dynamicOutline;
            public VertexStore vxsStore;
            public float avgXOffsetToFit;
            public Bounds orgBounds;

            public GlyphControlParameters GetControlPars()
            {
                var pars = new GlyphControlParameters();
                pars.minX = orgBounds.XMin;
                pars.minY = orgBounds.YMin;
                pars.maxX = orgBounds.XMax;
                pars.maxY = orgBounds.YMax;
                pars.avgXOffsetToFit = avgXOffsetToFit;
                return pars;
            }

        }
        /// <summary>
        /// store typeface and its builder
        /// </summary>
        Dictionary<Typeface, GlyphPathBuilder> _cacheGlyphPathBuilders = new Dictionary<Typeface, GlyphPathBuilder>();
        /// <summary>
        /// glyph mesh data for specific condition
        /// </summary>
        GlyphMeshCollection<GlyphMeshData> _hintGlyphCollection = new GlyphMeshCollection<GlyphMeshData>();

        GlyphPathBuilder _currentGlyphBuilder;
        Typeface _currentTypeface;
        float _currentFontSizeInPoints;
        HintTechnique _currentHintTech;


        GlyphTranslatorToVxs _tovxs = new GlyphTranslatorToVxs();

        public GlyphMeshStore()
        {

        }
        public void SetHintTechnique(HintTechnique hintTech)
        {
            _currentHintTech = hintTech;

        }

        /// <summary>
        /// set current font
        /// </summary>
        /// <param name="typeface"></param>
        /// <param name="fontSizeInPoints"></param>
        public void SetFont(Typeface typeface, float fontSizeInPoints)
        {
            //temp fix,            


            if (_currentGlyphBuilder != null && !_cacheGlyphPathBuilders.ContainsKey(typeface))
            {
                //store current typeface to cache
                _cacheGlyphPathBuilders[_currentTypeface] = _currentGlyphBuilder;
            }
            _currentTypeface = typeface;
            _currentGlyphBuilder = null;
            if (typeface == null) return;

            //----------------------------
            //check if we have this in cache ?
            //if we don't have it, this _currentTypeface will set to null ***                  
            _cacheGlyphPathBuilders.TryGetValue(_currentTypeface, out _currentGlyphBuilder);
            if (_currentGlyphBuilder == null)
            {
                _currentGlyphBuilder = new GlyphPathBuilder(typeface);
            }
            //----------------------------------------------
            this._currentFontSizeInPoints = fontSizeInPoints;

            //@prepare'note, 2017-10-20
            //temp fix, temp disable customfit if we build emoji font
            _currentGlyphBuilder.TemporaryDisableCustomFit = (typeface.COLRTable != null) && (typeface.CPALTable != null);
            //------------------------------------------ 
            _hintGlyphCollection.SetCacheInfo(typeface, this._currentFontSizeInPoints, _currentHintTech);
        }

        bool _flipGlyphUpward;
        public bool FlipGlyphUpward
        {
            get { return _flipGlyphUpward; }
            set { _flipGlyphUpward = value; }
        }
        /// <summary>
        /// get existing or create new one from current font setting
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <returns></returns>
        GlyphMeshData InternalGetGlyphMesh(ushort glyphIndex)
        {
            GlyphMeshData glyphMeshData;
            if (!_hintGlyphCollection.TryGetCacheGlyph(glyphIndex, out glyphMeshData))
            {
                //if not found then create new glyph vxs and cache it
                _currentGlyphBuilder.SetHintTechnique(_currentHintTech);
                _currentGlyphBuilder.BuildFromGlyphIndex(glyphIndex, _currentFontSizeInPoints);
                GlyphDynamicOutline dynamicOutline = _currentGlyphBuilder.LatestGlyphFitOutline;
                //-----------------------------------  
                glyphMeshData = new GlyphMeshData();

                if (dynamicOutline != null)
                {
                    //has dynamic outline data
                    glyphMeshData.avgXOffsetToFit = dynamicOutline.AvgXFitOffset;
                    glyphMeshData.orgBounds = dynamicOutline.OriginalGlyphControlBounds;
                    glyphMeshData.dynamicOutline = dynamicOutline;
                }
                _hintGlyphCollection.RegisterCachedGlyph(glyphIndex, glyphMeshData);
                //-----------------------------------    
            }
            return glyphMeshData;
        }
        /// <summary>
        /// get glyph left offset-to-fit value from current font setting
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <returns></returns>
        public GlyphControlParameters GetControlPars(ushort glyphIndex)
        {
            return InternalGetGlyphMesh(glyphIndex).GetControlPars();
        }

        PixelFarm.CpuBlit.VertexProcessing.Affine _invertY = PixelFarm.CpuBlit.VertexProcessing.Affine.NewScaling(1, -1);
        VertexStore _vxs1 = new VertexStore();

        /// <summary>
        /// get glyph mesh from current font setting
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <returns></returns>
        public VertexStore GetGlyphMesh(ushort glyphIndex)
        {
            GlyphMeshData glyphMeshData = InternalGetGlyphMesh(glyphIndex);
            if (glyphMeshData.vxsStore == null)
            {
                //build vxs
                _tovxs.Reset();
                float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(_currentFontSizeInPoints);
                GlyphDynamicOutline dynamicOutline = glyphMeshData.dynamicOutline;
                if (dynamicOutline != null)
                {
                    dynamicOutline.GenerateOutput(_tovxs, pxscale);

                    //version 3 
                    if (_flipGlyphUpward)
                    {
                        _vxs1.Clear(); //write to temp buffer first
                        _tovxs.WriteOutput(_vxs1);

                        VertexStore vxs = new VertexStore();
                        PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(_invertY, _vxs1, vxs);
                        //then
                        glyphMeshData.vxsStore = vxs;
                    }
                    else
                    {
                        glyphMeshData.vxsStore = new VertexStore();
                        _tovxs.WriteOutput(glyphMeshData.vxsStore);
                    }
                }
                else
                {

                    if (_flipGlyphUpward)
                    {
                        _vxs1.Clear(); //write to temp buffer first

                        _currentGlyphBuilder.ReadShapes(_tovxs);
                        _tovxs.WriteOutput(_vxs1);

                        VertexStore vxs = new VertexStore();
                        PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(_invertY, _vxs1, vxs);
                        //then
                        glyphMeshData.vxsStore = vxs;
                    }
                    else
                    {
                        //no dynamic outline

                        _currentGlyphBuilder.ReadShapes(_tovxs);
                        //TODO: review here,
                        //float pxScale = _glyphPathBuilder.GetPixelScale(); 

                        glyphMeshData.vxsStore = new VertexStore();
                        _tovxs.WriteOutput(glyphMeshData.vxsStore);
                    }
                }

            }
            return glyphMeshData.vxsStore;

        }
    }


}