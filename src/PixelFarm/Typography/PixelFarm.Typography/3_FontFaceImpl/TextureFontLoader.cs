﻿//MIT, 2014-2018, WinterDev 

using System.IO;
using System.Collections.Generic;

using PixelFarm.Agg;
using Typography.OpenFont;

using Typography.Contours;
using Typography.Rendering;



namespace PixelFarm.Drawing.Fonts
{
    public struct TextureFontCreationParams
    {
        public ScriptLang[] scriptLangs;
        public TextureKind textureKind;
        public float originalFontSizeInPoint;
        public HintTechnique hintTechnique;
    }

    public static class TextureFontLoader
    {


        public static FontFace LoadFont(
            Typeface typeface,
            TextureFontCreationParams creationParams,
            out SimpleFontAtlas fontAtlas)
        {
            //1. read font info
            NOpenFontFace openFont = (NOpenFontFace)OpenFontLoader.LoadFont(typeface);
            //------------------------
            SimpleFontAtlasBuilder atlasBuilder = null;

            var textureGen = new GlyphTextureBitmapGenerator();
            textureGen.CreateTextureFontFromScriptLangs(
                typeface,
                creationParams.originalFontSizeInPoint,
                creationParams.textureKind,
                creationParams.scriptLangs,
                (glyphIndex, glyphImage, outputAtlasBuilder) =>
                {
                    if (outputAtlasBuilder != null)
                    {
                        //finish
                        atlasBuilder = outputAtlasBuilder;
                    }
                }
             );

            GlyphImage totalGlyphsImg = atlasBuilder.BuildSingleImage();
            //create atlas
            fontAtlas = atlasBuilder.CreateSimpleFontAtlas();
            fontAtlas.TotalGlyph = totalGlyphsImg;

            //save for debug

#if DEBUG
            ActualImage.SaveImgBufferToPngFile(totalGlyphsImg.GetImageBuffer(), totalGlyphsImg.Width * 4, totalGlyphsImg.Width, totalGlyphsImg.Height, "d:\\WImageTest\\total.png");
#endif
            return openFont;
        }
        //static IEnumerable<ushort> GetGlyphIndexIter(Typeface typeface, params UnicodeLangBits[] rangeBits)
        //{
        //    //temp fixed
        //    GlyphIndexCollector collector = new GlyphIndexCollector();
        //    int j = rangeBits.Length;
        //    for (int i = 0; i < j; ++i)
        //    {
        //        UnicodeRangeInfo rangeInfo = rangeBits[i].ToUnicodeRangeInfo();
        //        //get start and end bit
        //        int startChar = rangeInfo.StartAt;
        //        int startGlyphIndex = typeface.LookupIndex((char)startChar);
        //        while (startGlyphIndex < 1)
        //        {
        //            startChar++;
        //            startGlyphIndex = typeface.LookupIndex((char)startChar);
        //        }
        //        for (int gindex = startGlyphIndex; gindex < startGlyphIndex + 125; ++gindex)
        //        {
        //            yield return (ushort)gindex;
        //        }
        //        //char endAt = (char)rangeInfo.EndAt;
        //        //for (char c = (char)rangeInfo.StartAt; c <= endAt; ++c)
        //        //{
        //        //    typeface.CollectGlyphIndexListFromSampleChar(c, collector);
        //        //}
        //    }
        //}
        //static IEnumerable<ushort> GetGlyphIndexIterFromSampleChar(Typeface typeface, params char[] sampleChars)
        //{
        //    //GlyphIndexCollector collector = new GlyphIndexCollector();
        //    //int j = sampleChars.Length;
        //    //for (int i = 0; i < j; ++i)
        //    //{
        //    //    typeface.CollectGlyphIndexListFromSampleChar(sampleChars[i], collector);
        //    //}
        //    //return collector.GetGlyphIndexIter();
        //    int j = sampleChars.Length;
        //    for (int i = 0; i < j; ++i)
        //    {
        //        yield return (ushort)typeface.LookupIndex(sampleChars[i]);
        //    }
        //}
        //static IEnumerable<ushort> GetGlyphIndexIter(Typeface typeface, params char[] chars)
        //{
        //    int j = chars.Length;
        //    for (int i = 0; i < j; ++i)
        //    {
        //        char c = chars[i];
        //        yield return (ushort)typeface.LookupIndex(c);
        //    }
        //}

        //static SimpleFontAtlasBuilder CreateSampleMsdfTextureFont(
        //    Typeface typeface,
        //    float sizeInPoint,
        //    HintTechnique hintTech,
        //    IEnumerable<ushort> glyphIndexIter)
        //{

        //    ////read type face from file
        //    //Typeface typeface;
        //    //using (var fs = new FileStream(fontfile, FileMode.Open, FileAccess.Read))
        //    //{
        //    //    var reader = new OpenFontReader();
        //    //    //1. read typeface from font file
        //    //    typeface = reader.Read(fs);
        //    //}
        //    //sample: create sample msdf texture 
        //    //-------------------------------------------------------------
        //    var builder = new GlyphPathBuilder(typeface);
        //    builder.SetHintTechnique(hintTech);
        //    MsdfGenParams genParams = new MsdfGenParams();
        //    var atlasBuilder = new SimpleFontAtlasBuilder();
        //    atlasBuilder.SetAtlasInfo(TextureKind.Msdf, sizeInPoint);
        //    foreach (ushort gindex in glyphIndexIter)
        //    {
        //        //build glyph 
        //        builder.BuildFromGlyphIndex(gindex, -1); //use original glyph size (assign -1)
        //        var glyphToContour = new GlyphContourBuilder();


        //        //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
        //        builder.ReadShapes(glyphToContour);
        //        genParams.shapeScale = 1f / 64; //we scale later (as original C++ code use 1/64)
        //        GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour, genParams);
        //        atlasBuilder.AddGlyph(gindex, glyphImg);


        //        //int[] buffer = glyphImage.GetImageBuffer();
        //        //using (var bmp = new System.Drawing.Bitmap(glyphImage.Width, glyphImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //        //{
        //        //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImage.Width, glyphImage.Height),
        //        //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //        //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
        //        //    bmp.UnlockBits(bmpdata);
        //        //    bmp.Save("d:\\WImageTest\\a001_xn2_" + c + ".png");
        //        //}
        //    }





        //    //var glyphImg2 = atlasBuilder.BuildSingleImage();
        //    //using (var bmp = new System.Drawing.Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    //{
        //    //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
        //    //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //    //    int[] intBuffer = glyphImg2.GetImageBuffer();

        //    //    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
        //    //    bmp.UnlockBits(bmpdata);
        //    //    bmp.Save("d:\\WImageTest\\a_total.png");
        //    //}
        //    //atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");

        //    return atlasBuilder;
        //}



        //static SimpleFontAtlasBuilder CreateAggTextureFont(
        //    Typeface typeface, float sizeInPoint, HintTechnique hintTech, IEnumerable<ushort> glyphIndexIter)
        //{

        //    ////read type face from file
        //    //Typeface typeface;
        //    //using (var fs = new FileStream(fontfile, FileMode.Open, FileAccess.Read))
        //    //{
        //    //    var reader = new OpenFontReader();
        //    //    //1. read typeface from font file
        //    //    typeface = reader.Read(fs);
        //    //}
        //    //sample: create sample msdf texture 
        //    //-------------------------------------------------------------
        //    var builder = new GlyphPathBuilder(typeface);
        //    builder.SetHintTechnique(hintTech);
        //    //-------------------------------------------------------------
        //    var atlasBuilder = new SimpleFontAtlasBuilder();
        //    atlasBuilder.SetAtlasInfo(TextureKind.StencilGreyScale, sizeInPoint);
        //    VertexStorePool vxsPool = new VertexStorePool();
        //    //create agg cavnas

        //    foreach (ushort gindex in glyphIndexIter)
        //    {
        //        //build glyph 
        //        builder.BuildFromGlyphIndex(gindex, sizeInPoint);

        //        var txToVxs = new GlyphTranslatorToVxs();
        //        builder.ReadShapes(txToVxs);
        //        //
        //        //create new one
        //        var glyphVxs = new VertexStore();
        //        txToVxs.WriteOutput(glyphVxs);
        //        //find bound 
        //        RectD bounds = new Agg.RectD();
        //        BoundingRect.GetBoundingRect(new VertexStoreSnap(glyphVxs), ref bounds);

        //        //-------------------------------------------- 
        //        int w = (int)System.Math.Ceiling(bounds.Width);
        //        int h = (int)System.Math.Ceiling(bounds.Height);
        //        if (w < 5)
        //        {
        //            w = 5;
        //        }
        //        if (h < 5)
        //        {
        //            h = 5;
        //        }
        //        //translate to positive quadrant
        //        double dx = (bounds.Left < 0) ? -bounds.Left : 0;
        //        double dy = (bounds.Bottom < 0) ? -bounds.Bottom : 0;

        //        if (dx != 0 || dy != 0)
        //        {
        //            Agg.Transform.Affine transformMat = Agg.Transform.Affine.NewTranslation(dx, dy);
        //            VertexStore vxs2 = new VertexStore();
        //            glyphVxs.TranslateToNewVxs(dx, dy, vxs2);
        //            glyphVxs = vxs2;
        //        }
        //        //-------------------------------------------- 
        //        //create glyph img 
        //        ActualImage img = new Agg.ActualImage(w, h, PixelFormat.ARGB32);
        //        AggRenderSurface aggsx = new Agg.AggRenderSurface(img);
        //        AggPainter painter = new Agg.AggPainter(aggsx);
        //        painter.FillColor = Color.Black;
        //        painter.StrokeColor = Color.White;
        //        painter.Fill(glyphVxs);
        //        //-------------------------------------------- 
        //        var glyphImage = new GlyphImage(w, h);
        //        glyphImage.TextureOffsetX = dx;
        //        glyphImage.TextureOffsetY = dy;

        //        glyphImage.SetImageBuffer(ActualImage.CopyImgBuffer(img), false);
        //        //copy data from agg canvas to glyph image
        //        atlasBuilder.AddGlyph(gindex, glyphImage);

        //        ActualImage.SaveImgBufferToPngFile(glyphImage.GetImageBuffer(),
        //            img.Stride, img.Width, img.Height, "d:\\WImageTest\\glyph_gen\\" + gindex + ".png");

        //        //int[] buffer = glyphImage.GetImageBuffer();
        //        //using (var bmp = new System.Drawing.Bitmap(glyphImage.Width, glyphImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //        //{
        //        //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImage.Width, glyphImage.Height),
        //        //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //        //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
        //        //    bmp.UnlockBits(bmpdata);
        //        //    bmp.Save("d:\\WImageTest\\a001_xn2_" + gindex + ".png");
        //        //}
        //    } 

        //    //var glyphImg2 = atlasBuilder.BuildSingleImage();
        //    //using (var bmp = new System.Drawing.Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    //{
        //    //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
        //    //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //    //    int[] intBuffer = glyphImg2.GetImageBuffer();

        //    //    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
        //    //    bmp.UnlockBits(bmpdata);
        //    //    bmp.Save("d:\\WImageTest\\a_total.png");
        //    //}
        //    //atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");

        //    return atlasBuilder;
        //}


        //static SimpleFontAtlasBuilder CreateAggSubPixelRenderingTextureFont(
        //    Typeface typeface, float sizeInPoint, HintTechnique hintTech, IEnumerable<ushort> glyphIndexIter)
        //{

        //    ////read type face from file
        //    //Typeface typeface;
        //    //using (var fs = new FileStream(fontfile, FileMode.Open, FileAccess.Read))
        //    //{
        //    //    var reader = new OpenFontReader();
        //    //    //1. read typeface from font file
        //    //    typeface = reader.Read(fs);
        //    //}
        //    //sample: create sample msdf texture 
        //    //-------------------------------------------------------------
        //    var builder = new GlyphPathBuilder(typeface);
        //    builder.SetHintTechnique(hintTech);
        //    //-------------------------------------------------------------
        //    var atlasBuilder = new SimpleFontAtlasBuilder();
        //    atlasBuilder.SetAtlasInfo(TextureKind.StencilLcdEffect, sizeInPoint);
        //    VertexStorePool vxsPool = new VertexStorePool();
        //    //create agg cavnas

        //    foreach (ushort gindex in glyphIndexIter)
        //    {
        //        //build glyph

        //        builder.BuildFromGlyphIndex(gindex, sizeInPoint);
        //        var txToVxs = new GlyphTranslatorToVxs();
        //        builder.ReadShapes(txToVxs);
        //        //
        //        //create new one
        //        var glyphVxs = new VertexStore();
        //        txToVxs.WriteOutput(glyphVxs);
        //        //find bound

        //        //-------------------------------------------- 
        //        //GlyphImage glyphImg = new GlyphImage()
        //        RectD bounds = new Agg.RectD();
        //        BoundingRect.GetBoundingRect(new VertexStoreSnap(glyphVxs), ref bounds);

        //        //-------------------------------------------- 
        //        int w = (int)System.Math.Ceiling(bounds.Width);
        //        int h = (int)System.Math.Ceiling(bounds.Height);
        //        if (w < 5)
        //        {
        //            w = 5;
        //        }
        //        if (h < 5)
        //        {
        //            h = 5;
        //        }
        //        //translate to positive quadrant 
        //        double dx = (bounds.Left < 0) ? -bounds.Left : 0;
        //        double dy = (bounds.Bottom < 0) ? -bounds.Bottom : 0;
        //        w = w * 4;

        //        if (dx != 0 || dy != 0)
        //        {
        //            Agg.Transform.Affine transformMat = Agg.Transform.Affine.NewTranslation(dx, dy);
        //            VertexStore vxs2 = new VertexStore();
        //            glyphVxs.TranslateToNewVxs(dx, dy, vxs2);
        //            glyphVxs = vxs2;
        //        }
        //        //-------------------------------------------- 
        //        //create glyph img 
        //        ActualImage img = new Agg.ActualImage(w, h, PixelFormat.ARGB32);
        //        AggRenderSurface imgCanvas2d = new Agg.AggRenderSurface(img);
        //        AggPainter painter = new Agg.AggPainter(imgCanvas2d);
        //        //we use white glyph on black bg for this texture                
        //        painter.Clear(Color.Black); //fill with black
        //        painter.FillColor = Color.White;
        //        painter.StrokeColor = Color.White;
        //        //--------------------------------------------  

        //        painter.UseSubPixelRendering = true;
        //        //--------------------------------------------  


        //        //-------------------------------------------- 
        //        painter.Fill(glyphVxs);
        //        //-------------------------------------------- 
        //        var glyphImage = new GlyphImage(w, h);
        //        glyphImage.TextureOffsetX = dx;
        //        glyphImage.TextureOffsetY = dy;
        //        glyphImage.SetImageBuffer(ActualImage.CopyImgBuffer(img), false);
        //        //copy data from agg canvas to glyph image
        //        atlasBuilder.AddGlyph(gindex, glyphImage);

        //        //int[] buffer = glyphImage.GetImageBuffer();
        //        //using (var bmp = new System.Drawing.Bitmap(glyphImage.Width, glyphImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //        //{
        //        //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImage.Width, glyphImage.Height),
        //        //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //        //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
        //        //    bmp.UnlockBits(bmpdata);
        //        //    bmp.Save("d:\\WImageTest\\a001_subpix_xn2_" + gindex + ".png");
        //        //}
        //    }
        //    //var glyphImg2 = atlasBuilder.BuildSingleImage();
        //    //using (var bmp = new System.Drawing.Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    //{
        //    //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
        //    //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //    //    int[] intBuffer = glyphImg2.GetImageBuffer();

        //    //    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
        //    //    bmp.UnlockBits(bmpdata);
        //    //    bmp.Save("d:\\WImageTest\\a_total.png");
        //    //}
        //    //atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");

        //    return atlasBuilder;
        //}



        //static SimpleFontAtlasBuilder CreateSampleMsdfTextureFont(string fontfile,
        //    float sizeInPoint, UnicodeRangeInfo[] ranges)
        //{

        //    //read type face from file
        //    Typeface typeface;
        //    using (var fs = new FileStream(fontfile, FileMode.Open, FileAccess.Read))
        //    {
        //        var reader = new OpenFontReader();
        //        //1. read typeface from font file
        //        typeface = reader.Read(fs);
        //    }
        //    //sample: create sample msdf texture 
        //    //-------------------------------------------------------------
        //    var builder = new GlyphPathBuilder(typeface);
        //    //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
        //    //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
        //    //-------------------------------------------------------------
        //    var atlasBuilder = new SimpleFontAtlasBuilder();
        //    var msdfBuilder = new MsdfGlyphGen();

        //    int rangeCount = ranges.Length;
        //    for (int r = 0; r < rangeCount; ++r)
        //    {
        //        UnicodeRangeInfo rangeInfo = ranges[r];
        //        char startAtUnicode = (char)rangeInfo.StartAt;
        //        char endAtUnicode = (char)rangeInfo.EndAt;
        //        for (char c = startAtUnicode; c <= endAtUnicode; ++c)
        //        {
        //            //build glyph
        //            ushort glyphIndex = builder.Build(c, sizeInPoint);
        //            //builder.BuildFromGlyphIndex(n, sizeInPoint);

        //            var msdfGlyphGen = new MsdfGlyphGen();
        //            GlyphImage glyphImage = msdfGlyphGen.CreateMsdfImage(
        //                builder.GetOutputPoints(),
        //                builder.GetOutputContours(),
        //                builder.GetPixelScale());

        //            atlasBuilder.AddGlyph((int)glyphIndex, glyphImage);


        //            int[] buffer = glyphImage.GetImageBuffer();
        //            using (var bmp = new System.Drawing.Bitmap(glyphImage.Width, glyphImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //            {
        //                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImage.Width, glyphImage.Height),
        //                    System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
        //                bmp.UnlockBits(bmpdata);
        //                bmp.Save("d:\\WImageTest\\a001_xn2_" + c + ".png");
        //            }
        //        }
        //    }




        //    //var glyphImg2 = atlasBuilder.BuildSingleImage();
        //    //using (var bmp = new System.Drawing.Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        //    //{
        //    //    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
        //    //        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
        //    //    int[] intBuffer = glyphImg2.GetImageBuffer();

        //    //    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
        //    //    bmp.UnlockBits(bmpdata);
        //    //    bmp.Save("d:\\WImageTest\\a_total.png");
        //    //}
        //    //atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");

        //    return atlasBuilder;
        //}
    }


}