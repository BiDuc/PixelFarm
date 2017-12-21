﻿//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx

using PixelFarm.DrawingBuffer;
namespace WinFormGdiPlus
{
    public class Particle
    {

        public PointD Position;
        public PointD Velocity;
        public ColorInt Color;
        public double Lifespan;
        public double Elapsed;



        public void Initiailize()
        {
            Elapsed = 0;
        }

        public void Update(double elapsedSeconds)
        {
            Elapsed += elapsedSeconds;
            if (Elapsed > Lifespan)
            {

                Color = PixelFarm.DrawingBuffer.ColorInt.CreateNew(Color, 0);
                return;
            }
     
            Color = PixelFarm.DrawingBuffer.ColorInt.CreateNew(Color, (byte)(255 - ((255 * Elapsed)) / Lifespan));
            Position.X += Velocity.X * elapsedSeconds;
            Position.Y += Velocity.Y * elapsedSeconds;
        }

    }

}