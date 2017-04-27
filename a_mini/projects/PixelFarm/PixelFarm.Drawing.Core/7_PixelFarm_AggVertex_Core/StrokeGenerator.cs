//BSD, 2014-2017, WinterDev
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

using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;

namespace PixelFarm.Agg
{
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
        public static bool MinDistanceFirst(Vector2 baseVec, Vector2 compare0, Vector2 compare1)
        {
            return (SquareDistance(baseVec, compare0) < SquareDistance(baseVec, compare1)) ? true : false;
        }

        public static double SquareDistance(Vector2 v0, Vector2 v1)
        {
            double xdiff = v1.X - v0.X;
            double ydiff = v1.Y - v0.Y;
            return (xdiff * xdiff) + (ydiff * ydiff);
        }
        public static int Min(double v0, double v1, double v2)
        {
            //find min of 3
            unsafe
            {
                double* doubleArr = stackalloc double[3];
                doubleArr[0] = v0;
                doubleArr[1] = v1;
                doubleArr[2] = v2;

                double min = double.MaxValue;
                int foundAt = 0;
                for (int i = 0; i < 3; ++i)
                {
                    if (doubleArr[i] < min)
                    {
                        foundAt = i;
                        min = doubleArr[i];
                    }
                }
                return foundAt;
            }

        }

        /// <summary>
        /// find parameter A,B,C from Ax + By = C, with given 2 points
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        static void FindABC(Vector2 p0, Vector2 p1, out double a, out double b, out double c)
        {
            //line is in the form
            //Ax + By = C 
            //from http://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
            //and https://www.topcoder.com/community/data-science/data-science-tutorials/geometry-concepts-line-intersection-and-its-applications/
            a = p1.Y - p0.Y;
            b = p0.X - p1.X;
            c = a * p0.X + b * p0.Y;
        }
        public static bool FindCutPoint(
              Vector2 p0, Vector2 p1,
              Vector2 p2, Vector2 p3, out Vector2 result)
        {
            //TODO: review here
            //from http://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
            //and https://www.topcoder.com/community/data-science/data-science-tutorials/geometry-concepts-line-intersection-and-its-applications/

            //------------------------------------------
            //use matrix style ***
            //------------------------------------------
            //line is in the form
            //Ax + By = C
            //so   A1x +B1y= C1 ... line1
            //     A2x +B2y=C2  ... line2
            //------------------------------------------
            //
            //from Ax+By=C ... (1)
            //By = C- Ax;

            double a1, b1, c1;
            FindABC(p0, p1, out a1, out b1, out c1);

            double a2, b2, c2;
            FindABC(p2, p3, out a2, out b2, out c2);

            double delta = a1 * b2 - a2 * b1; //delta is the determinant in math parlance
            if (delta == 0)
            {
                //"Lines are parallel"
                result = Vector2.Zero;
                return false; //
                throw new System.ArgumentException("Lines are parallel");
            }
            double x = (b2 * c1 - b1 * c2) / delta;
            double y = (a1 * c2 - a2 * c1) / delta;
            result = new Vector2((float)x, (float)y);
            return true; //has cutpoint
        }


        static double FindB(Vector2 p0, Vector2 p1)
        {

            double m1 = (p1.Y - p0.Y) / (p1.X - p0.X);
            //y = mx + b ...(1)
            //b = y- mx

            //substitue with known value to gett b 
            //double b0 = p0.Y - (slope_m) * p0.X;
            //double b1 = p1.Y - (slope_m) * p1.X;
            //return b0;

            return p0.Y - (m1) * p0.X;
        }


    }



    class EdgeLine
    {

        double latest_moveto_x;
        double latest_moveto_y;
        double positiveSide, negativeSide;
        //line core (x0,y0) ->  (x1,y1) -> (x2,y2)
        double x0, y0, x1, y1, x2, y2;

        public Vector delta0, delta1;
        public Vector e1_positive;
        public Vector e1_negative;
        public Vector line_vector; //latest line vector
        int coordCount = 0;
        public void SetEdgeWidths(double positiveSide, double negativeSide)
        {
            this.positiveSide = positiveSide;
            this.negativeSide = negativeSide;
        }


        public void AcceptLatest()
        {
            //TODO: rename this method
            this.x0 = x1;
            this.y0 = y1;
        }
        public void LineTo(double x1, double y1)
        {
            if (coordCount > 0)
            {
                //perpendicular line
                //create line vector
                line_vector = delta0 = new Vector(x1 - x0, y1 - y0);
                delta1 = delta0 = delta0.Rotate(90).NewLength(positiveSide);
            }
            this.x1 = x1;
            this.y1 = y1;
            //------------------------------------------------------
            e1_positive = new Vector(x1 + delta1.X, y1 + delta1.Y);
            e1_negative = new Vector(x1 - delta1.X, y1 - delta1.Y);
            //------------------------------------------------------
            //create both positive and negative edge 
            coordCount++;
        }
        public void MoveTo(double x0, double y0)
        {
            //reset data
            coordCount = 0;
            latest_moveto_x = x0;
            latest_moveto_y = y0;

            this.x0 = x0;
            this.y0 = y0;


            coordCount++;
        }
        public void GetEdge0(out double ex0, out double ey0, out double ex0_n, out double ey0_n)
        {
            ex0 = x0 + delta0.X;
            ey0 = y0 + delta0.Y;
            ex0_n = x0 - delta0.X;
            ey0_n = y0 - delta0.Y;
        }

        public void CreateLineJoin(
         double previewX1, double previewY1,
         List<Vector> outputPositiveSideList,
         List<Vector> outputNegativeSideList)
        {


            //----------
            //create line join for previewX1,previewY1
            //in this version, 
            //we find the cutpoint of 2 edge
            Vector2 newline_vector = new Vector2(previewX1 - x1, previewY1 - y1);
            //find line cut between line2_vec and line_vector 
            Vector2 previewDelta = newline_vector.RotateInDegree(90).NewLength(positiveSide);

            Vector2 x1_preview_positive = new Vector2(previewX1 + previewDelta.X, previewY1 + previewDelta.Y);
            Vector2 x1_preview_positive_1 = new Vector2(x1_preview_positive.X + newline_vector.X,
                                                        x1_preview_positive.y + newline_vector.Y);

            //------------------------------
            //find cutpoint on each side
            Vector2 cutPoint;

            if (MyMath.FindCutPoint(
                   x1_preview_positive,
                   x1_preview_positive_1,
                   new Vector2(e1_positive.X + line_vector.X, e1_positive.Y + line_vector.Y),
                   new Vector2(e1_positive.X, e1_positive.Y),
               out cutPoint))
            {
                outputPositiveSideList.Add(new Vector(cutPoint.x, cutPoint.y));
            }
            else
            {

            }
            //---------------------------------------------------
            Vector2 e1_preview_negative = new Vector2(previewX1 - previewDelta.X, previewY1 - previewDelta.Y);
            Vector2 e1_preview_negative_1 = new Vector2(e1_preview_negative.X + newline_vector.X,
                                                        e1_preview_negative.y + newline_vector.Y);
            if (MyMath.FindCutPoint(
               e1_preview_negative,
               e1_preview_negative_1,
               new Vector2(e1_negative.X + line_vector.X, e1_negative.Y + line_vector.Y),
               new Vector2(e1_negative.X, e1_negative.Y),
               out cutPoint))
            {
                outputNegativeSideList.Add(new Vector(cutPoint.x, cutPoint.y));
            }
            else
            {

            }
        }

    }


    public class StrokeGen2
    {

        VertexStore m_out_vertices = new VertexStore();
        StrokeMath m_stroker = new StrokeMath();
        MultipartVerextDistanceList multipartVertexDistanceList = new MultipartVerextDistanceList();
        EdgeLine currentEdgeLine = new EdgeLine();
        List<Vector> positiveSideVectors = new List<Vector>();
        List<Vector> negativeSideVectors = new List<Vector>();
        LineCap lineCap = LineCap.Butt;

        float positiveSide, negativeSide;
        public StrokeGen2()
        {
            //use 2 vertext list to store perpendicular outline 
            currentEdgeLine.SetEdgeWidths(0.5f, 0.5f);//default
        }
        public void SetEdgeWidth(float positiveSide, float negativeSide)
        {
            this.positiveSide = positiveSide;
            this.negativeSide = negativeSide;
            //
            currentEdgeLine.SetEdgeWidths(positiveSide, negativeSide);
        }
        public void Generate(VertexStore srcVxs, VertexStore outputVxs)
        {
            //read data from src
            //generate stroke and 
            //write to output
            //-----------
            int cmdCount = srcVxs.Count;
            VertexCmd cmd;
            double x, y;

            positiveSideVectors.Clear();
            negativeSideVectors.Clear();

            double ex0, ey0, ex0_n, ey0_n;
            int current_coord_count = 0;
            double latest_moveto_x = 0;
            double latest_moveto_y = 0;
            double first_lineto_x = 0;
            double first_lineto_y = 0;
            for (int i = 0; i < cmdCount; ++i)
            {
                cmd = srcVxs.GetVertex(i, out x, out y);
                switch (cmd)
                {
                    case VertexCmd.LineTo:
                        {

                            if (current_coord_count > 1)
                            {
                                currentEdgeLine.CreateLineJoin(x, y, positiveSideVectors, negativeSideVectors);
                                //
                                currentEdgeLine.LineTo(x, y);
                                //consider create joint here
                                currentEdgeLine.GetEdge0(out ex0, out ey0, out ex0_n, out ey0_n);
                                //add to vectors
                                positiveSideVectors.Add(new Vector(ex0, ey0));
                                positiveSideVectors.Add(currentEdgeLine.e1_positive);
                                //
                                negativeSideVectors.Add(new Vector(ex0_n, ey0_n));
                                negativeSideVectors.Add(currentEdgeLine.e1_negative);
                            }
                            else
                            {
                                currentEdgeLine.LineTo(first_lineto_x = x, first_lineto_y = y);
                                currentEdgeLine.GetEdge0(out ex0, out ey0, out ex0_n, out ey0_n);
                                //add to vectors
                                positiveSideVectors.Add(new Vector(ex0, ey0));
                                positiveSideVectors.Add(currentEdgeLine.e1_positive);
                                //
                                negativeSideVectors.Add(new Vector(ex0_n, ey0_n));
                                negativeSideVectors.Add(currentEdgeLine.e1_negative);

                            }
                            currentEdgeLine.AcceptLatest();
                            current_coord_count++;
                        }

                        break;
                    case VertexCmd.MoveTo:
                        //if we have current shape
                        //leave it and start the new shape
                        currentEdgeLine.MoveTo(latest_moveto_x = x, latest_moveto_y = y);
                        current_coord_count = 1;
                        break;
                    case VertexCmd.Close:
                    case VertexCmd.CloseAndEndFigure:
                        {
                            //------------------------------------------
                            currentEdgeLine.CreateLineJoin(latest_moveto_x, latest_moveto_y, positiveSideVectors, negativeSideVectors);
                            currentEdgeLine.LineTo(latest_moveto_x, latest_moveto_y);
                            if (current_coord_count > 1)
                            {

                                //consider create joint here
                                currentEdgeLine.GetEdge0(out ex0, out ey0, out ex0_n, out ey0_n);
                                //add to vectors
                                positiveSideVectors.Add(new Vector(ex0, ey0));
                                positiveSideVectors.Add(currentEdgeLine.e1_positive);
                                //
                                negativeSideVectors.Add(new Vector(ex0_n, ey0_n));
                                negativeSideVectors.Add(currentEdgeLine.e1_negative);
                            }
                            else
                            {
                                currentEdgeLine.GetEdge0(out ex0, out ey0, out ex0_n, out ey0_n);
                                //add to vectors
                                positiveSideVectors.Add(new Vector(ex0, ey0));
                                positiveSideVectors.Add(currentEdgeLine.e1_positive);
                                //
                                negativeSideVectors.Add(new Vector(ex0_n, ey0_n));
                                negativeSideVectors.Add(currentEdgeLine.e1_negative);

                            }
                            currentEdgeLine.AcceptLatest();
                            current_coord_count++;
                            //------------------------------------------
                            currentEdgeLine.CreateLineJoin(first_lineto_x, first_lineto_y, positiveSideVectors, negativeSideVectors);
                            //------------------------------------------
                            WriteOutput(outputVxs, true);
                            current_coord_count = 0;
                        }//create line cap
                        break;
                    default:
                        break;
                }
            }
            //-------------
            if (current_coord_count > 0)
            {
                WriteOutput(outputVxs, false);
            }
        }
        void WriteOutput(VertexStore outputVxs, bool close)
        {

            //write output to 

            if (close)
            {
                int positive_edgeCount = positiveSideVectors.Count;
                int negative_edgeCount = negativeSideVectors.Count;

                int n = positive_edgeCount - 1;
                Vector v = positiveSideVectors[n];
                outputVxs.AddMoveTo(v.X, v.Y);
                for (; n >= 0; --n)
                {
                    v = positiveSideVectors[n];
                    outputVxs.AddLineTo(v.X, v.Y);
                }
                outputVxs.AddCloseFigure();
                //end ... create join to negative side
                //------------------------------------------ 
                //create line join from positive  to negative side
                v = negativeSideVectors[0];
                outputVxs.AddMoveTo(v.X, v.Y);
                n = 1;
                for (; n < negative_edgeCount; ++n)
                {
                    v = negativeSideVectors[n];
                    outputVxs.AddLineTo(v.X, v.Y);
                }
                //------------------------------------------
                //close
                outputVxs.AddCloseFigure();
            }
            else
            {

                int positive_edgeCount = positiveSideVectors.Count;
                int negative_edgeCount = negativeSideVectors.Count;

                //no a close shape stroke
                //create line cap for this
                //
                //positive
                Vector v = positiveSideVectors[0];
                //-----------

                CreateStartLineCap(outputVxs, v, positiveSideVectors[1],
                    negativeSideVectors[0], positiveSide);
                //-----------

                int n = 1;
                for (; n < positive_edgeCount; ++n)
                {
                    //increment n
                    v = positiveSideVectors[n];
                    outputVxs.AddLineTo(v.X, v.Y);
                }
                //negative 
                //---------------------------------- 
                CreateEndLineCap(outputVxs,
                    positiveSideVectors[positive_edgeCount - 2],
                    positiveSideVectors[positive_edgeCount - 1],
                    negativeSideVectors[negative_edgeCount - 1],
                    positiveSide);
                //----------------------------------
                for (n = negative_edgeCount - 2; n >= 0; --n)
                {
                    //decrement n
                    v = negativeSideVectors[n];
                    outputVxs.AddLineTo(v.X, v.Y);
                }

                outputVxs.AddCloseFigure();
            }
            //reset
            positiveSideVectors.Clear();
            negativeSideVectors.Clear();
        }
        void CreateStartLineCap(VertexStore outputVxs, Vector v0, Vector v1, Vector v0_n, double edgeWidth)
        {
            //TODO: review 
            switch (lineCap)
            {
                default: throw new NotSupportedException();
                case LineCap.Butt:
                    outputVxs.AddMoveTo(v0.X, v0.Y);// moveto

                    break;
                case LineCap.Square:
                    Vector delta = (v1 - v0).NewLength(edgeWidth);
                    //------------------------
                    outputVxs.AddMoveTo(v0_n.X - delta.X, v0_n.Y - delta.Y);// moveto
                    outputVxs.AddLineTo(v0.X - delta.X, v0.Y - delta.Y);
                    break;
            }
        }
        void CreateEndLineCap(VertexStore outputVxs, Vector v0, Vector v1, Vector v0_n, double edgeWidth)
        {
            //TODO: review 
            switch (lineCap)
            {
                default: throw new NotSupportedException();
                case LineCap.Butt:

                    outputVxs.AddLineTo(v1.X, v1.Y);// moveto
                    outputVxs.AddLineTo(v0_n.X, v0_n.Y);
                    break;
                case LineCap.Square:
                    Vector delta = (v1 - v0).NewLength(edgeWidth);
                    outputVxs.AddLineTo(v1.X + delta.X, v1.Y + delta.Y);// moveto
                    outputVxs.AddLineTo(v0_n.X + delta.X, v0_n.Y + delta.Y);
                    break;
            }
        }
    }
    class StrokeGenerator
    {

        /// <summary>
        /// stroke generator's status
        /// </summary>
        public enum Status
        {
            Init,
            Ready,
            Cap1,
            Cap2,
            Outline1,
            CloseFirst,
            Outline2,
            OutVertices,
            EndPoly1,
            EndPoly2,
            Stop
        }


        StrokeMath m_stroker;
        MultipartVerextDistanceList multipartVertexDistanceList = new MultipartVerextDistanceList();
        VertexStore m_out_vertices;
        double m_shorten;
        bool m_closed;
        Status m_status;
        Status m_prev_status;
        int m_src_vertex;
        int m_out_vertex;
        public StrokeGenerator()
        {
            m_stroker = new StrokeMath();
            m_out_vertices = new VertexStore();
            m_status = Status.Init;
        }

        public LineCap LineCap
        {
            get { return this.m_stroker.LineCap; }
            set { this.m_stroker.LineCap = value; }
        }
        public LineJoin LineJoin
        {
            get { return this.m_stroker.LineJoin; }
            set { this.m_stroker.LineJoin = value; }
        }
        public InnerJoin InnerJoin
        {
            get { return this.m_stroker.InnerJoin; }
            set { this.m_stroker.InnerJoin = value; }
        }

        public double Width
        {
            get { return m_stroker.Width; }
            set { this.m_stroker.Width = value; }
        }
        public void SetMiterLimitTheta(double t) { m_stroker.SetMiterLimitTheta(t); }


        public double InnerMiterLimit
        {
            get { return this.m_stroker.InnerMiterLimit; }
            set { this.m_stroker.InnerMiterLimit = value; }
        }
        public double MiterLimit
        {
            get { return this.m_stroker.InnerMiterLimit; }
            set { this.m_stroker.InnerMiterLimit = value; }
        }
        public double ApproximateScale
        {
            get { return this.m_stroker.ApproximateScale; }
            set { this.m_stroker.ApproximateScale = value; }
        }
        public bool AutoDetectOrientation
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public double Shorten
        {
            get { return this.m_shorten; }
            set { this.m_shorten = value; }
        }
        // Vertex Generator Interface
        public void Reset()
        {
            multipartVertexDistanceList.Clear();
            m_closed = false;
            m_status = Status.Init;

        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            //TODO: review 
            m_status = Status.Init;
            switch (cmd)
            {
                case VertexCmd.MoveTo:
                    multipartVertexDistanceList.AddMoveTo(x, y);
                    break;
                case VertexCmd.Close:
                case VertexCmd.CloseAndEndFigure:
                    //m_closed = true;
                    break;
                default:
                    multipartVertexDistanceList.AddVertex(new Vertex2d(x, y));
                    break;
            }
        }

        public void WriteTo(VertexStore outputVxs)
        {

            this.Rewind();
            int currentRangeIndex = 0;
            double x = 0, y = 0;
            //int n = 0;
            for (;;)
            {
                VertexCmd cmd = GetNextVertex(out x, out y);
                if (cmd == VertexCmd.NoMore)
                {
                    if (currentRangeIndex + 1 < multipartVertexDistanceList.RangeCount)
                    {
                        //move to next range
                        multipartVertexDistanceList.SetRangeIndex(currentRangeIndex + 1);
                        currentRangeIndex++;

                        m_status = Status.Ready;
                        m_src_vertex = 0;
                        m_out_vertex = 0;
                        continue;
                    }
                    else
                    {
                        break;//exit from loop
                    }
                }
                outputVxs.AddVertex(x, y, cmd);


                //Console.WriteLine(n + " " + x + "," + y);
                //n++;
                //if (n == 419)
                //{ 
                //}
            }
        }
        void Rewind()
        {
            if (m_status == Status.Init)
            {
                multipartVertexDistanceList.Rewind();
                if (multipartVertexDistanceList.CurrentRangeLen < 3)
                {
                    //force
                    m_closed = false;
                }
                //_curCurtexDistanceList.Close(m_closed);
                //VertexHelper.ShortenPath(_curCurtexDistanceList, m_shorten, m_closed);
                //if (_curCurtexDistanceList.Count < 3) { m_closed = false; }
            }
            m_status = Status.Ready;
            m_src_vertex = 0;
            m_out_vertex = 0;
            //if (_vertextDistanceListQueue.Count > 0)
            //{
            //    _vertextDistanceListQueue.Enqueue(_curCurtexDistanceList);
            //    //switch to first one
            //    _curCurtexDistanceList = _vertextDistanceListQueue.Dequeue();
            //}
            multipartVertexDistanceList.Rewind();
        }

        VertexCmd GetNextVertex(out double x, out double y)
        {
            x = 0; y = 0;
            VertexCmd cmd = VertexCmd.LineTo;
            do
            {
                switch (m_status)
                {
                    case Status.Init:
                        this.Rewind();
                        goto case Status.Ready;
                    case Status.Ready:

                        if (multipartVertexDistanceList.CurrentRangeLen < 2 + (m_closed ? 1 : 0))
                        {
                            cmd = VertexCmd.NoMore;
                            break;
                        }
                        m_status = m_closed ? Status.Outline1 : Status.Cap1;
                        cmd = VertexCmd.MoveTo;
                        m_src_vertex = 0;
                        m_out_vertex = 0;
                        break;
                    case Status.CloseFirst:
                        m_status = Status.Outline2;
                        cmd = VertexCmd.MoveTo;
                        goto case Status.Outline2;
                    case Status.Cap1:
                        {
                            Vertex2d v0, v1;

                            multipartVertexDistanceList.GetFirst2(out v0, out v1);
                            m_stroker.CreateCap(
                                m_out_vertices,
                                v0,
                                v1,
                                v0.CalLen(v1));

                            m_src_vertex = 1;
                            m_prev_status = Status.Outline1;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;
                        }
                        break;
                    case Status.Cap2:
                        {
                            Vertex2d beforeLast, last;
                            multipartVertexDistanceList.GetLast2(out beforeLast, out last);
                            m_stroker.CreateCap(m_out_vertices,
                                last,
                                beforeLast,
                                beforeLast.CalLen(last));
                            m_prev_status = Status.Outline2;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;
                        }
                        break;
                    case Status.Outline1:
                        {
                            if (m_closed)
                            {
                                if (m_src_vertex >= multipartVertexDistanceList.CurrentRangeLen)
                                {
                                    m_prev_status = Status.CloseFirst;
                                    m_status = Status.EndPoly1;
                                    break;
                                }
                            }
                            else
                            {
                                if (m_src_vertex >= multipartVertexDistanceList.CurrentRangeLen - 1)
                                {
                                    m_status = Status.Cap2;
                                    break;
                                }
                            }

                            Vertex2d prev, cur, next;
                            multipartVertexDistanceList.GetTripleVertices(m_src_vertex,
                                out prev,
                                out cur,
                                out next);
                            //check if we should join or not ?

                            //don't join it
                            m_stroker.CreateJoin(m_out_vertices,
                           prev,
                           cur,
                           next,
                           prev.CalLen(cur),
                           cur.CalLen(next));

                            ++m_src_vertex;
                            m_prev_status = m_status;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;

                        }
                        break;

                    case Status.Outline2:
                        {
                            if (m_src_vertex <= (!m_closed ? 1 : 0))
                            {
                                m_status = Status.EndPoly2;
                                m_prev_status = Status.Stop;
                                break;
                            }

                            --m_src_vertex;

                            Vertex2d prev, cur, next;
                            multipartVertexDistanceList.GetTripleVertices(m_src_vertex,
                                out prev,
                                out cur,
                                out next);

                            m_stroker.CreateJoin(m_out_vertices,
                              next,
                              cur,
                              prev,
                              cur.CalLen(next),
                              prev.CalLen(cur));
                            m_prev_status = m_status;
                            m_status = Status.OutVertices;
                            m_out_vertex = 0;

                        }
                        break;
                    case Status.OutVertices:
                        if (m_out_vertex >= m_out_vertices.Count)
                        {
                            m_status = m_prev_status;
                        }
                        else
                        {
                            m_out_vertices.GetVertex(m_out_vertex++, out x, out y);
                            return cmd;
                        }
                        break;
                    case Status.EndPoly1:
                        m_status = m_prev_status;
                        x = (int)EndVertexOrientation.CCW;
                        y = 0;
                        return VertexCmd.Close;
                    case Status.EndPoly2:
                        m_status = m_prev_status;
                        x = (int)EndVertexOrientation.CW;
                        y = 0;
                        return VertexCmd.Close;
                    case Status.Stop:
                        cmd = VertexCmd.NoMore;
                        break;
                }

            } while (!VertexHelper.IsEmpty(cmd));
            return cmd;
        }
    }

    class MultipartVerextDistanceList
    {
        class Range
        {
            public int beginAt;
            public int len;
            public Range(int beginAt)
            {
                this.beginAt = beginAt;
                this.len = 0;
            }
            public int Count
            {
                get { return len; }
            }
            public void SetLen(int len)
            {
                this.len = len;
            }
            public void SetEndAt(int endAt)
            {
                this.len = endAt - beginAt;
            }
        }
        List<Vertex2d> _vertextDistanceList = new List<Vertex2d>();
        List<Range> _ranges = new List<Range>();
        Range _range;
        Vertex2d _latest = new Vertex2d();
        int _rangeIndex = 0;
        public void AddMoveTo(double x, double y)
        {
            //TODO: review here
            //1. stop current range
            if (_ranges.Count > 0)
            {
                _ranges[_ranges.Count - 1].SetEndAt(_vertextDistanceList.Count);
            }

            _ranges.Add(_range = new Range(_vertextDistanceList.Count));
            AddVertex(new Agg.Vertex2d(x, y));
        }

        public int RangeIndex { get { return this._rangeIndex; } }
        public void SetRangeIndex(int index)
        {
            this._rangeIndex = index;
            _range = _ranges[index];
        }
        public int RangeCount
        {
            get { return _ranges.Count; }
        }
        public int CurrentRangeLen
        {
            get
            {
                return (_range == null) ? 0 : _range.len;
            }
        }
        public void AddLineTo(double x, double y)
        {
            AddVertex(new Agg.Vertex2d(x, y));
        }
        public void AddVertex(Vertex2d val)
        {
            int count = _range.Count;
            //Ensure that the new one is not duplicate with the last one
            switch (count)
            {
                case 0:
                    _vertextDistanceList.Add(_latest = val);
                    _range.SetLen(count + 1);
                    break;
                default:
                    if (!_latest.IsEqual(val))
                    {
                        _range.SetLen(count + 1);
                        _vertextDistanceList.Add(_latest = val);
                    }
                    break;
            }
        }
        public void Clear()
        {
            _ranges.Clear();
            _vertextDistanceList.Clear();
            _latest = new Agg.Vertex2d();
            _rangeIndex = 0;
            _range = null;
        }
        public void Rewind()
        {
            _rangeIndex = 0;
            if (_ranges.Count > 0)
            {
                _range = _ranges[_rangeIndex];
            }
        }

        public void ReplaceLast(Vertex2d val)
        {
            _vertextDistanceList.RemoveAt(_vertextDistanceList.Count - 1);
            AddVertex(val);
        }
        public void GetTripleVertices(int idx, out Vertex2d prev, out Vertex2d cur, out Vertex2d next)
        {
            //we want 3 vertices
            if (idx > 0 && idx + 2 <= _range.Count)
            {
                prev = _vertextDistanceList[_range.beginAt + idx - 1];
                cur = _vertextDistanceList[_range.beginAt + idx];
                next = _vertextDistanceList[_range.beginAt + idx + 1];

            }
            else
            {
                prev = cur = next = new Vertex2d();
            }
        }
        public void GetFirst2(out Vertex2d first, out Vertex2d second)
        {
            first = _vertextDistanceList[_range.beginAt];
            second = _vertextDistanceList[_range.beginAt + 1];

        }
        public void GetLast2(out Vertex2d beforeLast, out Vertex2d last)
        {
            beforeLast = _vertextDistanceList[_range.beginAt + _range.len - 2];
            last = _vertextDistanceList[_range.beginAt + _range.len - 1];

        }
    }


}