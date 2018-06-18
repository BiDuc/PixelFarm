﻿//BSD, 2014-present, WinterDev


//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
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
//#define USE_UNSAFE // no real code for this yet

using System;
namespace PixelFarm.CpuBlit
{
    public static class AggMemMx
    {
        //----------------------------------------------------------filling_rule_e
        public static void memcpy(byte[] dest,
            int destIndex, byte[] source,
            int sourceIndex, int count)
        {
            NaitveMemMx.MemCopy(dest, destIndex, source, sourceIndex, count);
        }
        public static unsafe void memcpy(byte* dest, byte* src, int len)
        {
            NaitveMemMx.MemCopy(dest, src, len);
        }
        public static void memmove(byte[] dest, int destIndex, byte[] source, int sourceIndex, int count)
        {
            if (source != dest
                || destIndex < sourceIndex)
            {
                memcpy(dest, destIndex, source, sourceIndex, count);
            }
            else
            {
                throw new Exception("this code needs to be tested");
            }
        }
        public static unsafe void memmove(byte* dest, int destIndex, byte* source, int sourceIndex, int count)
        {
            if (source != dest
                || destIndex < sourceIndex)
            {
                NaitveMemMx.memcpy(dest + destIndex, source + sourceIndex, count);
                // memcpy(dest, destIndex, source, sourceIndex, Count);
            }
            else
            {
                throw new Exception("this code needs to be tested");
            }
        }
        public static void memset(byte[] dest, int destIndex, byte byteValue, int count)
        {
            NaitveMemMx.MemSet(dest, destIndex, byteValue, count);
        }
        public static void MemClear(Byte[] dest, int destIndex, int count)
        {
            NaitveMemMx.MemSet(dest, destIndex, 0, count);
        }
    }
}