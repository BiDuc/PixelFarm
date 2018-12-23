﻿//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
namespace LayoutFarm.Text
{
    class CaretRenderElement : RenderElement
    {
        //implement caret for text edit
        public CaretRenderElement(RootGraphic g, int w, int h)
            : base(g, w, h)
        {
        }
        public override void CustomDrawToThisCanvas(DrawBoard canvas, Rectangle updateArea)
        {
        }
        public override void ResetRootGraphics(RootGraphic rootgfx)
        {
            DirectSetRootGraphics(this, rootgfx);
        }
        internal void DrawCaret(DrawBoard canvas, int x, int y)
        {
            //TODO: config? color or shape of caret
            canvas.FillRectangle(Color.Black, x, y, this.Width, this.Height);
        }
    }
}