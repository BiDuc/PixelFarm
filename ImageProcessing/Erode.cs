﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MatterHackers.Agg.Image;
using MatterHackers.Agg.ImageProcessing;

namespace MatterHackers.Agg.ImageProcessing
{
    public class Erode
    {
        public static void DoErode3x3Binary(ImageBuffer sourceAndDest, int threshold)
        {
            ImageBuffer temp = new ImageBuffer(sourceAndDest);
            DoErode3x3Binary(temp, sourceAndDest, threshold);
        }

        public static void DoErode3x3Binary(ImageBuffer source, ImageBuffer dest, int threshold)
        {
            if (source.BitDepth != 32 || dest.BitDepth != 32)
            {
                throw new NotImplementedException("We only work with 32 bit at the moment.");
            }

            if (source.Width != dest.Width || source.Height != dest.Height)
            {
                throw new NotImplementedException("Source and Dest have to be the same size");
            }

            int height = source.Height;
            int width = source.Width;
            int sourceStrideInBytes = source.StrideInBytes();
            int destStrideInBytes = dest.StrideInBytes();
            byte[] sourceBuffer = source.GetBuffer();
            byte[] destBuffer = dest.GetBuffer();

            for (int testY = 1; testY < height - 1; testY++)
            {
                for (int testX = 1; testX < width - 1; testX++)
                {
                    for (int sourceY = -1; sourceY <= 1; sourceY++)
                    {
                        for (int sourceX = -1; sourceX <= 1; sourceX++)
                        {
                            int sourceOffset = source.GetBufferOffsetXY(testX + sourceX, testY + sourceY);
                            if (sourceBuffer[sourceOffset] < threshold)
                            {
                                int destOffset = dest.GetBufferOffsetXY(testX, testY);
                                destBuffer[destOffset++] = 0;
                                destBuffer[destOffset++] = 0;
                                destBuffer[destOffset++] = 0;
                                destBuffer[destOffset++] = 255;
                            }
                        }
                    }
                }
            }
        }

        public static void DoErode3x3MinValue(ImageBuffer sourceAndDest)
        {
            ImageBuffer temp = new ImageBuffer(sourceAndDest);
            DoErode3x3MinValue(temp, sourceAndDest);
        }

        public static void DoErode3x3MinValue(ImageBuffer source, ImageBuffer dest)
        {
            if (source.BitDepth != 32 || dest.BitDepth != 32)
            {
                throw new NotImplementedException("We only work with 32 bit at the moment.");
            }

            if (source.Width != dest.Width || source.Height != dest.Height)
            {
                throw new NotImplementedException("Source and Dest have to be the same size");
            }

            int height = source.Height;
            int width = source.Width;
            int sourceStrideInBytes = source.StrideInBytes();
            int destStrideInBytes = dest.StrideInBytes();
            byte[] sourceBuffer = source.GetBuffer();
            byte[] destBuffer = dest.GetBuffer();

            // This can be made much faster by holding the buffer pointer and offsets better // LBB 2013 06 09
            for (int testY = 1; testY < height - 1; testY++)
            {
                for (int testX = 1; testX < width - 1; testX++)
                {
                    RGBA_Bytes minColor = RGBA_Bytes.White;
                    int sourceOffset = source.GetBufferOffsetXY(testX, testY -1);

                    // x-1, y-1
                    //minColor = MinColor(sourceBuffer, minColor, sourceOffset - 4);
                    // x0, y-1
                    minColor = MinColor(sourceBuffer, minColor, sourceOffset + 0);
                    // x1, y-1
                    //minColor = MinColor(sourceBuffer, minColor, sourceOffset + 4);

                    // x-1, y0
                    minColor = MinColor(sourceBuffer, minColor, sourceOffset + sourceStrideInBytes - 4);
                    // x0, y0
                    minColor = MinColor(sourceBuffer, minColor, sourceOffset + sourceStrideInBytes + 0);
                    // x+1, y0
                    minColor = MinColor(sourceBuffer, minColor, sourceOffset + sourceStrideInBytes + 4);

                    // x-1, y+1
                    //minColor = MinColor(sourceBuffer, minColor, sourceOffset + sourceStrideInBytes * 2 - 4);
                    // x0, y+1
                    minColor = MinColor(sourceBuffer, minColor, sourceOffset + sourceStrideInBytes * 2 + 0);
                    // x+1, y+1
                    //minColor = MinColor(sourceBuffer, minColor, sourceOffset + sourceStrideInBytes * 2 + 4);

                    int destOffset = dest.GetBufferOffsetXY(testX, testY);
                    destBuffer[destOffset + 2] = minColor.red;
                    destBuffer[destOffset + 1] = minColor.green;
                    destBuffer[destOffset + 0] = minColor.blue;
                    destBuffer[destOffset + 3] = 255;
                }
            }
        }

        private static RGBA_Bytes MinColor(byte[] sourceBuffer, RGBA_Bytes minColor, int sourceOffset)
        {
            minColor.red = Math.Min(minColor.red, sourceBuffer[sourceOffset + 2]);
            minColor.green = Math.Min(minColor.green, sourceBuffer[sourceOffset + 1]);
            minColor.blue = Math.Min(minColor.blue, sourceBuffer[sourceOffset + 0]);
            return minColor;
        }
    }
}
