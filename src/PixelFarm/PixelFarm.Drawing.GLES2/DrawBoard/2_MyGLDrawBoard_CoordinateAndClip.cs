﻿//BSD, 2014-2018, WinterDev
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
namespace PixelFarm.Drawing.GLES2
{
    partial class MyGLDrawBoard
    {
        int _left;
        int _top;
        int _width;
        int _height;

        int canvasOriginX = 0;
        int canvasOriginY = 0;
        Rectangle invalidateArea;

        bool isEmptyInvalidateArea;
        //--------------------------------------------------------------------
        public override void SetCanvasOrigin(int x, int y)
        {

            painter1.SetOrigin(x, -y);
            //----------- 
            int total_dx = x - canvasOriginX;
            int total_dy = y - canvasOriginY;
            //this.gx.TranslateTransform(total_dx, total_dy);
            //clip rect move to another direction***
            this.currentClipRect.Offset(-total_dx, -total_dy);
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
        public override void SetClipRect(Rectangle rect, CombineMode combineMode = CombineMode.Replace)
        {
            //TODO: reivew clip combine mode
            painter1.SetClipBox(rect.Left, rect.Bottom, rect.Right, rect.Top);
        }
        public override bool PushClipAreaRect(int width, int height, ref Rectangle updateArea)
        {
            //TODO: review here
            return true;
            // throw new NotSupportedException();
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
            //    updateArea = Conv.ToRect(intersectResult);
            //    gx.SetClip(intersectResult);
            //    return true;
            //}
        }
        public override void PopClipAreaRect()
        {
            if (clipRectStack.Count > 0)
            {
                currentClipRect = clipRectStack.Pop();
                painter1.SetClipBox(currentClipRect.Left, currentClipRect.Top, currentClipRect.Right, currentClipRect.Bottom);
                //gx.SetClip(currentClipRect);
            }
        }
        public override Rectangle CurrentClipRect
        {
            get
            {
                return currentClipRect;
            }
        }
        public override int Top
        {
            get
            {
                return _top;
            }
        }
        public override int Left
        {
            get
            {
                return _left;
            }
        }
        public override int Width
        {
            get
            {
                return _width;
            }
        }
        public override int Height
        {
            get
            {
                return _height;
            }
        }
        public override int Bottom
        {
            get
            {
                return _top + _height;
            }
        }
        public override int Right
        {
            get
            {
                return _left + _width;
            }
        }
        public override Rectangle Rect
        {
            get
            {
                return new Rectangle(_left, _top, _width, _height);
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