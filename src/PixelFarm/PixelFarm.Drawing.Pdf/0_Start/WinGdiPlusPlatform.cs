﻿//BSD, 2014-present, WinterDev 

using Typography.FontManagement;
namespace PixelFarm.Drawing.Pdf
{
    public static class PdfPlaform
    {

        static PdfPlaform()
        {

            //PixelFarm.Agg.AggBuffMx.SetNaiveBufferImpl(new Win32AggBuffMx());
            //3. set default encoding
            // WinGdiTextService.SetDefaultEncoding(System.Text.Encoding.ASCII);
        }

        public static void SetFontEncoding(System.Text.Encoding encoding)
        {
            //WinGdiTextService.SetDefaultEncoding(encoding);
        }
        public static void SetInstalledTypefaceProvider(IInstalledTypefaceProvider provider)
        {
            // WinGdiFontFace.SetFontLoader(fontLoader);
        }
    }



}