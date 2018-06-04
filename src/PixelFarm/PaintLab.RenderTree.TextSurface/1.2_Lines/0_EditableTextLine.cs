﻿//Apache2, 2014-2018, WinterDev

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing;
using LayoutFarm.RenderBoxes;
namespace LayoutFarm.Text
{
#if DEBUG
    [DebuggerDisplay("ELN {dbugShortLineInfo}")]
#endif
    sealed partial class EditableTextLine
    {
        LinkedList<EditableRun> _runs = new LinkedList<EditableRun>();

        internal EditableTextFlowLayer editableFlowLayer;
        int currentLineNumber;
        int actualLineHeight;
        int actualLineWidth;
        int lineTop;
        int lineFlags;
        const int LINE_CONTENT_ARRANGED = 1 << (1 - 1);
        const int LINE_SIZE_VALID = 1 << (2 - 1);
        const int LOCAL_SUSPEND_LINE_REARRANGE = 1 << (3 - 1);
        const int END_WITH_LINE_BREAK = 1 << (4 - 1);

#if DEBUG
        static int dbugLineTotalCount = 0;
        internal int dbugLineId;
#endif
        internal EditableTextLine(EditableTextFlowLayer ownerFlowLayer)
        {

            this.editableFlowLayer = ownerFlowLayer;
            this.actualLineHeight = ownerFlowLayer.DefaultLineHeight; //we start with default line height
#if DEBUG
            this.dbugLineId = dbugLineTotalCount;
            dbugLineTotalCount++;
#endif
        }
        public int RunCount
        {
            get { return _runs.Count; }
        }
        /// <summary>
        /// first run node
        /// </summary>
        public LinkedListNode<EditableRun> First
        {
            get { return _runs.First; }
        }
        /// <summary>
        /// last run node
        /// </summary>
        public LinkedListNode<EditableRun> Last
        {
            get { return _runs.Last; }
        }

        RootGraphic Root
        {
            get { return this.OwnerElement.Root; }
        }
        public IEnumerable<EditableRun> GetTextRunIter()
        {
            foreach (EditableRun r in _runs)
            {
                yield return r;
            }
        }
        internal EditableRun LastRun
        {
            get
            {
                if (this.RunCount > 0)
                {
                    return this.Last.Value;
                }
                else
                {
                    return null;
                }
            }
        }
        public float GetXOffsetAtCharIndex(int charIndex)
        {
            float xoffset = 0;
            int acc_charCount = 0;
            foreach (EditableRun r in _runs)
            {
                if (r.CharacterCount + acc_charCount >= charIndex)
                {
                    //found at this run
                    return xoffset + r.GetRunWidth(charIndex - acc_charCount);
                }
                xoffset += r.Width;
                acc_charCount += r.CharacterCount;
            }
            return 0;//?
        }
        public void TextLineReCalculateActualLineSize()
        {
            EditableRun r = this.FirstRun;
            int maxHeight = 2;
            int accumWidth = 0;
            while (r != null)
            {
                if (r.Height > maxHeight)
                {
                    maxHeight = r.Height;
                }
                accumWidth += r.Width;
                r = r.NextTextRun;
            }
            this.actualLineWidth = accumWidth;
            this.actualLineHeight = maxHeight;

            if (this.RunCount == 0)
            {
                //no span
                this.actualLineHeight = OwnerFlowLayer.DefaultLineHeight;
            }
        }
        internal bool HitTestCore(HitChain hitChain)
        {
            int testX;
            int testY;
            hitChain.GetTestPoint(out testX, out testY);
            if (this.RunCount == 0)
            {
                return false;
            }
            else
            {
                LinkedListNode<EditableRun> cnode = this.First;
                int curLineTop = this.lineTop;
                hitChain.OffsetTestPoint(0, -curLineTop);
                while (cnode != null)
                {
                    if (cnode.Value.HitTestCore(hitChain))
                    {
                        hitChain.OffsetTestPoint(0, curLineTop);
                        return true;
                    }
                    cnode = cnode.Next;
                }
                hitChain.OffsetTestPoint(0, curLineTop);
                return false;
            }
        }

        public RenderElement OwnerElement
        {
            get
            {
                if (editableFlowLayer != null)
                {
                    return editableFlowLayer.OwnerRenderElement;
                }
                else
                {
                    return null;
                }
            }
        }
        public EditableTextFlowLayer OwnerFlowLayer
        {
            get
            {
                return this.editableFlowLayer;
            }
        }
        public bool EndWithLineBreak
        {
            get
            {
                return (lineFlags & END_WITH_LINE_BREAK) != 0;
            }
            set
            {
                if (value)
                {
                    lineFlags |= END_WITH_LINE_BREAK;
                }
                else
                {
                    lineFlags &= ~END_WITH_LINE_BREAK;
                }
            }
        }

        public bool IntersectsWith(int y)
        {
            return y >= lineTop && y < (lineTop + actualLineHeight);
        }
        public int LineTop
        {
            get
            {
                return lineTop;
            }
        }
        public int ActualLineHeight
        {
            get
            {
                return actualLineHeight;
            }
        }
        public int ActualLineWidth
        {
            get
            {
                return actualLineWidth;
            }
        }
        public Rectangle ActualLineArea
        {
            get
            {
                return new Rectangle(0, lineTop, actualLineWidth, actualLineHeight);
            }
        }
        public Rectangle ParentLineArea
        {
            get
            {
                return new Rectangle(0, lineTop, this.editableFlowLayer.OwnerRenderElement.Width, actualLineHeight);
            }
        }
        internal IEnumerable<EditableRun> GetVisualElementForward(EditableRun startVisualElement)
        {
            if (startVisualElement != null)
            {
                yield return startVisualElement;
                var curRun = startVisualElement.NextTextRun;
                while (curRun != null)
                {
                    yield return curRun;
                    curRun = curRun.NextTextRun;
                }
            }
        }
        internal IEnumerable<EditableRun> GetVisualElementForward(EditableRun startVisualElement, EditableRun stopVisualElement)
        {
            if (startVisualElement != null)
            {
                LinkedListNode<EditableRun> lexnode = GetLineLinkedNode(startVisualElement);
                while (lexnode != null)
                {
                    yield return lexnode.Value;
                    if (lexnode.Value == stopVisualElement)
                    {
                        break;
                    }
                    lexnode = lexnode.Next;
                }
            }
        }
        public int CharCount
        {
            get
            {
                //TODO: reimplement this again
                int charCount = 0;
                foreach (EditableRun r in this._runs)
                {
                    charCount += r.CharacterCount;
                }
                return charCount;
            }
        }
        public int LineBottom
        {
            get
            {
                return lineTop + actualLineHeight;
            }
        }

        internal int LineWidth
        {
            get
            {
                var lastRun = this.LastRun;
                if (lastRun == null)
                {
                    return 0;
                }
                else
                {
                    return lastRun.Right;
                }
            }
        }

        public int Top
        {
            get
            {
                return lineTop;
            }
        }

        public void SetTop(int linetop)
        {
            this.lineTop = linetop;
        }
#if DEBUG
        public override string ToString()
        {
            return this.dbugShortLineInfo;
        }
        public string dbugShortLineInfo
        {
            get
            {
                return "LINE[" + dbugLineId + "]:" + this.currentLineNumber + "{T:" + lineTop.ToString() + ",W:" +
                   actualLineWidth + ",H:" + this.actualLineHeight + "}";
            }
        }
#endif
        public int LineNumber
        {
            get
            {
                return currentLineNumber;
            }
        }
        internal void SetLineNumber(int value)
        {
            this.currentLineNumber = value;
        }
        bool IsFirstLine
        {
            get
            {
                return currentLineNumber == 0;
            }
        }
        bool IsLastLine
        {
            get
            {
                return currentLineNumber == editableFlowLayer.LineCount - 1;
            }
        }
        bool IsSingleLine
        {
            get
            {
                return IsFirstLine && IsLastLine;
            }
        }
        public bool IsBlankLine
        {
            get
            {
                return RunCount == 0;
            }
        }
        public EditableTextLine Next
        {
            get
            {
                if (currentLineNumber < editableFlowLayer.LineCount - 1)
                {
                    return editableFlowLayer.GetTextLine(currentLineNumber + 1);
                }
                else
                {
                    return null;
                }
            }
        }
        public EditableTextLine Prev
        {
            get
            {
                if (currentLineNumber > 0)
                {
                    return editableFlowLayer.GetTextLine(currentLineNumber - 1);
                }
                else
                {
                    return null;
                }
            }
        }

        public EditableRun FirstRun
        {
            get
            {
                if (this.RunCount > 0)
                {
                    return this.First.Value;
                }
                else
                {
                    return null;
                }
            }
        }
        public bool NeedArrange
        {
            get
            {
                return (lineFlags & LINE_CONTENT_ARRANGED) == 0;
            }
        }
        internal void ValidateContentArrangement()
        {
            lineFlags |= LINE_CONTENT_ARRANGED;
        }
        public static void InnerCopyLineContent(EditableTextLine line, StringBuilder stBuilder)
        {
            line.CopyLineContent(stBuilder);
        }
        public void CopyLineContent(StringBuilder stBuilder)
        {
            LinkedListNode<EditableRun> curNode = this.First;
            while (curNode != null)
            {
                EditableRun v = curNode.Value;
                v.CopyContentToStringBuilder(stBuilder);
                curNode = curNode.Next;
            }
        }
        internal bool IsLocalSuspendLineRearrange
        {
            get
            {
                return (this.lineFlags & LOCAL_SUSPEND_LINE_REARRANGE) != 0;
            }
        }

        internal void InvalidateLineLayout()
        {
            this.lineFlags &= ~LINE_SIZE_VALID;
            this.lineFlags &= ~LINE_CONTENT_ARRANGED;
        }
    }
}