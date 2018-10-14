﻿//Apache2, 2014-present, WinterDev

namespace LayoutFarm.Text
{
    public abstract class VisualPointInfo
    {
        int lineCharIndex;
        EditableRun onVisualElement;
        int onTextRunCharOffset;
        int caretXPos;
        int onTextRunPixelOffset;
        public VisualPointInfo(int lineCharIndex)
        {
            this.lineCharIndex = lineCharIndex;
        }

        public void SetAdditionVisualInfo(EditableRun onTextRun, int onTextRunCharOffset, int caretXPos, int textRunPixelOffset)
        {
            this.caretXPos = caretXPos;
            this.onVisualElement = onTextRun;
            this.onTextRunCharOffset = onTextRunCharOffset;
            this.onTextRunPixelOffset = textRunPixelOffset;
        }
        public int LineCharIndex
        {
            get
            {
                return lineCharIndex;
            }
        }
        public int TextRunCharOffset
        {
            get
            {
                return onTextRunCharOffset;
            }
        }
        public EditableRun TextRun
        {
            get
            {
                return onVisualElement;
            }
        }
        public bool IsOnTheBeginOfLine
        {
            get
            {
                return RunLocalSelectedIndex == -1;
            }
        }
        public abstract int LineId
        {
            get;
        }
        public abstract int LineTop
        {
            get;
        }
        public abstract int ActualLineHeight
        {
            get;
        }
        public abstract int LineNumber
        {
            get;
        }
        public abstract int CurrentWidth
        {
            get;
        }

        public int RunLocalSelectedIndex
        {
            get
            {
                return lineCharIndex - onTextRunCharOffset;
            }
        }
        public int X
        {
            get
            {
                return caretXPos;
            }
        }
        public int TextRunPixelOffset
        {
            get
            {
                return onTextRunPixelOffset;
            }
        }

#if DEBUG
        public override string ToString()
        {
            if (onVisualElement == null)
            {
                return "null " + " ,local[" + RunLocalSelectedIndex + "]";
            }
            else
            {
                return onVisualElement.ToString() + " ,local[" + RunLocalSelectedIndex + "]";
            }
        }
#endif

    }


    public class EditableVisualPointInfo : VisualPointInfo
    {
        EditableTextLine line;
        internal EditableVisualPointInfo(EditableTextLine line, int index)
            : base(index)
        {
            this.line = line;
        }
        internal EditableTextLine Line
        {
            get
            {
                return this.line;
            }
        }
        internal EditableTextLine EditableLine
        {
            get
            {
                return this.line;
            }
        }
        public override int LineTop
        {
            get { return this.line.LineTop; }
        }
        public override int CurrentWidth
        {
            get { return this.line.LineWidth; }
        }
        public override int ActualLineHeight
        {
            get
            {
                return line.ActualLineHeight;
            }
        }
        public override int LineNumber
        {
            get
            {
                return this.line.LineNumber;
            }
        }
        public override int LineId
        {
            get
            {
                return this.line.LineNumber;
            }
        }
    }
}
