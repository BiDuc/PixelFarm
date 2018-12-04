﻿//MIT, 2014-present, WinterDev

namespace PixelFarm.DrawingGL
{


    /// <summary>
    /// sharing data between GLRenderSurface and shaders
    /// </summary>
    class ShaderSharedResource
    {
        /// <summary>
        /// stroke width here is the sum of both side of the line.
        /// </summary>
        internal float _strokeWidth = 1;
        Drawing.Color _strokeColor;
        MyMat4 _orthoView;
        internal ShaderBase _currentShader;
        int _orthoViewVersion = 0;

        internal MyMat4 OrthoView
        {
            get { return _orthoView; }
            set
            {

                _orthoView = value;
                unchecked { _orthoViewVersion++; }
            }
        }
        public int OrthoViewVersion
        {
            get { return _orthoViewVersion; }
        }

        internal Drawing.Color StrokeColor
        {
            get { return _strokeColor; }
            set
            {
                _strokeColor = value;
                _stroke_r = value.R / 255f;
                _stroke_g = value.G / 255f;
                _stroke_b = value.B / 255f;
                _stroke_a = value.A / 255f;
            }
        }

        float _stroke_r;
        float _stroke_g;
        float _stroke_b;
        float _stroke_a;
        internal void AssignStrokeColorToVar(OpenTK.Graphics.ES20.ShaderUniformVar4 color)
        {
            color.SetValue(_stroke_r, _stroke_g, _stroke_b, _stroke_a);
        }
    }
}