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

namespace PixelFarm.Drawing.GLES2
{
    partial class MyGLDrawBoard
    {
        //TODO: review drawstring again ***
        Color _currentTextColor;
        public override RequestFont CurrentFont
        {
            get
            {
                return _painter.CurrentFont;
            }
            set
            {
                _painter.CurrentFont = value;
            }
        }
        /// <summary>
        /// current text fill color
        /// </summary>
        public override Color CurrentTextColor
        {
            get
            {
                return _currentTextColor;
            }
            set
            {
                this._currentTextColor = value;
                //set this to 
                _painter.TextPrinter.ChangeFillColor(value);
            }
        }
        public override void DrawText(char[] buffer, int x, int y)
        {
            var prevColor = _painter.FillColor;
            _painter.FillColor = PixelFarm.Drawing.Color.Black;
            _painter.TextPrinter.DrawString(buffer, 0, buffer.Length, x, this.Height - y);
            _painter.FillColor = prevColor;
        }
        public override void DrawText(char[] buffer, Rectangle logicalTextBox, int textAlignment)
        {
            var prevColor = _painter.FillColor;
            _painter.FillColor = PixelFarm.Drawing.Color.Black;
            _painter.TextPrinter.DrawString(buffer, 0, buffer.Length, logicalTextBox.X, this.Height - logicalTextBox.Y);
            _painter.FillColor = prevColor;
        }
        public override void DrawText(char[] str, int startAt, int len, Rectangle logicalTextBox, int textAlignment)
        {
            //TODO: review again
            var prevColor = _painter.FillColor;
            _painter.FillColor = PixelFarm.Drawing.Color.Black;
            _painter.TextPrinter.DrawString(str, startAt, len, logicalTextBox.X, this.Height - logicalTextBox.Y);
            _painter.FillColor = prevColor;
            //TODO: review here
            //painter1.FillRectangle(0, 0, 20, 20, Color.Red);

            //painter1.FillColor = Color.Blue;
            //painter1.FillRectangle(0, 0, 20, 20);
            ////var intersectRect = Rectangle.Intersect(logicalTextBox,
            ////    new Rectangle(currentClipRect.Left,
            ////        currentClipRect.Top,
            ////        currentClipRect.Width,
            ////        currentClipRect.Height));
            ////intersectRect.Offset(canvasOriginX, canvasOriginY);
            ////MyWin32.SetRectRgn(hRgn,
            //// intersectRect.Left,
            //// intersectRect.Top,
            //// intersectRect.Right,
            //// intersectRect.Bottom);
            ////MyWin32.SelectClipRgn(tempDc, hRgn);



            //var tmpColor = this.internalSolidBrush.Color;
            //internalSolidBrush.Color = this.currentTextColor;
            //gx.DrawString(new string(str, startAt, len),
            //    (System.Drawing.Font)this.currentTextFont.InnerFont,
            //    internalSolidBrush,
            //    logicalTextBox.X,
            //    logicalTextBox.Y);
            ////new System.Drawing.RectangleF(
            ////    logicalTextBox.X,
            ////    logicalTextBox.Y,
            ////    logicalTextBox.Width,
            ////    logicalTextBox.Height));
            //internalSolidBrush.Color = tmpColor;
            ////var str= new string(
            ////fixed (char* startAddr = &str[0])
            ////{
            ////    Win32.Win32Utils.TextOut2(tempDc,
            ////        (int)logicalTextBox.X + canvasOriginX,
            ////        (int)logicalTextBox.Y + canvasOriginY,
            ////        (startAddr + startAt), len);
            ////}


        }
    }
}