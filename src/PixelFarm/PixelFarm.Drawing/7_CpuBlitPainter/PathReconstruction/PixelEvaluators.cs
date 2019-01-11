﻿//MIT, 2014-present, WinterDev
//MatterHackers 
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------



using PixelFarm.Drawing;
using PixelFarm.CpuBlit;


namespace PixelFarm.PathReconstruction
{


    public abstract class PixelEvaluatorBitmap32 : IPixelEvaluator
    {
        /// <summary>
        /// address to head of the source of _bmpSrc
        /// </summary>
        unsafe int* _destBuffer;
        unsafe int* _currentAddr;

        int _srcW;
        /// <summary>
        /// width -1
        /// </summary>
        int _rightLim;
        int _srcH;
        int _curX;
        int _curY;
        int _bufferOffset;

        /// <summary>
        /// start move to at x
        /// </summary>
        int _moveToX;
        /// <summary>
        /// start move to at y
        /// </summary>
        int _moveToY;

        protected abstract unsafe bool CheckPixel(int* pixelAddr);
        protected abstract unsafe void SetStartColor(int* pixelAddr);

        public void SetSourceBitmap(MemBitmap bmpSrc)
        {
            ((IPixelEvaluator)this).SetSourceDimension(bmpSrc.Width, bmpSrc.Height);
            var memPtr = MemBitmap.GetBufferPtr(bmpSrc);
            unsafe
            {
                _currentAddr = _destBuffer = (int*)memPtr.Ptr;
            }
        }
        public void ReleaseSourceBitmap()
        {

        }
        void InternalMoveTo(int x, int y)
        {
            _moveToX = _curX = x;
            _moveToY = _curY = y;
            unsafe
            {
                //assign _bufferOffset too!!! 
                _currentAddr = _destBuffer + (_bufferOffset = (y * _srcW) + x);
            }
        }
        //------------------------------


        int IPixelEvaluator.BufferOffset => _bufferOffset;
        int IPixelEvaluator.X => _curX;
        int IPixelEvaluator.Y => _curY;
        int IPixelEvaluator.OrgBitmapWidth => _srcW;
        int IPixelEvaluator.OrgBitmapHeight => _srcH;
        /// <summary>
        /// set init pos, collect init check data
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void IPixelEvaluator.SetStartPos(int x, int y)
        {
            InternalMoveTo(x, y);//*** 
            unsafe
            {
                SetStartColor(_currentAddr);
            }
        }

        /// <summary>
        /// move evaluaion point to 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void IPixelEvaluator.MoveTo(int x, int y) => InternalMoveTo(x, y);
        void IPixelEvaluator.RestoreMoveToPos() => InternalMoveTo(_moveToX, _moveToY);

        /// <summary>
        /// move check position to right side 1 px and check , if not pass, return back to prev pos
        /// </summary>
        /// <returns>true if pass condition</returns>
        bool IPixelEvaluator.ReadNext()
        {
            //append right pos 1 step
            unsafe
            {
                if (_curX < _rightLim)
                {
                    _curX++;
                    _bufferOffset++;
                    _currentAddr++;
                    if (!CheckPixel(_currentAddr))
                    {
                        //if not pass check => move back to prev pos
                        _curX--;
                        _bufferOffset--;
                        _currentAddr--;
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// move check position to left side 1 px, and check, if not pass, return back to prev pos
        /// </summary>
        /// <returns>true if pass condition</returns>
        bool IPixelEvaluator.ReadPrev()
        {
            unsafe
            {
                if (_curX > 0)
                {
                    _curX--;
                    _bufferOffset--;
                    _currentAddr--;
                    if (!CheckPixel(_currentAddr))
                    {
                        //if not pass check => move back to prev pos
                        _curX++;
                        _bufferOffset++;
                        _currentAddr++;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// read current and check
        /// </summary>
        /// <returns></returns>
        bool IPixelEvaluator.Read()
        {
            //check current pos
            unsafe
            {
                return CheckPixel(_currentAddr);
            }
        }

        void IPixelEvaluator.SetSourceDimension(int width, int height)
        {
            _srcW = width;
            _srcH = height;
            _rightLim = _srcW - 1;
        }
    }

    public class ExactMatch : PixelEvaluatorBitmap32
    {
        int _startColorInt32;
        public ExactMatch()
        {

        }
        protected override unsafe void SetStartColor(int* colorAddr)
        {
            _startColorInt32 = *colorAddr;
        }
        protected override unsafe bool CheckPixel(int* pixelAddr)
        {
            //ARGB
            return (_startColorInt32 == *pixelAddr);
        }
    }

    public class ToleranceMatch : PixelEvaluatorBitmap32
    {
        byte _tolerance0To255;
        //** only RGB?
        byte _red_min, _red_max;
        byte _green_min, _green_max;
        byte _blue_min, _blue_max;
        public ToleranceMatch(byte initTolerance)
        {
            _tolerance0To255 = initTolerance;
        }
        public byte Tolerance
        {
            get => _tolerance0To255;
            set => _tolerance0To255 = value;
        }

        static byte Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        protected override unsafe void SetStartColor(int* colorAddr)
        {
            int pixelValue32 = *colorAddr;

            int r = (pixelValue32 >> CO.R_SHIFT) & 0xff;
            int g = (pixelValue32 >> CO.G_SHIFT) & 0xff;
            int b = (pixelValue32 >> CO.B_SHIFT) & 0xff;

            _red_min = Clamp(r - _tolerance0To255);
            _red_max = Clamp(r + _tolerance0To255);
            //
            _green_min = Clamp(g - _tolerance0To255);
            _green_max = Clamp(g + _tolerance0To255);
            //
            _blue_min = Clamp(b - _tolerance0To255);
            _blue_max = Clamp(b + _tolerance0To255);

        }
        protected override unsafe bool CheckPixel(int* pixelAddr)
        {
            int pixelValue32 = *pixelAddr;
            int r = (pixelValue32 >> CO.R_SHIFT) & 0xff;
            int g = (pixelValue32 >> CO.G_SHIFT) & 0xff;
            int b = (pixelValue32 >> CO.B_SHIFT) & 0xff;
            //range test
            return ((r >= _red_min) && (r <= _red_max) &&
                   (g >= _green_min) && (g <= _green_max) &&
                   (b >= _blue_min) && (b <= _blue_max));
        }
    }
}