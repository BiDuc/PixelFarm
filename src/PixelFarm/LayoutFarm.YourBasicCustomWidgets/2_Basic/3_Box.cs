﻿//Apache2, 2014-present, WinterDev

using LayoutFarm.UI;
namespace LayoutFarm.CustomWidgets
{
    public enum BoxContentLayoutKind
    {
        Absolute,
        VerticalStack,
        HorizontalStack
    }

    public enum ContentStretch
    {
        None,
        Horizontal,
        Vertical,
        Both,
    }

    public sealed class Box : AbstractBox
    {
        public Box(int w, int h)
            : base(w, h)
        {

        }
        public void SetInnerContentSize(int w, int h)
        {   
            SetDesiredSize(w, h);
        }
        public override void Walk(UIVisitor visitor)
        {
            visitor.BeginElement(this, "box");
            this.Describe(visitor);
            //descrube child 
            visitor.EndElement();
        }
    }
}