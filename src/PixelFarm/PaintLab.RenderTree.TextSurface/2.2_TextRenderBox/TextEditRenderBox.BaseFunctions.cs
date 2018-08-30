﻿//Apache2, 2014-present, WinterDev

using System.Text;
using PixelFarm.Drawing;
using LayoutFarm.UI;
namespace LayoutFarm.Text
{
    public sealed partial class TextEditRenderBox : RenderBoxBase
    {
        CaretRenderElement myCaret;
        EditableTextFlowLayer textLayer;
        InternalTextLayerController internalTextLayerController;
        int verticalExpectedCharIndex;
        bool isMultiLine = false;
        bool isInVerticalPhase = false;
        bool isFocus = false;
        bool stateShowCaret = false;
        bool isDragBegin;
        TextSpanStyle currentSpanStyle;
        public TextEditRenderBox(RootGraphic rootgfx,
            int width, int height,
            bool isMultiLine)
            : base(rootgfx, width, height)
        {
            GlobalCaretController.RegisterCaretBlink(rootgfx);
            myCaret = new CaretRenderElement(rootgfx, 2, 17);
            myCaret.TransparentForAllEvents = true;
            this.MayHasViewport = true;
            this.BackgroundColor = Color.White;// Color.Transparent;
            this.currentSpanStyle = new TextSpanStyle();
            this.currentSpanStyle.FontInfo = rootgfx.DefaultTextEditFontInfo;
            textLayer = new EditableTextFlowLayer(this);
            internalTextLayerController = new InternalTextLayerController(this, textLayer);
            this.isMultiLine = isMultiLine;
            if (isMultiLine)
            {
                textLayer.SetUseDoubleCanvas(false, true);
            }
            else
            {
                textLayer.SetUseDoubleCanvas(true, false);
            }
            this.NeedClipArea = true;
            this.IsBlockElement = false;
        }

        public TextSpanStyle CurrentTextSpanStyle
        {
            get { return this.currentSpanStyle; }
            set
            {
                this.currentSpanStyle = value;
            }
        }

        public static void NotifyTextContentSizeChanged(TextEditRenderBox ts)
        {
            ts.BoxEvaluateScrollBar();
        }

        public void DoHome(bool pressShitKey)
        {
            if (!pressShitKey)
            {
                internalTextLayerController.DoHome();
                internalTextLayerController.CancelSelect();
            }
            else
            {
                internalTextLayerController.StartSelectIfNoSelection();
                internalTextLayerController.DoHome();
                internalTextLayerController.EndSelect();
            }

            EnsureCaretVisible();
        }
        public void DoEnd(bool pressShitKey)
        {
            if (!pressShitKey)
            {
                internalTextLayerController.DoEnd();
                internalTextLayerController.CancelSelect();
            }
            else
            {
                internalTextLayerController.StartSelectIfNoSelection();
                internalTextLayerController.DoEnd();
                internalTextLayerController.EndSelect();
            }

            EnsureCaretVisible();
        }


        public Rectangle GetRectAreaOf(int beginlineNum, int beginColumnNum, int endLineNum, int endColumnNum)
        {
            EditableTextFlowLayer flowLayer = this.textLayer;
            EditableTextLine beginLine = flowLayer.GetTextLineAtPos(beginlineNum);
            if (beginLine == null)
            {
                return Rectangle.Empty;
            }
            if (beginlineNum == endLineNum)
            {
                VisualPointInfo beginPoint = beginLine.GetTextPointInfoFromCharIndex(beginColumnNum);
                VisualPointInfo endPoint = beginLine.GetTextPointInfoFromCharIndex(endColumnNum);
                return new Rectangle(beginPoint.X, beginLine.Top, endPoint.X, beginLine.ActualLineHeight);
            }
            else
            {
                VisualPointInfo beginPoint = beginLine.GetTextPointInfoFromCharIndex(beginColumnNum);
                EditableTextLine endLine = flowLayer.GetTextLineAtPos(endLineNum);
                VisualPointInfo endPoint = endLine.GetTextPointInfoFromCharIndex(endColumnNum);
                return new Rectangle(beginPoint.X, beginLine.Top, endPoint.X, beginLine.ActualLineHeight);
            }
        }
        public void HandleKeyPress(UIKeyEventArgs e)
        {
            this.SetCaretState(true);
            //------------------------
            if (e.IsControlCharacter)
            {
                HandleKeyDown(e);
                return;
            }

            char c = e.KeyChar;
            e.CancelBubbling = true;
            if (internalTextLayerController.SelectionRange != null
                && internalTextLayerController.SelectionRange.IsValid)
            {
                InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
            }
            bool preventDefault = false;
            if (textSurfaceEventListener != null &&
                !(preventDefault = TextSurfaceEventListener.NotifyPreviewKeydown(textSurfaceEventListener, c)))
            {
                internalTextLayerController.UpdateSelectionRange();
            }
            if (preventDefault)
            {
                return;
            }
            if (internalTextLayerController.SelectionRange != null)
            {
                internalTextLayerController.AddCharToCurrentLine(c);
                if (textSurfaceEventListener != null)
                {
                    TextSurfaceEventListener.NotifyCharactersReplaced(textSurfaceEventListener, e.KeyChar);
                }
            }
            else
            {
                internalTextLayerController.AddCharToCurrentLine(c);
                if (textSurfaceEventListener != null)
                {
                    TextSurfaceEventListener.NotifyCharacterAdded(textSurfaceEventListener, e.KeyChar);
                }
            }

            EnsureCaretVisible();
            if (textSurfaceEventListener != null)
            {
                TextSurfaceEventListener.NotifyKeyDown(textSurfaceEventListener, e.KeyCode);
            }
        }
        void InvalidateGraphicOfCurrentLineArea()
        {
#if DEBUG
            Rectangle c_lineArea = this.internalTextLayerController.CurrentParentLineArea;
#endif
            InvalidateGraphicLocalArea(this, this.internalTextLayerController.CurrentParentLineArea);
        }


        internal void SwapCaretState()
        {
            //TODO: review here ***

            this.stateShowCaret = !stateShowCaret;
            this.InvalidateGraphics();
            //int swapcount = dbugCaretSwapCount++;
            //if (stateShowCaret)
            //{
            //    Console.WriteLine(">>on " + swapcount);
            //    this.InvalidateGraphics();
            //    Console.WriteLine("<<on " + swapcount);
            //}
            //else
            //{
            //    Console.WriteLine(">>off " + swapcount);
            //    this.InvalidateGraphics();
            //    Console.WriteLine("<<off " + swapcount);
            //}

        }
        internal void SetCaretState(bool visible)
        {
            this.stateShowCaret = visible;
            this.InvalidateGraphics();
        }
        public void Focus()
        {
            GlobalCaretController.CurrentTextEditBox = this;
            this.SetCaretState(true);
            this.isFocus = true;
        }
        public void Blur()
        {
            GlobalCaretController.CurrentTextEditBox = null;
            this.SetCaretState(false);
            this.isFocus = false;
        }
        public bool IsFocused
        {
            get
            {
                return this.isFocus;
            }
        }

        public void HandleMouseDown(UIMouseEventArgs e)
        {
            if (e.Button == UIMouseButtons.Left)
            {
                InvalidateGraphicOfCurrentLineArea();

                if (!e.Shift)
                {
                    internalTextLayerController.SetCaretPos(e.X, e.Y);
                    if (internalTextLayerController.SelectionRange != null)
                    {
                        Rectangle r = GetSelectionUpdateArea();
                        internalTextLayerController.CancelSelect();
                        InvalidateGraphicLocalArea(this, r);
                    }
                    else
                    {
                        InvalidateGraphicOfCurrentLineArea();
                    }
                }
                else
                {
                    internalTextLayerController.StartSelectIfNoSelection();
                    internalTextLayerController.SetCaretPos(e.X, e.Y);
                    internalTextLayerController.EndSelect();
                    InvalidateGraphicOfCurrentLineArea();
                }
            }
        }
        public void HandleDoubleClick(UIMouseEventArgs e)
        {
            internalTextLayerController.CancelSelect();
            EditableRun textRun = this.CurrentTextRun;
            if (textRun != null)
            {

                VisualPointInfo pointInfo = internalTextLayerController.GetCurrentPointInfo();
                int lineCharacterIndex = pointInfo.LineCharIndex;
                int local_sel_Index = pointInfo.RunLocalSelectedIndex;
                //default behaviour is select only a hit word under the caret
                //so ask the text layer to find a hit word
                int startAt, len;
                internalTextLayerController.FindUnderlyingWord(out startAt, out len);
                if (len > 0)
                {
                    InvalidateGraphicOfCurrentLineArea();
                    internalTextLayerController.TryMoveCaretTo(startAt, true);
                    internalTextLayerController.StartSelect();
                    internalTextLayerController.TryMoveCaretTo(startAt + len);
                    internalTextLayerController.EndSelect();


                    //internalTextLayerController.TryMoveCaretTo(lineCharacterIndex - local_sel_Index, true);
                    //internalTextLayerController.StartSelect();
                    //internalTextLayerController.TryMoveCaretTo(internalTextLayerController.CharIndex + textRun.CharacterCount);
                    //internalTextLayerController.EndSelect();

                    InvalidateGraphicOfCurrentLineArea();
                }
            }
        }
        public void FindCurrentUnderlyingWord(out int startAt, out int len)
        {
            EditableRun textRun = this.CurrentTextRun;
            if (textRun != null)
            {

                VisualPointInfo pointInfo = internalTextLayerController.GetCurrentPointInfo();
                int lineCharacterIndex = pointInfo.LineCharIndex;
                int local_sel_Index = pointInfo.RunLocalSelectedIndex;
                //default behaviour is select only a hit word under the caret
                //so ask the text layer to find a hit word                 
                internalTextLayerController.FindUnderlyingWord(out startAt, out len);
            }
            else
            {
                startAt = len = 0;
            }
        }
        public void HandleDrag(UIMouseEventArgs e)
        {
            if (!isDragBegin)
            {
                //dbugMouseDragBegin++;
                //first time
                isDragBegin = true;
                if ((UIMouseButtons)e.Button == UIMouseButtons.Left)
                {
                    internalTextLayerController.SetCaretPos(e.X, e.Y);
                    internalTextLayerController.StartSelect();
                    internalTextLayerController.EndSelect();
                    this.InvalidateGraphics();
                }
            }
            else
            {
                //dbugMouseDragging++;
                if ((UIMouseButtons)e.Button == UIMouseButtons.Left)
                {
                    internalTextLayerController.StartSelectIfNoSelection();
                    internalTextLayerController.SetCaretPos(e.X, e.Y);
                    internalTextLayerController.EndSelect();
                    this.InvalidateGraphics();
                }
            }
        }
        public void HandleDragEnd(UIMouseEventArgs e)
        {
            isDragBegin = false;
            if ((UIMouseButtons)e.Button == UIMouseButtons.Left)
            {
                internalTextLayerController.StartSelectIfNoSelection();
                internalTextLayerController.SetCaretPos(e.X, e.Y);
                internalTextLayerController.EndSelect();
                this.InvalidateGraphics();
            }
        }

        Rectangle GetSelectionUpdateArea()
        {
            VisualSelectionRange selectionRange = internalTextLayerController.SelectionRange;
            if (selectionRange != null && selectionRange.IsValid)
            {
                return Rectangle.FromLTRB(0,
                    selectionRange.TopEnd.LineTop,
                    Width,
                    selectionRange.BottomEnd.Line.LineBottom);
            }
            else
            {
                return Rectangle.Empty;
            }
        }
        public void HandleMouseUp(UIMouseEventArgs e)
        {
            //empty?
        }
        public void HandleKeyUp(UIKeyEventArgs e)
        {
            this.SetCaretState(true);
        }
        public void HandleKeyDown(UIKeyEventArgs e)
        {
            this.SetCaretState(true);
            if (!e.HasKeyData)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case UIKeys.Home:
                    {
                        this.DoHome(e.Shift);
                    }
                    break;
                case UIKeys.End:
                    {
                        this.DoEnd(e.Shift);
                    }
                    break;
                case UIKeys.Back:
                    {
                        if (internalTextLayerController.SelectionRange != null)
                        {
                            InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                        }
                        else
                        {
                            InvalidateGraphicOfCurrentLineArea();
                        }
                        if (textSurfaceEventListener == null)
                        {
                            internalTextLayerController.DoBackspace();
                        }
                        else
                        {
                            if (!TextSurfaceEventListener.NotifyPreviewBackSpace(textSurfaceEventListener) &&
                                internalTextLayerController.DoBackspace())
                            {
                                TextSurfaceEventListener.NotifyCharactersRemoved(textSurfaceEventListener,
                                    new TextDomEventArgs(internalTextLayerController.updateJustCurrentLine));
                            }
                        }

                        EnsureCaretVisible();
                    }
                    break;
                case UIKeys.Delete:
                    {
                        if (internalTextLayerController.SelectionRange != null)
                        {
                            InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                        }
                        else
                        {
                            InvalidateGraphicOfCurrentLineArea();
                        }
                        if (textSurfaceEventListener == null)
                        {
                            internalTextLayerController.DoDelete();
                        }
                        else
                        {
                            VisualSelectionRangeSnapShot delpart = internalTextLayerController.DoDelete();
                            TextSurfaceEventListener.NotifyCharactersRemoved(textSurfaceEventListener,
                                new TextDomEventArgs(internalTextLayerController.updateJustCurrentLine));
                        }

                        EnsureCaretVisible();
                    }
                    break;
                default:
                    {
                        if (textSurfaceEventListener != null)
                        {
                            UIKeys keycode = e.KeyCode;
                            if (keycode >= UIKeys.F1 && keycode <= UIKeys.F12)
                            {
                                InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                                TextSurfaceEventListener.NotifyFunctionKeyDown(textSurfaceEventListener, keycode);
                                EnsureCaretVisible();
                            }
                        }

                    }
                    break;
            }

            if (e.HasKeyData && e.Ctrl)
            {
                switch (e.KeyCode)
                {
                    case UIKeys.A:
                        {
                            //select all
                            //....
                            this.CurrentLineNumber = 0;
                            //start select to end
                            DoHome(false);//1st simulate 
                            DoHome(true); //2nd
                            this.CurrentLineNumber = this.LineCount - 1;
                            DoEnd(true); //
                        }
                        break;
                    case UIKeys.C:
                        {
                            StringBuilder stBuilder = GetFreeStringBuilder();
                            internalTextLayerController.CopySelectedTextToPlainText(stBuilder);
                            if (stBuilder != null)
                            {
                                if (stBuilder.Length == 0)
                                {
                                    Clipboard.Clear();
                                }
                                else
                                {
                                    Clipboard.SetText(stBuilder.ToString());
                                }
                            }
                            ReleaseStringBuilder(stBuilder);
                        }
                        break;
                    case UIKeys.V:
                        {
                            if (Clipboard.ContainUnicodeText())
                            {
                                //1. we need to parse multi-line to single line
                                //this may need text-break services

                                internalTextLayerController.AddUnformattedStringToCurrentLine(
                                    Clipboard.GetUnicodeText(), this.currentSpanStyle);

                                EnsureCaretVisible();
                            }
                        }
                        break;
                    case UIKeys.X:
                        {
                            if (internalTextLayerController.SelectionRange != null)
                            {
                                if (internalTextLayerController.SelectionRange != null)
                                {
                                    InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                                }
                                StringBuilder stBuilder = GetFreeStringBuilder();
                                internalTextLayerController.CopySelectedTextToPlainText(stBuilder);
                                if (stBuilder != null)
                                {
                                    Clipboard.SetText(stBuilder.ToString());
                                }

                                internalTextLayerController.DoDelete();
                                EnsureCaretVisible();
                                ReleaseStringBuilder(stBuilder);
                            }
                        }
                        break;
                    case UIKeys.Z:
                        {
                            internalTextLayerController.UndoLastAction();
                            EnsureCaretVisible();
                        }
                        break;
                    case UIKeys.Y:
                        {
                            internalTextLayerController.ReverseLastUndoAction();
                            EnsureCaretVisible();
                        }
                        break;
                    case UIKeys.B:
                        {
                            //
                            //test add markers
                            //
                            //if (internalTextLayerController.SelectionRange != null)
                            //{
                            //    //
                            //    internalTextLayerController.SelectionRange.SwapIfUnOrder();
                            //    VisualMarkerSelectionRange markerSelRange =
                            //        VisualMarkerSelectionRange.CreateFromSelectionRange(
                            //            internalTextLayerController.SelectionRange.GetSelectionRangeSnapshot());
                            //    //then add to the marker layers
                            //    markerSelRange.BindToTextLayer(textLayer);

                            //    internalTextLayerController.VisualMarkers.Add(markerSelRange);
                            //}

                            //
                            //TextSpanStyle style = internalTextLayerController.GetFirstTextStyleInSelectedRange(); 
                            //TextSpanStyle textStyle = null;

                            ////test only ***
                            ////TODO: make this more configurable
                            //if (style != null)
                            //{
                            //    TextSpanStyle defaultBeh = ((TextSpanStyle)style);
                            //    if (defaultBeh.FontBold)
                            //    {
                            //        textStyle = StyleHelper.CreateNewStyle(Color.Black);
                            //    }
                            //    else
                            //    {
                            //        textStyle = StyleHelper.CreateNewStyle(Color.Blue);
                            //    }
                            //}
                            //else
                            //{
                            //    textStyle = StyleHelper.CreateNewStyle(Color.Blue); 
                            //} 

                            //internalTextLayerController.DoFormatSelection(textStyle);

                            //if (internalTextLayerController.updateJustCurrentLine)
                            //{

                            //    InvalidateGraphicOfCurrentLineArea();
                            //}
                            //else
                            //{
                            //    InvalidateGraphics(); 
                            //}

                        }
                        break;
                }
            }
            if (textSurfaceEventListener != null)
            {
                TextSurfaceEventListener.NotifyKeyDown(textSurfaceEventListener, e.KeyCode);
            }
        }
        public Point CurrentCaretPos
        {
            get { return this.internalTextLayerController.CaretPos; }
        }

        public bool HandleProcessDialogKey(UIKeyEventArgs e)
        {
            UIKeys keyData = (UIKeys)e.KeyData;
            SetCaretState(true);
            if (isInVerticalPhase && (keyData != UIKeys.Up || keyData != UIKeys.Down))
            {
                isInVerticalPhase = false;
            }

            switch (e.KeyCode)
            {
                case UIKeys.Home:
                    {
                        HandleKeyDown(e);
                        return true;
                    }
                case UIKeys.Return:
                    {
                        if (textSurfaceEventListener != null &&
                            TextSurfaceEventListener.NotifyPreviewEnter(textSurfaceEventListener))
                        {
                            return true;
                        }
                        if (isMultiLine)
                        {
                            if (internalTextLayerController.SelectionRange != null)
                            {
                                InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                            }
                            internalTextLayerController.SplitCurrentLineIntoNewLine();
                            if (textSurfaceEventListener != null)
                            {
                                TextSurfaceEventListener.NofitySplitNewLine(textSurfaceEventListener, e);
                            }

                            Rectangle lineArea = internalTextLayerController.CurrentLineArea;
                            if (lineArea.Bottom > this.ViewportBottom)
                            {
                                ScrollBy(0, lineArea.Bottom - this.ViewportBottom);
                            }
                            else
                            {
                                InvalidateGraphicOfCurrentLineArea();
                            }
                            EnsureCaretVisible();
                            return true;
                        }
                        return true;
                    }

                case UIKeys.Left:
                    {
                        if (textSurfaceEventListener != null &&
                            TextSurfaceEventListener.NotifyPreviewArrow(textSurfaceEventListener, keyData))
                        {
                            return true;
                        }

                        InvalidateGraphicOfCurrentLineArea();
                        if (!e.Shift)
                        {
                            internalTextLayerController.CancelSelect();
                        }
                        else
                        {
                            internalTextLayerController.StartSelectIfNoSelection();
                        }

                        Point currentCaretPos = Point.Empty;
                        if (!isMultiLine)
                        {
                            if (!internalTextLayerController.IsOnStartOfLine)
                            {
#if DEBUG
                                Point prvCaretPos = internalTextLayerController.CaretPos;
#endif
                                internalTextLayerController.TryMoveCaretBackward();
                                currentCaretPos = internalTextLayerController.CaretPos;
                            }
                        }
                        else
                        {
                            if (internalTextLayerController.IsOnStartOfLine)
                            {
                                internalTextLayerController.TryMoveCaretBackward();
                                currentCaretPos = internalTextLayerController.CaretPos;
                            }
                            else
                            {
                                if (!internalTextLayerController.IsOnStartOfLine)
                                {
#if DEBUG
                                    Point prvCaretPos = internalTextLayerController.CaretPos;
#endif
                                    internalTextLayerController.TryMoveCaretBackward();
                                    currentCaretPos = internalTextLayerController.CaretPos;
                                }
                            }
                        }
                        //-------------------
                        if (e.Shift)
                        {
                            internalTextLayerController.EndSelectIfNoSelection();
                        }
                        //-------------------

                        EnsureCaretVisible();
                        if (textSurfaceEventListener != null)
                        {
                            TextSurfaceEventListener.NotifyArrowKeyCaretPosChanged(textSurfaceEventListener, keyData);
                        }

                        return true;
                    }
                case UIKeys.Right:
                    {
                        if (textSurfaceEventListener != null &&
                            TextSurfaceEventListener.NotifyPreviewArrow(textSurfaceEventListener, keyData))
                        {
                            return true;
                        }

                        InvalidateGraphicOfCurrentLineArea();
                        if (!e.Shift)
                        {
                            internalTextLayerController.CancelSelect();
                        }
                        else
                        {
                            internalTextLayerController.StartSelectIfNoSelection();
                        }


                        Point currentCaretPos = Point.Empty;
                        if (!isMultiLine)
                        {
#if DEBUG
                            Point prvCaretPos = internalTextLayerController.CaretPos;
#endif
                            internalTextLayerController.TryMoveCaretForward();
                            currentCaretPos = internalTextLayerController.CaretPos;
                        }
                        else
                        {
                            if (internalTextLayerController.IsOnEndOfLine)
                            {
                                internalTextLayerController.TryMoveCaretForward();
                                currentCaretPos = internalTextLayerController.CaretPos;
                            }
                            else
                            {
#if DEBUG
                                Point prvCaretPos = internalTextLayerController.CaretPos;
#endif
                                internalTextLayerController.TryMoveCaretForward();
                                currentCaretPos = internalTextLayerController.CaretPos;
                            }
                        }
                        //-------------------
                        if (e.Shift)
                        {
                            internalTextLayerController.EndSelectIfNoSelection();
                        }
                        //-------------------

                        EnsureCaretVisible();
                        if (textSurfaceEventListener != null)
                        {
                            TextSurfaceEventListener.NotifyArrowKeyCaretPosChanged(textSurfaceEventListener, keyData);
                        }

                        return true;
                    }
                case UIKeys.Down:
                    {
                        if (textSurfaceEventListener != null &&
                            TextSurfaceEventListener.NotifyPreviewArrow(textSurfaceEventListener, keyData))
                        {
                            return true;
                        }
                        if (isMultiLine)
                        {
                            if (!isInVerticalPhase)
                            {
                                isInVerticalPhase = true;
                                verticalExpectedCharIndex = internalTextLayerController.CharIndex;
                            }

                            //----------------------------                          
                            if (!e.Shift)
                            {
                                internalTextLayerController.CancelSelect();
                            }
                            else
                            {
                                internalTextLayerController.StartSelectIfNoSelection();
                            }
                            //---------------------------- 

                            internalTextLayerController.CurrentLineNumber++;
                            if (verticalExpectedCharIndex > internalTextLayerController.CurrentLineCharCount - 1)
                            {
                                internalTextLayerController.TryMoveCaretTo(internalTextLayerController.CurrentLineCharCount - 1);
                            }
                            else
                            {
                                internalTextLayerController.TryMoveCaretTo(verticalExpectedCharIndex);
                            }
                            //----------------------------

                            if (e.Shift)
                            {
                                internalTextLayerController.EndSelectIfNoSelection();
                            }
                            //----------------------------
                            Rectangle lineArea = internalTextLayerController.CurrentLineArea;
                            if (lineArea.Bottom > this.ViewportBottom)
                            {
                                ScrollBy(0, lineArea.Bottom - this.ViewportBottom);
                            }
                            else
                            {
                                InvalidateGraphicOfCurrentLineArea();
                            }
                        }

                        if (textSurfaceEventListener != null)
                        {
                            TextSurfaceEventListener.NotifyArrowKeyCaretPosChanged(textSurfaceEventListener, keyData);
                        }
                        return true;
                    }
                case UIKeys.Up:
                    {
                        if (textSurfaceEventListener != null &&
                            TextSurfaceEventListener.NotifyPreviewArrow(textSurfaceEventListener, keyData))
                        {
                            return true;
                        }

                        if (isMultiLine)
                        {
                            if (!isInVerticalPhase)
                            {
                                isInVerticalPhase = true;
                                verticalExpectedCharIndex = internalTextLayerController.CharIndex;
                            }

                            //----------------------------                          
                            if (!e.Shift)
                            {
                                internalTextLayerController.CancelSelect();
                            }
                            else
                            {
                                internalTextLayerController.StartSelectIfNoSelection();
                            }
                            //----------------------------

                            internalTextLayerController.CurrentLineNumber--;
                            if (verticalExpectedCharIndex > internalTextLayerController.CurrentLineCharCount - 1)
                            {
                                internalTextLayerController.TryMoveCaretTo(internalTextLayerController.CurrentLineCharCount - 1);
                            }
                            else
                            {
                                internalTextLayerController.TryMoveCaretTo(verticalExpectedCharIndex);
                            }

                            //----------------------------
                            if (e.Shift)
                            {
                                internalTextLayerController.EndSelectIfNoSelection();
                            }

                            Rectangle lineArea = internalTextLayerController.CurrentLineArea;
                            if (lineArea.Top < ViewportY)
                            {
                                ScrollBy(0, lineArea.Top - ViewportY);
                            }
                            else
                            {
                                EnsureCaretVisible();
                                InvalidateGraphicOfCurrentLineArea();
                            }
                        }
                        else
                        {
                        }
                        if (textSurfaceEventListener != null)
                        {
                            TextSurfaceEventListener.NotifyArrowKeyCaretPosChanged(textSurfaceEventListener, keyData);
                        }
                        return true;
                    }
                case UIKeys.Tab:
                    {
                        DoTab();
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
        public override Size InnerContentSize
        {
            get
            {
                return internalTextLayerController.CurrentLineArea.Size;
            }
        }
        void EnsureCaretVisible()
        {
            //----------------------
            Point textManCaretPos = internalTextLayerController.CaretPos;
            myCaret.SetHeight(internalTextLayerController.CurrentCaretHeight);
            textManCaretPos.Offset(-ViewportX, -ViewportY);
            //----------------------  
            if (textManCaretPos.X >= this.Width)
            {
                if (!isMultiLine)
                {
                    var r = internalTextLayerController.CurrentLineArea;
                    //Rectangle r = internalTextLayerController.CurrentParentLineArea;
                    if (r.Width >= this.Width)
                    {
#if DEBUG
                        dbug_SetInitObject(this);
                        dbug_StartLayoutTrace(dbugVisualElementLayoutMsg.ArtVisualTextSurafce_EnsureCaretVisible);
#endif
                        //SetCalculatedSize(this, r.Width, r.Height);
                        //InnerDoTopDownReCalculateContentSize(this);
                        this.BoxEvaluateScrollBar();
                        RefreshSnapshotCanvas();
#if DEBUG
                        dbug_EndLayoutTrace();
#endif
                    }
                }
                else
                {
                }

                ScrollBy(textManCaretPos.X - this.Width, 0);
            }
            else if (textManCaretPos.X < 0)
            {
                ScrollBy(textManCaretPos.X - this.X, 0);
            }

            Size innerContentSize = this.InnerContentSize;
            if (ViewportX > 0 && innerContentSize.Width - ViewportX < this.Width)
            {
                ScrollTo(this.InnerContentSize.Width - ViewportX, 0);
            }


            if (internalTextLayerController.updateJustCurrentLine)
            {
                InvalidateGraphicOfCurrentLineArea();
            }
            else
            {
                InvalidateGraphics();
            }
        }
        void RefreshSnapshotCanvas()
        {
        }
        public bool OnlyCurrentlineUpdated
        {
            get
            {
                return internalTextLayerController.updateJustCurrentLine;
            }
        }
        public int CurrentLineHeight
        {
            get
            {
                return internalTextLayerController.CurrentLineArea.Height;
            }
        }
        public int CurrentLineCharIndex
        {
            get
            {
                return internalTextLayerController.CurrentLineCharIndex;
            }
        }
        public int CurrentTextRunCharIndex
        {
            get
            {
                return internalTextLayerController.CurrentTextRunCharIndex;
            }
        }
        public int CurrentLineNumber
        {
            get
            {
                return internalTextLayerController.CurrentLineNumber;
            }
            set
            {
                internalTextLayerController.CurrentLineNumber = value;
            }
        }
        public void ScrollToCurrentLine()
        {
            this.ScrollTo(0, internalTextLayerController.CaretPos.Y);
        }

        public void DoTab()
        {
            if (internalTextLayerController.SelectionRange != null)
            {
                InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
            }

            internalTextLayerController.AddCharToCurrentLine(' ');
            internalTextLayerController.AddCharToCurrentLine(' ');
            internalTextLayerController.AddCharToCurrentLine(' ');
            internalTextLayerController.AddCharToCurrentLine(' ');
            internalTextLayerController.AddCharToCurrentLine(' ');
            if (textSurfaceEventListener != null)
            {
                TextSurfaceEventListener.NotifyCharacterAdded(textSurfaceEventListener, '\t');
            }

            InvalidateGraphicOfCurrentLineArea();
        }

        public void DoTyping(string text)
        {
            if (internalTextLayerController.SelectionRange != null)
            {
                InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
            }

            char[] charBuff = text.ToCharArray();
            int j = charBuff.Length;
            for (int i = 0; i < j; ++i)
            {
                internalTextLayerController.AddCharToCurrentLine(charBuff[i]);
            }
            InvalidateGraphicOfCurrentLineArea();
        }
    }
}
