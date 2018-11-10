﻿//Apache2, 2014-present, WinterDev


using System;
using System.Windows.Forms;

using PixelFarm.Drawing;
using Typography.FontManagement;

namespace LayoutFarm.UI
{
    public static partial class FormCanvasHelper
    {
        static LayoutFarm.UI.UIPlatformWinForm s_platform;
        static IInstalledTypefaceProvider s_fontstore;
        static void InitWinform()
        {
            if (s_platform != null) return;
            //----------------------------------------------------
            s_platform = new LayoutFarm.UI.UIPlatformWinForm();
            var instTypefaces = new InstalledTypefaceCollection();
            instTypefaces.LoadSystemFonts();
            s_fontstore = instTypefaces;

        }
        public static Form CreateNewFormCanvas(
           int w, int h,
           InnerViewportKind internalViewportKind,
           out LayoutFarm.UI.UISurfaceViewportControl canvasViewport)
        {
            return CreateNewFormCanvas(0, 0, w, h, internalViewportKind, out canvasViewport);
        }



        public static Form CreateNewFormCanvas(
            int xpos, int ypos,
            int w, int h,
            InnerViewportKind internalViewportKind,
            out LayoutFarm.UI.UISurfaceViewportControl canvasViewport)
        {
            //1. init
            InitWinform();
            IInstalledTypefaceProvider fontLoader = s_fontstore;
            //2. 
            PixelFarm.Drawing.ITextService ifont = null;
            switch (internalViewportKind)
            {
                default:
                    ifont = PixelFarm.Drawing.WinGdi.WinGdiPlusPlatform.GetTextService();
                    //ifont = new OpenFontTextService();
                    break;
                case InnerViewportKind.GLES:
                    ifont = new OpenFontTextService();
                    break;

            }

            //PixelFarm.Drawing.WinGdi.WinGdiPlusPlatform.SetInstalledTypefaceProvider(fontLoader);

            //

            //---------------------------------------------------------------------------

            MyRootGraphic myRootGfx = new MyRootGraphic(
               w, h,
               ifont
               );

            //---------------------------------------------------------------------------

            var innerViewport = canvasViewport = new LayoutFarm.UI.UISurfaceViewportControl();
            Rectangle screenClientAreaRect = Conv.ToRect(Screen.PrimaryScreen.WorkingArea);

            canvasViewport.InitRootGraphics(myRootGfx, myRootGfx.TopWinEventPortal, internalViewportKind);
            canvasViewport.Bounds =
                new System.Drawing.Rectangle(xpos, ypos,
                    screenClientAreaRect.Width,
                    screenClientAreaRect.Height);
            //---------------------- 
            Form form1 = new Form();
            //LayoutFarm.Dev.FormNoBorder form1 = new Dev.FormNoBorder();
            form1.Controls.Add(canvasViewport);
            //----------------------
            MakeFormCanvas(form1, canvasViewport);

            form1.SizeChanged += (s, e) =>
            {
                if (form1.WindowState == FormWindowState.Maximized)
                {
                    Screen currentScreen = GetScreenFromX(form1.Left);
                    //make full screen ?
                    if (innerViewport != null)
                    {
                        innerViewport.Size = currentScreen.WorkingArea.Size;
                    }
                }
            };
            //----------------------
            return form1;
        }



        public static void MakeFormCanvas(Form form1, LayoutFarm.UI.UISurfaceViewportControl surfaceViewportControl)
        {
            form1.FormClosing += (s, e) =>
            {
                surfaceViewportControl.Close();
            };

        }

        static Screen GetScreenFromX(int xpos)
        {
            Screen[] allScreens = Screen.AllScreens;
            int j = allScreens.Length;
            int accX = 0;
            for (int i = 0; i < j; ++i)
            {
                Screen sc1 = allScreens[i];
                if (accX + sc1.WorkingArea.Width > xpos)
                {
                    return sc1;
                }
            }
            return Screen.PrimaryScreen;
        }

        public static void CreateCanvasControlOnExistingControl(
              Control landingControl,
              int xpos, int ypos,
              int w, int h,
              InnerViewportKind internalViewportKind,
              out LayoutFarm.UI.UISurfaceViewportControl canvasViewport)
        {
            //1. init
            InitWinform();
            IInstalledTypefaceProvider fontLoader = s_fontstore;
            //2. 
            PixelFarm.Drawing.ITextService ifont = null;
            switch (internalViewportKind)
            {
                default:
                    ifont = PixelFarm.Drawing.WinGdi.WinGdiPlusPlatform.GetTextService();
                    //ifont = new OpenFontTextService();
                    break;
                case InnerViewportKind.GLES:
                    ifont = new OpenFontTextService();
                    break;

            }

            PixelFarm.Drawing.WinGdi.WinGdiPlusPlatform.SetInstalledTypefaceProvider(fontLoader);
            //---------------------------------------------------------------------------

            MyRootGraphic myRootGfx = new MyRootGraphic(w, h, ifont);
            //---------------------------------------------------------------------------

            var innerViewport = canvasViewport = new LayoutFarm.UI.UISurfaceViewportControl();
            Rectangle screenClientAreaRect = Conv.ToRect(Screen.PrimaryScreen.WorkingArea);

            canvasViewport.InitRootGraphics(myRootGfx, myRootGfx.TopWinEventPortal, internalViewportKind);
            canvasViewport.Bounds =
                new System.Drawing.Rectangle(xpos, ypos,
                    screenClientAreaRect.Width,
                    screenClientAreaRect.Height);

            landingControl.Controls.Add(canvasViewport);
            //
            Form ownerForm = landingControl.FindForm();
            if (ownerForm != null)
            {
                ownerForm.FormClosing += (s, e) =>
                {
                    innerViewport.Close();
                };
            }

        }
    }



}