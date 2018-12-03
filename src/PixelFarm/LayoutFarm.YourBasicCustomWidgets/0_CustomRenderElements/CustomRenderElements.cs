﻿//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
namespace LayoutFarm.CustomWidgets
{
    public class CustomRenderBox : RenderBoxBase
    {
        Color _backColor;

        public CustomRenderBox(RootGraphic rootgfx, int width, int height)
            : base(rootgfx, width, height)
        {
            this.BackColor = Color.LightGray;
        }

        public Color BackColor
        {
            get => _backColor;
            set
            {
                _backColor = value;
                if (this.HasParentLink)
                {
                    this.InvalidateGraphics();
                }
            }
        }
        protected override void DrawBoxContent(DrawBoard canvas, Rectangle updateArea)
        {
#if DEBUG
            if (this.dbugBreak)
            {
            }
#endif

            if (this.MayHasViewport)
            {
                //TODO: review here
                //start pos of background fill
                //(0,0) 
                //(viewportX,viewportY)
                //tile or limit
                canvas.FillRectangle(BackColor, ViewportX, ViewportY, this.Width, this.Height);
            }
            else
            {
                canvas.FillRectangle(BackColor, 0, 0, this.Width, this.Height);
            }

            this.DrawDefaultLayer(canvas, ref updateArea);
#if DEBUG
            //canvas.dbug_DrawCrossRect(PixelFarm.Drawing.Color.Black,
            //    new Rectangle(0, 0, this.Width, this.Height));

            //canvas.dbug_DrawCrossRect(PixelFarm.Drawing.Color.Black,
            //   new Rectangle(updateArea.Left, updateArea.Top, updateArea.Width, updateArea.Height));
#endif
        }
    }
}