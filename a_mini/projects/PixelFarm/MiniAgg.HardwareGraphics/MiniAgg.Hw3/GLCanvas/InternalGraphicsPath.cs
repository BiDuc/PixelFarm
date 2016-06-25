﻿//MIT 2016, WinterDev

using System.Collections.Generic;
using PixelFarm.Agg;
namespace PixelFarm.DrawingGL
{
    class Figure
    {
        public float[] coordXYs; //this is user provide coord
        //---------
        //system tess ...
        public float[] areaTess;
        float[] smoothBorderTess;
        int borderTriangleStripCount;
        int tessAreaTriangleCount;
        public Figure(float[] coordXYs)
        {
            this.coordXYs = coordXYs;
        }
        public int BorderTriangleStripCount { get { return borderTriangleStripCount; } }
        public int TessAreaTriangleCount { get { return tessAreaTriangleCount; } }
        public void BuildSmoothBorders()
        {
            float[] coords = this.coordXYs;
            int coordCount = coordXYs.Length;
            //from user input coords
            //expand it
            List<float> expandCoords = new List<float>();
            int lim = coordCount - 2;
            for (int i = 0; i < lim;)
            {
                CreateLineSegment(expandCoords, coords[i], coords[i + 1], coords[i + 2], coords[i + 3]);
                i += 2;
            }
            //close coord
            CreateLineSegment(expandCoords, coords[coordCount - 2], coords[coordCount - 1], coords[0], coords[1]);
            //we need exact close the polygon
            CreateLineSegment(expandCoords, coords[0], coords[1], coords[0], coords[1]);
            borderTriangleStripCount = (coordCount + 2) * 2;
            this.smoothBorderTess = expandCoords.ToArray();
        }

        public float[] GetSmoothBorders()
        {
            if (smoothBorderTess == null)
            {
                BuildSmoothBorders();
            }
            return smoothBorderTess;
        }

        public float[] GetAreaTess(ref TessTool tess)
        {
            if (areaTess == null)
            {
                List<Vertex> vertextList = tess.TessPolygon(coordXYs);
                //-----------------------------   
                //switch how to fill polygon
                int j = vertextList.Count;
                float[] vtx = new float[j * 2];
                int n = 0;
                for (int p = 0; p < j; ++p)
                {
                    var v = vertextList[p];
                    vtx[n] = (float)v.m_X;
                    vtx[n + 1] = (float)v.m_Y;
                    n += 2;
                }
                //triangle list
                tessAreaTriangleCount = j;
                //-------------------------------------                              
                return this.areaTess = vtx;
            }
            return areaTess;
        }
        static void CreateLineSegment(List<float> coords, float x1, float y1, float x2, float y2)
        {
            //create wiht no line join
            float dx = x2 - x1;
            float dy = y2 - y1;
            float rad1 = (float)System.Math.Atan2(
                   y2 - y1,  //dy
                   x2 - x1); //dx
            coords.Add(x1); coords.Add(y1); coords.Add(0); coords.Add(rad1);
            coords.Add(x1); coords.Add(y1); coords.Add(1); coords.Add(rad1);
            coords.Add(x2); coords.Add(y2); coords.Add(0); coords.Add(rad1);
            coords.Add(x2); coords.Add(y2); coords.Add(1); coords.Add(rad1);
        }
    }
    struct TessTool
    {
        internal readonly Tesselate.Tesselator tess;
        internal readonly TessListener2 tessListener;
        public TessTool(Tesselate.Tesselator tess)
        {
            this.tess = tess;
            this.tessListener = new TessListener2();
            tessListener.Connect(tess, true);
        }
        public List<Vertex> TessPolygon(float[] vertex2dCoords)
        {
            int ncoords = vertex2dCoords.Length / 2;
            List<Vertex> vertexts = new List<Vertex>(ncoords);
            int nn = 0;
            for (int i = 0; i < ncoords; ++i)
            {
                vertexts.Add(new Vertex(vertex2dCoords[nn++], vertex2dCoords[nn++]));
            }
            //-----------------------
            tessListener.Reset(vertexts);
            //-----------------------
            tess.BeginPolygon();
            tess.BeginContour();
            int j = vertexts.Count;
            for (int i = 0; i < j; ++i)
            {
                Vertex v = vertexts[i];
                tess.AddVertex(v.m_X, v.m_Y, 0, i);
            }
            tess.EndContour();
            tess.EndPolygon();
            return tessListener.resultVertexList;
        }
    }

    class InternalGraphicsPath
    {
        internal List<Figure> figures = new List<Figure>();
        private InternalGraphicsPath()
        {
        }
        public static InternalGraphicsPath CreateGraphicsPath(VertexStoreSnap vxsSnap)
        {
            VertexSnapIter vxsIter = vxsSnap.GetVertexSnapIter();
            double prevX = 0;
            double prevY = 0;
            double prevMoveToX = 0;
            double prevMoveToY = 0;
            List<List<float>> allXYlist = new List<List<float>>(); //all include sub path
            List<float> xylist = new List<float>();
            allXYlist.Add(xylist);
            bool isAddToList = true;
            for (;;)
            {
                double x, y;
                VertexCmd cmd = vxsIter.GetNextVertex(out x, out y);
                switch (cmd)
                {
                    case PixelFarm.Agg.VertexCmd.MoveTo:
                        if (!isAddToList)
                        {
                            allXYlist.Add(xylist);
                            isAddToList = true;
                        }
                        prevMoveToX = prevX = x;
                        prevMoveToY = prevY = y;
                        xylist.Add((float)x);
                        xylist.Add((float)y);
                        break;
                    case PixelFarm.Agg.VertexCmd.LineTo:
                        xylist.Add((float)x);
                        xylist.Add((float)y);
                        prevX = x;
                        prevY = y;
                        break;
                    case PixelFarm.Agg.VertexCmd.CloseAndEndFigure:
                        //from current point 
                        xylist.Add((float)prevMoveToX);
                        xylist.Add((float)prevMoveToY);
                        prevX = prevMoveToX;
                        prevY = prevMoveToY;
                        //start the new one
                        xylist = new List<float>();
                        isAddToList = false;
                        break;
                    case PixelFarm.Agg.VertexCmd.EndFigure:
                        break;
                    case PixelFarm.Agg.VertexCmd.Stop:
                        goto EXIT_LOOP;
                    default:
                        throw new System.NotSupportedException();
                }
            }
            EXIT_LOOP:

            InternalGraphicsPath gfxPath = new InternalGraphicsPath();
            List<Figure> figures = new List<Figure>();
            int j = allXYlist.Count;
            for (int i = 0; i < j; ++i)
            {
                figures.Add(new Figure(allXYlist[i].ToArray()));
            }
            gfxPath.figures = figures;
            return gfxPath;
        }
    }
}