﻿//BSD, 2014-2016, WinterDev  

namespace PixelFarm.Drawing.WinGdi
{
    class MyRegion : Region
    {
        SkiaSharp.SKRegion rgn = new SkiaSharp.SKRegion();
        public override object InnerRegion
        {
            get { return this.rgn; }
        }
        public override void Dispose()
        {
            if (rgn != null)
            {
                rgn.Dispose();
                rgn = null;
            }
        }
    }
}