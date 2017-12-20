﻿//BSD, 2014-2017, WinterDev
//ArthurHub  , Jose Manuel Menendez Poo

// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using Win32;
namespace PixelFarm.Drawing.WinGdi
{
    partial class GdiPlusRenderSurface
    {
        float strokeWidth = 1f;
        Color fillSolidColor = Color.Transparent;
        Color strokeColor = Color.Black;
        //==========================================================
        public Color StrokeColor
        {
            get
            {
                return this.strokeColor;
            }
            set
            {
                this.internalPen.Color = ConvColor(this.strokeColor = value);
            }
        }
        public float StrokeWidth
        {
            get
            {
                return this.strokeWidth;
            }
            set
            {
                this.internalPen.Width = this.strokeWidth = value;
            }
        }

        public void RenderTo(IntPtr destHdc, int sourceX, int sourceY, Rectangle destArea)
        {

            MyWin32.SetViewportOrgEx(win32MemDc.DC, canvasOriginX, canvasOriginY, IntPtr.Zero);
            MyWin32.BitBlt(
                destHdc, destArea.X, destArea.Y, destArea.Width, destArea.Height, //dest
                win32MemDc.DC, sourceX, sourceY, MyWin32.SRCCOPY); //src
            MyWin32.SetViewportOrgEx(win32MemDc.DC, -canvasOriginX, -canvasOriginY, IntPtr.Zero);

        }
        public void Clear(PixelFarm.Drawing.Color c)
        {

            gx.Clear(System.Drawing.Color.FromArgb(
                c.A,
                c.R,
                c.G,
                c.B));
        }
        public void DrawPath(GraphicsPath gfxPath)
        {
            gx.DrawPath(internalPen, ResolveGraphicsPath(gfxPath));
        }
        public void FillRectangle(Brush brush, float left, float top, float width, float height)
        {

            switch (brush.BrushKind)
            {
                case BrushKind.Solid:
                    {
                        //use default solid brush
                        SolidBrush solidBrush = (SolidBrush)brush;
                        var prevColor = internalSolidBrush.Color;
                        internalSolidBrush.Color = ConvColor(solidBrush.Color);
                        gx.FillRectangle(internalSolidBrush, left, top, width, height);
                        internalSolidBrush.Color = prevColor;
                    }
                    break;
                case BrushKind.LinearGradient:
                    {
                        //draw with gradient
                        LinearGradientBrush linearBrush = (LinearGradientBrush)brush;
                        var colors = linearBrush.GetColors();
                        var points = linearBrush.GetStopPoints();
                        using (var linearGradBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                             points[0].ToPointF(),
                             points[1].ToPointF(),
                             ConvColor(colors[0]),
                             ConvColor(colors[1])))
                        {
                            gx.FillRectangle(linearGradBrush, left, top, width, height);
                        }
                    }
                    break;
                case BrushKind.GeometryGradient:
                    {
                    }
                    break;
                case BrushKind.CircularGraident:
                    {
                    }
                    break;
                case BrushKind.Texture:
                    {
                    }
                    break;
            }
        }
        public void FillRectangle(Color color, float left, float top, float width, float height)
        {

            internalSolidBrush.Color = ConvColor(color);
            gx.FillRectangle(internalSolidBrush, left, top, width, height);
        }


        public void DrawRectangle(Color color, float left, float top, float width, float height)
        {

            internalPen.Color = ConvColor(color);
            gx.DrawRectangle(internalPen, left, top, width, height);
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {

            gx.DrawLine(internalPen, x1, y1, x2, y2);
        }


        //public override void DrawRoundRect(int x, int y, int w, int h, Size cornerSize)
        //{

        //    int cornerSizeW = cornerSize.Width;
        //    int cornerSizeH = cornerSize.Height;

        //    System.Drawing.Drawing2D.GraphicsPath gxPath = new System.Drawing.Drawing2D.GraphicsPath();
        //    gxPath.AddArc(new System.Drawing.Rectangle(x, y, cornerSizeW * 2, cornerSizeH * 2), 180, 90);
        //    gxPath.AddLine(new System.Drawing.Point(x + cornerSizeW, y), new System.Drawing.Point(x + w - cornerSizeW, y));

        //    gxPath.AddArc(new System.Drawing.Rectangle(x + w - cornerSizeW * 2, y, cornerSizeW * 2, cornerSizeH * 2), -90, 90);
        //    gxPath.AddLine(new System.Drawing.Point(x + w, y + cornerSizeH), new System.Drawing.Point(x + w, y + h - cornerSizeH));

        //    gxPath.AddArc(new System.Drawing.Rectangle(x + w - cornerSizeW * 2, y + h - cornerSizeH * 2, cornerSizeW * 2, cornerSizeH * 2), 0, 90);
        //    gxPath.AddLine(new System.Drawing.Point(x + w - cornerSizeW, y + h), new System.Drawing.Point(x + cornerSizeW, y + h));

        //    gxPath.AddArc(new System.Drawing.Rectangle(x, y + h - cornerSizeH * 2, cornerSizeW * 2, cornerSizeH * 2), 90, 90);
        //    gxPath.AddLine(new System.Drawing.Point(x, y + cornerSizeH), new System.Drawing.Point(x, y + h - cornerSizeH));

        //    gx.FillPath(System.Drawing.Brushes.Yellow, gxPath);
        //    gx.DrawPath(System.Drawing.Pens.Red, gxPath);
        //    gxPath.Dispose();
        //}


        /// <summary>
        /// Gets or sets the rendering quality for this <see cref="T:System.Drawing.Graphics"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Drawing.Drawing2D.SmoothingMode"/> values.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public SmoothingMode SmoothingMode
        {
            get
            {

                return (SmoothingMode)(gx.SmoothingMode);
            }
            set
            {

                gx.SmoothingMode = (System.Drawing.Drawing2D.SmoothingMode)value;
            }
        }

        static System.Drawing.Bitmap ResolveInnerBmp(Image image)
        {

            if (image is PixelFarm.Agg.ActualImage)
            {
                //this is known image
                var cacheBmp = Image.GetCacheInnerImage(image) as System.Drawing.Bitmap;
                if (cacheBmp == null)
                {

                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image.Width,
                        image.Height,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    //
                    PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize((PixelFarm.Agg.ActualImage)image, bmp);
                    //
                    Image.SetCacheInnerImage(image, bmp);
                    return bmp;
                }
                else
                {
                    //check if cache image is update or not 
                    return cacheBmp;
                }
            }
            else
            {
                //other image
                return Image.GetCacheInnerImage(image) as System.Drawing.Bitmap;
            }
        }

        /// <summary>
        /// Draws the specified portion of the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param>
        /// <param name="destRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle. </param>
        /// <param name="srcRect"><see cref="T:System.Drawing.RectangleF"/> structure that specifies the portion of the <paramref name="image"/> object to draw. </param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception>
        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect)
        {

            gx.DrawImage(ResolveInnerBmp(image),
                destRect.ToRectF(),
                srcRect.ToRectF(),
                System.Drawing.GraphicsUnit.Pixel);
        }
        public void DrawImages(Image image, RectangleF[] destAndSrcPairs)
        {

            int j = destAndSrcPairs.Length;
            if (j > 1)
            {
                if ((j % 2) != 0)
                {
                    //make it even number
                    j -= 1;
                }
                //loop draw
                var inner = ResolveInnerBmp(image);
                for (int i = 0; i < j;)
                {
                    gx.DrawImage(inner,
                        destAndSrcPairs[i].ToRectF(),
                        destAndSrcPairs[i + 1].ToRectF(),
                        System.Drawing.GraphicsUnit.Pixel);
                    i += 2;
                }
            }
        }
        /// <summary>
        /// Draws the specified <see cref="T:System.Drawing.Image"/> at the specified location and with the specified size.
        /// </summary>
        /// <param name="image"><see cref="T:System.Drawing.Image"/> to draw. </param><param name="destRect"><see cref="T:System.Drawing.Rectangle"/> structure that specifies the location and size of the drawn image. </param><exception cref="T:System.ArgumentNullException"><paramref name="image"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void DrawImage(Image image, RectangleF destRect)
        {

            System.Drawing.Bitmap inner = ResolveInnerBmp(image);
            if (image.IsReferenceImage)
            {
                gx.DrawImage(inner,
                    destRect.ToRectF(),
                     new System.Drawing.RectangleF(
                         image.ReferenceX, image.ReferenceY,
                         image.Width, image.Height),
                    System.Drawing.GraphicsUnit.Pixel);
            }
            else
            {
                gx.DrawImage(inner, destRect.ToRectF());
            }
        }
        public void FillPath(Color color, GraphicsPath gfxPath)
        {

            //solid color
            var prevColor = internalSolidBrush.Color;
            internalSolidBrush.Color = ConvColor(color);
            System.Drawing.Drawing2D.GraphicsPath innerPath = ResolveGraphicsPath(gfxPath);
            gx.FillPath(internalSolidBrush, innerPath);
            internalSolidBrush.Color = prevColor;
        }
        /// <summary>
        /// Fills the interior of a <see cref="T:System.Drawing.Drawing2D.GraphicsPath"/>.
        /// </summary>
        /// <param name="brush"><see cref="T:System.Drawing.Brush"/> that determines the characteristics of the fill. </param><param name="path"><see cref="T:System.Drawing.Drawing2D.GraphicsPath"/> that represents the path to fill. </param><exception cref="T:System.ArgumentNullException"><paramref name="brush"/> is null.-or-<paramref name="path"/> is null.</exception><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/></PermissionSet>
        public void FillPath(Brush brush, GraphicsPath path)
        {

            switch (brush.BrushKind)
            {
                case BrushKind.Solid:
                    {
                        SolidBrush solidBrush = (SolidBrush)brush;
                        var prevColor = internalSolidBrush.Color;
                        internalSolidBrush.Color = ConvColor(solidBrush.Color);
                        //
                        System.Drawing.Drawing2D.GraphicsPath innerPath = ResolveGraphicsPath(path);
                        gx.FillPath(internalSolidBrush, innerPath);
                        //
                        internalSolidBrush.Color = prevColor;
                    }
                    break;
                case BrushKind.LinearGradient:
                    {
                        LinearGradientBrush solidBrush = (LinearGradientBrush)brush;
                        var prevColor = internalSolidBrush.Color;
                        internalSolidBrush.Color = ConvColor(solidBrush.Color);
                        //
                        System.Drawing.Drawing2D.GraphicsPath innerPath = ResolveGraphicsPath(path);
                        gx.FillPath(internalSolidBrush, innerPath);
                        //
                        internalSolidBrush.Color = prevColor;
                    }
                    break;
                default:
                    {
                    }
                    break;
            }
        }
        static System.Drawing.Drawing2D.GraphicsPath ResolveGraphicsPath(GraphicsPath path)
        {
            //convert from graphics path to internal presentation
            System.Drawing.Drawing2D.GraphicsPath innerPath = path.InnerPath as System.Drawing.Drawing2D.GraphicsPath;
            if (innerPath != null)
            {
                return innerPath;
            }
            //--------
            innerPath = new System.Drawing.Drawing2D.GraphicsPath();
            path.InnerPath = innerPath;
            List<float> points;
            List<PathCommand> cmds;
            GraphicsPath.GetPathData(path, out points, out cmds);
            int j = cmds.Count;
            int p_index = 0;
            for (int i = 0; i < j; ++i)
            {
                PathCommand cmd = cmds[i];
                switch (cmd)
                {
                    default:
                        throw new NotSupportedException();
                    case PathCommand.Arc:
                        innerPath.AddArc(
                            points[p_index],
                            points[p_index + 1],
                            points[p_index + 2],
                            points[p_index + 3],
                            points[p_index + 4],
                            points[p_index + 5]);
                        p_index += 6;
                        break;
                    case PathCommand.Bezier:
                        innerPath.AddBezier(
                            points[p_index],
                            points[p_index + 1],
                            points[p_index + 2],
                            points[p_index + 3],
                            points[p_index + 4],
                            points[p_index + 5],
                            points[p_index + 6],
                            points[p_index + 7]);
                        p_index += 8;
                        break;
                    case PathCommand.CloseFigure:
                        innerPath.CloseFigure();
                        break;
                    case PathCommand.Ellipse:
                        innerPath.AddEllipse(
                            points[p_index],
                            points[p_index + 1],
                            points[p_index + 2],
                            points[p_index + 3]);
                        p_index += 4;
                        break;
                    case PathCommand.Line:
                        innerPath.AddLine(
                            points[p_index],
                            points[p_index + 1],
                            points[p_index + 2],
                            points[p_index + 3]);
                        p_index += 4;
                        break;
                    case PathCommand.Rect:
                        innerPath.AddRectangle(
                           new System.Drawing.RectangleF(
                          points[p_index],
                          points[p_index + 1],
                          points[p_index + 2],
                          points[p_index + 3]));
                        p_index += 4;
                        break;
                    case PathCommand.StartFigure:
                        break;
                }
            }


            return innerPath;
        }
        public void FillPolygon(Brush brush, PointF[] points)
        {

            var pps = ConvPointFArray(points);
            //use internal solid color            
            gx.FillPolygon(brush.InnerBrush as System.Drawing.Brush, pps);
        }
        public void FillPolygon(Color color, PointF[] points)
        {

            var pps = ConvPointFArray(points);
            internalSolidBrush.Color = ConvColor(color);
            gx.FillPolygon(this.internalSolidBrush, pps);
        }

        ////==========================================================
        //public override void CopyFrom(Canvas sourceCanvas, int logicalSrcX, int logicalSrcY, Rectangle destArea)
        //{
        //    MyCanvas s1 = (MyCanvas)sourceCanvas;

        //    if (s1.gx != null)
        //    {
        //        int phySrcX = logicalSrcX - s1.left;
        //        int phySrcY = logicalSrcY - s1.top;

        //        System.Drawing.Rectangle postIntersect =
        //            System.Drawing.Rectangle.Intersect(currentClipRect, destArea.ToRect());
        //        phySrcX += postIntersect.X - destArea.X;
        //        phySrcY += postIntersect.Y - destArea.Y;
        //        destArea = postIntersect.ToRect();

        //        IntPtr gxdc = gx.GetHdc();

        //        MyWin32.SetViewportOrgEx(gxdc, CanvasOrgX, CanvasOrgY, IntPtr.Zero);
        //        IntPtr source_gxdc = s1.gx.GetHdc();
        //        MyWin32.SetViewportOrgEx(source_gxdc, s1.CanvasOrgX, s1.CanvasOrgY, IntPtr.Zero);


        //        MyWin32.BitBlt(gxdc, destArea.X, destArea.Y, destArea.Width, destArea.Height, source_gxdc, phySrcX, phySrcY, MyWin32.SRCCOPY);


        //        MyWin32.SetViewportOrgEx(source_gxdc, -s1.CanvasOrgX, -s1.CanvasOrgY, IntPtr.Zero);

        //        s1.gx.ReleaseHdc();

        //        MyWin32.SetViewportOrgEx(gxdc, -CanvasOrgX, -CanvasOrgY, IntPtr.Zero);
        //        gx.ReleaseHdc();



        //    }
        //}
    }
}