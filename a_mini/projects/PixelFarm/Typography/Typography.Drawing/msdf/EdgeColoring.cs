﻿//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)
using System;
using System.Collections.Generic;

namespace Msdfgen
{
    public struct Vector2
    {
        public readonly double x;
        public readonly double y;
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2 getOrthoNormal(bool polarity = true, bool allowZero = false)
        {
            double len = Length();
            if (len == 0)
            {
                return polarity ? new Vector2(0, (!allowZero ? 1 : 0)) : new Vector2(0, -(!allowZero ? 1 : 0));
            }
            return polarity ? new Vector2(-y / len, x / len) : new Vector2(y / len, -x / len);
        }
        public Vector2 getOrthogonal(bool polarity = true)
        {
            return polarity ? new Vector2(-y, x) : new Vector2(y, -x); 
        } 
        public static double dotProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }
        public static double crossProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x - b.x,
                a.y - b.y);
        }
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x + b.x,
                a.y + b.y);
        }
        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x * b.x,
                a.y * b.y);
        }
        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x / b.x,
                a.y / b.y);
        }
        public static Vector2 operator *(Vector2 a, double n)
        {
            return new Vector2(
                a.x * n,
                a.y * n);
        }
        public static Vector2 operator /(Vector2 a, double n)
        {
            return new Vector2(
                a.x / n,
                a.y / n);
        }
        public static Vector2 operator *(double n, Vector2 a)
        {
            return new Vector2(
                a.x * n,
                a.y * n);
        }
        public static Vector2 operator /(double n, Vector2 a)
        {
            return new Vector2(
                a.x / n,
                a.y / n);
        }
        public Vector2 normalize(bool allowZero = false)
        {
            double len = Length();
            if (len == 0)
            {
                return new Vector2(0, !allowZero ? 1 : 0);
            }
            return new Vector2(x / len, y / len);
        }
        public double Length()
        {
            return Math.Sqrt(x * x + y * y);
        }
        /// <summary>
        /// Clamps the number to the interval from 0 to b.
        /// </summary>
        /// <returns></returns>
        public static int Clamp(int n, int b)
        {

            if (n > 0)
            {
                return (n <= b) ? n : b;
            }
            return 0;
        }
        public override string ToString()
        {
            return x + "," + y;
        }
        ///// Clamps the number to the interval from 0 to b.
        //template<typename T>
        //inline T clamp(T n, T b)
        //{
        //    return n >= T(0) && n <= b ? n : T(n > T(0)) * b;
        //}

        ///// Clamps the number to the interval from a to b.
        //template<typename T>
        //inline T clamp(T n, T a, T b)
        //{
        //    return n >= a && n <= b ? n : n < a ? a : b;
        //}
    }
    public class Shape
    {
        public List<Contour> contours = new List<Contour>();
        public bool InverseYAxis { get; set; }
        public void normalized()
        {
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                Contour contour = contours[i];
                if (contour.edges.Count == 1)
                {

                }
            }
        }
    }
    public class Contour
    {
        public List<EdgeHolder> edges = new List<EdgeHolder>();
        public void AddEdge(EdgeSegment edge)
        {
            EdgeHolder holder = new EdgeHolder(edge);
            edges.Add(holder);
        }
        public void AddLine(double x0, double y0, double x1, double y1)
        {
            this.AddEdge(new LinearSegment(new Vector2(x0, y0), new Vector2(x1, y1)));
        }
        public void AddQuadraticSegment(double x0, double y0,
            double ctrl0X, double ctrl0Y,
            double x1, double y1)
        {
            this.AddEdge(new QuadraticSegment(
                new Vector2(x0, y0),
                new Vector2(ctrl0X, ctrl0Y),
                new Vector2(x1, y1)
                ));
        }
        public void AddCubicSegment(double x0, double y0,
            double ctrl0X, double ctrl0Y,
            double ctrl1X, double ctrl1Y,
            double x1, double y1)
        {
            this.AddEdge(new CubicSegment(
               new Vector2(x0, y0),
               new Vector2(ctrl0X, ctrl0Y),
               new Vector2(ctrl1X, ctrl1Y),
               new Vector2(x1, y1)
               ));
        }

    }

    public static class EdgeColoring
    {

        static bool isCorner(Vector2 aDir, Vector2 bDir, double crossThreshold)
        {
            return Vector2.dotProduct(aDir, bDir) <= 0 || Math.Abs(Vector2.crossProduct(aDir, bDir)) > crossThreshold;
        }

        public static void edgeColoringSimple(Shape shape, double angleThreshold)
        {
            double crossThreshold = Math.Sin(angleThreshold);
            List<int> corners = new List<int>();

            // for (std::vector<Contour>::iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour)
            foreach (Contour contour in shape.contours)
            {
                // Identify corners 
                corners.Clear();
                List<EdgeHolder> edges = contour.edges;
                int edgeCount = edges.Count;
                if (edgeCount != 0)
                {
                    Vector2 prevDirection = edges[edgeCount - 1].Direction(1);// (*(contour->edges.end() - 1))->direction(1); 
                    for (int i = 0; i < edgeCount; ++i)
                    {
                        EdgeHolder edge = edges[i];
                        if (isCorner(prevDirection.normalize(),
                            edge.Direction(0).normalize(), crossThreshold))
                        {
                            corners.Add(i);
                        }
                        prevDirection = edge.Direction(1);
                    }
                }

                // Smooth contour
                if (corners.Count == 0) //is empty
                {
                    for (int i = edgeCount - 1; i >= 0; --i)
                    {
                        edges[i].color = EdgeColor.WHITE;
                    }

                }
                else if (corners.Count == 1)
                {
                    // "Teardrop" case
                    EdgeColor[] colors = { EdgeColor.MAGENTA, EdgeColor.WHITE, EdgeColor.YELLOW };
                    int corner = corners[0];
                    if (edgeCount >= 3)
                    {
                        int m = edgeCount;
                        for (int i = 0; i < m; ++i)
                        {
                            //TODO: review here 
                            contour.edges[(corner + i) % m].color = colors[((int)(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3) + 1];
                            //(colors + 1)[int(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3];
                        }
                    }
                    else if (edgeCount >= 1)
                    {
                        // Less than three edge segments for three colors => edges must be split
                        EdgeSegment[] parts = new EdgeSegment[7]; //empty array
                        edges[0].edgeSegment.splitInThirds(
                            out parts[0 + 3 * corner],
                            out parts[1 + 3 * corner],
                            out parts[2 + 3 * corner]);

                        if (edgeCount >= 2)
                        {
                            edges[1].edgeSegment.splitInThirds(
                                out parts[3 - 3 * corner],
                                out parts[4 - 3 * corner],
                                out parts[5 - 3 * corner]
                                );
                            parts[0].color = parts[1].color = colors[0];
                            parts[2].color = parts[3].color = colors[1];
                            parts[4].color = parts[5].color = colors[2];
                        }
                        else
                        {
                            parts[0].color = colors[0];
                            parts[1].color = colors[1];
                            parts[2].color = colors[2];
                        }
                        contour.edges.Clear();
                        for (int i = 0; i < 7; ++i)
                        {
                            edges.Add(new EdgeHolder(parts[i]));
                        }
                    }
                }
                // Multiple corners
                else
                {
                    int cornerCount = corners.Count;
                    // CMYCMYCMYCMY / YMYCMYC if corner count % 3 == 1
                    EdgeColor[] colors = { cornerCount % 3 == 1 ? EdgeColor.YELLOW : EdgeColor.CYAN, EdgeColor.CYAN, EdgeColor.MAGENTA, EdgeColor.YELLOW };
                    int spline = 0;
                    int start = corners[0];
                    int m = contour.edges.Count;
                    for (int i = 0; i < m; ++i)
                    {
                        int index = (start + i) % m;
                        if (cornerCount > spline + 1 && corners[spline + 1] == index)
                        {
                            ++spline;
                        }

                        int tmp = (spline % 3 - ((spline == 0) ? 1 : 0));
                        edges[i].color = colors[tmp + 1];
                        //contour->edges[index]->color = (colors + 1)[spline % 3 - !spline];
                    }
                }
            }
        }

    }
}


//namespace msdfgen
//{

//    static bool isCorner(const Vector2 &aDir, const Vector2 &bDir, double crossThreshold)
//    {
//        return dotProduct(aDir, bDir) <= 0 || fabs(crossProduct(aDir, bDir)) > crossThreshold;
//    }

//    void edgeColoringSimple(Shape &shape, double angleThreshold)
//    {
//        double crossThreshold = sin(angleThreshold);
//        std::vector<int> corners;
//        for (std::vector<Contour>::iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour)
//        {
//            // Identify corners
//            corners.clear();
//            if (!contour->edges.empty())
//            {
//                Vector2 prevDirection = (*(contour->edges.end() - 1))->direction(1);
//                int index = 0;
//                for (std::vector<EdgeHolder>::const_iterator edge = contour->edges.begin(); edge != contour->edges.end(); ++edge, ++index)
//                {
//                    if (isCorner(prevDirection.normalize(), (*edge)->direction(0).normalize(), crossThreshold))
//                        corners.push_back(index);
//                    prevDirection = (*edge)->direction(1);
//                }
//            }

//            // Smooth contour
//            if (corners.empty())
//                for (std::vector<EdgeHolder>::iterator edge = contour->edges.begin(); edge != contour->edges.end(); ++edge)
//                    (*edge)->color = WHITE;
//            // "Teardrop" case
//            else if (corners.size() == 1)
//            {
//                const EdgeColor colors[] = { MAGENTA, WHITE, YELLOW };
//                int corner = corners[0];
//                if (contour->edges.size() >= 3)
//                {
//                    int m = contour->edges.size();
//                    for (int i = 0; i < m; ++i)
//                        contour->edges[(corner + i) % m]->color = (colors + 1)[int(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3];
//                }
//                else if (contour->edges.size() >= 1)
//                {
//                    // Less than three edge segments for three colors => edges must be split
//                    EdgeSegment* parts[7] = { };
//                    contour->edges[0]->splitInThirds(parts[0 + 3 * corner], parts[1 + 3 * corner], parts[2 + 3 * corner]);
//                    if (contour->edges.size() >= 2)
//                    {
//                        contour->edges[1]->splitInThirds(parts[3 - 3 * corner], parts[4 - 3 * corner], parts[5 - 3 * corner]);
//                        parts[0]->color = parts[1]->color = colors[0];
//                        parts[2]->color = parts[3]->color = colors[1];
//                        parts[4]->color = parts[5]->color = colors[2];
//                    }
//                    else
//                    {
//                        parts[0]->color = colors[0];
//                        parts[1]->color = colors[1];
//                        parts[2]->color = colors[2];
//                    }
//                    contour->edges.clear();
//                    for (int i = 0; parts[i]; ++i)
//                        contour->edges.push_back(EdgeHolder(parts[i]));
//                }
//            }
//            // Multiple corners
//            else
//            {
//                int cornerCount = corners.size();
//                // CMYCMYCMYCMY / YMYCMYC if corner count % 3 == 1
//                EdgeColor colors[] = { cornerCount % 3 == 1 ? YELLOW : CYAN, CYAN, MAGENTA, YELLOW };
//                int spline = 0;
//                int start = corners[0];
//                int m = contour->edges.size();
//                for (int i = 0; i < m; ++i)
//                {
//                    int index = (start + i) % m;
//                    if (cornerCount > spline + 1 && corners[spline + 1] == index)
//                        ++spline;
//                    contour->edges[index]->color = (colors + 1)[spline % 3 - !spline];
//                }
//            }
//        }
//    }

//}
