﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using LayoutFarm.UI;
namespace LayoutFarm.CustomWidgets
{
    /// <summary>
    /// abstract box ui element
    /// </summary>
    public abstract class AbstractBox : AbstractRectUI
    {
        BoxContentLayoutKind panelLayoutKind;
        bool needContentLayout;
        bool draggable;
        bool dropable;
        CustomRenderBox primElement;
        Color backColor = Color.LightGray;
        int viewportX;
        int viewportY;
        int _innerHeight;
        int _innerWidth;
        UICollection uiList;

        public event EventHandler<UIMouseEventArgs> MouseDown;
        public event EventHandler<UIMouseEventArgs> MouseMove;
        public event EventHandler<UIMouseEventArgs> MouseUp;
        public event EventHandler<UIMouseEventArgs> MouseDoubleClick;
        public event EventHandler<UIMouseEventArgs> MouseLeave;
        public event EventHandler<UIMouseEventArgs> MouseDrag;
        public event EventHandler<UIMouseEventArgs> MouseWheel;
        public event EventHandler<UIMouseEventArgs> LostMouseFocus;
        public event EventHandler<UIGuestTalkEventArgs> DragOver;
        //--------------------------------------------------------

        public event EventHandler<UIKeyEventArgs> KeyDown;

        bool _needClipArea;
        bool _supportViewport;
        public AbstractBox(int width, int height)
            : base(width, height)
        {
            _innerHeight = height;
            _innerWidth = width;
            _supportViewport = true;
        }

        public bool NeedClipArea
        {
            get { return _needClipArea; }
            set
            {
                _needClipArea = value;
                if (primElement != null)
                {
                    primElement.NeedClipArea = value;
                }
            }
        }
        public override void Walk(UIVisitor visitor)
        {

        }
        protected override bool HasReadyRenderElement
        {
            get { return this.primElement != null; }
        }
        public override RenderElement CurrentPrimaryRenderElement
        {
            get { return this.primElement; }
        }
        public Color BackColor
        {
            get { return this.backColor; }
            set
            {
                this.backColor = value;
                if (HasReadyRenderElement)
                {
                    this.primElement.BackColor = value;

                }
            }
        }

        protected void SetPrimaryRenderElement(CustomRenderBox primElement)
        {
            this.primElement = primElement;
        }
        protected virtual void BuildChildrenRenderElement(RenderElement parent)
        {
            parent.HasSpecificHeight = this.HasSpecificHeight;
            parent.HasSpecificWidth = this.HasSpecificWidth;
            parent.SetController(this);
            parent.SetVisible(this.Visible);
#if DEBUG
            //if (dbugBreakMe)
            //{
            //    renderE.dbugBreak = true;
            //}
#endif
            parent.SetLocation(this.Left, this.Top);
            if (parent is CustomRenderBox)
            {
                ((CustomRenderBox)parent).BackColor = backColor;
            }

            parent.HasSpecificWidthAndHeight = true;
            parent.SetViewport(this.ViewportX, this.ViewportY);
            //------------------------------------------------


            //create visual layer 
            int childCount = this.ChildCount;
            for (int m = 0; m < childCount; ++m)
            {
                parent.AddChild(this.GetChild(m));
            }
            //set primary render element
            //---------------------------------

        }

        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx)
        {
            if (primElement == null)
            {
                var renderE = new CustomRenderBox(rootgfx, this.Width, this.Height);
                renderE.NeedClipArea = this.NeedClipArea;
                renderE.TransparentForAllEvents = this.TransparentAllMouseEvents;
                BuildChildrenRenderElement(renderE);
                this.primElement = renderE;
            }
            return primElement;
        }
        //----------------------------------------------------

        public bool AcceptKeyboardFocus
        {
            get;
            set;
        }
        protected override void OnDoubleClick(UIMouseEventArgs e)
        {
            if (this.MouseDoubleClick != null)
            {
                MouseDoubleClick(this, e);
            }
            if (this.AcceptKeyboardFocus)
            {
                this.Focus();
            }
        }
        protected override void OnMouseDown(UIMouseEventArgs e)
        {
            if (this.MouseDown != null)
            {
                this.MouseDown(this, e);
            }

            if (this.AcceptKeyboardFocus)
            {
                this.Focus();
            }
        }
        protected override void OnMouseMove(UIMouseEventArgs e)
        {
            if (e.IsDragging)
            {
                if (this.MouseDrag != null)
                {
                    this.MouseDrag(this, e);
                }
            }
            else
            {
                if (this.MouseMove != null)
                {
                    this.MouseMove(this, e);
                }
            }
        }
        protected override void OnMouseLeave(UIMouseEventArgs e)
        {
            if (this.MouseLeave != null)
            {
                this.MouseLeave(this, e);
            }
        }
        protected override void OnMouseEnter(UIMouseEventArgs e)
        {
            base.OnMouseEnter(e);
        }
        protected override void OnMouseHover(UIMouseEventArgs e)
        {
            base.OnMouseHover(e);
        }
        protected override void OnMouseUp(UIMouseEventArgs e)
        {
            if (this.MouseUp != null)
            {
                MouseUp(this, e);
            }
        }
        protected override void OnLostMouseFocus(UIMouseEventArgs e)
        {
            if (this.LostMouseFocus != null)
            {
                this.LostMouseFocus(this, e);
            }
        }

        public bool Draggable
        {
            get { return this.draggable; }
            set
            {
                this.draggable = value;
            }
        }
        public bool Droppable
        {
            get { return this.dropable; }
            set
            {
                this.dropable = value;
            }
        }

        public void RemoveSelf()
        {
            if (CurrentPrimaryRenderElement == null) { return; }
            //

            var parentBox = this.CurrentPrimaryRenderElement.ParentRenderElement as LayoutFarm.RenderElement;
            if (parentBox != null)
            {
                parentBox.RemoveChild(this.CurrentPrimaryRenderElement);
            }
            this.InvalidateOuterGraphics();
        }
        //----------------------------------------------------
        public override int ViewportX
        {
            get { return this.viewportX; }
        }
        public override int ViewportY
        {
            get { return this.viewportY; }
        }
        public int ViewportBottom
        {
            get { return this.ViewportY + this.Height; }
        }
        public int ViewportRight
        {
            get { return this.ViewportX + this.Width; }
        }
        public override void SetViewport(int x, int y, object reqBy)
        {
            //check if viewport is changed or not
            bool isChanged = (viewportX != x) || (viewportY != y);
            this.viewportX = x;
            this.viewportY = y;
            if (this.HasReadyRenderElement)
            {
                primElement.SetViewport(viewportX, viewportY);
                if (isChanged)
                {
                    RaiseViewportChanged();
                }

            }
        }
        protected override void OnMouseWheel(UIMouseEventArgs e)
        {
            //vertical scroll

            if (this._innerHeight > this.Height)
            {
                if (e.Delta < 0)
                {
                    //down
                    this.viewportY += 20;
                    if (viewportY > _innerHeight - this.Height)
                    {
                        this.viewportY = _innerHeight - this.Height;
                    }
                }
                else
                {
                    //up
                    this.viewportY -= 20;
                    if (viewportY < 0)
                    {
                        viewportY = 0;
                    }
                }
                this.primElement.SetViewport(viewportX, viewportY);
                this.InvalidateGraphics();
            }
            if (MouseWheel != null)
            {
                MouseWheel(this, e);
            }
        }
        //-------------------
        protected override bool OnProcessDialogKey(UIKeyEventArgs e)
        {
            if (KeyDown != null)
            {
                KeyDown(this, e);
            }
            //return true if you want to stop event bubble to other 
            if (e.CancelBubbling)
            {
                return true;
            }
            else
            {
                return base.OnProcessDialogKey(e);
            }
        }
        protected override void OnKeyDown(UIKeyEventArgs e)
        {
            base.OnKeyDown(e);
        }
        protected override void OnKeyPress(UIKeyEventArgs e)
        {
            base.OnKeyPress(e);
        }
        protected override void OnKeyUp(UIKeyEventArgs e)
        {
            base.OnKeyUp(e);
        }
        //-------------------
        public override int InnerWidth
        {
            get
            {
                return this._innerWidth;
            }
        }
        public override int InnerHeight
        {
            get
            {

                return this._innerHeight;
            }
        }

        protected virtual void SetInnerContentSize(int w, int h)
        {
            this._innerWidth = w;
            this._innerHeight = h;
        }

        //----------------------------------------------------
        public IEnumerable<UIElement> GetChildIter()
        {
            if (uiList != null)
            {
                int j = uiList.Count;
                for (int i = 0; i < j; ++i)
                {
                    yield return uiList.GetElement(i);
                }
            }
        }


        public void AddChild(UIElement ui)
        {
            if (this.uiList == null)
            {
                this.uiList = new UICollection(this);
            }

            needContentLayout = true;
            this.uiList.AddUI(ui);
            if (this.HasReadyRenderElement)
            {
                primElement.AddChild(ui);
                //if (this.panelLayoutKind != BoxContentLayoutKind.Absolute)
                //{
                //    this.InvalidateLayout();
                //}
                //check if we support
                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
            }

            if (ui.NeedContentLayout)
            {
                ui.InvalidateLayout();
            }
        }
        public void RemoveChild(UIElement ui)
        {
            needContentLayout = true;
            this.uiList.RemoveUI(ui);
            if (this.HasReadyRenderElement)
            {
                //if (this.ContentLayoutKind != BoxContentLayoutKind.Absolute)
                //{
                //    this.InvalidateLayout();
                //}
                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
                this.primElement.RemoveChild(ui.CurrentPrimaryRenderElement);
            }
        }
        public void ClearChildren()
        {
            needContentLayout = true;
            if (this.uiList != null)
            {
                this.uiList.Clear();
            }
            if (this.HasReadyRenderElement)
            {
                primElement.ClearAllChildren();
                //if (this.panelLayoutKind != BoxContentLayoutKind.Absolute)
                //{
                //    this.InvalidateLayout();
                //}
                if (_supportViewport)
                {
                    this.InvalidateLayout();
                }
            }
        }

        public int ChildCount
        {
            get
            {
                if (this.uiList != null)
                {
                    return this.uiList.Count;
                }
                return 0;
            }
        }
        public UIElement GetChild(int index)
        {
            return uiList.GetElement(index);
        }
        public override bool NeedContentLayout
        {
            get
            {
                return this.needContentLayout;
            }
        }
        public BoxContentLayoutKind ContentLayoutKind
        {
            get { return this.panelLayoutKind; }
            set
            {
                this.panelLayoutKind = value;
            }
        }
        protected override void OnContentLayout()
        {
            this.PerformContentLayout();
        }
        public override void PerformContentLayout()
        {
            this.InvalidateGraphics();
            //temp : arrange as vertical stack***
            switch (this.ContentLayoutKind)
            {
                case CustomWidgets.BoxContentLayoutKind.VerticalStack:
                    {
                        int count = this.ChildCount;
                        int ypos = 0;
                        int maxRight = 0;
                        for (int i = 0; i < count; ++i)
                        {
                            var element = this.GetChild(i) as AbstractRectUI;
                            if (element != null)
                            {

                                element.PerformContentLayout();
                                //int elemH = element.HasSpecificHeight ?
                                //    element.Height :
                                //    element.DesiredHeight;
                                //int elemW = element.HasSpecificWidth ?
                                //    element.Width :
                                //    element.DesiredWidth;
                                //element.SetBounds(0, ypos, element.Width, elemH);
                                element.SetLocationAndSize(0, ypos, element.Width, element.Height);
                                ypos += element.Height;
                                int tmp_right = element.Right;// element.InnerWidth + element.Left;
                                if (tmp_right > maxRight)
                                {
                                    maxRight = tmp_right;
                                }
                            }
                        }

                        this.SetInnerContentSize(maxRight, ypos);
                    }
                    break;
                case CustomWidgets.BoxContentLayoutKind.HorizontalStack:
                    {
                        int count = this.ChildCount;
                        int xpos = 0;
                        int maxBottom = 0;
                        for (int i = 0; i < count; ++i)
                        {
                            var element = this.GetChild(i) as AbstractRectUI;
                            if (element != null)
                            {
                                element.PerformContentLayout();
                                element.SetLocationAndSize(xpos, 0, element.InnerWidth, element.InnerHeight);
                                xpos += element.InnerWidth;
                                int tmp_bottom = element.Bottom;
                                if (tmp_bottom > maxBottom)
                                {
                                    maxBottom = tmp_bottom;
                                }
                            }
                        }

                        this.SetInnerContentSize(xpos, maxBottom);
                    }
                    break;
                default:
                    {
                        int count = this.ChildCount;
                        int maxRight = 0;
                        int maxBottom = 0;
                        for (int i = 0; i < count; ++i)
                        {
                            var element = this.GetChild(i) as AbstractRectUI;
                            if (element != null)
                            {
                                element.PerformContentLayout();
                                int tmp_right = element.Right;// element.InnerWidth + element.Left;
                                if (tmp_right > maxRight)
                                {
                                    maxRight = tmp_right;
                                }
                                int tmp_bottom = element.Bottom;// element.InnerHeight + element.Top;
                                if (tmp_bottom > maxBottom)
                                {
                                    maxBottom = tmp_bottom;
                                }
                            }
                        }

                        if (!this.HasSpecificWidth)
                        {
                            this.SetInnerContentSize(maxRight, this.InnerHeight);
                        }
                        if (!this.HasSpecificHeight)
                        {
                            this.SetInnerContentSize(this.InnerWidth, maxBottom);
                        }
                    }
                    break;
            }
            //------------------------------------------------
            base.RaiseLayoutFinished();
        }
        protected override void Describe(UIVisitor visitor)
        {
            //describe base properties
            base.Describe(visitor);
            //describe child content
            if (uiList != null)
            {
                int j = this.uiList.Count;
                for (int i = 0; i < j; ++i)
                {
                    uiList.GetElement(i).Walk(visitor);
                }
            }
        }

        protected override void OnGuestTalk(UIGuestTalkEventArgs e)
        {
            if (this.DragOver != null)
            {
                this.DragOver(this, e);
            }
            base.OnGuestTalk(e);
        }
    }
}