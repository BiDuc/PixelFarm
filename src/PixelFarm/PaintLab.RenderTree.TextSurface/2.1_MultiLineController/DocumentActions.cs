﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
namespace LayoutFarm.TextEditing
{
    abstract class DocumentAction
    {
        protected int _startLineNumber;
        protected int _startCharIndex;
        public DocumentAction(int lineNumber, int charIndex)
        {
            _startLineNumber = lineNumber;
            _startCharIndex = charIndex;
        }

        public abstract void InvokeUndo(InternalTextLayerController textLayer);
        public abstract void InvokeRedo(InternalTextLayerController textLayer);
    }

    class DocActionCharTyping : DocumentAction
    {
        char _c;
        public DocActionCharTyping(char c, int lineNumber, int charIndex)
            : base(lineNumber, charIndex)
        {
            _c = c;
        }

        public override void InvokeUndo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.DoBackspace();
        }
        public override void InvokeRedo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.AddCharToCurrentLine(_c);
        }
    }

    class DocActionSplitToNewLine : DocumentAction
    {
        public DocActionSplitToNewLine(int lineNumber, int charIndex)
            : base(lineNumber, charIndex)
        {
        }
        public override void InvokeUndo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.DoEnd();
            textLayer.DoDelete();
        }
        public override void InvokeRedo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.SplitCurrentLineIntoNewLine();
        }
    }
    class DocActionJoinWithNextLine : DocumentAction
    {
        public DocActionJoinWithNextLine(int lineNumber, int charIndex)
            : base(lineNumber, charIndex)
        {
        }
        public override void InvokeUndo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.SplitCurrentLineIntoNewLine();
        }
        public override void InvokeRedo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.DoDelete();
        }
    }


    class DocActionDeleteChar : DocumentAction
    {
        char _c;
        public DocActionDeleteChar(char c, int lineNumber, int charIndex)
            : base(lineNumber, charIndex)
        {
            _c = c;
        }
        public override void InvokeUndo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.AddCharToCurrentLine(_c);
        }
        public override void InvokeRedo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.DoDelete();
        }
    }
    class DocActionDeleteRange : DocumentAction
    {
        List<EditableRun> _deletedTextRuns;
        int _endLineNumber;
        int _endCharIndex;
        public DocActionDeleteRange(List<EditableRun> deletedTextRuns, int startLineNum, int startColumnNum,
            int endLineNum, int endColumnNum)
            : base(startLineNum, startColumnNum)
        {
            _deletedTextRuns = deletedTextRuns;
            _endLineNumber = endLineNum;
            _endCharIndex = endColumnNum;
        }

        public override void InvokeUndo(InternalTextLayerController textLayer)
        {
            textLayer.CancelSelect();
            textLayer.AddTextRunsToCurrentLine(_deletedTextRuns);
        }
        public override void InvokeRedo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.StartSelect();
            textLayer.CurrentLineNumber = _endLineNumber;
            textLayer.TryMoveCaretTo(_endCharIndex);
            textLayer.EndSelect();
            textLayer.DoDelete();
        }
    }

    class DocActionInsertRuns : DocumentAction
    {
        EditableRun _singleInsertTextRun;
        IEnumerable<EditableRun> _insertingTextRuns;
        int _endLineNumber;
        int _endCharIndex;
        public DocActionInsertRuns(IEnumerable<EditableRun> insertingTextRuns,
            int startLineNumber, int startCharIndex, int endLineNumber, int endCharIndex)
            : base(startLineNumber, startCharIndex)
        {
            _insertingTextRuns = insertingTextRuns;
            _endLineNumber = endLineNumber;
            _endCharIndex = endCharIndex;
        }
        public DocActionInsertRuns(EditableRun insertingTextRun,
           int startLineNumber, int startCharIndex, int endLineNumber, int endCharIndex)
            : base(startLineNumber, startCharIndex)
        {
            _singleInsertTextRun = insertingTextRun;
            _endLineNumber = endLineNumber;
            _endCharIndex = endCharIndex;
        }
        public override void InvokeUndo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.StartSelect();
            textLayer.CurrentLineNumber = _endLineNumber;
            textLayer.TryMoveCaretTo(_endCharIndex);
            textLayer.EndSelect();
            textLayer.DoDelete();
        }
        public override void InvokeRedo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            if (_singleInsertTextRun != null)
            {
                textLayer.AddTextRunToCurrentLine(_singleInsertTextRun);
            }
            else
            {
                textLayer.AddTextRunsToCurrentLine(_insertingTextRuns);
            }
        }
    }
    class DocActionFormatting : DocumentAction
    {
        int _endLineNumber;
        int _endCharIndex;
        TextSpanStyle _textStyle;
        public DocActionFormatting(TextSpanStyle textStyle, int startLineNumber, int startCharIndex, int endLineNumber, int endCharIndex)
            : base(startLineNumber, startCharIndex)
        {
            _textStyle = textStyle;
            _endLineNumber = endLineNumber;
            _endCharIndex = endCharIndex;
        }
        public override void InvokeUndo(InternalTextLayerController textLayer)
        {
            textLayer.CurrentLineNumber = _startLineNumber;
            textLayer.TryMoveCaretTo(_startCharIndex);
            textLayer.StartSelect();
            textLayer.CurrentLineNumber = _endLineNumber;
            textLayer.TryMoveCaretTo(_endCharIndex);
            textLayer.EndSelect();
        }
        public override void InvokeRedo(InternalTextLayerController textLayer)
        {
        }
    }

    class DocumentCommandCollection
    {
        LinkedList<DocumentAction> _undoList = new LinkedList<DocumentAction>();
        Stack<DocumentAction> _reverseUndoAction = new Stack<DocumentAction>();
        int _maxCommandsCount = 20;
        InternalTextLayerController _textdom;
        public DocumentCommandCollection(InternalTextLayerController textdomManager)
        {
            _textdom = textdomManager;
        }

        public void Clear()
        {
            _undoList.Clear();
            _reverseUndoAction.Clear();
        }

        public int MaxCommandCount
        {
            get
            {
                return _maxCommandsCount;
            }
            set
            {
                _maxCommandsCount = value;
                if (_undoList.Count > _maxCommandsCount)
                {
                    int diff = _undoList.Count - _maxCommandsCount;
                    for (int i = 0; i < diff; i++)
                    {
                        _undoList.RemoveFirst();
                    }
                }
            }
        }

        public void AddDocAction(DocumentAction docAct)
        {
            if (_textdom.EnableUndoHistoryRecording)
            {
                _undoList.AddLast(docAct);
                EnsureCapacity();
            }
        }

        void EnsureCapacity()
        {
            if (_undoList.Count > _maxCommandsCount)
            {
                _undoList.RemoveFirst();
            }
        }
        public void UndoLastAction()
        {
            DocumentAction docAction = PopUndoCommand();
            if (docAction != null)
            {
                _textdom.EnableUndoHistoryRecording = false;
                docAction.InvokeUndo(_textdom);
                _textdom.EnableUndoHistoryRecording = true;
                _reverseUndoAction.Push(docAction);
            }
        }
        public void ReverseLastUndoAction()
        {
            if (_reverseUndoAction.Count > 0)
            {
                _textdom.EnableUndoHistoryRecording = false;
                DocumentAction docAction = _reverseUndoAction.Pop();
                _textdom.EnableUndoHistoryRecording = true;
                docAction.InvokeRedo(_textdom);
                _undoList.AddLast(docAction);
            }
        }

        public DocumentAction PeekCommand => _undoList.Last.Value;

        public int Count => _undoList.Count;

        public DocumentAction PopUndoCommand()
        {
            if (_undoList.Count > 0)
            {
                DocumentAction lastCmd = _undoList.Last.Value;
                _undoList.RemoveLast();
                return lastCmd;
            }
            else
            {
                return null;
            }
        }
    }
}