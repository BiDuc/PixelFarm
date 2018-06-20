﻿//BSD, 2014-present, WinterDev
//MatterHackers

using System;
using PixelFarm.CpuBlit.Transform;
using PixelFarm.CpuBlit.Imaging;
using PixelFarm.CpuBlit.VertexProcessing;
using Mini;
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.Sample_Perspective
{
    [Info(OrderCode = "04")]
    [Info("Perspective and bilinear transformations. In general, these classes can transform an arbitrary quadrangle "
            + " to another arbitrary quadrangle (with some restrictions). The example demonstrates how to transform "
            + "a rectangle to a quadrangle defined by 4 vertices. You can drag the 4 corners of the quadrangle, "
            + "as well as its boundaries. Note, that the perspective transformations don't work correctly if "
            + "the destination quadrangle is concave. Bilinear thansformations give a different result, but "
            + "remain valid with any shape of the destination quadrangle.")]
    public class perspective_application : DemoBase
    {
        UI.PolygonEditWidget quadPolygonControl;
        private SpriteShape lionShape;
        public perspective_application()
        {

            lionShape = new SpriteShape(SvgRenderVxLoader.CreateSvgRenderVxFromFile(@"Samples\lion.svg"));

            quadPolygonControl = new PixelFarm.CpuBlit.UI.PolygonEditWidget(4, 5.0);
            quadPolygonControl.SetXN(0, lionShape.Bounds.Left);
            quadPolygonControl.SetYN(0, lionShape.Bounds.Top);
            quadPolygonControl.SetXN(1, lionShape.Bounds.Right);
            quadPolygonControl.SetYN(1, lionShape.Bounds.Top);
            quadPolygonControl.SetXN(2, lionShape.Bounds.Right);
            quadPolygonControl.SetYN(2, lionShape.Bounds.Bottom);
            quadPolygonControl.SetXN(3, lionShape.Bounds.Left);
            quadPolygonControl.SetYN(3, lionShape.Bounds.Bottom);
        }
        public override void Init()
        {
            OnInitialize();
            base.Init();
        }

        [DemoConfig]
        public PerspectiveTransformType PerspectiveTransformType
        {
            get;
            set;
        }
        public void OnInitialize()
        {
            double dx = Width / 2.0 - (quadPolygonControl.GetXN(1) - quadPolygonControl.GetXN(0)) / 2.0;
            double dy = Height / 2.0 - (quadPolygonControl.GetYN(0) - quadPolygonControl.GetYN(2)) / 2.0;
            quadPolygonControl.AddXN(0, dx);
            quadPolygonControl.AddYN(0, dy);
            quadPolygonControl.AddXN(1, dx);
            quadPolygonControl.AddYN(1, dy);
            quadPolygonControl.AddXN(2, dx);
            quadPolygonControl.AddYN(2, dy);
            quadPolygonControl.AddXN(3, dx);
            quadPolygonControl.AddYN(3, dy);
        }
        bool didInit = false;
        public override void Draw(Painter p)
        {
            Painter painter = p;
            if (!didInit)
            {
                didInit = true;
                OnInitialize();
            }


            //-----------------------------------
            painter.Clear(Drawing.Color.White);
            //IBitmapBlender backBuffer = ImageHelper.CreateChildImage(gx.DestImage, gx.GetClippingRect());
            //ChildImage image;
            //if (backBuffer.BitDepth == 32)
            //{
            //    image = new ChildImage(backBuffer, new PixelBlenderBGRA());
            //}
            //else
            //{
            //    if (backBuffer.BitDepth != 24)
            //    {
            //        throw new System.NotSupportedException();
            //    }
            //    image = new ChildImage(backBuffer, new PixelBlenderBGR());
            //}
            //ClipProxyImage dest = new ClipProxyImage(image);
            //gx.Clear(ColorRGBA.White);
            //gx.SetClippingRect(new RectInt(0, 0, Width, Height)); 
            //ScanlineRasToDestBitmapRenderer sclineRasToBmp = gx.ScanlineRasToDestBitmap;

            if (this.PerspectiveTransformType == Sample_Perspective.PerspectiveTransformType.Bilinear)
            {
                var bound = lionShape.Bounds;
                Bilinear txBilinear = Bilinear.RectToQuad(bound.Left,
                    bound.Bottom,
                    bound.Right,
                    bound.Top,
                    quadPolygonControl.GetInnerCoords());
                if (txBilinear.IsValid)
                {


                    VectorToolBox.GetFreeVxs(out var v3);
                    lionShape.ApplyTransform(txBilinear);
                    lionShape.Paint(painter);
                    RectD lionBound = lionShape.Bounds;
                    Ellipse ell = new Ellipse((lionBound.Left + lionBound.Right) * 0.5,
                                     (lionBound.Bottom + lionBound.Top) * 0.5,
                                     (lionBound.Right - lionBound.Left) * 0.5,
                                     (lionBound.Top - lionBound.Bottom) * 0.5,
                                     200);
                    VectorToolBox.ReleaseVxs(ref v3);

                    //



                    VectorToolBox.GetFreeVxs(out var v1, out var trans_ell);

                    txBilinear.TransformToVxs(ell.MakeVxs(v1), trans_ell);
                    painter.FillColor = ColorEx.Make(0.5f, 0.3f, 0.0f, 0.3f);
                    painter.Fill(trans_ell);
                    //-------------------------------------------------------------
                    //outline
                    double prevStrokeWidth = painter.StrokeWidth;
                    painter.StrokeWidth = 3;
                    painter.StrokeColor = ColorEx.Make(0.0f, 0.3f, 0.2f, 1.0f);
                    painter.Draw(trans_ell);
                    painter.StrokeWidth = prevStrokeWidth;


                    VectorToolBox.ReleaseVxs(ref v1, ref trans_ell);
                }
            }
            else
            {
                RectD r = lionShape.Bounds;

                var txPerspective = new Perspective(
                   r.Left, r.Bottom, r.Right, r.Top,
                    quadPolygonControl.GetInnerCoords());
                if (txPerspective.IsValid)
                {

 
                    lionShape.Paint(p, txPerspective); //transform -> paint

                    //painter.PaintSeries(txPerspective.TransformToVxs(lionShape.Vxs, v1),
                    //  lionShape.Colors,
                    //  lionShape.PathIndexList,
                    //  lionShape.NumPaths);
                    //--------------------------------------------------------------------------------------
                    //filled Ellipse
                    //1. create original fill ellipse
                    RectD lionBound = lionShape.Bounds;
                    var filledEllipse = new Ellipse((lionBound.Left + lionBound.Right) * 0.5,
                                      (lionBound.Bottom + lionBound.Top) * 0.5,
                                      (lionBound.Right - lionBound.Left) * 0.5,
                                      (lionBound.Top - lionBound.Bottom) * 0.5,
                                      200);

                    VectorToolBox.GetFreeVxs(out var v2, out var transformedEll);

                    txPerspective.TransformToVxs(filledEllipse.MakeVxs(v2), transformedEll);
                    painter.FillColor = ColorEx.Make(0.5f, 0.3f, 0.0f, 0.3f);
                    painter.Fill(transformedEll);
                    //-------------------------------------------------------- 
                    var prevStrokeW = painter.StrokeWidth;
                    painter.StrokeWidth = 3;
                    painter.StrokeColor = ColorEx.Make(0.0f, 0.3f, 0.2f, 1.0f);
                    painter.Draw(transformedEll);
                    painter.StrokeWidth = prevStrokeW;


                    VectorToolBox.ReleaseVxs(ref v2, ref transformedEll);
                     
                }
            }

            //--------------------------
            // Render the "quad" tool and controls
            painter.FillColor = ColorEx.Make(0f, 0.3f, 0.5f, 0.6f);

            VectorToolBox.GetFreeVxs(out var v4);
            painter.Fill(quadPolygonControl.MakeVxs(v4));
            VectorToolBox.ReleaseVxs(ref v4);
        }

        public override void MouseDown(int x, int y, bool isRightButton)
        {
            var mouseEvent = new UI.MouseEventArgs(UI.MouseButtons.Left, 1, x, y, 0);
            quadPolygonControl.OnMouseDown(mouseEvent);
        }
        public override void MouseDrag(int x, int y)
        {
            var mouseEvent = new UI.MouseEventArgs(UI.MouseButtons.Left, 1, x, y, 0);
            quadPolygonControl.OnMouseMove(mouseEvent);
            base.MouseDrag(x, y);
        }
        public override void MouseUp(int x, int y)
        {
            var mouseEvent = new UI.MouseEventArgs(UI.MouseButtons.Left, 1, x, y, 0);
            quadPolygonControl.OnMouseUp(mouseEvent);
            base.MouseUp(x, y);
        }
    }


    public enum PerspectiveTransformType
    {
        Bilinear,
        Perspective
    }
}

