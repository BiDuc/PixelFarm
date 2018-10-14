﻿//Apache2, 2014-present, WinterDev

namespace LayoutFarm.UI
{
    public interface ITopWindowEventRoot
    {
        IUIEventListener CurrentKeyboardFocusedElement { get; set; }
        MouseCursorStyle MouseCursorStyle { get; }


        void RootMouseDown(int x, int y, UIMouseButtons button);
        void RootMouseUp(int x, int y, UIMouseButtons button);
        void RootMouseWheel(int delta);
        void RootMouseMove(int x, int y, UIMouseButtons button);
        void RootGotFocus();
        void RootLostFocus();
        void RootKeyPress(char c);
        void RootKeyDown(int keydata);
        void RootKeyUp(int keydata);
        bool RootProcessDialogKey(int keydata);
    }
    public interface ITopWindowEventRootProvider
    {
        ITopWindowEventRoot EventRoot { get; }
    }
}