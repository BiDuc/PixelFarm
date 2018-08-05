﻿//BSD, 2014-present, WinterDev
//ArthurHub, Jose Manuel Menendez Poo

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

namespace PixelFarm.Drawing.WinGdi
{
    partial class GdiPlusDrawBoard
    {
        int left;
        int top;
        int right;
        int bottom;
        int canvasOriginX = 0;
        int canvasOriginY = 0;
        Rectangle invalidateArea;

        bool isEmptyInvalidateArea;
        //--------------------------------------------------------------------
        public override void SetCanvasOrigin(int x, int y)
        {
            _gdigsx.SetCanvasOrigin(x, y);
            //----------- 
            int total_dx = x - canvasOriginX;
            int total_dy = y - canvasOriginY;

            this.canvasOriginX = x;
            this.canvasOriginY = y;
        }

        public override int OriginX
        {
            get { return this.canvasOriginX; }
        }
        public override int OriginY
        {
            get { return this.canvasOriginY; }
        }


        /// <summary>
        /// Sets the clipping region of this <see cref="T:System.Drawing.Graphics"/> to the result of the specified operation combining the current clip region and the rectangle specified by a <see cref="T:System.Drawing.RectangleF"/> structure.
        /// </summary>
        /// <param name="rect"><see cref="T:System.Drawing.RectangleF"/> structure to combine. </param>
        /// <param name="combineMode">Member of the <see cref="T:System.Drawing.Drawing2D.CombineMode"/> enumeration that specifies the combining operation to use. </param>
        public override void SetClipRect(Rectangle rect, CombineMode combineMode = CombineMode.Replace)
        {
            _gdigsx.SetClipRect(rect, combineMode);
        }
        public bool IntersectsWith(Rectangle clientRect)
        {
            return clientRect.IntersectsWith(left, top, right, bottom);
        }

        public override bool PushClipAreaRect(int width, int height, ref Rectangle updateArea)
        {
            return _gdigsx.PushClipAreaRect(width, height, ref updateArea);
        }
        public override void PopClipAreaRect()
        {
            _gdigsx.PopClipAreaRect();
        }
        public override Rectangle CurrentClipRect
        {
            get
            {
                return _gdigsx.CurrentClipRect;
            }
        }



        public override int Top
        {
            get
            {
                return top;
            }
        }
        public override int Left
        {
            get
            {
                return left;
            }
        }

        public override int Width
        {
            get
            {
#if DEBUG
                if(right -left == 863)
                {

                }
#endif
                return right - left;
            }
        }
        public override int Height
        {
            get
            {
                return bottom - top;
            }
        }
        public override int Bottom
        {
            get
            {
                return bottom;
            }
        }
        public override int Right
        {
            get
            {
                return right;
            }
        }
        public override Rectangle Rect
        {
            get
            {
                return Rectangle.FromLTRB(left, top, right, bottom);
            }
        }
        public override Rectangle InvalidateArea
        {
            get
            {
                return invalidateArea;
            }
        }

        public override void ResetInvalidateArea()
        {
            this.invalidateArea = Rectangle.Empty;
            this.isEmptyInvalidateArea = true;//set
        }
        public override void Invalidate(Rectangle rect)
        {
            if (isEmptyInvalidateArea)
            {
                invalidateArea = rect;
                isEmptyInvalidateArea = false;
            }
            else
            {
                invalidateArea = Rectangle.Union(rect, invalidateArea);
            }

            //need to draw again
            this.IsContentReady = false;
        }
        public bool IsContentReady { get; set; }
    }
}