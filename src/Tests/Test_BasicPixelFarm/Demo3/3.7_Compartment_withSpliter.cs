﻿//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
using LayoutFarm.CustomWidgets;
namespace LayoutFarm
{
    [DemoNote("3.7 Demo_CompartmentWithSpliter")]
    class Demo_CompartmentWithSpliter : DemoBase
    {
        NinespaceBox ninespaceBox;
        protected override void OnStartDemo(AppHost host)
        {
            //--------------------------------
            {
                //background element
                var bgbox = new LayoutFarm.CustomWidgets.Box(800, 600);
                bgbox.BackColor = Color.White;
                bgbox.SetLocation(0, 0);
                SetupBackgroundProperties(bgbox);
                host.AddChild(bgbox);
            }
            //--------------------------------
            //ninespace compartment
            ninespaceBox = new NinespaceBox(800, 600);
            ninespaceBox.ShowGrippers = true;
            host.AddChild(ninespaceBox);
            ninespaceBox.SetSize(800, 600);
        }
        void SetupBackgroundProperties(LayoutFarm.CustomWidgets.Box backgroundBox)
        {
        }
    }
}