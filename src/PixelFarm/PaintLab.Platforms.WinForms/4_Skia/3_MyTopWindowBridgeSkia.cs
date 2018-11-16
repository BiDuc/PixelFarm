﻿//Apache2, 2014-present, WinterDev

using System;
using System.Windows.Forms;
using PixelFarm.Drawing;
#if __SKIA__
namespace LayoutFarm.UI.Skia
{
    class MyTopWindowBridgeSkia : TopWindowBridgeWinForm
    {
        Control windowControl;
        SkiaCanvasViewport canvasViewport;
        public MyTopWindowBridgeSkia(RootGraphic root, ITopWindowEventRoot topWinEventRoot)
            : base(root, topWinEventRoot)
        {
        }
        public override void PaintToOutputWindow(Rectangle invalidateArea)
        {
            throw new NotImplementedException();
        }
        public override void BindWindowControl(Control windowControl)
        {
            //bind to anycontrol GDI control  
            this.windowControl = windowControl;
            this.SetBaseCanvasViewport(this.canvasViewport = new SkiaCanvasViewport(this.RootGfx,
                this.Size.ToSize()));

            this.RootGfx.SetPaintDelegates(
                    this.canvasViewport.CanvasInvalidateArea,
                    this.PaintToOutputWindow);
#if DEBUG
            this.dbugWinControl = windowControl;
            this.canvasViewport.dbugOutputWindow = this;
#endif
            this.EvaluateScrollbar();
        }
        System.Drawing.Size Size
        {
            get { return this.windowControl.Size; }
        }
        public override void InvalidateRootArea(Rectangle r)
        {
            Rectangle rect = r;
            this.RootGfx.InvalidateGraphicArea(
                RootGfx.TopWindowRenderBox,
                ref rect);
        }
        public override void PaintToOutputWindow()
        {
            //*** force paint to output viewdow
            IntPtr hdc = GetDC(this.windowControl.Handle);
            this.canvasViewport.PaintMe(hdc);
            ReleaseDC(this.windowControl.Handle, hdc);
        }
        public override void CopyOutputPixelBuffer(int x, int y, int w, int h, IntPtr outputBuffer)
        {
            throw new NotImplementedException();
        }
        protected override void ChangeCursorStyle(MouseCursorStyle cursorStyle)
        {
            switch (cursorStyle)
            {
                case MouseCursorStyle.Pointer:
                    {
                        windowControl.Cursor = Cursors.Hand;
                    }
                    break;
                case MouseCursorStyle.IBeam:
                    {
                        windowControl.Cursor = Cursors.IBeam;
                    }
                    break;
                default:
                    {
                        windowControl.Cursor = Cursors.Default;
                    }
                    break;
            }
        }
    }
}

#endif