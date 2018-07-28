﻿//----------------------------------------------------------------------------
//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using PaintLab.Svg;
using PixelFarm.Drawing;
using LayoutFarm.Svg;
using LayoutFarm.Svg.Pathing;

namespace PixelFarm.CpuBlit
{
    public class SvgRenderVxDocBuilder
    {
        SvgDocument _svgdoc;
        List<SvgElement> _defsList = new List<SvgElement>();
        MySvgPathDataParser _pathDataParser = new MySvgPathDataParser();
        PixelFarm.CpuBlit.VertexProcessing.CurveFlattener _curveFlatter = new VertexProcessing.CurveFlattener();

        Dictionary<string, VgCmdClipPath> _clipPathDic = new Dictionary<string, VgCmdClipPath>();

        public VgRenderVx CreateRenderVx(SvgDocument svgdoc)
        {
            _svgdoc = svgdoc;

            int childCount = svgdoc.Root.ChildCount;
            List<VgCmd> cmds = new List<VgCmd>();

            for (int i = 0; i < childCount; ++i)
            {
                //translate SvgElement to  
                //command stream?
                RenderSvgElements(svgdoc.Root.GetChild(i), cmds);
            }
            VgRenderVx renderVx = new VgRenderVx(cmds.ToArray());
            return renderVx;
        }
        void RenderSvgElements(SvgElement elem, List<VgCmd> cmds)
        {
            switch (elem.WellknowElemName)
            {
                default:
                    throw new KeyNotFoundException();
                case WellknownSvgElementName.Unknown:
                    return;
                case WellknownSvgElementName.Svg:
                    break;
                case WellknownSvgElementName.Defs:
                    _defsList.Add(elem);
                    return;
                case WellknownSvgElementName.Rect:
                case WellknownSvgElementName.Polyline:
                case WellknownSvgElementName.Polygon:
                case WellknownSvgElementName.Ellipse:
                    break;
                case WellknownSvgElementName.Path:
                    RenderPathElement(elem, cmds);
                    return;
                case WellknownSvgElementName.ClipPath:
                    CreateClipPath(elem, cmds);
                    return;
                case WellknownSvgElementName.Group:
                    RenderGroupElement(elem, cmds);
                    return;
            }


            int childCount = elem.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                //translate SvgElement to  
                //command stream?
                RenderSvgElements(elem.GetChild(i), cmds);
            }
        }
        void CreateClipPath(SvgElement elem, List<VgCmd> cmds)
        {

            int childCount = elem.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                //translate SvgElement to  
                //command stream?
                RenderSvgElements(elem.GetChild(i), cmds);
            }
        }
        bool _buildDefs = false;
        void BuildDefinitionNodes()
        {
            if (_buildDefs)
            {
                return;
            }
            _buildDefs = true;

            int j = _defsList.Count;
            for (int i = 0; i < j; ++i)
            {
                SvgElement defsElem = _defsList[i];
                //get definition content
                int childCount = defsElem.ChildCount;
                for (int c = 0; c < childCount; ++c)
                {
                    SvgElement child = defsElem.GetChild(c);
                    if (child.WellknowElemName == WellknownSvgElementName.ClipPath)
                    {
                        //make this as a clip path
                        List<VgCmd> cmds = new List<VgCmd>();
                        VgCmdClipPath clipPath = new VgCmdClipPath();
                        RenderSvgElements(child, cmds);

                        clipPath._svgParts = cmds;
                        _clipPathDic.Add(child._visualSpec.Id, clipPath);
                    }
                }
            }
        }

        void AssignAttributes(SvgVisualSpec spec, List<VgCmd> cmds)
        {
            if (spec.HasFillColor)
            {
                cmds.Add(new VgCmdFillColor(spec.FillColor));
            }
            if (spec.HasStrokeColor)
            {
                cmds.Add(new VgCmdStrokeColor(spec.StrokeColor));
            }
            if (spec.HasStrokeWidth)
            {
                cmds.Add(new VgCmdStrokeWidth(spec.StrokeWidth.Number));
            }
            if (spec.Transform != null)
            {
                //convert from svg transform to 
                cmds.Add(new VgCmdAffineTransform(CreateAffine(spec.Transform)));
            }
            //
            if (spec.ClipPathLink != null)
            {
                //resolve this clip
                BuildDefinitionNodes();
                if (_clipPathDic.TryGetValue(spec.ClipPathLink.Value, out VgCmdClipPath clip))
                {
                    cmds.Add(clip);
                }
            }
        }
        void RenderPathElement(SvgElement elem, List<VgCmd> cmds)
        {
            SvgPathSpec pathSpec = elem._visualSpec as SvgPathSpec;
            //d             
            VgCmdPath pathCmd = new VgCmdPath();
            pathCmd.SetVxsAsOriginal(ParseSvgPathDefinitionToVxs(pathSpec.D.ToCharArray()));
            AssignAttributes(pathSpec, cmds);

            cmds.Add(pathCmd);
        }
        VertexStore ParseSvgPathDefinitionToVxs(char[] buffer)
        {
            using (VxsContext.Temp(out var flattenVxs))
            {
                VectorToolBox.GetFreePathWriter(out PathWriter pathWriter);
                _pathDataParser.SetPathWriter(pathWriter);
                _pathDataParser.Parse(buffer);
                _curveFlatter.MakeVxs(pathWriter.Vxs, flattenVxs);

                //create a small copy of the vxs 
                VectorToolBox.ReleasePathWriter(ref pathWriter);
                return flattenVxs.CreateTrim();
            }
        }

        static PixelFarm.CpuBlit.VertexProcessing.Affine CreateAffine(SvgTransform transformation)
        {
            switch (transformation.TransformKind)
            {
                default: throw new NotSupportedException();

                case SvgTransformKind.Matrix:

                    SvgTransformMatrix matrixTx = (SvgTransformMatrix)transformation;
                    float[] elems = matrixTx.Elements;
                    PixelFarm.CpuBlit.VertexProcessing.Affine affine = new VertexProcessing.Affine(
                        elems[0], elems[1],
                        elems[2], elems[3],
                        elems[4], elems[5]);
                    return affine;
                case SvgTransformKind.Rotation:
                    SvgRotate rotateTx = (SvgRotate)transformation;
                    return PixelFarm.CpuBlit.VertexProcessing.Affine.NewRotation(rotateTx.Angle);

                case SvgTransformKind.Scale:
                    SvgScale scaleTx = (SvgScale)transformation;
                    return PixelFarm.CpuBlit.VertexProcessing.Affine.NewScaling(scaleTx.X, scaleTx.Y);
                case SvgTransformKind.Shear:
                    SvgShear shearTx = (SvgShear)transformation;
                    return PixelFarm.CpuBlit.VertexProcessing.Affine.NewSkewing(shearTx.X, shearTx.Y);
                case SvgTransformKind.Translation:
                    SvgTranslate translateTx = (SvgTranslate)transformation;
                    return PixelFarm.CpuBlit.VertexProcessing.Affine.NewTranslation(translateTx.X, translateTx.Y);
            }
        }

        void RenderGroupElement(SvgElement elem, List<VgCmd> cmds)
        {
            var beginGroup = new VgCmdBeginGroup();
            cmds.Add(beginGroup);
            AssignAttributes(elem._visualSpec, cmds);
            int childCount = elem.ChildCount;
            for (int i = 0; i < childCount; ++i)
            {
                //translate SvgElement to  
                //command stream?
                RenderSvgElements(elem.GetChild(i), cmds);
            }

            cmds.Add(new VgCmdEndGroup());
        }
    }
    public static class SvgRenderVxDocBuilderExt
    {
        public static VgRenderVx CreateRenderVx(this SvgDocument svgdoc)
        {
            //create svg render vx from svgdoc
            //resolve the svg 
            SvgRenderVxDocBuilder builder = new SvgRenderVxDocBuilder();
            return builder.CreateRenderVx(svgdoc);
        }
    }

    class MySvgPathDataParser : SvgPathDataParser
    {
        PathWriter _writer;
        public void SetPathWriter(PathWriter writer)
        {
            this._writer = writer;
            _writer.StartFigure();
        }
        protected override void OnArc(float r1, float r2, float xAxisRotation, int largeArcFlag, int sweepFlags, float x, float y, bool isRelative)
        {

            //TODO: implement arc again
            throw new NotSupportedException();
            //base.OnArc(r1, r2, xAxisRotation, largeArcFlag, sweepFlags, x, y, isRelative);
        }
        protected override void OnCloseFigure()
        {
            _writer.CloseFigure();

        }
        protected override void OnCurveToCubic(
            float x1, float y1,
            float x2, float y2,
            float x, float y, bool isRelative)
        {

            if (isRelative)
            {
                _writer.Curve4Rel(x1, y1, x2, y2, x, y);
            }
            else
            {
                _writer.Curve4(x1, y1, x2, y2, x, y);
            }
        }
        protected override void OnCurveToCubicSmooth(float x2, float y2, float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.SmoothCurve4Rel(x2, y2, x, y);
            }
            else
            {
                _writer.SmoothCurve4(x2, y2, x, y);
            }

        }
        protected override void OnCurveToQuadratic(float x1, float y1, float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.Curve3Rel(x1, y1, x, y);
            }
            else
            {
                _writer.Curve3(x1, y1, x, y);
            }
        }
        protected override void OnCurveToQuadraticSmooth(float x, float y, bool isRelative)
        {
            if (isRelative)
            {
                _writer.SmoothCurve3Rel(x, y);
            }
            else
            {
                _writer.SmoothCurve3(x, y);
            }

        }
        protected override void OnHLineTo(float x, bool relative)
        {
            if (relative)
            {
                _writer.HorizontalLineToRel(x);
            }
            else
            {
                _writer.HorizontalLineTo(x);
            }
        }

        protected override void OnLineTo(float x, float y, bool relative)
        {
            if (relative)
            {
                _writer.LineToRel(x, y);
            }
            else
            {
                _writer.LineTo(x, y);
            }
        }
        protected override void OnMoveTo(float x, float y, bool relative)
        {

            if (relative)
            {
                _writer.MoveToRel(x, y);
            }
            else
            {


                _writer.MoveTo(x, y);
            }
        }
        protected override void OnVLineTo(float y, bool relative)
        {
            if (relative)
            {
                _writer.VerticalLineToRel(y);
            }
            else
            {
                _writer.VerticalLineTo(y);
            }
        }
    }
}