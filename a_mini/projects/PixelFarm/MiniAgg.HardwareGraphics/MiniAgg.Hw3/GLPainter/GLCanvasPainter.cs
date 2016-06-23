﻿//2016 MIT, WinterDev

using PixelFarm.Drawing;
using PixelFarm.Agg;
using PixelFarm.Agg.Fonts;
using PixelFarm.Agg.Transform;
using System;

using PixelFarm.DrawingGL;
namespace PixelFarm.Drawing.HardwareGraphics
{
    public class GLCanvasPainter : CanvasPainter
    {
        CanvasGL2d _canvas;
        int _width;
        int _height;
        ColorRGBA _fillColor;
        ColorRGBA _strokeColor;
        RectInt _rectInt;
        Agg.VertexSource.CurveFlattener curveFlattener;
        PixelFarm.Agg.VertexSource.RoundedRect roundRect;
        public GLCanvasPainter(CanvasGL2d canvas, int w, int h)
        {
            _canvas = canvas;
            _width = w;
            _height = h;
            _rectInt = new RectInt(0, 0, w, h);
        }
        public override RectInt ClipBox
        {
            get
            {
                return _rectInt;
            }

            set
            {
                _rectInt = value;
            }
        }

        public override Agg.Fonts.Font CurrentFont
        {
            get
            {
                return null;
            }
            set
            {

            }
        }
        public override ColorRGBA FillColor
        {
            get
            {
                return _fillColor;
            }
            set
            {
                _fillColor = value;
            }
        }
        public override int Height
        {
            get
            {
                return this._height;
            }
        }

        public override ColorRGBA StrokeColor
        {
            get
            {
                return new ColorRGBA(_strokeColor.red,
                    _strokeColor.green,
                    _strokeColor.blue,
                    _strokeColor.alpha);
            }
            set
            {
                _strokeColor = value;
                _canvas.StrokeColor = ToPixelFarmColor(value);
            }
        }
        static Color ToPixelFarmColor(ColorRGBA value)
        {
            return new Color(value.alpha, value.red, value.green, value.blue);
        }
        public override double StrokeWidth
        {
            get
            {
                return _canvas.StrokeWidth;
            }
            set
            {
                _canvas.StrokeWidth = (float)value;
            }
        }

        public override bool UseSubPixelRendering
        {
            get
            {
                return _canvas.SmoothMode == CanvasSmoothMode.Smooth;
            }

            set
            {
                _canvas.SmoothMode = value ? CanvasSmoothMode.Smooth : CanvasSmoothMode.No;
            }
        }

        public override int Width
        {
            get
            {
                return _width;
            }
        }

        public override void Clear(ColorRGBA color)
        {
            _canvas.Clear(ToPixelFarmColor(color));
        }
        public override void DoFilterBlurRecursive(RectInt area, int r)
        {
            //filter with glsl
        }
        public override void DoFilterBlurStack(RectInt area, int r)
        {

        }
        public override void Draw(VertexStore vxs)
        {
            _canvas.DrawVxsSnap(ToPixelFarmColor(this._strokeColor), new VertexStoreSnap(vxs));
        }

        public override void DrawBezierCurve(float startX, float startY, float endX, float endY, float controlX1, float controlY1, float controlX2, float controlY2)
        {
            _canvas.DrawBezierCurve(startX, startY, endX, endY, controlX1, controlY1, controlY1, controlY2);
        }

        public override void DrawImage(ActualImage actualImage, params AffinePlan[] affinePlans)
        {
            //create gl bmp
            GLBitmap glBmp = new GLBitmap(actualImage.Width, actualImage.Height, actualImage.GetBuffer(), false);
            _canvas.DrawImage(glBmp, 0, 0);
            glBmp.Dispose();
        }
        public override void DrawImage(ActualImage actualImage, double x, double y)
        {
            GLBitmap glBmp = new GLBitmap(actualImage.Width, actualImage.Height, actualImage.GetBuffer(), false);
            _canvas.DrawImage(glBmp, 0, 0);
            glBmp.Dispose();
        }
        public override void DrawRoundRect(double left, double bottom, double right, double top, double radius)
        {
            if (roundRect == null)
            {
                roundRect = new Agg.VertexSource.RoundedRect(left, bottom, right, top, radius);
                roundRect.NormalizeRadius();
            }
            else
            {
                roundRect.SetRect(left, bottom, right, top);
                roundRect.SetRadius(radius);
                roundRect.NormalizeRadius();
            }
            this.Draw(roundRect.MakeVxs());
        }
        public override void DrawString(string text, double x, double y)
        {

        }
        public override void Fill(VertexStore vxs)
        {
            _canvas.FillVxsSnap(
                ToPixelFarmColor(this._fillColor),
                new VertexStoreSnap(vxs)
                );
        }
        public override void Fill(VertexStoreSnap snap)
        {
            _canvas.FillVxsSnap(
              ToPixelFarmColor(this._fillColor),
              snap
              );
        }
        public override void Draw(VertexStoreSnap snap)
        {
            _canvas.DrawVxsSnap(
             ToPixelFarmColor(this._fillColor),
             snap
             );
        }
        public override void FillCircle(double x, double y, double radius)
        {
            _canvas.FillCircle(ToPixelFarmColor(_fillColor), (float)x, (float)y, (float)radius);
        }

        public override void FillCircle(double x, double y, double radius, ColorRGBA color)
        {
            _canvas.FillCircle(ToPixelFarmColor(color), (float)x, (float)y, (float)radius);
        }

        public override void DrawEllipse(double left, double bottom, double right, double top)
        {
            double midX = (left + right) / 2;
            double midY = (bottom + top) / 2;
            double radiusX = Math.Abs(right - midX);
            double radiusY = Math.Abs(top - midY);

            _canvas.DrawEllipse((float)midX, (float)midY, radiusX, radiusY);

        }
        public override void FillEllipse(double left, double bottom, double right, double top)
        {
            double midX = (left + right) / 2;
            double midY = (bottom + top) / 2;
            double radiusX = Math.Abs(right - midX);
            double radiusY = Math.Abs(top - midY);

            _canvas.FillEllipse(ToPixelFarmColor(_fillColor), (float)midX, (float)midY, (float)radiusX, (float)radiusY);

        }
        public override void FillRectangle(double left, double bottom, double right, double top)
        {
            _canvas.FillRect(ToPixelFarmColor(_fillColor), (float)left, (float)bottom, (float)(right - left), (float)(top - bottom));
        }
        public override void FillRectangle(double left, double bottom, double right, double top, ColorRGBA fillColor)
        {
            _canvas.FillRect(ToPixelFarmColor(fillColor), (float)left, (float)bottom, (float)(right - left), (float)(top - bottom));
        }
        public override void FillRectLBWH(double left, double bottom, double width, double height)
        {
            _canvas.FillRect(ToPixelFarmColor(_fillColor), (float)left, (float)bottom, (float)width, (float)height);
        }
        public override void FillRoundRectangle(double left, double bottom, double right, double top, double radius)
        {
            if (roundRect == null)
            {
                roundRect = new Agg.VertexSource.RoundedRect(left, bottom, right, top, radius);
                roundRect.NormalizeRadius();
            }
            else
            {
                roundRect.SetRect(left, bottom, right, top);
                roundRect.SetRadius(radius);
                roundRect.NormalizeRadius();
            }
            this.Fill(roundRect.MakeVxs());
        }

        public override VertexStore FlattenCurves(VertexStore srcVxs)
        {
            if (curveFlattener == null)
            {
                curveFlattener = new Agg.VertexSource.CurveFlattener();
            }
            return curveFlattener.MakeVxs(srcVxs);
        }

        public override void Line(double x1, double y1, double x2, double y2)
        {
            _canvas.StrokeColor = ToPixelFarmColor(_strokeColor);
            _canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2);
        }
        public override void Line(double x1, double y1, double x2, double y2, ColorRGBA color)
        {
            _canvas.StrokeColor = ToPixelFarmColor(color);
            _canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2);
        }
        public override void PaintSeries(VertexStore vxs, ColorRGBA[] colors, int[] pathIndexs, int numPath)
        {
            for (int i = 0; i < numPath; ++i)
            {
                _canvas.FillVxsSnap(ToPixelFarmColor(colors[i]), new VertexStoreSnap(vxs, pathIndexs[i]));
            }
        }
        public override void Rectangle(double left, double bottom, double right, double top)
        {
            //draw rectangle
            _canvas.DrawRect((float)left, (float)bottom, (float)(right - left), (float)(top - bottom));
        }
        public override void Rectangle(double left, double bottom, double right, double top, ColorRGBA color)
        {   //draw rectangle
            var prev = _canvas.StrokeColor;
            _canvas.StrokeColor = ToPixelFarmColor(color);
            _canvas.DrawRect((float)left, (float)bottom, (float)(right - left), (float)(top - bottom));
            _canvas.StrokeColor = prev;

        }
        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {

        }
    }
}