﻿//MIT, 2016-present, WinterDev

using System;
using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;
namespace PixelFarm.Drawing.Pdf
{

    public class PdfPainter : Painter
    {
        //System.Drawing.Graphics _gfx;
        //System.Drawing.Bitmap _gfxBmp;

        //System.Drawing.SolidBrush _currentFillBrush;
        //System.Drawing.Pen _currentPen;
        //
        RectInt _clipBox;
        Color _fillColor;
        Color _strokeColor;
        int _width, _height;
        double _strokeWidth;
        bool _useSubPixelRendering;
        //BufferBitmapStore _bmpStore;
        RequestFont _currentFont;
        CpuBlit.VertexProcessing.RoundedRect roundRect;


        SmoothingMode _smoothingMode;

        public PdfPainter()
        {

        }

        public override ICoordTransformer CoordTransformer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override FillingRule FillingRule { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool EnableMask { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override float FillOpacity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override TargetBuffer TargetBuffer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override void Fill(Region rgn)
        {
            throw new NotImplementedException();
        }
        public override void Draw(Region rgn)
        {
            throw new NotImplementedException();
        }
        public override void DrawImage(Image actualImage, double left, double top, ICoordTransformer coordTx)
        {
            throw new NotImplementedException();
        }
        public override void Render(RenderVx renderVx)
        {
            throw new NotImplementedException();
        }
        public override void SetClipRgn(VertexStore vxs)
        {
            throw new NotImplementedException();
        }
        PixelFarm.Drawing.RenderSurfaceOrientation _orientation;
        public override PixelFarm.Drawing.RenderSurfaceOrientation Orientation
        {
            get { return _orientation; }
            set
            { _orientation = value; }
        }
        Brush _currentBrush;
        public override Brush CurrentBrush
        {
            get { return _currentBrush; }
            set
            {
                _currentBrush = value;
            }
        }

        Pen _currentPen;
        public override Pen CurrentPen
        {
            get { return _currentPen; }
            set
            {
                _currentPen = value;
            }
        }
        public override float OriginX
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        PixelFarm.Drawing.RenderQuality _renderQuality;
        public override PixelFarm.Drawing.RenderQuality RenderQuality
        {
            get { return _renderQuality; }
            set { _renderQuality = value; }
        }

        public override float OriginY
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public override void SetOrigin(float ox, float oy)
        {
            throw new NotImplementedException();
        }
        public override SmoothingMode SmoothingMode
        {
            get
            {
                return _smoothingMode;
            }
            set
            {
                switch (_smoothingMode = value)
                {
                    case Drawing.SmoothingMode.AntiAlias:
                        //_gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        break;
                    case Drawing.SmoothingMode.HighSpeed:
                        //_gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                        break;
                    case Drawing.SmoothingMode.HighQuality:
                        //_gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        break;
                    case Drawing.SmoothingMode.Default:
                    default:
                        //_gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                        break;
                }
            }
        }
        //public System.Drawing.Drawing2D.CompositingMode CompositingMode
        //{
        //    get { return _gfx.CompositingMode; }
        //    set { _gfx.CompositingMode = value; }
        //}

        public override RectInt ClipBox
        {
            get
            {
                return _clipBox;
            }
            set
            {
                _clipBox = value;
            }
        }

        public override RequestFont CurrentFont
        {
            get
            {
                return _currentFont;
            }

            set
            {
                _currentFont = value;
                throw new System.NotSupportedException();
                //_winGdiFont = WinGdiFontSystem.GetWinGdiFont(value);
            }
        }
        public override Color FillColor
        {
            get
            {
                return _fillColor;
            }
            set
            {
                _fillColor = value;
                //_currentFillBrush.Color = VxsHelper.ToDrawingColor(value);
            }
        }

        public override int Height
        {
            get
            {
                return _height;
            }
        }

        public override Color StrokeColor
        {
            get
            {
                return _strokeColor;
            }
            set
            {
                _strokeColor = value;
                //_currentPen.Color = VxsHelper.ToDrawingColor(value);
            }
        }
        public override double StrokeWidth
        {
            get
            {
                return _strokeWidth;
            }
            set
            {
                _strokeWidth = value;
                //_currentPen.Width = (float)value;
            }
        }

        public override bool UseSubPixelLcdEffect
        {
            get
            {
                return _useSubPixelRendering;
            }
            set
            {
                _useSubPixelRendering = value;
            }
        }

        public override int Width
        {
            get
            {
                return _width;
            }
        }

        public override LineJoin LineJoin { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override LineCap LineCap { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IDashGenerator LineDashGen { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Clear(Color color)
        {
            //_gfx.Clear(VxsHelper.ToDrawingColor(color));
        }
        //public override void DoFilterBlurRecursive(RectInt area, int r)
        //{
        //    //TODO: implement this
        //}
        //public override void DoFilter(RectInt area, int r)
        //{

        //}
        //public override void DoFilterBlurStack(RectInt area, int r)
        //{
        //    //since area is Windows coord
        //    //so we need to invert it 
        //    //System.Drawing.Bitmap backupBmp = _gfxBmp;
        //    //int bmpW = backupBmp.Width;
        //    //int bmpH = backupBmp.Height;
        //    //System.Drawing.Imaging.BitmapData bmpdata = backupBmp.LockBits(
        //    //    new System.Drawing.Rectangle(0, 0, bmpW, bmpH),
        //    //    System.Drawing.Imaging.ImageLockMode.ReadWrite,
        //    //     backupBmp.PixelFormat);
        //    ////copy sub buffer to int32 array
        //    ////this version bmpdata must be 32 argb 
        //    //int a_top = area.Top;
        //    //int a_bottom = area.Bottom;
        //    //int a_width = area.Width;
        //    //int a_stride = bmpdata.Stride;
        //    //int a_height = Math.Abs(area.Height);
        //    //int[] src_buffer = new int[(a_stride / 4) * a_height];
        //    //int[] destBuffer = new int[src_buffer.Length];
        //    //int a_lineOffset = area.Left * 4;
        //    //unsafe
        //    //{
        //    //    IntPtr scan0 = bmpdata.Scan0;
        //    //    byte* src = (byte*)scan0;
        //    //    if (a_top > a_bottom)
        //    //    {
        //    //        int tmp_a_bottom = a_top;
        //    //        a_top = a_bottom;
        //    //        a_bottom = tmp_a_bottom;
        //    //    }

        //    //    //skip  to start line
        //    //    src += ((a_stride * a_top) + a_lineOffset);
        //    //    int index_start = 0;
        //    //    for (int y = a_top; y < a_bottom; ++y)
        //    //    {
        //    //        //then copy to int32 buffer 
        //    //        System.Runtime.InteropServices.Marshal.Copy(new IntPtr(src), src_buffer, index_start, a_width);
        //    //        index_start += a_width;
        //    //        src += (a_stride + a_lineOffset);
        //    //    }
        //    //    PixelFarm.Agg.Imaging.StackBlurARGB.FastBlur32ARGB(src_buffer, destBuffer, a_width, a_height, r);
        //    //    //then copy back to bmp
        //    //    index_start = 0;
        //    //    src = (byte*)scan0;
        //    //    src += ((a_stride * a_top) + a_lineOffset);
        //    //    for (int y = a_top; y < a_bottom; ++y)
        //    //    {
        //    //        //then copy to int32 buffer 
        //    //        System.Runtime.InteropServices.Marshal.Copy(destBuffer, index_start, new IntPtr(src), a_width);
        //    //        index_start += a_width;
        //    //        src += (a_stride + a_lineOffset);
        //    //    }
        //    //}
        //    ////--------------------------------
        //    //backupBmp.UnlockBits(bmpdata);
        //}
        public override void ApplyFilter(PixelFarm.Drawing.IImageFilter imgFilter)
        {
            throw new NotImplementedException();
        }
        public override void Draw(VertexStore vxs)
        {
            //VxsHelper.DrawVxsSnap(_gfx, new VertexStoreSnap(vxs), _strokeColor);
        }
        //public override void DrawBezierCurve(float startX, float startY, float endX, float endY, float controlX1, float controlY1, float controlX2, float controlY2)
        //{
        //    //_gfx.DrawBezier(_currentPen,
        //    //     startX, startY,
        //    //     controlX1, controlY1,
        //    //     controlX2, controlY2,
        //    //     endX, endY);
        //}
        public override void DrawImage(Image actualImage, double left, double top, int srcX, int srcY, int srcW, int srcH)
        {
            throw new NotImplementedException();
        }
        public override void DrawImage(Image actualImage, params AffinePlan[] affinePlans)
        {
            //1. create special graphics 
            //using (System.Drawing.Bitmap srcBmp = CreateBmpBRGA(actualImage))
            //{
            //    var bmp = _bmpStore.GetFreeBmp();
            //    using (var g2 = System.Drawing.Graphics.FromImage(bmp))
            //    {
            //        //we can use recycle tmpVxsStore
            //        Affine destRectTransform = Affine.NewMatix(affinePlans);
            //        double x0 = 0, y0 = 0, x1 = bmp.Width, y1 = bmp.Height;
            //        destRectTransform.Transform(ref x0, ref y0);
            //        destRectTransform.Transform(ref x0, ref y1);
            //        destRectTransform.Transform(ref x1, ref y1);
            //        destRectTransform.Transform(ref x1, ref y0);
            //        var matrix = new System.Drawing.Drawing2D.Matrix(
            //           (float)destRectTransform.m11, (float)destRectTransform.m12,
            //           (float)destRectTransform.m21, (float)destRectTransform.m22,
            //           (float)destRectTransform.dx, (float)destRectTransform.dy);
            //        g2.Clear(System.Drawing.Color.Transparent);
            //        g2.Transform = matrix;
            //        //------------------------
            //        g2.DrawImage(srcBmp, new System.Drawing.PointF(0, 0));
            //        _gfx.DrawImage(bmp, new System.Drawing.Point(0, 0));
            //    }
            //    _bmpStore.RelaseBmp(bmp);
            //}
        }

        //static System.Drawing.Bitmap CreateBmpBRGA(ActualImage actualImage)
        //{
        //    int w = actualImage.Width;
        //    int h = actualImage.Height;
        //    //copy data to bitmap
        //    //bgra  
        //    var bmp = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //    byte[] acutalBuffer = ActualImage.GetBuffer(actualImage);
        //    var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
        //    System.Runtime.InteropServices.Marshal.Copy(acutalBuffer, 0, bmpData.Scan0, acutalBuffer.Length);
        //    bmp.UnlockBits(bmpData);
        //    return bmp;
        //}
        //public void DrawImage(System.Drawing.Bitmap bmp, float x, float y)
        //{
        //    _gfx.DrawImage(bmp, x, y);
        //}
        public override void DrawImage(Image actualImage, double left, double top)
        {
            ////create Gdi bitmap from actual image
            //int w = actualImage.Width;
            //int h = actualImage.Height;
            //switch (actualImage.PixelFormat)
            //{
            //    case Agg.PixelFormat.ARGB32:
            //        {
            //            //copy data from acutal buffer to internal representation bitmap
            //            using (var bmp = CreateBmpBRGA(actualImage))
            //            {
            //                _gfx.DrawImageUnscaled(bmp, new System.Drawing.Point((int)x, (int)y));
            //            }
            //        }
            //        break;
            //    case Agg.PixelFormat.RGB24:
            //        {
            //        }
            //        break;
            //    case Agg.PixelFormat.GrayScale8:
            //        {
            //        }
            //        break;
            //    default:
            //        throw new NotSupportedException();
            //}
        }
        public override void DrawString(string text, double x, double y)
        {
            ////use current brush and font
            //_gfx.ResetTransform();
            //_gfx.TranslateTransform(0.0F, (float)Height);// Translate the drawing area accordingly   

            ////draw with native win32
            ////------------

            ///*_gfx.DrawString(text,
            //    _latestWinGdiPlusFont.InnerFont,
            //    _currentFillBrush,
            //    new System.Drawing.PointF((float)x, (float)y));
            //*/
            ////------------
            ////restore back
            //_gfx.ResetTransform();//again
            //_gfx.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis
            //_gfx.TranslateTransform(0.0F, -(float)Height);// Translate the drawing area accordingly                
        }
        public override RenderVxFormattedString CreateRenderVx(string textspan)
        {
            return new PdfRenderVxFormattedString(textspan);
        }
        public override void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// we do NOT store snap/vxs
        /// </summary>
        /// <param name="vxs"></param>
        public override void Fill(VertexStore vxs)
        {
            // VxsHelper.FillVxsSnap(_gfx, new VertexStoreSnap(vxs), _fillColor);
        }

        //public override void FillCircle(double x, double y, double radius)
        //{
        //    //  _gfx.FillEllipse(_currentFillBrush, (float)x, (float)y, (float)(radius + radius), (float)(radius + radius));
        //}

        //public override void FillCircle(double x, double y, double radius, Drawing.Color color)
        //{
        //    var prevColor = _currentFillBrush.Color;
        //    _currentFillBrush.Color = VxsHelper.ToDrawingColor(color);
        //    _gfx.FillEllipse(_currentFillBrush, (float)x, (float)y, (float)(radius + radius), (float)(radius + radius));
        //    _currentFillBrush.Color = prevColor;
        //}

        public override void FillEllipse(double left, double top, double width, double height)
        {

        }
        public override void DrawEllipse(double left, double top, double width, double height)
        {

        }

        public override void FillRect(double left, double top, double width, double height)
        {

        }


        //VertexStorePool _vxsPool = new VertexStorePool();
        //VertexStore GetFreeVxs()
        //{

        //    return _vxsPool.GetFreeVxs();
        //}
        //void ReleaseVxs(ref VertexStore vxs)
        //{
        //    _vxsPool.Release(ref vxs);
        //}
        //public override void DrawRoundRect(double left, double bottom, double right, double top, double radius)
        //{
        //    if (roundRect == null)
        //    {
        //        roundRect = new PixelFarm.Agg.VertexSource.RoundedRect(left, bottom, right, top, radius);
        //        roundRect.NormalizeRadius();
        //    }
        //    else
        //    {
        //        roundRect.SetRect(left, bottom, right, top);
        //        roundRect.SetRadius(radius);
        //        roundRect.NormalizeRadius();
        //    }

        //    var v1 = GetFreeVxs();
        //    this.Draw(roundRect.MakeVxs(v1));
        //    ReleaseVxs(ref v1);
        //}
        //public override void FillRoundRectangle(double left, double bottom, double right, double top, double radius)
        //{
        //    if (roundRect == null)
        //    {
        //        roundRect = new PixelFarm.Agg.VertexSource.RoundedRect(left, bottom, right, top, radius);
        //        roundRect.NormalizeRadius();
        //    }
        //    else
        //    {
        //        roundRect.SetRect(left, bottom, right, top);
        //        roundRect.SetRadius(radius);
        //        roundRect.NormalizeRadius();
        //    }
        //    var v1 = GetFreeVxs();
        //    this.Fill(roundRect.MakeVxs(v1));
        //    ReleaseVxs(ref v1);
        //}



        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            //_gfx.DrawLine(_currentPen, new System.Drawing.PointF((float)x1, (float)y1), new System.Drawing.PointF((float)x2, (float)y2));
        }


        public override void DrawRect(double left, double bottom, double right, double top)
        {
            //_gfx.DrawRectangle(_currentPen, (float)left, (float)top, (float)(right - left), (float)(top - bottom));
        }

        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {
            //_gfx.SetClip(new System.Drawing.Rectangle(x1, y1, x2 - x1, y2 - y1));
        }
        public override RenderVx CreateRenderVx(VertexStore vxs)
        {
            throw new NotSupportedException();
            //var renderVx = new WinGdiRenderVx(snap);
            //renderVx.path = VxsHelper.CreateGraphicsPath(snap);
            //return renderVx;
        }
        public override void FillRenderVx(Brush brush, RenderVx renderVx)
        {
            ////TODO: review brush implementation here
            //WinGdiRenderVx wRenderVx = (WinGdiRenderVx)renderVx;
            //VxsHelper.FillPath(_gfx, wRenderVx.path, this.FillColor);
        }
        public override void DrawRenderVx(RenderVx renderVx)
        {
            //WinGdiRenderVx wRenderVx = (WinGdiRenderVx)renderVx;
            //VxsHelper.DrawPath(_gfx, wRenderVx.path, _strokeColor);
        }
        public override void FillRenderVx(RenderVx renderVx)
        {
            //WinGdiRenderVx wRenderVx = (WinGdiRenderVx)renderVx;
            //VxsHelper.FillPath(_gfx, wRenderVx.path, this.FillColor);
        }

        public override void DrawImage(Image actualImage)
        {
            throw new NotImplementedException();
        }
    }
}