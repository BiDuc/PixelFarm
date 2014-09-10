﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MatterHackers.Agg;
using MatterHackers.Agg.Image;

namespace MatterHackers.Agg.ImageProcessing
{
    static public class Dilate
    {
        public static void DoDilate3x3Binary(ImageBuffer sourceAndDest, int threshold)
        {
            ImageBuffer temp = new ImageBuffer(sourceAndDest);
            DoDilate3x3Binary(temp, sourceAndDest, threshold);
        }

        public static void DoDilate3x3Binary(ImageBuffer source, ImageBuffer dest, int threshold)
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

            for (int testY = 1; testY < height-1; testY++)
            {
                for (int testX = 1; testX < width - 1; testX++)
                {
                    for (int sourceY = -1; sourceY <= 1; sourceY++)
                    {
                        for (int sourceX = -1; sourceX <= 1; sourceX++)
                        {
                            int sourceOffset = source.GetBufferOffsetXY(testX + sourceX, testY + sourceY);
                            if (sourceBuffer[sourceOffset] > threshold)
                            {
                                int destOffset = dest.GetBufferOffsetXY(testX, testY);
                                destBuffer[destOffset++] = 255;
                                destBuffer[destOffset++] = 255;
                                destBuffer[destOffset++] = 255;
                                destBuffer[destOffset++] = 255;
                            }
                        }
                    }
                }
            }
        }

        public static void DoDilate3x3MaxValue(ImageBuffer sourceAndDest)
        {
            ImageBuffer temp = new ImageBuffer(sourceAndDest);
            DoDilate3x3MaxValue(temp, sourceAndDest);
        }

        public static void DoDilate3x3MaxValue(ImageBuffer source, ImageBuffer dest)
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
                    RGBA_Bytes maxColor = RGBA_Bytes.Black;
                    int sourceOffset = source.GetBufferOffsetXY(testX, testY -1);

                    // x-1, y-1
                    //maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset - 4);
                    // x0, y-1
                    maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + 0);
                    // x1, y-1
                    //maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + 4);

                    // x-1, y0
                    maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + sourceStrideInBytes - 4);
                    // x0, y0
                    maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + sourceStrideInBytes + 0);
                    // x+1, y0
                    maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + sourceStrideInBytes + 4);

                    // x-1, y+1
                    //maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + sourceStrideInBytes * 2 - 4);
                    // x0, y+1
                    maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + sourceStrideInBytes * 2 + 0);
                    // x+1, y+1
                    //maxColor = MaxColor(sourceBuffer, maxColor, sourceOffset + sourceStrideInBytes * 2 + 4);

                    int destOffset = dest.GetBufferOffsetXY(testX, testY);
                    destBuffer[destOffset + 2] = maxColor.red;
                    destBuffer[destOffset + 1] = maxColor.green;
                    destBuffer[destOffset + 0] = maxColor.blue;
                    destBuffer[destOffset + 3] = 255;
                }
            }
        }

        private static RGBA_Bytes MaxColor(byte[] sourceBuffer, RGBA_Bytes maxColor, int sourceOffset)
        {
            maxColor.red = Math.Max(maxColor.red, sourceBuffer[sourceOffset + 2]);
            maxColor.green = Math.Max(maxColor.green, sourceBuffer[sourceOffset + 1]);
            maxColor.blue = Math.Max(maxColor.blue, sourceBuffer[sourceOffset + 0]);
            return maxColor;
        }
    }


}
