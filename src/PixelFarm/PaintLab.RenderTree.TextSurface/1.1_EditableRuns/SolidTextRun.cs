﻿//Apache2, 2014-present, WinterDev

using System;
using System.Text;
using PixelFarm.Drawing;

namespace LayoutFarm.Text
{
    public class SolidTextRun : EditableRun
    {
        //TODO: review here=> who should store/handle this handle? , owner TextBox or this run?
        Action<SolidTextRun, DrawBoard, Rectangle> _externalCustomDraw;
        TextSpanStyle spanStyle;
        char[] mybuffer;

        RenderElement _externalRenderE;


        public SolidTextRun(RootGraphic gfx, char[] copyBuffer, TextSpanStyle style)
            : base(gfx)
        {   //check line break? 
            this.spanStyle = style;
            this.mybuffer = copyBuffer;
            UpdateRunWidth();
        }
        //public SolidTextRun(RootGraphic gfx, char c, TextSpanStyle style)
        //    : base(gfx)
        //{
        //    this.spanStyle = style;
        //    mybuffer = new char[] { c };
        //    if (c == '\n')
        //    {
        //        this.IsLineBreak = true;
        //    }
        //    //check line break?
        //    UpdateRunWidth();
        //}
        public SolidTextRun(RootGraphic gfx, string str, TextSpanStyle style)
            : base(gfx)
        {
            this.spanStyle = style;
            if (str != null && str.Length > 0)
            {
                mybuffer = str.ToCharArray();
                if (mybuffer.Length == 1 && mybuffer[0] == '\n')
                {
                    this.IsLineBreak = true;
                }
                UpdateRunWidth();
            }
            else
            {
                throw new Exception("string must be null or zero length");
            }
        }
        //
        public void SetCustomExternalDraw(Action<SolidTextRun, DrawBoard, Rectangle> externalCustomDraw)
        {
            _externalCustomDraw = externalCustomDraw;
        }
        public void SetExternalRenderElement(RenderElement externalRenderE)
        {
            _externalRenderE = externalRenderE;
        }
        public string RawText
        {
            get; set;
        }

        public override void ResetRootGraphics(RootGraphic rootgfx)
        {
            DirectSetRootGraphics(this, rootgfx);
        }
        public override EditableRun Clone()
        {
            return new SolidTextRun(this.Root, this.GetText(), this.SpanStyle)
            {
                RawText = this.RawText
            };
        }
        public override EditableRun Copy(int startIndex)
        {
            if (startIndex == 0)
            {
                int length = mybuffer.Length - startIndex;
                if (startIndex > -1 && length > 0)
                {
                    return MakeTextRun(startIndex, length);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        EditableRun MakeTextRun(int sourceIndex, int length)
        {
            if (length > 0)
            {
                sourceIndex = 0;
                length = mybuffer.Length;
                EditableRun newTextRun = null;
                char[] newContent = new char[length];
                Array.Copy(this.mybuffer, sourceIndex, newContent, 0, length);
                SolidTextRun solidRun = new SolidTextRun(this.Root, newContent, this.SpanStyle) { RawText = this.RawText };


                solidRun.SetCustomExternalDraw(this._externalCustomDraw); //also copy drawing handler?
                newTextRun = solidRun;

                newTextRun.IsLineBreak = this.IsLineBreak;
                newTextRun.UpdateRunWidth();
                return newTextRun;
            }
            else
            {
                throw new Exception("string must be null or zero length");
            }
        }
        public override int GetRunWidth(int charOffset)
        {
            return CalculateDrawingStringSize(mybuffer, charOffset).Width;
        }
        public override string GetText()
        {
            return new string(mybuffer);
        }



        public override void UpdateRunWidth()
        {
            Size size;
            if (IsLineBreak)
            {
                size = new Size(0, (int)Math.Round(Root.TextServices.MeasureBlankLineHeight(GetFont())));
            }
            else
            {
                size = CalculateDrawingStringSize(this.mybuffer, mybuffer.Length);
            }
            this.SetSize(size.Width, size.Height);
            MarkHasValidCalculateSize();
        }
        public override char GetChar(int index)
        {
            return mybuffer[index];
        }
        public override void CopyContentToStringBuilder(StringBuilder stBuilder)
        {
            if (IsLineBreak)
            {
                stBuilder.Append("\r\n");
            }
            else
            {
                stBuilder.Append(RawText);
            }
        }
        public override int CharacterCount
        {
            get
            {
                switch (mybuffer.Length)
                {
                    case 0: return 0;
                    default: return 1;
                }
            }
        }
        public override TextSpanStyle SpanStyle
        {
            get
            {
                return this.spanStyle;
            }
        }
        public override void SetStyle(TextSpanStyle spanStyle)
        {
            this.InvalidateGraphics();
            this.spanStyle = spanStyle;
            this.InvalidateGraphics();
            UpdateRunWidth();
        }
        Size CalculateDrawingStringSize(char[] buffer, int length)
        {
            var textBufferSpan = new TextBufferSpan(buffer, 0, length);
            return this.Root.TextServices.MeasureString(ref textBufferSpan, GetFont());

        }
        protected RequestFont GetFont()
        {
            if (!HasStyle)
            {
                return this.Root.DefaultTextEditFontInfo;
            }
            else
            {
                TextSpanStyle spanStyle = this.SpanStyle;
                if (spanStyle.ReqFont != null)
                {
                    return spanStyle.ReqFont;
                }
                else
                {
                    //TODO: review here
                    return this.Root.DefaultTextEditFontInfo;
                }
            }
        }

        public override EditableRun Copy(int startIndex, int length)
        {
            if (startIndex > -1 && length > 0)
            {
                return MakeTextRun(startIndex, length);
            }
            else
            {
                return null;
            }
        }
        const int SAME_FONT_SAME_TEXT_COLOR = 0;
        const int SAME_FONT_DIFF_TEXT_COLOR = 1;
        const int DIFF_FONT_SAME_TEXT_COLOR = 2;
        const int DIFF_FONT_DIFF_TEXT_COLOR = 3;
        static int EvaluateFontAndTextColor(DrawBoard canvas, TextSpanStyle spanStyle)
        {
            var font = spanStyle.ReqFont;
            var color = spanStyle.FontColor;
            var currentTextFont = canvas.CurrentFont;
            var currentTextColor = canvas.CurrentTextColor;
            if (font != null && font != currentTextFont)
            {
                if (currentTextColor != color)
                {
                    return DIFF_FONT_DIFF_TEXT_COLOR;
                }
                else
                {
                    return DIFF_FONT_SAME_TEXT_COLOR;
                }
            }
            else
            {
                if (currentTextColor != color)
                {
                    return SAME_FONT_DIFF_TEXT_COLOR;
                }
                else
                {
                    return SAME_FONT_SAME_TEXT_COLOR;
                }
            }
        }
        protected bool HasStyle
        {
            get
            {
                return !this.SpanStyle.IsEmpty();
            }
        }

        public override void CustomDrawToThisCanvas(DrawBoard canvas, Rectangle updateArea)
        {
            if (_externalCustomDraw != null)
            {
                _externalCustomDraw(this, canvas, updateArea);
                return;
            }
            else if (_externalRenderE != null)
            {
                _externalRenderE.CustomDrawToThisCanvas(canvas, updateArea);
                return;
            }

            int bWidth = this.Width;
            int bHeight = this.Height;

            //1. bg
            canvas.FillRectangle(Color.Yellow, 0, 0, bWidth, bHeight);

            if (!this.HasStyle)
            {
                canvas.DrawText(this.mybuffer, new Rectangle(0, 0, bWidth, bHeight), 0);
            }
            else
            {
                //TODO: review here, we don't need to do this

                TextSpanStyle style = this.SpanStyle;
                switch (EvaluateFontAndTextColor(canvas, style))
                {
                    case DIFF_FONT_SAME_TEXT_COLOR:
                        {
                            var prevFont = canvas.CurrentFont;
                            canvas.CurrentFont = style.ReqFont;
                            canvas.DrawText(this.mybuffer,
                               new Rectangle(0, 0, bWidth, bHeight),
                               style.ContentHAlign);
                            canvas.CurrentFont = prevFont;
                        }
                        break;
                    case DIFF_FONT_DIFF_TEXT_COLOR:
                        {
                            var prevFont = canvas.CurrentFont;
                            var prevColor = canvas.CurrentTextColor;
                            canvas.CurrentFont = style.ReqFont;
                            canvas.CurrentTextColor = style.FontColor;
                            canvas.DrawText(this.mybuffer,
                               new Rectangle(0, 0, bWidth, bHeight),
                               style.ContentHAlign);
                            canvas.CurrentFont = prevFont;
                            canvas.CurrentTextColor = prevColor;
                        }
                        break;
                    case SAME_FONT_DIFF_TEXT_COLOR:
                        {
                            var prevColor = canvas.CurrentTextColor;
                            canvas.DrawText(this.mybuffer,
                                new Rectangle(0, 0, bWidth, bHeight),
                                style.ContentHAlign);
                            canvas.CurrentTextColor = prevColor;
                        }
                        break;
                    default:
                        {
                            canvas.DrawText(this.mybuffer,
                               new Rectangle(0, 0, bWidth, bHeight),
                               style.ContentHAlign);
                        }
                        break;
                }
            }
        }


        public override EditableRunCharLocation GetCharacterFromPixelOffset(int pixelOffset)
        {
            if (pixelOffset < Width)
            {
                return new EditableRunCharLocation(0, 0);
            }
            else
            {
                //exceed than the bound of this run
                return new EditableRunCharLocation(0, 1);
            }
        }
        //-------------------------------------------
        internal override bool IsInsertable
        {
            get
            {
                return false;
            }
        }
        public override EditableRun LeftCopy(int index)
        {
            if (index == 0)
            {
                return null;
            }

            if (index > -1)
            {
                return MakeTextRun(0, this.mybuffer.Length);
            }
            else
            {
                return null;
            }
        }
        internal override void InsertAfter(int index, char c)
        {
            //TODO: review here
            //solid text run should not be editable
            int oldLexLength = mybuffer.Length;
            char[] newBuff = new char[oldLexLength + 1];
            if (index > -1 && index < mybuffer.Length - 1)
            {
                Array.Copy(mybuffer, newBuff, index + 1);
                newBuff[index + 1] = c;
                Array.Copy(mybuffer, index + 1, newBuff, index + 2, oldLexLength - index - 1);
            }
            else if (index == -1)
            {
                newBuff[0] = c;
                Array.Copy(mybuffer, 0, newBuff, 1, mybuffer.Length);
            }
            else if (index == oldLexLength - 1)
            {
                Array.Copy(mybuffer, newBuff, oldLexLength);
                newBuff[oldLexLength] = c;
            }
            else
            {
                throw new NotSupportedException();
            }
            this.mybuffer = newBuff;
            UpdateRunWidth();
        }
        internal override EditableRun Remove(int startIndex, int length, bool withFreeRun)
        {
            if (startIndex == this.mybuffer.Length)
            {
                //at the end
                return null;
            }

            //
            startIndex = 0; //***
            length = this.mybuffer.Length;
            EditableRun freeRun = null;
            if (startIndex > -1 && length > 0)
            {
                int oldLexLength = mybuffer.Length;
                char[] newBuff = new char[oldLexLength - length];
                if (withFreeRun)
                {
                    freeRun = MakeTextRun(startIndex, length);
                }
                if (startIndex > 0)
                {
                    Array.Copy(mybuffer, 0, newBuff, 0, startIndex);
                }

                Array.Copy(mybuffer, startIndex + length, newBuff, startIndex, oldLexLength - startIndex - length);
                this.mybuffer = newBuff;
                UpdateRunWidth();
            }

            if (withFreeRun)
            {
                return freeRun;
            }
            else
            {
                return null;
            }
        }
    }
}
