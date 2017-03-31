﻿//Apache2, 2014-2017, WinterDev

namespace LayoutFarm.UI
{
    public class UIPlatformWinForm : UIPlatform
    {
        static UIPlatformWinForm platform;
        static UIPlatformWinForm()
        {

        }
        public static UIPlatformWinForm GetDefault()
        {
            return platform;
        }
        public UIPlatformWinForm()
        {
            //--------------------------------------------------------------------
            //TODO: review here again
            //NOTE: this class load native dll images (GLES2)
            //since GLES2 that we use is x86, 
            //so we must specific the file type to x86 ***
            //else this will error on TypeInitializer ( from BadImageFormatException);
            //--------------------------------------------------------------------

            if (platform == null)
            {
                platform = this;
            }

            var fontloader = new PixelFarm.Drawing.WindowsFontLoader();

            try
            {
                //set up winform platform 
                ////gdi+
                PixelFarm.Drawing.WinGdi.WinGdiPlusPlatform.SetFontLoader(fontloader);
                LayoutFarm.UI.Clipboard.SetUIPlatform(this);
            }
            catch (System.Exception ex)
            {

            }
            ////gles2
            //OpenTK.Toolkit.Init();
            //PixelFarm.Drawing.GLES2.GLES2Platform.SetFontLoader(YourImplementation.BootStrapOpenGLES2.myFontLoader);
            ////skia 
            //if (!YourImplementation.BootStrapSkia.IsNativeLibAvailable())
            //{
            //    //handle  when native dll is not ready
            //}
            //else
            //{
            //    //when ready
            //    PixelFarm.Drawing.Skia.SkiaGraphicsPlatform.SetFontLoader(YourImplementation.BootStrapSkia.myFontLoader);
            //}
            //_gdiPlusIFonts = new PixelFarm.Drawing.WinGdi.Gdi32IFonts();
        }

        public override UITimer CreateUITimer()
        {
            return new MyUITimer();
        }
        public override void ClearClipboardData()
        {
            System.Windows.Forms.Clipboard.Clear();
        }
        public override string GetClipboardData()
        {
            return System.Windows.Forms.Clipboard.GetText();
        }
        public override void SetClipboardData(string textData)
        {
            System.Windows.Forms.Clipboard.SetText(textData);
        }


        PixelFarm.Drawing.WinGdi.Gdi32IFonts _gdiPlusIFonts;
        public PixelFarm.Drawing.IFonts GetIFonts()
        {
            return this._gdiPlusIFonts;
        }


    }
}