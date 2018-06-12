﻿//Apache2, 2014-2018, WinterDev

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

    public sealed class SimpleBox : AbstractBox
    {
        public SimpleBox(int w, int h)
            : base(w, h)
        {
        }
        public override void Walk(UIVisitor visitor)
        {
            visitor.BeginElement(this, "simplebox");
            this.Describe(visitor);
            //descrube child 
            visitor.EndElement();
        }
    }
}