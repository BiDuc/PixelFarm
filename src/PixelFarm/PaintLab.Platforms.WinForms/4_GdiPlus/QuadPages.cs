﻿//MIT, 2014-present, WinterDev

using System;
using LayoutFarm;
namespace PixelFarm.Drawing.WinGdi
{
    class QuadPages
    {
        GdiPlusDrawBoard _pageA;
        //CanvasCollection physicalCanvasCollection;

        public QuadPages(
            int cachedPageNum,
            int eachCachedPageWidth,
            int eachCachedPageHeight)
        {

            _pageA = new GdiPlusDrawBoard(0, 0, eachCachedPageWidth, eachCachedPageHeight);

            //physicalCanvasCollection = new CanvasCollection(
            //    cachedPageNum,
            //    eachCachedPageWidth,
            //    eachCachedPageHeight);
        }

        public void Dispose()
        {
            //if (physicalCanvasCollection != null)
            //{
            //    physicalCanvasCollection.Dispose();
            //    physicalCanvasCollection = null;
            //}
        }
        public void CanvasInvalidate(Rectangle rect)
        {
            Rectangle r = rect;
            if (_pageA != null && _pageA.IntersectsWith(r))
            {
                _pageA.Invalidate(r);
            }
        }
        public bool IsValid
        {
            get
            {
                if (_pageA != null)
                {
                    if (!_pageA.IsContentReady)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void RenderToOutputWindowFullMode(
            IRenderElement topWindowRenderBox,
            IntPtr destOutputHdc,
            int viewportX, int viewportY, int viewportWidth, int viewportHeight)
        {

            if (_pageA != null && !_pageA.IsContentReady)
            {
                UpdateAllArea(_pageA, topWindowRenderBox);
            }
            _pageA.RenderTo(destOutputHdc, viewportX - _pageA.Left,
                      viewportY - _pageA.Top,
                          new Rectangle(0, 0,
                          viewportWidth,
                          viewportHeight));

        }

        static void UpdateAllArea(GdiPlusDrawBoard mycanvas, IRenderElement topWindowRenderBox)
        {
            mycanvas.OffsetCanvasOrigin(-mycanvas.Left, -mycanvas.Top);
            Rectangle rect = mycanvas.Rect;
            topWindowRenderBox.DrawToThisCanvas(mycanvas, rect);
#if DEBUG
            topWindowRenderBox.dbugShowRenderPart(mycanvas, rect);
#endif

            mycanvas.IsContentReady = true;
            mycanvas.OffsetCanvasOrigin(mycanvas.Left, mycanvas.Top);
        }


        static void UpdateInvalidArea(GdiPlusDrawBoard mycanvas, IRenderElement rootElement)
        {
            mycanvas.OffsetCanvasOrigin(-mycanvas.Left, -mycanvas.Top);
            Rectangle rect = mycanvas.InvalidateArea;
            rootElement.DrawToThisCanvas(mycanvas, rect);
#if DEBUG
            rootElement.dbugShowRenderPart(mycanvas, rect);
#endif

            mycanvas.IsContentReady = true;
            mycanvas.OffsetCanvasOrigin(mycanvas.Left, mycanvas.Top);
        }


        public void RenderToOutputWindowPartialMode(
            IRenderElement renderE,
            IntPtr destOutputHdc,
            int viewportX, int viewportY,
            int viewportWidth, int viewportHeight)
        {
            if (!_pageA.IsContentReady)
            {
                UpdateInvalidArea(_pageA, renderE);
            }

            Rectangle invalidateArea = _pageA.InvalidateArea;

            _pageA.RenderTo(destOutputHdc, invalidateArea.Left - _pageA.Left, invalidateArea.Top - _pageA.Top,
                new Rectangle(invalidateArea.Left -
                    viewportX, invalidateArea.Top - viewportY,
                    invalidateArea.Width, invalidateArea.Height));
            _pageA.ResetInvalidateArea();


        }
        public void CalculateCanvasPages(int viewportX, int viewportY, int viewportWidth, int viewportHeight)
        {
            //int firstVerticalPageNum = viewportY / physicalCanvasCollection.EachPageHeight;
            //int firstHorizontalPageNum = viewportX / physicalCanvasCollection.EachPageWidth;
            ////render_parts = PAGE_A;
            //if (_pageA == null)
            //{
            //    _pageA = physicalCanvasCollection.GetCanvasPage(0, 0);
            //}
            //else
            //{
            //    if (!pageA.IsPageNumber(firstHorizontalPageNum, firstVerticalPageNum))
            //    {
            //        physicalCanvasCollection.ReleasePage(pageA);
            //        pageA = physicalCanvasCollection.GetCanvasPage(firstHorizontalPageNum, firstVerticalPageNum);
            //    }
            //}

        }
        public void ResizeAllPages(int newWidth, int newHeight)
        {
            //physicalCanvasCollection.Dispose();
            //physicalCanvasCollection.ResizeAllPages(newWidth, newHeight);
            //if (_pageA != null)
            //{
            //    _pageA.IsUnused = true;
            //    _pageA = null;
            //}
        }
    }
}