﻿//Apache2, 2014-2017, WinterDev

using System;
using System.Collections.Generic;
namespace LayoutFarm.Text
{
    partial class EditableTextLine
    {
        public void RefreshInlineArrange()
        {
            EditableRun r = this.FirstRun;
            int lastestX = 0;
            while (r != null)
            {
                r.SetLocation(
                        lastestX,
                        r.Y);
                lastestX += r.Width;
                r = r.NextTextRun;
            }
        }
        internal void SetPostArrangeLineSize(int lineWidth, int lineHeight)
        {
            this.actualLineWidth = lineWidth;
            this.actualLineHeight = lineHeight;

        }
        public void LocalSuspendLineReArrange()
        {
            lineFlags |= LOCAL_SUSPEND_LINE_REARRANGE;
        }
        public void LocalResumeLineReArrange()
        {
            lineFlags &= ~LOCAL_SUSPEND_LINE_REARRANGE;
            LinkedListNode<EditableRun> curNode = this.First;
            int cx = 0;
            while (curNode != null)
            {
                EditableRun ve = curNode.Value;
                ve.SetLocation(cx, 0);
                cx += ve.Width;
                curNode = curNode.Next;
            }
        }
    }
}