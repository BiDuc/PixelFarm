﻿using System;
using System.Windows.Forms;
using SkiaSharp;

namespace TestSkia1
{
    public partial class Form1 : Form
    {
        public enum SkiaBackend
        {
            Memory,
            GLES
        }

        SkiaBackend selectedBackend;
        public Form1()
        {
            InitializeComponent();


            //init
            canvas.Visible = true;
            glControl.Visible = false;

            glControl.PaintSurface += GlControl_PaintSurface;
            canvas.PaintSurface += Canvas_PaintSurface;

            //
            cmbBackEnd.Items.Add(SkiaBackend.Memory);
            cmbBackEnd.Items.Add(SkiaBackend.GLES);
            cmbBackEnd.SelectedIndex = 0;//init
            cmbBackEnd.SelectedIndexChanged += (s, e) =>
            {
                SelectBackend((SkiaBackend)cmbBackEnd.SelectedItem);
            };
        }
        public void SelectBackend(SkiaBackend backend)
        {
            switch (this.selectedBackend = backend)
            {
                default: throw new NotSupportedException();
                case SkiaBackend.Memory:
                    glControl.Visible = false;
                    canvas.Visible = true;//show memory canvas
                    break;
                case SkiaBackend.GLES:
                    canvas.Visible = false;
                    glControl.Visible = true; //show glcanvas
                    break;
            }

        }
        private void Canvas_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            var skCanvas = e.Surface.Canvas;
            skCanvas.Clear(new SkiaSharp.SKColor(255, 255, 255));

            using (var paint = new SKPaint())
            {
                paint.TextSize = 36.0f;
                paint.IsAntialias = true;
                paint.Color = (SKColor)0xFF4281A4;
                paint.IsStroke = false;
                skCanvas.DrawText("PixelFarm+SkiaSharp MemoryCanvas", 20, 64.0f, paint);
                paint.StrokeWidth = 3;
                skCanvas.DrawLine(0, 0, 100, 80, paint);
            }



        }
        private void GlControl_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs e)
        {
            var skCanvas = e.Surface.Canvas;
            skCanvas.Clear(new SkiaSharp.SKColor(255, 255, 255));

            using (var paint = new SKPaint())
            {
                paint.TextSize = 36.0f;
                paint.IsAntialias = true;
                paint.Color = (SKColor)0xFF4281A4;
                paint.IsStroke = false;
                skCanvas.DrawText("PixelFarm+SkiaSharp GLCanvas", 20, 64.0f, paint);
                paint.StrokeWidth = 3;
                skCanvas.DrawLine(0, 0, 100, 80, paint);
            }
        }
    }
}
