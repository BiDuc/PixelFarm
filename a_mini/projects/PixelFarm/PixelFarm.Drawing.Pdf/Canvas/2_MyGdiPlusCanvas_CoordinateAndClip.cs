﻿//BSD, 2014-2017, WinterDev 

namespace PixelFarm.Drawing.Pdf
{
    partial class MyGdiPlusCanvas
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

            ////----------- 
            //int total_dx = x - canvasOriginX;
            //int total_dy = y - canvasOriginY;
            //this.gx.TranslateTransform(total_dx, total_dy);
            ////clip rect move to another direction***
            //this.currentClipRect.Offset(-total_dx, -total_dy);
            //this.canvasOriginX = x;
            //this.canvasOriginY = y;
        }

        public override int CanvasOriginX
        {
            get { return this.canvasOriginX; }
        }
        public override int CanvasOriginY
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

            //gx.SetClip(
            //   this.currentClipRect = new System.Drawing.Rectangle(
            //        rect.X, rect.Y,
            //        rect.Width, rect.Height),
            //        (System.Drawing.Drawing2D.CombineMode)combineMode);
        }
        public bool IntersectsWith(Rectangle clientRect)
        {
            return clientRect.IntersectsWith(left, top, right, bottom);
        }

        public override bool PushClipAreaRect(int width, int height, ref Rectangle updateArea)
        {
            throw new System.NotSupportedException();
            //this.clipRectStack.Push(currentClipRect);
            //System.Drawing.Rectangle intersectResult =
            //      System.Drawing.Rectangle.Intersect(
            //      System.Drawing.Rectangle.FromLTRB(updateArea.Left, updateArea.Top, updateArea.Right, updateArea.Bottom),
            //      new System.Drawing.Rectangle(0, 0, width, height));
            //currentClipRect = intersectResult;
            //if (intersectResult.Width <= 0 || intersectResult.Height <= 0)
            //{
            //    //not intersec?
            //    return false;
            //}
            //else
            //{
            //    //updateArea = Conv.ToRect(intersectResult);
            //    //gx.SetClip(intersectResult);
            //    return true;
            //}
        }
        public override void PopClipAreaRect()
        {
            //if (clipRectStack.Count > 0)
            //{

            //    currentClipRect = clipRectStack.Pop();
            //    gx.SetClip(currentClipRect);
            //}
        }



        public override Rectangle CurrentClipRect
        {
            get
            {
                throw new System.NotSupportedException();
                //return currentClipRect.ToRect();
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