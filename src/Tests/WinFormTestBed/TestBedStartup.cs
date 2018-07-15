﻿//#define GL_ENABLE
using System;
using System.Windows.Forms;
using LayoutFarm.UI;
namespace YourImplementation
{
    public static class TestBedStartup
    {
        public static void Setup()
        {
#if GL_ENABLE
            YourImplementation.BootStrapOpenGLES2.SetupDefaultValues();
#else
            CommonTextServiceSetup.SetupDefaultValues();
#endif 
            PixelFarm.CpuBlit.ActualBitmap.InstallImageSaveToFileService((IntPtr imgBuffer, int stride, int width, int height, string filename) =>
            {

                using (System.Drawing.Bitmap newBmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    PixelFarm.CpuBlit.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(imgBuffer, newBmp);
                    //save
                    newBmp.Save(filename);
                }
            });

            //you can use your font loader
            YourImplementation.BootStrapWinGdi.SetupDefaultValues();
            //default text breaker, this bridge between 
            LayoutFarm.Composers.Default.TextBreaker = new LayoutFarm.Composers.MyManagedTextBreaker();
        }
        public static void RunDemoList(Type mainType)
        {
            //-------------------------------
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ////------------------------------- 
            var formDemoList = new LayoutFarm.Dev.FormDemoList();
            formDemoList.LoadDemoList(mainType);
            Application.Run(formDemoList);
        }

        static UISurfaceViewportControl _latestviewport;
        public static void RunSpecificDemo(LayoutFarm.DemoBase demo)
        {
            //-------------------------------
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ////------------------------------- 
            //1. select view port kind

            InnerViewportKind innerViewportKind = InnerViewportKind.GdiPlus;
            var workingArea = Screen.PrimaryScreen.WorkingArea;

            var formCanvas = FormCanvasHelper.CreateNewFormCanvas(
               workingArea.Width,
               workingArea.Height,
               innerViewportKind,
               out _latestviewport);
            formCanvas.Text = "PixelFarm" + innerViewportKind;

            demo.StartDemo(new LayoutFarm.AppHost(_latestviewport));
            _latestviewport.TopDownRecalculateContent();
            //==================================================  
            _latestviewport.PaintMe();
          

            //_latestviewport.PaintMe();

            //formCanvas.WindowState = FormWindowState.Maximized;
            formCanvas.Show();
            //got specfic example
            Application.Run(formCanvas);
        }
    }


    public static class DemoFormCreatorHelper
    {
        public static void CreateReadyForm(
         out LayoutFarm.UI.UISurfaceViewportControl viewport,
         out Form formCanvas)
        {

            //1. select view port kind
            InnerViewportKind innerViewportKind = InnerViewportKind.GdiPlus;

            var workingArea = Screen.PrimaryScreen.WorkingArea;

            formCanvas = FormCanvasHelper.CreateNewFormCanvas(
              workingArea.Width,
              workingArea.Height,
              innerViewportKind,
              out viewport);

            formCanvas.Text = "FormCanvas 1 :" + innerViewportKind;

            viewport.PaintMe();

            formCanvas.WindowState = FormWindowState.Maximized;
            formCanvas.Show();
        }
    }
}