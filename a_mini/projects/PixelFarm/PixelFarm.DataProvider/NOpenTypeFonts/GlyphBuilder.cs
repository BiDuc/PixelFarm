﻿//MIT, 2016,  WinterDev
using System;
using System.Collections.Generic;
using NOpenType;
using PixelFarm.Agg.VertexSource;

namespace PixelFarm.Agg
{
    //this is PixelFarm version ***
    //render with MiniAgg

    public class GlyphPathBuilderVxs : NOpenType.GlyphPathBuilderBase
    {
        PixelFarm.Agg.VertexSource.PathWriter ps = new PixelFarm.Agg.VertexSource.PathWriter();
        public GlyphPathBuilderVxs(Typeface typeface)
            : base(typeface)
        {

        }
        protected override void OnBeginRead(int countourCount)
        {
            ps.Clear();
        }
        protected override void OnEndRead()
        {

        }
        protected override void OnCloseFigure()
        {
            ps.CloseFigure();
        }
        protected override void OnCurve3(short p2x, short p2y, short x, short y)
        {
            ps.Curve3(p2x, p2y, x, y);
        }
        protected override void OnCurve4(short p2x, short p2y, short p3x, short p3y, short x, short y)
        {
            ps.Curve4(p2x, p2y, p3x, p3y, x, y);
        }
        protected override void OnLineTo(short x, short y)
        {
            ps.LineTo(x, y);
        }
        protected override void OnMoveTo(short x, short y)
        {
            ps.MoveTo(x, y);
        }

        /// <summary>
        /// get processed/scaled vxs
        /// </summary>
        /// <returns></returns>
        public VertexStore GetVxs(VertexStore vxs)
        {
            float scale = TypeFace.CalculateScale(SizeInPoints);// (float)(SizeInPoints * Resolution) / (pointsPerInch * TypeFaceUnitPerEm);
            var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                //scale
             new PixelFarm.Agg.Transform.AffinePlan(
                 PixelFarm.Agg.Transform.AffineMatrixCommand.Scale, scale, scale),
                //translate
             new PixelFarm.Agg.Transform.AffinePlan(
                 PixelFarm.Agg.Transform.AffineMatrixCommand.Translate, 1, 1)
                 );

           
            var v1 = mat.TransformToVxs(ps.Vxs, s_vxsPool.GetFreeVxs());
            curveFlattener.MakeVxs(v1, vxs);
            s_vxsPool.Release(ref v1);
            return vxs;
        }
        public VertexStore GetUnscaledVxs()
        {
            return VertexStore.CreateCopy(ps.Vxs);
        }

        static VertexStorePool s_vxsPool = new VertexStorePool();
        static CurveFlattener curveFlattener = new CurveFlattener();
    }


}