﻿//BSD, 2014-present, WinterDev

using PixelFarm.Drawing.Fonts;
using Typography.FontManagement;

namespace PixelFarm.Drawing.GLES2
{

    public static class GLES2Platform
    {
        
        static LayoutFarm.OpenFontTextService s_textService;

        public static LayoutFarm.OpenFontTextService TextService
        {
            get
            {
                if (s_textService == null)
                {
                    s_textService = new LayoutFarm.OpenFontTextService();
                }
                return s_textService;
            } 
        }



        public static void SetInstalledTypefaceProvider(IInstalledTypefaceProvider provider)
        {
            GLES2PlatformFontMx.SetInstalledTypefaceProvider(provider);
        }
        public static InstalledTypeface GetInstalledFont(string fontName, Typography.FontManagement.TypefaceStyle style)
        {
            return GLES2PlatformFontMx.GetInstalledFont(fontName, style);
        }
    }
}