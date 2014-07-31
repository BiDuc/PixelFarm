/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

using System;
using System.Collections.Generic;
using System.Text;

using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.VectorMath;

namespace MatterHackers.RayTracer
{
    public class Camera
    {
        double cameraFOV = MathHelper.DegreesToRadians(56);
        double distanceToCameraPlane;

        public Matrix4X4 axisToWorld;

        public int widthInPixels;
        public int heightInPixels;

        public Camera(int widthInPixels, int heightInPixels, double fieldOfViewRad)
        {
            if (fieldOfViewRad > 3.14)
            {
                throw new Exception("You need to give the Field of View in radians.");
            }
            cameraFOV = fieldOfViewRad;
            double sin = Math.Sin(cameraFOV / 2);
            distanceToCameraPlane = Math.Cos(cameraFOV / 2) / sin;

            this.widthInPixels = widthInPixels;
            this.heightInPixels = heightInPixels;
        }

        public Camera(Camera cameraToCopy)
        {
            this.cameraFOV = cameraToCopy.cameraFOV;
            this.distanceToCameraPlane = cameraToCopy.distanceToCameraPlane;

            this.axisToWorld = cameraToCopy.axisToWorld;

            this.widthInPixels = cameraToCopy.widthInPixels;
            this.heightInPixels = cameraToCopy.heightInPixels;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public Vector3 Origin
        {
            get
            {
                return new Vector3(axisToWorld[3, 0], axisToWorld[3, 1], axisToWorld[3, 2]);
            }

            set
            {
                axisToWorld[3, 0] = value.x;
                axisToWorld[3, 1] = value.y;
                axisToWorld[3, 2] = value.z;
            }
        }

        Vector3 GetDirectionMinus1To1(double screenX, double screenY)
        {
            Vector3 direction = new Vector3();
            double oneOverScale = 1.0 / (widthInPixels / 2.0);
            double x = screenX - widthInPixels / 2.0;
            double y = screenY - heightInPixels / 2.0;
            x *= oneOverScale;
            y *= oneOverScale;
            direction.x = x;
            direction.y = y;
            direction.z = -distanceToCameraPlane;

            direction.Normalize();

            return direction;
        }

        public Ray GetRay(double screenX, double screenY)
        {
            Vector3 origin = Origin;
            Vector3 direction = GetDirectionMinus1To1(screenX, screenY);

            direction = Vector3.TransformVector(direction, axisToWorld);

            Ray ray = new Ray(origin, direction);
            return ray;
        }

        #region Equality Functions
        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Camera left, Camera right)
        {
            if ((object)left == null)
            {
                if ((object)right == null)
                {
                    return true;
                }

                return false;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Camera left, Camera right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Camera))
                return false;

            return this.Equals((Camera)obj);
        }

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="other">An matrix to compare with this matrix.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public bool Equals(Camera other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return cameraFOV == other.cameraFOV
            && distanceToCameraPlane == other.distanceToCameraPlane
            && axisToWorld == other.axisToWorld
            && widthInPixels == other.widthInPixels
            && heightInPixels == other.heightInPixels;
        }

        #endregion
    }
}
