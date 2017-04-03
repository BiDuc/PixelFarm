﻿//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using Poly2Tri;
using Typography.OpenFont;
namespace Typography.Rendering
{

    public class GlyphFitOutline
    {
        //this class store result of poly2tri

        Polygon _polygon;
        List<GlyphTriangle> _triangles = new List<GlyphTriangle>();
        List<GlyphBone> bones;
        List<GlyphBone> boneList2;

        public GlyphFitOutline(Polygon polygon, List<GlyphContour> contours)
        {
            this.Contours = contours;
            this._polygon = polygon;
            foreach (DelaunayTriangle tri in polygon.Triangles)
            {
                tri.MarkAsActualTriangle();
                _triangles.Add(new GlyphTriangle(tri));
            }
        }
        internal List<GlyphContour> Contours { get; set; }

        public void Analyze()
        {
            //we analyze each triangle here 
            int j = _triangles.Count;
            bones = new List<GlyphBone>();
            List<GlyphTriangle> usedTriList = new List<GlyphTriangle>();
            for (int i = 0; i < j; ++i)
            {
                GlyphTriangle tri = _triangles[i];
                if (i > 0)
                {
                    //check the new tri is connected with latest tri or not?
                    int foundIndex = FindLatestConnectedTri(usedTriList, tri);
                    if (foundIndex > -1)
                    {
                        usedTriList.Add(tri);
                        bones.Add(new GlyphBone(usedTriList[foundIndex], tri));
                    }
                    else
                    {
                        //not found
                        //?

                    }
                }
                else
                {
                    usedTriList.Add(tri);
                }
            }

            if (j > 1)
            {
                //connect the last tri to the first tri
                //if it is connected
                GlyphTriangle firstTri = _triangles[0];
                GlyphTriangle lastTri = _triangles[j - 1];
                if (firstTri.IsConnectedWith(lastTri))
                {
                    bones.Add(new GlyphBone(lastTri, firstTri));
                }
            }
            //----------------------------------------
            int boneCount = bones.Count;
            //do bone length histogram
            boneList2 = new List<GlyphBone>(boneCount);
            boneList2.AddRange(bones);
            //----------------------------------------
            AnalyzeBoneLength();

            //----------------------------------------
            for (int i = 0; i < boneCount; ++i)
            {
                //each bone has 2 triangles at its ends
                //we analyze both triangles' roles
                //eg...
                //left -right 
                //top-bottom
                GlyphBone bone = bones[i];
                bone.Analyze();
            }
            //---------------------------------------- 
        }
        void AnalyzeBoneLength()
        {
            //sort by bone len
            int j = boneList2.Count;
            boneList2.Sort((b0, b1) => b0.boneLength.CompareTo(b1.boneLength));

            ////find length of the 1st percentile
            ////avg 
            //double total = 0;
            //for (int i = j - 1; i >= 0; --i)
            //{
            //    total += boneList2[i].boneLength;
            //}
            //double avg = total / j;
            //we use 

            if (j >= 10)
            {
                //find 1st 10
                int group_n = j / 10;
                double total = 0;
                int index = j - 1;
                for (int i = group_n - 1; i >= 0; --i)
                {
                    //avg of first group
                    total += boneList2[index].boneLength;
                    index--;
                }
                //
                double maxgroup_avg = total / group_n;

                int mid = (j - 1) / 2;
                double median = boneList2[mid].boneLength;
                //assign long bone
                double median_x2 = median + median;
                //
                for (int i = j - 1; i >= 0; --i)
                {
                    GlyphBone bone = boneList2[i];
                    if (bone.boneLength > median_x2)
                    {
                        bone.IsLongBone = true;
                    }
                }

            }
            else
            {

            }

        }
        int FindLatestConnectedTri(List<GlyphTriangle> usedTriList, GlyphTriangle tri)
        {
            //search back ***
            for (int i = usedTriList.Count - 1; i >= 0; --i)
            {
                GlyphTriangle t = usedTriList[i];
                if (t.IsConnectedWith(tri))
                {
                    return i;
                }
            }
            return -1;
        }
#if DEBUG
        public List<GlyphTriangle> dbugGetTriangles()
        {
            return _triangles;
        }
        public List<GlyphBone> dbugGetBones()
        {
            return bones;
        }
#endif
    }


    public static class GlyphFitOutlineExtensions
    {
        /// <summary>
        /// read fitting output
        /// </summary>
        /// <param name="tx">glyph translator</param>
        public static void ReadOutput(this GlyphFitOutline glyphOutline, IGlyphTranslator tx, float pxScale)
        {
            if (glyphOutline == null) return;
            //
            //-----------------------------------------------------------            
            //create fit contour
            //this version use only Agg's vertical hint only ****
            //(ONLY vertical fitting , NOT apply horizontal fit)
            //-----------------------------------------------------------     
            //create outline
            //then create     
            List<GlyphContour> contours = glyphOutline.Contours;
            int j = contours.Count;
            tx.BeginRead(j);
            for (int i = 0; i < j; ++i)
            {
                //new contour
                CreateFitShape(tx, contours[i], pxScale, false, true, false);
                tx.CloseContour();
            }
            tx.EndRead();
        }
        const int GRID_SIZE = 1;
        const float GRID_SIZE_25 = 1f / 4f;
        const float GRID_SIZE_50 = 2f / 4f;
        const float GRID_SIZE_75 = 3f / 4f;

        const float GRID_SIZE_33 = 1f / 3f;
        const float GRID_SIZE_66 = 2f / 3f;


        static float RoundToNearestVerticalSide(float org, bool useHalfPixel)
        {
            float actual1 = org;
            float integer1 = (int)(actual1);//floor 
            float remaining = actual1 - integer1;
            if (useHalfPixel)
            {
                if (remaining > GRID_SIZE_66)
                {
                    return (integer1 + 1f);
                }
                else if (remaining > (GRID_SIZE_33))
                {
                    return (integer1 + 0.5f);
                }
                else
                {
                    return integer1;
                }
            }
            else
            {
                if (remaining > GRID_SIZE_50)
                {
                    return (integer1 + 1f);
                }
                else
                {
                    return integer1;
                }
            }
        }
        static float RoundToNearestHorizontalSide(float org)
        {
            float actual1 = org;
            float integer1 = (int)(actual1);//lower
            float floatModulo = actual1 - integer1;

            if (floatModulo >= (GRID_SIZE_50))
            {
                return (integer1 + 1);
            }
            else
            {
                return integer1;
            }
        }
        static void CreateFitShape(IGlyphTranslator tx,
            GlyphContour contour,
            float pixelScale,
            bool x_axis,
            bool y_axis,
            bool useHalfPixel)
        {
            List<GlyphPoint2D> mergePoints = contour.mergedPoints;
            int j = mergePoints.Count;
            //merge 0 = start
            double prev_px = 0;
            double prev_py = 0;
            double p_x = 0;
            double p_y = 0;
            double first_px = 0;
            double first_py = 0;

            {
                GlyphPoint2D p = mergePoints[0];
                p_x = p.x * pixelScale;
                p_y = p.y * pixelScale;

                if (y_axis && p.isPartOfHorizontalEdge && p.isUpperSide && p_y > 3) //TODO: review here
                {
                    //vertical fitting
                    //fit p_y to grid
                    p_y = RoundToNearestVerticalSide((float)p_y, useHalfPixel);
                }

                if (x_axis && p.IsPartOfVerticalEdge && p.IsLeftSide)
                {
                    float new_x = RoundToNearestHorizontalSide((float)p_x);
                    //adjust right-side vertical edge
                    EdgeLine rightside = p.GetMatchingVerticalEdge();
                    if (rightside != null)
                    {

                    }
                    p_x = new_x;
                }
                tx.MoveTo((float)p_x, (float)p_y);
                //-------------
                first_px = prev_px = p_x;
                first_py = prev_py = p_y;
            }

            for (int i = 1; i < j; ++i)
            {
                //all merge point is polygon point
                GlyphPoint2D p = mergePoints[i];
                p_x = p.x * pixelScale;
                p_y = p.y * pixelScale;

                if (y_axis && p.isPartOfHorizontalEdge && p.isUpperSide && p_y > 3)
                {
                    //vertical fitting
                    //fit p_y to grid
                    p_y = RoundToNearestVerticalSide((float)p_y, useHalfPixel);
                }

                if (x_axis && p.IsPartOfVerticalEdge && p.IsLeftSide)
                {
                    //horizontal fitting
                    //fix p_x to grid
                    float new_x = RoundToNearestHorizontalSide((float)p_x);
                    ////adjust right-side vertical edge
                    //PixelFarm.Agg.Typography.EdgeLine rightside = p.GetMatchingVerticalEdge();
                    //if (rightside != null && !rightside.IsLeftSide && rightside.IsOutside)
                    //{
                    //    var rightSideP = rightside.p.userData as GlyphPoint2D;
                    //    var rightSideQ = rightside.q.userData as GlyphPoint2D;
                    //    //find move diff
                    //    float movediff = (float)p_x - new_x;
                    //    //adjust right side edge
                    //    rightSideP.x = rightSideP.x + movediff;
                    //    rightSideQ.x = rightSideQ.x + movediff;
                    //}
                    p_x = new_x;
                }
                //                 
                tx.LineTo((float)p_x, (float)p_y);
                //
                prev_px = p_x;
                prev_py = p_y;
            }

            tx.LineTo((float)first_px, (float)first_py);
        }
    }


    static class MyMath
    {

        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees">An angle in degrees</param>
        /// <returns>The angle expressed in radians</returns>
        public static double DegreesToRadians(double degrees)
        {
            const double degToRad = System.Math.PI / 180.0f;
            return degrees * degToRad;
        }

    }

    public enum BoneDirection : byte
    {
        /// <summary>
        /// 0 degree direction (horizontal left to right)
        /// </summary>
        D0,
        D45,
        D90,
        D135,
        D180,
        D225,
        D270,
        D315
    }
    /// <summary>
    /// a line that connects between centroid of 2 GlyphTriangle(p => q)
    /// </summary>
    public class GlyphBone
    {
        public readonly GlyphTriangle p, q;
        public readonly double boneLength;

        public GlyphBone(GlyphTriangle p, GlyphTriangle q)
        {
            this.p = p;
            this.q = q;

            double dy = q.CentroidY - p.CentroidY;
            double dx = q.CentroidX - p.CentroidX;
            this.boneLength = Math.Sqrt(
                (dy * dy) + (dx * dx)
                );
        } 
        public double SlopAngle { get; private set; }
        public bool IsLongBone { get; set; }

        public LineSlopeKind SlopKind { get; private set; }

        static void CalculateMidPoint(EdgeLine e, out double midX, out double midY)
        {
            midX = (e.x0 + e.x1) / 2;
            midY = (e.y0 + e.y1) / 2;
        }
        
        public void Analyze()
        {

            //
            //p => (x0,y0)
            //q => (x1,y1)
            //line move from p to q 
            //...
            //tasks:
            //1. find slop angle
            //2. find slope kind



            //check if q is upper or lower when compare with p
            //check if q is on left side or right side of p
            //then we know the direction
            //....
            //p
            double x0 = p.CentroidX;
            double y0 = p.CentroidY;
            //q
            double x1 = q.CentroidX;
            double y1 = q.CentroidY;

            if (x1 == x0)
            {
                this.SlopKind = LineSlopeKind.Vertical;
                SlopAngle = 1;
            }
            else
            {
                SlopAngle = Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
                if (SlopAngle > _85degreeToRad)
                {
                    SlopKind = LineSlopeKind.Vertical;
                }
                else if (SlopAngle < _15degreeToRad)
                {
                    SlopKind = LineSlopeKind.Horizontal;
                }
                else
                {
                    SlopKind = LineSlopeKind.Other;
                }
            }
            //--------------------------------------
            //for p and q, count number of outside edge
            //if outsideEdgeCount of triangle >=2 -> this triangle is tip part

            int p_outsideEdgeCount = OutSideEdgeCount(p);
            int q_outsideEdgeCount = OutSideEdgeCount(q);
            bool p_isTip = false;
            bool q_isTip = false;

            if (p_outsideEdgeCount >= 2)
            {
                //tip bone
                p_isTip = true;
            }
            if (q_outsideEdgeCount >= 2)
            {
                //tipbone
                q_isTip = true;
            }
            //-------------------------------------- 
            //p_isTip && q_isTip is possible eg. dot or dot of  i etc.
            //-------------------------------------- 
            //find matching side:
            //the bone connects between triangle p and q (via centroid)
            //
            if (p.e0.IsOutside)
            {
                //find matching side on q
                MarkMatchingEdge(p.e0, q);
            }
            if (p.e1.IsOutside)
            {
                //find matching side on q   
                MarkMatchingEdge(p.e1, q);
            }
            if (p.e2.IsOutside)
            {
                //find matching side on q
                MarkMatchingEdge(p.e2, q);
            }


            if (q.e0.IsOutside)
            {
                //find matching side on q
                MarkMatchingEdge(q.e0, p);
            }
            if (q.e1.IsOutside)
            {
                //find matching side on q   
                MarkMatchingEdge(q.e1, p);
            }
            if (q.e2.IsOutside)
            {
                //find matching side on q
                MarkMatchingEdge(q.e2, p);
            }
        }
        static void MarkMatchingEdge(EdgeLine targetEdge, GlyphTriangle q)
        {

            EdgeLine matchingEdgeLine;
            int matchingEdgeSideNo;
            if (FindMatchingOuterSide(targetEdge, q, out matchingEdgeLine, out matchingEdgeSideNo))
            {
                //assign matching edge line   
                //mid point of each edge
                //p-triangle's edge midX,midY
                double pe_midX, pe_midY;
                CalculateMidPoint(targetEdge, out pe_midX, out pe_midY);
                //q-triangle's edge midX,midY
                double qe_midX, qe_midY;
                CalculateMidPoint(matchingEdgeLine, out qe_midX, out qe_midY);

                if (targetEdge.SlopKind == LineSlopeKind.Vertical)
                {
                    //TODO: review same side edge (Fan shape)
                    if (pe_midX < qe_midX)
                    {
                        targetEdge.IsLeftSide = true;
                        if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Vertical)
                        {
                            targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                        }
                    }
                    else
                    {
                        //matchingEdgeLine.IsLeftSide = true;
                        if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Vertical)
                        {
                            targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                        }
                    }
                }
                else if (targetEdge.SlopKind == LineSlopeKind.Horizontal)
                {
                    //TODO: review same side edge (Fan shape)

                    if (pe_midY > qe_midY)
                    {
                        //p side is upper , q side is lower
                        if (targetEdge.SlopKind == LineSlopeKind.Horizontal)
                        {
                            targetEdge.IsUpper = true;
                            if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Horizontal)
                            {
                                targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                            }
                        }
                    }
                    else
                    {
                        if (matchingEdgeLine.SlopKind == LineSlopeKind.Horizontal)
                        {
                            // matchingEdgeLine.IsUpper = true;
                            if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopKind == LineSlopeKind.Horizontal)
                            {
                                targetEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                            }
                        }
                    }
                }
            }
        }
        static bool FindMatchingOuterSide(EdgeLine compareEdge, GlyphTriangle another, out EdgeLine result, out int edgeIndex)
        {
            //compare by radian of edge line
            double compareSlope = Math.Abs(compareEdge.SlopAngle);
            double diff0 = double.MaxValue;
            double diff1 = double.MaxValue;
            double diff2 = double.MaxValue;

            diff0 = Math.Abs(Math.Abs(another.e0.SlopAngle) - compareSlope);

            diff1 = Math.Abs(Math.Abs(another.e1.SlopAngle) - compareSlope);

            diff2 = Math.Abs(Math.Abs(another.e2.SlopAngle) - compareSlope);

            //find min
            int minDiffSide = FindMinIndex(diff0, diff1, diff2);
            if (minDiffSide > -1)
            {
                edgeIndex = minDiffSide;
                switch (minDiffSide)
                {
                    default: throw new NotSupportedException();
                    case 0:
                        result = another.e0;
                        break;
                    case 1:
                        result = another.e1;
                        break;
                    case 2:
                        result = another.e2;
                        break;
                }
                return true;
            }
            else
            {
                edgeIndex = -1;
                result = null;
                return false;
            }
        }
        static int FindMinIndex(double d0, double d1, double d2)
        {
            unsafe
            {
                double* tmpArr = stackalloc double[3];
                tmpArr[0] = d0;
                tmpArr[1] = d1;
                tmpArr[2] = d2;

                int minAt = -1;
                double currentMin = double.MaxValue;
                for (int i = 0; i < 3; ++i)
                {
                    double d = tmpArr[i];
                    if (d < currentMin)
                    {
                        currentMin = d;
                        minAt = i;
                    }
                }
                return minAt;
            }
        }
        /// <summary>
        /// count number of outside edge
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        static int OutSideEdgeCount(GlyphTriangle t)
        {
            int n = 0;
            n += t.e0.IsOutside ? 1 : 0;
            n += t.e1.IsOutside ? 1 : 0;
            n += t.e2.IsOutside ? 1 : 0;
            return n;
        }
        static readonly double _85degreeToRad = MyMath.DegreesToRadians(85);
        static readonly double _15degreeToRad = MyMath.DegreesToRadians(15);
        static readonly double _90degreeToRad = MyMath.DegreesToRadians(90);
        public override string ToString()
        {
            return p + " -> " + q;
        }
    }

    //public enum GlyphTrianglePart : byte
    //{
    //    Unknown,
    //    VericalStem,
    //    HorizontalStem,
    //    Other,
    //}
    public class GlyphTriangle
    {


        DelaunayTriangle _tri;
        public EdgeLine e0;
        public EdgeLine e1;
        public EdgeLine e2;

        //centroid of edge mass
        double centroidX;
        double centroidY;

        public GlyphTriangle(DelaunayTriangle tri)
        {
            this._tri = tri;
            TriangulationPoint p0 = _tri.P0;
            TriangulationPoint p1 = _tri.P1;
            TriangulationPoint p2 = _tri.P2;
            e0 = new EdgeLine(p0, p1);
            e1 = new EdgeLine(p1, p2);
            e2 = new EdgeLine(p2, p0);
            tri.Centroid2(out centroidX, out centroidY);

            e0.IsOutside = tri.EdgeIsConstrained(tri.FindEdgeIndex(tri.P0, tri.P1));
            e1.IsOutside = tri.EdgeIsConstrained(tri.FindEdgeIndex(tri.P1, tri.P2));
            e2.IsOutside = tri.EdgeIsConstrained(tri.FindEdgeIndex(tri.P2, tri.P0));
        }
        //static int RoundToNearestSide(float org, int gridsize)
        //{
        //    float actual1 = org / (float)gridsize;
        //    int integer1 = (int)(actual1);
        //    float floatModulo = actual1 - integer1;
        //    if (floatModulo > (gridsize / 2))
        //    {
        //        return (integer1 + 1) + gridsize;
        //    }
        //    else
        //    {
        //        return integer1 * gridsize;
        //    }
        //}
        //public void Analyze(int pixelWidth, int pixelHeight)
        //{
        //    //check if triangle is part of vertical/horizontal stem or not
        //    //snap some edge to match with pixel size            
        //    //1. outside count

        //    int outside_count =
        //        ((e0.IsOutside) ? 1 : 0) +
        //        ((e1.IsOutside) ? 1 : 0) +
        //        ((e2.IsOutside) ? 1 : 0);
        //    switch (outside_count)
        //    {
        //        case 0:
        //            break;
        //        case 1:
        //            {
        //                //check this
        //            }
        //            break;
        //        case 2:
        //            {
        //                //have 2 outside
        //                //usu
        //            }
        //            break;
        //        default:

        //            break;

        //    }

        //}
        public double CentroidX
        {
            get { return centroidX; }
        }
        public double CentroidY
        {
            get { return centroidY; }
        }
        public bool IsConnectedWith(GlyphTriangle anotherTri)
        {
            DelaunayTriangle t2 = anotherTri._tri;
            if (t2 == this._tri)
            {
                throw new NotSupportedException();
            }
            //else 
            return this._tri.N0 == t2 ||
                   this._tri.N1 == t2 ||
                   this._tri.N2 == t2;
        }

#if DEBUG
        public override string ToString()
        {
            return this._tri.ToString();
        }
#endif
    }

    public enum LineSlopeKind : byte
    {
        Vertical,
        Horizontal,
        Other
    }

    /// <summary>
    /// edge of GlyphTriangle
    /// </summary>
    public class EdgeLine
    {

        public double x0;
        public double y0;
        public double x1;
        public double y1;

        static readonly double _85degreeToRad = MyMath.DegreesToRadians(85);
        static readonly double _15degreeToRad = MyMath.DegreesToRadians(15);
        static readonly double _90degreeToRad = MyMath.DegreesToRadians(90);

        public Poly2Tri.TriangulationPoint p;
        public Poly2Tri.TriangulationPoint q;

        Dictionary<EdgeLine, bool> matchingEdges;

        public EdgeLine(Poly2Tri.TriangulationPoint p, Poly2Tri.TriangulationPoint q)
        {
            this.p = p;
            this.q = q;

            x0 = p.X;
            y0 = p.Y;
            x1 = q.X;
            y1 = q.Y;
            //-------------------
            if (x1 == x0)
            {
                this.SlopKind = LineSlopeKind.Vertical;
                SlopAngle = 1;
            }
            else
            {
                SlopAngle = Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
                if (SlopAngle > _85degreeToRad)
                {
                    SlopKind = LineSlopeKind.Vertical;
                }
                else if (SlopAngle < _15degreeToRad)
                {
                    SlopKind = LineSlopeKind.Horizontal;
                }
                else
                {
                    SlopKind = LineSlopeKind.Other;
                }
            }
        }
        public LineSlopeKind SlopKind
        {
            get;
            private set;
        }
        public bool IsOutside
        {
            get;
            internal set;
        }
        public double SlopAngle
        {
            get;
            private set;
        }
        public bool IsUpper
        {
            get;
            internal set;
        }
        public bool IsLeftSide
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            return SlopKind + ":" + x0 + "," + y0 + "," + x1 + "," + y1;
        }

        public EdgeLine GetMatchingOutsideEdge()
        {
            if (matchingEdges == null) { return null; }

            if (matchingEdges.Count == 1)
            {
                foreach (EdgeLine line in matchingEdges.Keys)
                {
                    return line;
                }
                return null;
            }
            else
            {
                return null;
            }

        }
        public void AddMatchingOutsideEdge(EdgeLine edgeLine)
        {
#if DEBUG
            if (edgeLine == this) { throw new NotSupportedException(); }
#endif
            if (matchingEdges == null)
            {
                matchingEdges = new Dictionary<EdgeLine, bool>();
            }
            if (!matchingEdges.ContainsKey(edgeLine))
            {
                matchingEdges.Add(edgeLine, true);
            }
#if DEBUG
            if (matchingEdges.Count > 1)
            {

            }
#endif
        }
    }

}