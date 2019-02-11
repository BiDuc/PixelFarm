﻿//MIT, 2019-present, WinterDev

using System;
using System.Collections.Generic;
namespace ExtMsdfgen
{

    public enum AreaKind : byte
    {
        Outside,
        Inside,
        OuterGap
    }
    public struct EdgeStructure
    {
        readonly ShapeCornerArms _shapeCornerArms;
        readonly AreaKind _areaKind;
        readonly bool _isEmpty;
        readonly ExtMsdfgen.EdgeSegment _edgeSegment;
        public EdgeStructure(ShapeCornerArms shapeCornerArms, AreaKind areaKind, ExtMsdfgen.EdgeSegment edgeSegment)
        {
            _isEmpty = false;
            _shapeCornerArms = shapeCornerArms;
            _areaKind = areaKind;
            _edgeSegment = edgeSegment;
        }
        public ExtMsdfgen.EdgeSegment Segment => _edgeSegment;
        public AreaKind AreaKind => _areaKind;
        public bool IsEmpty => _isEmpty;
        public static readonly EdgeStructure Empty = new EdgeStructure();
    }
    public class BmpEdgeLut
    {
        int _w;
        int _h;
        int[] _buffer;
        List<ShapeCornerArms> _cornerArms;
        List<ExtMsdfgen.EdgeSegment> _flattenEdges;
        List<int> _endContours;
        public BmpEdgeLut(List<ShapeCornerArms> cornerArms, List<ExtMsdfgen.EdgeSegment> flattenEdges, List<int> endContours)
        {
            //move first to last

            int startAt = 0;

            for (int i = 0; i < endContours.Count; ++i)
            {
                int nextStartAt = endContours[i];
                //
                ExtMsdfgen.EdgeSegment firstSegment = flattenEdges[startAt];
                ExtMsdfgen.EdgeSegment endSegment = flattenEdges[nextStartAt - 1];
                flattenEdges.RemoveAt(startAt);
                if (i == endContours.Count - 1)
                {
                    flattenEdges.Add(firstSegment);
                }
                else
                {
                    flattenEdges.Insert(nextStartAt - 1, firstSegment);
                }
                startAt = nextStartAt;
            }

            _endContours = endContours;
            _cornerArms = cornerArms;
            _flattenEdges = flattenEdges;

            ConnectExtendedPoints(cornerArms, endContours); //after arrange 
        }
        void ConnectExtendedPoints(List<ExtMsdfgen.ShapeCornerArms> cornerArms, List<int> endContours)
        {
            //test 2 if each edge has unique color 
            int startAt = 0;
            for (int i = 0; i < endContours.Count; ++i)
            {
                int nextStartAt = endContours[i];
                for (int n = startAt + 1; n < nextStartAt; ++n)
                {
                    ExtMsdfgen.ShapeCornerArms c_prev = cornerArms[n - 1];
                    ExtMsdfgen.ShapeCornerArms c_current = cornerArms[n]; 
                    c_prev.leftExtendedPointDest_Outer = c_current.ExtPoint_RightOuter;
                    c_prev.leftExtendedPointDest_Inner = c_current.ExtPoint_RightInner;
                    //
                    c_current.rightExtendedPointDest_Outer = c_prev.ExtPoint_LeftOuter;
                    c_current.rightExtendedPointDest_Inner = c_prev.ExtPoint_LeftInner;
                }

                //last 
                {
                    //the last one
                    ExtMsdfgen.ShapeCornerArms c_prev = cornerArms[nextStartAt - 1];
                    ExtMsdfgen.ShapeCornerArms c_current = cornerArms[startAt]; 
                    c_prev.leftExtendedPointDest_Outer = c_current.ExtPoint_RightOuter;
                    c_prev.leftExtendedPointDest_Inner = c_current.ExtPoint_RightInner;
                    //
                    c_current.rightExtendedPointDest_Outer = c_prev.ExtPoint_LeftOuter;
                    c_current.rightExtendedPointDest_Inner = c_prev.ExtPoint_LeftInner;
                }

                startAt = nextStartAt;//***
            }
        }
        //
        public List<int> EndContours => _endContours;
        //
        public void SetBmpBuffer(int w, int h, int[] buffer)
        {
            _w = w;
            _h = h;
            _buffer = buffer;
        }
        public List<ShapeCornerArms> CornerArms => _cornerArms;

        public int GetPixel(int x, int y) => _buffer[y * _w + x];

        const int WHITE = (255 << 24) | (255 << 16) | (255 << 8) | 255;

        public EdgeStructure GetCornerArm(int x, int y)
        {
            int pixel = _buffer[y * _w + x];
            if (pixel == 0)
            {
                return EdgeStructure.Empty;
            }
            else if (pixel == WHITE)
            {
                return EdgeStructure.Empty;
            }
            else
            {
                //G
                int g = (pixel >> 8) & 0xFF;
                //find index
                int r = pixel & 0xFF;
                int index = (r - 50) / 2;//just our encoding (see ShapeCornerArms.OuterColor, ShapeCornerArms.InnerColor)

                ShapeCornerArms cornerArm = _cornerArms[index];
                EdgeSegment segment = _flattenEdges[index];
                if (g == 50)
                {
                    //outside
                    return new EdgeStructure(cornerArm, AreaKind.Outside, segment);
                }
                else if (g == 25)
                {
                    return new EdgeStructure(cornerArm, AreaKind.OuterGap, segment);
                }
                else
                {
                    //inside
                    return new EdgeStructure(cornerArm, AreaKind.Inside, segment);
                }
            }
        }
    }
    public class Vec2Info
    {
        public double x, y;
        public Vec2PointKind Kind;
        public ExtMsdfgen.EdgeSegment owner;
        public Vec2Info(ExtMsdfgen.EdgeSegment owner)
        {
            this.owner = owner;
        }
    }
    public enum Vec2PointKind
    {
        Touch1,//on curve point
        C2, //quadratic curve control point (off-curve)
        C3, //cubic curve control point (off-curve)
        Touch2, //on curve point
    }
    public class ShapeCornerArms
    {

        /// <summary>
        /// corner number in flatten list
        /// </summary>
        public int CornerNo;

#if  DEBUG
        public int dbugLeftIndex;
        public int dbugMiddleIndex;
        public int dbugRightIndex;
#endif

        public PixelFarm.Drawing.PointF leftPoint;
        public Vec2PointKind LeftPointKind;
        //
        public PixelFarm.Drawing.PointF middlePoint;
        public Vec2PointKind MiddlePointKind;
        public ExtMsdfgen.EdgeSegment MiddlePointEdgeSegment;
        //
        public PixelFarm.Drawing.PointF rightPoint;
        public Vec2PointKind RightPointKind;


        //to other point
        public PixelFarm.Drawing.PointF leftExtendedPointDest_Inner;
        public PixelFarm.Drawing.PointF leftExtendedPointDest_Outer;

        public PixelFarm.Drawing.PointF rightExtendedPointDest_Outer;
        public PixelFarm.Drawing.PointF rightExtendedPointDest_Inner;
        //-----------


        public ShapeCornerArms()
        {

        }
        public PixelFarm.Drawing.Color OuterColor
        {
            get
            {
                float color = (CornerNo * 2) + 50;
                return new PixelFarm.Drawing.Color((byte)color, 50, (byte)color);
            }
        }
        public PixelFarm.Drawing.Color InnerColor
        {
            get
            {
                float color = (CornerNo * 2) + 50;
                return new PixelFarm.Drawing.Color((byte)color, 0, (byte)color);
            }
        }
        public void Offset(float dx, float dy)
        {
            //
            leftPoint.Offset(dx, dy);
            middlePoint.Offset(dx, dy);
            rightPoint.Offset(dx, dy);

            ExtPoint_LeftOuter.Offset(dx, dy);
            ExtPoint_RightOuter.Offset(dx, dy);
            leftExtendedPointDest_Outer.Offset(dx, dy);
            rightExtendedPointDest_Outer.Offset(dx, dy);
            //

            ExtPoint_LeftInner.Offset(dx, dy);
            ExtPoint_RightInner.Offset(dx, dy);
            leftExtendedPointDest_Inner.Offset(dx, dy);
            rightExtendedPointDest_Inner.Offset(dx, dy);
        }


        public bool MiddlePointKindIsTouchPoint => MiddlePointKind == Vec2PointKind.Touch1 || MiddlePointKind == Vec2PointKind.Touch2;
        public bool LeftPointKindIsTouchPoint => LeftPointKind == Vec2PointKind.Touch1 || LeftPointKind == Vec2PointKind.Touch2;
        public bool RightPointKindIsTouchPoint => RightPointKind == Vec2PointKind.Touch1 || RightPointKind == Vec2PointKind.Touch2;
        static double CurrentLen(PixelFarm.Drawing.PointF p0, PixelFarm.Drawing.PointF p1)
        {
            float dx = p1.X - p0.X;
            float dy = p1.Y - p0.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        //-----------
        /// <summary>
        /// extended point of left->middle line
        /// </summary>
        public PixelFarm.Drawing.PointF ExtPoint_LeftOuter => CreateExtendedOuterEdges(leftPoint, middlePoint);
        public PixelFarm.Drawing.PointF ExtPoint_LeftInner => CreateExtendedInnerEdges(leftPoint, middlePoint);
        /// <summary>
        /// extended point of right->middle line
        /// </summary>
        public PixelFarm.Drawing.PointF ExtPoint_RightOuter => CreateExtendedOuterEdges(rightPoint, middlePoint);
        public PixelFarm.Drawing.PointF ExtPoint_RightOuter2 => CreateExtendedOuterEdges(rightPoint, middlePoint, 2);

        public PixelFarm.Drawing.PointF ExtPoint_RightInner => CreateExtendedInnerEdges(rightPoint, middlePoint);


        PixelFarm.Drawing.PointF CreateExtendedOuterEdges(PixelFarm.Drawing.PointF p0, PixelFarm.Drawing.PointF p1, double dlen = 3)
        {

            double rad = Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
            double currentLen = CurrentLen(p0, p1);
            double newLen = currentLen + dlen;

            double new_dx = Math.Cos(rad) * newLen;
            double new_dy = Math.Sin(rad) * newLen;


            return new PixelFarm.Drawing.PointF((float)(p0.X + new_dx), (float)(p0.Y + new_dy));
        }

        PixelFarm.Drawing.PointF CreateExtendedInnerEdges(PixelFarm.Drawing.PointF p0, PixelFarm.Drawing.PointF p1)
        {

            double rad = Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
            double currentLen = CurrentLen(p0, p1);
            if (currentLen - 3 < 0)
            {
                return p0;//***
            }

            double newLen = currentLen - 3;
            double new_dx = Math.Cos(rad) * newLen;
            double new_dy = Math.Sin(rad) * newLen;
            return new PixelFarm.Drawing.PointF((float)(p0.X + new_dx), (float)(p0.Y + new_dy));
        }
        public override string ToString()
        {
            return dbugLeftIndex + "," + dbugMiddleIndex + "," + dbugRightIndex;
        }
    }
}