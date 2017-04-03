﻿//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;

using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Typography.OpenFont;
using Typography.Rendering;

using PixelFarm.Agg;
using Typography.TextLayout;
using PixelFarm.Drawing.Fonts;

namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter painter;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;

        OpenFontStore _openFontStore;

        DevTextPrinterBase selectedTextPrinter = null;
        VxsTextPrinter _devVxsTextPrinter = null;
        DevGdiTextPrinter _devGdiTextPrinter = null;

        UI.SampleTextBoxControllerForGdi _controllerForGdi = new UI.SampleTextBoxControllerForGdi();
        //
        UI.SampleTextBoxControllerForPixelFarm _controllerForPixelFarm = new UI.SampleTextBoxControllerForPixelFarm();

        InstalledFontCollection installedFontCollection;
        TypefaceStore _typefaceStore;
        float _fontSizeInPts = 14;//default
        InstalledFont _selectedInstallFont;

        public Form1()
        {
            InitializeComponent();



            _devGdiTextPrinter = new DevGdiTextPrinter();
            this.sampleTextBox1.Visible = false;
            _openFontStore = new OpenFontStore();

            //default
            //set script lang,
            //test with Thai for 'complex script' 

            _devGdiTextPrinter.ScriptLang = Typography.OpenFont.ScriptLangs.Thai;
            _devGdiTextPrinter.PositionTechnique = PositionTechnique.OpenFont;


            this.Load += new EventHandler(Form1_Load);

            this.txtGridSize.KeyDown += TxtGridSize_KeyDown;
            //----------
            txtInputChar.TextChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMiniAgg_SingleGlyph);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithGdiPlusPath);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithTextPrinterAndMiniAgg);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMsdfGen);
            cmbRenderChoices.SelectedIndex = 2;
            cmbRenderChoices.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbPositionTech.Items.Add(PositionTechnique.OpenFont);
            cmbPositionTech.Items.Add(PositionTechnique.Kerning);
            cmbPositionTech.Items.Add(PositionTechnique.None);
            cmbPositionTech.SelectedIndex = 0;
            cmbPositionTech.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 
            lstHintList.Items.Add(HintTechnique.None);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction);
            lstHintList.Items.Add(HintTechnique.TrueTypeInstruction_VerticalOnly);
            lstHintList.Items.Add(HintTechnique.CustomAutoFit);
            lstHintList.SelectedIndex = 0;
            lstHintList.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 
            //snapX

            lstGlyphSnapX.Items.Add(GlyphPosPixelSnapKind.None);
            lstGlyphSnapX.Items.Add(GlyphPosPixelSnapKind.Half);
            lstGlyphSnapX.Items.Add(GlyphPosPixelSnapKind.Integer);
            lstGlyphSnapX.SelectedIndex = 2;//integer             
            lstGlyphSnapX.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 
            //snapY  
            lstGlyphSnapY.Items.Add(GlyphPosPixelSnapKind.None);
            lstGlyphSnapY.Items.Add(GlyphPosPixelSnapKind.Half);
            lstGlyphSnapY.Items.Add(GlyphPosPixelSnapKind.Integer);
            lstGlyphSnapY.SelectedIndex = 2;//integer
            lstGlyphSnapY.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //---------- 
            //share text printer to our sample textbox
            //but you can create another text printer that specific to text textbox control
            Graphics gx = this.sampleTextBox1.CreateGraphics();
            _controllerForGdi.BindHostGraphics(gx);
            _controllerForGdi.TextPrinter = _devGdiTextPrinter;
            //---------- 
            _controllerForPixelFarm.BindHostGraphics(gx);
            _controllerForPixelFarm.TextPrinter = _devVxsTextPrinter;

            //---------- 
            this.sampleTextBox1.SetController(_controllerForPixelFarm);


            button1.Click += (s, e) => UpdateRenderOutput();
            chkShowGrid.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkShowTess.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkXGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkYGridFitting.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkFillBackground.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkLcdTechnique.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkDrawBone.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkGsubEnableLigature.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkShowTess.CheckedChanged += (s, e) => UpdateRenderOutput();
            //----------


            //1. create font collection             
            installedFontCollection = new InstalledFontCollection();
            //2. set some essential handler
            installedFontCollection.SetFontNameDuplicatedHandler((f1, f2) => FontNameDuplicatedDecision.Skip);
            foreach (string file in Directory.GetFiles("..\\..\\..\\TestFonts", "*.ttf"))
            {
                //eg. this is our custom font folder  
                installedFontCollection.AddFont(new FontFileStreamProvider(file));
            }
            //3.
            installedFontCollection.LoadWindowsSystemFonts();
            //---------- 
            //show result
            InstalledFont selectedFF = null;
            int selected_index = 0;
            int ffcount = 0;
            bool found = false;
            foreach (InstalledFont ff in installedFontCollection.GetInstalledFontIter())
            {
                if (!found && ff.FontName == "Tahoma")
                {
                    selectedFF = ff;
                    selected_index = ffcount;
                    _selectedInstallFont = ff;
                    found = true;
                }
                lstFontList.Items.Add(ff);
                ffcount++;
            }
            //set default font for current text printer
            //
            _typefaceStore = new TypefaceStore();
            _typefaceStore.FontCollection = installedFontCollection;
            //set default font for current text printer
            // selectedTextPrinter.Typeface = _typefaceStore.GetTypeface(selectedFF);
            //---------- 


            if (selected_index < 0) { selected_index = 0; }
            lstFontList.SelectedIndex = selected_index;
            lstFontList.SelectedIndexChanged += (s, e) =>
            {
                InstalledFont ff = lstFontList.SelectedItem as InstalledFont;
                if (ff != null)
                {
                    _selectedInstallFont = ff;
                    selectedTextPrinter.Typeface = _typefaceStore.GetTypeface(ff);
                    //sample text box 
                    UpdateRenderOutput();
                }
            };
            //----------
            //----------
            lstFontSizes.Items.AddRange(
              new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72,240,300,360
              });
            lstFontSizes.SelectedIndexChanged += (s, e) =>
            {
                //new font size
                _fontSizeInPts = (int)lstFontSizes.SelectedItem;
                UpdateRenderOutput();
            };

            //----------------
            //string inputstr = "ก้า";
            //string inputstr = "น้ำน้ำ";
            //string inputstr = "example";
            //string inputstr = "i";
            string inputstr = "e";
            //string inputstr = "fi";
            //string inputstr = "ก่นกิ่น";
            //string inputstr = "ญญู";
            //string inputstr = "ป่า"; //for gpos test 
            //string inputstr = "快速上手";
            //----------------
            this.txtInputChar.Text = inputstr;
            this.chkFillBackground.Checked = true;


        }



        enum RenderChoice
        {
            RenderWithMiniAgg_SingleGlyph,//for test single glyph 
            RenderWithGdiPlusPath,
            RenderWithTextPrinterAndMiniAgg,
            RenderWithMsdfGen, //rendering with multi-channel signed distance field img
            RenderWithSdfGen//not support sdfgen
        }

        void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Render with PixelFarm";
            this.lstFontSizes.SelectedIndex = lstFontSizes.Items.Count - 3;
            //this.lstFontSizes.SelectedIndex = 0; 
            var installedFont = lstFontList.SelectedItem as InstalledFont;
            if (installedFont != null)
            {
                _selectedInstallFont = installedFont;
            }

        }

        void UpdateRenderOutput()
        {
            if (g == null)
            {
                destImg = new ActualImage(400, 300, PixelFormat.ARGB32);
                imgGfx2d = new ImageGraphics2D(destImg); //no platform
                painter = new AggCanvasPainter(imgGfx2d);
                winBmp = new Bitmap(400, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = this.CreateGraphics();

                painter.CurrentFont = new PixelFarm.Drawing.RequestFont("tahoma", 14);


                _devVxsTextPrinter = new VxsTextPrinter(painter, _openFontStore);
                _devVxsTextPrinter.TargetCanvasPainter = painter;
                _devVxsTextPrinter.ScriptLang = _devGdiTextPrinter.ScriptLang;
                _devVxsTextPrinter.PositionTechnique = _devGdiTextPrinter.PositionTechnique;
                _devGdiTextPrinter.TargetGraphics = g;


            }

            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }

            var hintTech = (HintTechnique)lstHintList.SelectedItem;

            //1. read typeface from font file 
            RenderChoice renderChoice = (RenderChoice)this.cmbRenderChoices.SelectedItem;
            switch (renderChoice)
            {

                case RenderChoice.RenderWithGdiPlusPath:
                    {
                        selectedTextPrinter = _devGdiTextPrinter;
                        selectedTextPrinter.Typeface = _typefaceStore.GetTypeface(_selectedInstallFont);
                        selectedTextPrinter.FontSizeInPoints = _fontSizeInPts;
                        selectedTextPrinter.HintTechnique = hintTech;
                        selectedTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;

                        selectedTextPrinter.GlyphPosPixelSnapX = (GlyphPosPixelSnapKind)this.lstGlyphSnapX.SelectedItem;
                        selectedTextPrinter.GlyphPosPixelSnapY = (GlyphPosPixelSnapKind)this.lstGlyphSnapY.SelectedItem;
                        //
                        selectedTextPrinter.DrawString(this.txtInputChar.Text.ToCharArray(), 0, 0);

                    }
                    break;
                case RenderChoice.RenderWithTextPrinterAndMiniAgg:
                    {
                        //clear previous draw
                        painter.Clear(PixelFarm.Drawing.Color.White);
                        painter.UseSubPixelRendering = chkLcdTechnique.Checked;
                        painter.FillColor = PixelFarm.Drawing.Color.Black;

                        selectedTextPrinter = _devVxsTextPrinter;
                        selectedTextPrinter.Typeface = _typefaceStore.GetTypeface(_selectedInstallFont);
                        selectedTextPrinter.FontSizeInPoints = _fontSizeInPts;
                        selectedTextPrinter.HintTechnique = hintTech;
                        selectedTextPrinter.PositionTechnique = (PositionTechnique)cmbPositionTech.SelectedItem;
                        selectedTextPrinter.GlyphPosPixelSnapX = (GlyphPosPixelSnapKind)this.lstGlyphSnapX.SelectedItem;
                        selectedTextPrinter.GlyphPosPixelSnapY = (GlyphPosPixelSnapKind)this.lstGlyphSnapY.SelectedItem;
                        //test print 3 lines

                        char[] printTextBuffer = this.txtInputChar.Text.ToCharArray();
                        float x_pos = 0, y_pos = 200;
                        float lineSpacingPx = selectedTextPrinter.FontLineSpacingPx;
                        for (int i = 0; i < 3; ++i)
                        {
                            selectedTextPrinter.DrawString(printTextBuffer, x_pos, y_pos);
                            y_pos -= lineSpacingPx;
                        }


                        //copy from Agg's memory buffer to gdi 
                        PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
                        g.Clear(Color.White);
                        g.DrawImage(winBmp, new Point(10, 0));

                    }
                    break;

                //==============================================
                //render 1 glyph for debug and test
                case RenderChoice.RenderWithMsdfGen:
                case RenderChoice.RenderWithSdfGen:
                    {
                        char testChar = this.txtInputChar.Text[0];
                        Typeface typeFace = _typefaceStore.GetTypeface(_selectedInstallFont);
                        RenderWithMsdfImg(typeFace, testChar, _fontSizeInPts);

                    }
                    break;
                case RenderChoice.RenderWithMiniAgg_SingleGlyph:
                    {
                        //for test only 1 char
                        char testChar = this.txtInputChar.Text[0];
                        Typeface typeFace = _typefaceStore.GetTypeface(_selectedInstallFont);
                        RenderSingleCharWithMiniAgg(typeFace, testChar, _fontSizeInPts);

                    }
                    break;
                default:
                    throw new NotSupportedException();
            }


        }

        VertexStorePool _vxsPool = new VertexStorePool();

        void RenderSingleCharWithMiniAgg(Typeface typeface, char testChar, float sizeInPoint)
        {
            //----------------------------------------------------
            var builder = new GlyphPathBuilder(typeface);
            builder.SetHintTechnique((HintTechnique)lstHintList.SelectedItem);
#if DEBUG
            builder.dbugAlwaysDoCurveAnalysis = true;
#endif
            //----------------------------------------------------
            builder.Build(testChar, sizeInPoint);

            var txToVxs1 = new GlyphTranslatorToVxs();
            builder.ReadShapes(txToVxs1);



            VertexStore vxs = new VertexStore();
            txToVxs1.WriteOutput(vxs, _vxsPool);
            painter.SetOrigin(20, 20);
            //----------------------------------------------------
            painter.UseSubPixelRendering = chkLcdTechnique.Checked;

            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            painter.Clear(PixelFarm.Drawing.Color.White);

            if (chkFillBackground.Checked)
            {
                //5.2 
                painter.FillColor = PixelFarm.Drawing.Color.Black;
                //5.3
                painter.Fill(vxs);
            }
            if (chkBorder.Checked)
            {
                //5.4 
                // p.StrokeWidth = 3;
                painter.StrokeColor = PixelFarm.Drawing.Color.Green;
                //user can specific border width here...
                //p.StrokeWidth = 2;
                //5.5 
                painter.Draw(vxs);
            }

            //--------------------------
            if (chkShowTess.Checked)
            {
                //
#if DEBUG
                float scale = typeface.CalculateToPixelScaleFromPointSize(sizeInPoint);
                GlyphFitOutline fitOutline = builder.LatestGlyphFitOutline;
                if (fitOutline != null)
                {
                    debugDrawTriangulatedGlyph(fitOutline, scale);
                }
#endif

            }
            //--------------------------
#if DEBUG
            builder.dbugAlwaysDoCurveAnalysis = false;
#endif
            if (chkShowGrid.Checked)
            {
                //render grid
                RenderGrid(800, 600, _gridSize, painter);
            }
            painter.SetOrigin(0, 0);


            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }

        void RenderWithMsdfImg(Typeface typeface, char testChar, float sizeInPoint)
        {
            painter.FillColor = PixelFarm.Drawing.Color.Black;
            //p.UseSubPixelRendering = chkLcdTechnique.Checked;
            painter.Clear(PixelFarm.Drawing.Color.White);
            //----------------------------------------------------
            var builder = new GlyphPathBuilder(typeface);
            builder.SetHintTechnique((HintTechnique)lstHintList.SelectedItem);

            //----------------------------------------------------
            builder.Build(testChar, sizeInPoint);
            //----------------------------------------------------
            var glyphToContour = new GlyphTranslatorToContour();
            builder.ReadShapes(glyphToContour);
            //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
            GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour);
            var actualImg = ActualImage.CreateFromBuffer(glyphImg.Width, glyphImg.Height, PixelFormat.ARGB32, glyphImg.GetImageBuffer());
            painter.DrawImage(actualImg, 0, 0);

            //using (Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            //{
            //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
            //    bmp.UnlockBits(bmpdata);
            //    bmp.Save("d:\\WImageTest\\a001_xn2_" + n + ".png");
            //}

            if (chkShowGrid.Checked)
            {
                //render grid
                RenderGrid(800, 600, _gridSize, painter);
            }

            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(destImg, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(30, 20));
        }



        void RenderGrid(int width, int height, int sqSize, AggCanvasPainter p)
        {
            //render grid 
            p.FillColor = PixelFarm.Drawing.Color.Gray;
            for (int y = 0; y < height;)
            {
                for (int x = 0; x < width;)
                {
                    p.FillRectLBWH(x, y, 1, 1);
                    x += sqSize;
                }
                y += sqSize;
            }
        }
        static void DrawPointKind(AggCanvasPainter painter, GlyphPoint2D point, float scale)
        {
            switch (point.kind)
            {
                case PointKind.C3Start:
                case PointKind.C3End:
                case PointKind.C4Start:
                case PointKind.C4End:
                case PointKind.LineStart:
                case PointKind.LineStop:
                    {

                        var prevColor = painter.FillColor;
                        painter.FillColor = PixelFarm.Drawing.Color.Red;
                        if (point.AdjustedY != 0)
                        {
                            painter.FillRectLBWH(point.x * scale, point.y * scale, 5, 5);
                        }
                        else
                        {
                            painter.FillRectLBWH(point.x * scale, point.y * scale, 2, 2);
                        }
                        painter.FillColor = prevColor;
                    }
                    break;

            }
        }
        static void DrawEdge(AggCanvasPainter painter, EdgeLine edge, float scale)
        {
            if (edge.IsOutside)
            {
                //free side     

                Poly2Tri.TriangulationPoint p = edge.p;
                Poly2Tri.TriangulationPoint q = edge.q;

                var u_data_p = p.userData as GlyphPoint2D;
                var u_data_q = q.userData as GlyphPoint2D;

                //if show control point
                DrawPointKind(painter, u_data_p, scale);
                DrawPointKind(painter, u_data_q, scale);


                switch (edge.SlopKind)
                {
                    default:
                        painter.StrokeColor = PixelFarm.Drawing.Color.Green;
                        break;
                    case LineSlopeKind.Vertical:
                        if (edge.IsLeftSide)
                        {
                            painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                        }
                        else
                        {
                            painter.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                        }
                        break;
                    case LineSlopeKind.Horizontal:

                        if (edge.IsUpper)
                        {
                            painter.StrokeColor = PixelFarm.Drawing.Color.Red;
                        }
                        else
                        {
                            //lower edge
                            painter.StrokeColor = PixelFarm.Drawing.Color.Magenta;
                        }
                        break;
                }
            }
            else
            {
                switch (edge.SlopKind)
                {
                    default:
                        painter.StrokeColor = PixelFarm.Drawing.Color.LightGray;
                        break;
                    case LineSlopeKind.Vertical:
                        painter.StrokeColor = PixelFarm.Drawing.Color.Blue;
                        break;
                    case LineSlopeKind.Horizontal:
                        painter.StrokeColor = PixelFarm.Drawing.Color.Yellow;
                        break;
                }
            }
            painter.Line(edge.x0 * scale, edge.y0 * scale, edge.x1 * scale, edge.y1 * scale);
        }

#if DEBUG
        void debugDrawTriangulatedGlyph(GlyphFitOutline glyphFitOutline, float pixelScale)
        {
            painter.StrokeColor = PixelFarm.Drawing.Color.Magenta;
            List<GlyphTriangle> triAngles = glyphFitOutline.dbugGetTriangles();
            int j = triAngles.Count;
            //
            double prev_cx = 0, prev_cy = 0;
            // 
            bool drawBone = this.chkDrawBone.Checked;

            for (int i = 0; i < j; ++i)
            {
                //---------------
                GlyphTriangle tri = triAngles[i];
                EdgeLine e0 = tri.e0;
                EdgeLine e1 = tri.e1;
                EdgeLine e2 = tri.e2;
                //---------------
                //draw each triangles
                DrawEdge(painter, e0, pixelScale);
                DrawEdge(painter, e1, pixelScale);
                DrawEdge(painter, e2, pixelScale);
                //---------------
                //draw centroid
                double cen_x = tri.CentroidX;
                double cen_y = tri.CentroidY;
                //---------------
                painter.FillColor = PixelFarm.Drawing.Color.Yellow;
                painter.FillRectLBWH(cen_x * pixelScale, cen_y * pixelScale, 2, 2);
                if (!drawBone)
                {

                    if (i == 0)
                    {
                        //start mark
                        painter.FillColor = PixelFarm.Drawing.Color.Yellow;
                        painter.FillRectLBWH(cen_x * pixelScale, cen_y * pixelScale, 7, 7);
                    }
                    else
                    {
                        painter.StrokeColor = PixelFarm.Drawing.Color.Red;
                        painter.Line(
                            prev_cx * pixelScale, prev_cy * pixelScale,
                            cen_x * pixelScale, cen_y * pixelScale);
                    }
                    prev_cx = cen_x;
                    prev_cy = cen_y;
                }
            }
            //---------------
            //draw bone 
            if (drawBone)
            {
                List<GlyphBone> bones = glyphFitOutline.dbugGetBones();
                j = bones.Count;
                for (int i = 0; i < j; ++i)
                {
                    GlyphBone b = bones[i];
                    if (i == 0)
                    {
                        //start mark
                        painter.FillColor = PixelFarm.Drawing.Color.Yellow;
                        painter.FillRectLBWH(b.p.CentroidX * pixelScale, b.p.CentroidY * pixelScale, 7, 7);
                    }
                    //draw each bone

                    painter.StrokeColor = b.IsLongBone ? PixelFarm.Drawing.Color.Yellow : PixelFarm.Drawing.Color.Red;
                    painter.Line(
                        b.p.CentroidX * pixelScale, b.p.CentroidY * pixelScale,
                        b.q.CentroidX * pixelScale, b.q.CentroidY * pixelScale);
                }
            }
            //---------------
        }

#endif

        void DrawGlyphContour(GlyphContour cnt, AggCanvasPainter p)
        {
            //for debug
            List<GlyphPart> parts = cnt.parts;
            int j = parts.Count;
            for (int i = 0; i < j; ++i)
            {
                GlyphPart part = parts[i];
                switch (part.Kind)
                {
                    default: throw new NotSupportedException();
                    case GlyphPartKind.Line:
                        {
                            GlyphLine line = (GlyphLine)part;
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            var p0 = line.FirstPoint;
                            p.FillRectLBWH(p0.X, p0.Y, 2, 2);
                            p.FillRectLBWH(line.x1, line.y1, 2, 2);
                        }
                        break;
                    case GlyphPartKind.Curve3:
                        {
                            GlyphCurve3 c = (GlyphCurve3)part;
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            var p0 = c.FirstPoint;
                            p.FillRectLBWH(p0.X, p0.Y, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Blue;
                            p.FillRectLBWH(c.x1, c.y1, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            p.FillRectLBWH(c.x2, c.y2, 2, 2);
                        }
                        break;
                    case GlyphPartKind.Curve4:
                        {
                            GlyphCurve4 c = (GlyphCurve4)part;
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            var p0 = c.FirstPoint;
                            p.FillRectLBWH(p0.X, p0.Y, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Blue;
                            p.FillRectLBWH(c.x1, c.y1, 2, 2);
                            p.FillRectLBWH(c.x2, c.y2, 2, 2);
                            p.FillColor = PixelFarm.Drawing.Color.Red;
                            p.FillRectLBWH(c.x3, c.y3, 2, 2);
                        }
                        break;
                }
            }
        }





        VertexStorePool _vxsPool2 = new VertexStorePool();
        int _gridSize = 5;//default 
        private void TxtGridSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int result = this._gridSize;
                if (int.TryParse(this.txtGridSize.Text, out result))
                {
                    if (result < 5)
                    {
                        _gridSize = 5;
                    }
                    else if (result > 200)
                    {
                        _gridSize = 200;
                    }
                }
                this._gridSize = result;
                this.txtGridSize.Text = _gridSize.ToString();
                UpdateRenderOutput();
            }

        }
        private void cmdBuildMsdfTexture_Click(object sender, EventArgs e)
        {

            string sampleFontFile = @"..\..\..\TestFonts\tahoma.ttf";
            CreateSampleMsdfTextureFont(
                sampleFontFile,
                18,
                0,
                255,
                "d:\\WImageTest\\sample_msdf.png");

        }
        static void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, ushort startGlyphIndex, ushort endGlyphIndex, string outputFile)
        {
            //sample
            var reader = new OpenFontReader();

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeface = reader.Read(fs);
                //sample: create sample msdf texture 
                //-------------------------------------------------------------
                var builder = new GlyphPathBuilder(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasBuilder();


                for (ushort gindex = startGlyphIndex; gindex <= endGlyphIndex; ++gindex)
                {
                    //build glyph
                    builder.BuildFromGlyphIndex(gindex, sizeInPoint);

                    var glyphToContour = new GlyphTranslatorToContour();
                    //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
                    builder.ReadShapes(glyphToContour);
                    GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphToContour);
                    atlasBuilder.AddGlyph(gindex, glyphImg);

                    //using (Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    //{
                    //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                    //    bmp.UnlockBits(bmpdata);
                    //    bmp.Save("d:\\WImageTest\\a001_xn2_" + n + ".png");
                    //}
                }

                var glyphImg2 = atlasBuilder.BuildSingleImage();
                using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int[] intBuffer = glyphImg2.GetImageBuffer();

                    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save("d:\\WImageTest\\a_total.png");
                }
                atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");
            }
        }

        private void chkShowSampleTextBox_CheckedChanged(object sender, EventArgs e)
        {
            //if (this.sampleTextBox1.Visible = chkShowSampleTextBox.Visible)
            //{
            //    this.sampleTextBox1.Focus();
            //}
        }
    }
}
