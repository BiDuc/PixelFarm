﻿//Apache2, 2014-present, WinterDev


using PixelFarm.Drawing;
using LayoutFarm.ContentManagers;
using LayoutFarm.UI;

namespace LayoutFarm
{
    public class AppHost : IAppHost
    {
        ImageContentManager imageContentMan;
        LayoutFarm.UI.UISurfaceViewportControl vw;
        int primaryScreenWorkingAreaW;
        int primaryScreenWorkingAreaH;
        int _formTitleBarHeight;
        System.Windows.Forms.Form ownerForm;
        public AppHost(LayoutFarm.UI.UISurfaceViewportControl vw)
        {
            this.vw = vw;
            ownerForm = this.vw.FindForm();
            System.Drawing.Rectangle screenRectangle = ownerForm.RectangleToScreen(ownerForm.ClientRectangle);
            _formTitleBarHeight = screenRectangle.Top - ownerForm.Top;


            var primScreenWorkingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            this.primaryScreenWorkingAreaW = primScreenWorkingArea.Width;
            this.primaryScreenWorkingAreaH = primScreenWorkingArea.Height;

            //--------------
            imageContentMan = new ImageContentManager();
            imageContentMan.ImageLoadingRequest += (s, e) =>
            {
                e.SetResultImage(LoadBitmap(e.ImagSource));
            };
        }
        public string OwnerFormTitle
        {
            get { return ownerForm.Text; }
            set
            {
                ownerForm.Text = value;
            }
        }
        public int OwnerFormTitleBarHeight { get { return _formTitleBarHeight; } }

        public static Image LoadBitmap(string filename)
        {
            System.Drawing.Bitmap gdiBmp = new System.Drawing.Bitmap(filename);
            GdiPlusBitmap bmp = new GdiPlusBitmap(gdiBmp.Width, gdiBmp.Height, gdiBmp);
            return bmp;
        }
        void LazyImageLoad(ImageBinder binder)
        {
            //load here as need
            imageContentMan.AddRequestImage(binder);
        }

        public int PrimaryScreenWidth
        {
            get { return this.primaryScreenWorkingAreaW; }
        }
        public int PrimaryScreenHeight
        {
            get { return this.primaryScreenWorkingAreaH; }
        }
        public void AddChild(RenderElement renderElement)
        {
            this.vw.AddChild(renderElement);
        }

        internal LayoutFarm.UI.UISurfaceViewportControl ViewportControl
        {
            get { return this.vw; }
        }
        public RootGraphic RootGfx
        {
            get { return this.vw.RootGfx; }
        }
        public ImageBinder GetImageBinder(string src)
        {
            ClientImageBinder clientImgBinder = new ClientImageBinder(src);
            clientImgBinder.SetLazyLoaderFunc(LazyImageLoad);
            //if use lazy img load func
            imageContentMan.AddRequestImage(clientImgBinder);
            return clientImgBinder;
        }
        public ImageBinder GetImageBinder2(string src)
        {
            ClientImageBinder clientImgBinder = new ClientImageBinder(src);
            clientImgBinder.SetImage(LoadBitmap(src));
            clientImgBinder.State = ImageBinderState.Loaded;
            return clientImgBinder;
        }
        public ImageBinder GetImageBinder3(string src, float scale)
        {
            //scale image to fit the viewport 
            //
            ClientImageBinder clientImgBinder = new ClientImageBinderWithScale(src, scale);
            clientImgBinder.SetLazyLoaderFunc(LazyImageLoad);
            //if use lazy img load func
            imageContentMan.AddRequestImage(clientImgBinder);
            return clientImgBinder;
        }

        public Image LoadImage(string imgName)
        {
            return LoadBitmap(imgName);
        }


    }


}