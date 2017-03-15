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

namespace PixelFarm.Agg
{


    class StrokeGenerator
    {
        StrokeMath m_stroker;
        MultipartVerextDistanceList multipartVertexDistanceList = new MultipartVerextDistanceList();
        VertexStore m_out_vertices;
        double m_shorten;
        bool m_closed;
        StrokeMath.Status m_status;
        StrokeMath.Status m_prev_status;
        int m_src_vertex;
        int m_out_vertex;
        public StrokeGenerator()
        {
            m_stroker = new StrokeMath();
            m_out_vertices = new VertexStore();
            m_status = StrokeMath.Status.Init;
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
            m_status = StrokeMath.Status.Init;

        }
        public void AddVertex(double x, double y, VertexCmd cmd)
        {
            //TODO: review 
            m_status = StrokeMath.Status.Init;
            switch (cmd)
            {
                case VertexCmd.MoveTo:


                    multipartVertexDistanceList.AddMoveTo(x, y);
                    break;
                case VertexCmd.Close:
                case VertexCmd.CloseAndEndFigure:
                    m_closed = true;
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
                VertexCmd cmd = GetNextVertex(ref x, ref y);
                if (cmd == VertexCmd.NoMore)
                {
                    if (currentRangeIndex + 1 < multipartVertexDistanceList.RangeCount)
                    {
                        //move to next range
                        multipartVertexDistanceList.SetRangeIndex(currentRangeIndex + 1);
                        currentRangeIndex++;

                        m_status = StrokeMath.Status.Ready;
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
            if (m_status == StrokeMath.Status.Init)
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
            m_status = StrokeMath.Status.Ready;
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

        VertexCmd GetNextVertex(ref double x, ref double y)
        {
            VertexCmd cmd = VertexCmd.LineTo;
            do
            {
                switch (m_status)
                {
                    case StrokeMath.Status.Init:
                        this.Rewind();
                        goto case StrokeMath.Status.Ready;
                    case StrokeMath.Status.Ready:

                        if (multipartVertexDistanceList.CurrentRangeLen < 2 + (m_closed ? 1 : 0))
                        {
                            cmd = VertexCmd.NoMore;
                            break;
                        }
                        m_status = m_closed ? StrokeMath.Status.Outline1 : StrokeMath.Status.Cap1;
                        cmd = VertexCmd.MoveTo;
                        m_src_vertex = 0;
                        m_out_vertex = 0;
                        break;
                    case StrokeMath.Status.Cap1:
                        {
                            Vertex2d v0, v1;

                            multipartVertexDistanceList.GetFirst2(out v0, out v1);
                            m_stroker.CreateCap(
                                m_out_vertices,
                                v0,
                                v1,
                                v0.CalLen(v1));

                            m_src_vertex = 1;
                            m_prev_status = StrokeMath.Status.Outline1;
                            m_status = StrokeMath.Status.OutVertices;
                            m_out_vertex = 0;
                        }
                        break;
                    case StrokeMath.Status.Cap2:
                        {
                            Vertex2d beforeLast, last;
                            multipartVertexDistanceList.GetLast2(out beforeLast, out last);
                            m_stroker.CreateCap(m_out_vertices,
                                last,
                                beforeLast,
                                beforeLast.CalLen(last));
                            m_prev_status = StrokeMath.Status.Outline2;
                            m_status = StrokeMath.Status.OutVertices;
                            m_out_vertex = 0;
                        }
                        break;
                    case StrokeMath.Status.Outline1:
                        {
                            if (m_closed)
                            {
                                if (m_src_vertex >= multipartVertexDistanceList.CurrentRangeLen)
                                {
                                    m_prev_status = StrokeMath.Status.CloseFirst;
                                    m_status = StrokeMath.Status.EndPoly1;
                                    break;
                                }
                            }
                            else
                            {
                                if (m_src_vertex >= multipartVertexDistanceList.CurrentRangeLen - 1)
                                {
                                    m_status = StrokeMath.Status.Cap2;
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
                            m_status = StrokeMath.Status.OutVertices;
                            m_out_vertex = 0;

                        }
                        break;
                    case StrokeMath.Status.CloseFirst:
                        m_status = StrokeMath.Status.Outline2;
                        cmd = VertexCmd.MoveTo;
                        goto case StrokeMath.Status.Outline2;
                    case StrokeMath.Status.Outline2:
                        {
                            if (m_src_vertex <= (!m_closed ? 1 : 0))
                            {
                                m_status = StrokeMath.Status.EndPoly2;
                                m_prev_status = StrokeMath.Status.Stop;
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
                            m_status = StrokeMath.Status.OutVertices;
                            m_out_vertex = 0;

                        }
                        break;
                    case StrokeMath.Status.OutVertices:
                        if (m_out_vertex >= m_out_vertices.Count)
                        {
                            m_status = m_prev_status;
                        }
                        else
                        {
                            m_out_vertices.GetVertex(m_out_vertex++, out x, out y);
                            //Vector2 c = m_out_vertices[(int)m_out_vertex++];
                            //x = c.x;
                            //y = c.y;
                            return cmd;
                        }
                        break;
                    case StrokeMath.Status.EndPoly1:
                        m_status = m_prev_status;
                        x = (int)EndVertexOrientation.CCW;
                        return VertexCmd.Close;
                    case StrokeMath.Status.EndPoly2:
                        m_status = m_prev_status;
                        x = (int)EndVertexOrientation.CW;
                        return VertexCmd.Close;
                    case StrokeMath.Status.Stop:
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
            if (_ranges.Count >= 83)
            {

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
            get { return _range.len; }
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
            if (idx > 1 && idx + 2 <= _range.Count)
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