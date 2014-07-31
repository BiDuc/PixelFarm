﻿/*
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
#define USE_OPENGL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using MatterHackers.VectorMath;
using MatterHackers.Agg.Transform;
using MatterHackers.Agg;
using MatterHackers.Agg.VertexSource;
using MatterHackers.Agg.UI;

#if USE_OPENGL
using OpenTK.Graphics.OpenGL;
#endif

namespace MatterHackers.GCodeVisualizer
{
    [Flags]
    public enum RenderType
    {
        None = 0,
        Extrusions = 1,
        Moves = 2,
        Retractions = 4,
        All = Extrusions | Moves | Retractions
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorVertexData
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public float normalX;
        public float normalY;
        public float normalZ;

        public float positionX;
        public float positionY;
        public float positionZ;

        public static readonly int Stride = Marshal.SizeOf(default(ColorVertexData));

        public ColorVertexData(Vector3 position, Vector3 normal, RGBA_Bytes color)
        {
            r = (float)color.Red0To1;
            g = (float)color.Green0To1;
            b = (float)color.Blue0To1;
            a = (float)color.Alpha0To1;

            normalX = (float)normal.x;
            normalY = (float)normal.y;
            normalZ = (float)normal.z;

            positionX = (float)position.x;
            positionY = (float)position.y;
            positionZ = (float)position.z;
        }
    }

    public abstract class RenderFeatureBase
    {
        public abstract void Render(Graphics2D graphics2D, Affine transform, double layerScale, RenderType renderType);
        public abstract void CreateRender3DData(VectorPOD<ColorVertexData> colorVertexData, VectorPOD<uint> indexData, Affine transform, double layerScale, RenderType renderType);

        static public void CreateCylinder(VectorPOD<ColorVertexData> colorVertexData, VectorPOD<uint> indexData, Vector3 startPos, Vector3 endPos, double radius, int steps, RGBA_Bytes color, double layerHeight)
        {
            Vector3 direction = endPos - startPos;
            Vector3 directionNormal = direction.GetNormal();
            Vector3 startSweepDirection = Vector3.GetPerpendicular(startPos, endPos).GetNormal();

            uint[] tubeStartIndices = new uint[steps];
            uint[] tubeEndIndices = new uint[steps];

            uint[] capStartIndices = new uint[steps];
            uint[] capEndIndices = new uint[steps];

            double halfHeight = layerHeight / 2 + (layerHeight * .1);
            double halfWidth = (radius * radius) / halfHeight;
            double zScale = halfHeight/radius;
            double xScale = halfWidth/radius;

            Vector3 scale = new Vector3(xScale, xScale, zScale);

            for (int i = 0; i < steps; i++)
            {
                // create tube ends verts
                Vector3 tubeNormal = Vector3.Transform(startSweepDirection, Matrix4X4.CreateRotation(direction, MathHelper.Tau / (steps * 2) + MathHelper.Tau / (steps) * i));
                Vector3 offset = Vector3.Transform(startSweepDirection * radius, Matrix4X4.CreateRotation(direction, MathHelper.Tau / (steps * 2) + MathHelper.Tau / (steps) * i));
                offset *= scale;
                
                Vector3 tubeStart = startPos + offset;
                tubeStartIndices[i] = (uint)colorVertexData.Count;
                colorVertexData.Add(new ColorVertexData(tubeStart, tubeNormal, color));
                
                Vector3 tubeEnd = endPos + offset;
                tubeEndIndices[i] = (uint)colorVertexData.Count;
                colorVertexData.Add(new ColorVertexData(tubeEnd, tubeNormal, color));

                // create cap verts
                Vector3 rotateAngle = Vector3.Cross(direction, startSweepDirection);
                Vector3 capStartNormal = Vector3.Transform(startSweepDirection, Matrix4X4.CreateRotation(rotateAngle, MathHelper.Tau / 8));
                capStartNormal = Vector3.Transform(capStartNormal, Matrix4X4.CreateRotation(direction, MathHelper.Tau / (steps * 2) + MathHelper.Tau / (steps) * i));
                capStartNormal = (capStartNormal * scale).GetNormal();
                Vector3 capStartOffset = capStartNormal * radius;
                capStartOffset *= scale;
                Vector3 capStart = startPos + capStartOffset;
                capStartIndices[i] = (uint)colorVertexData.Count;
                colorVertexData.Add(new ColorVertexData(capStart, capStartNormal, color));

                Vector3 capEndNormal = Vector3.Transform(startSweepDirection, Matrix4X4.CreateRotation(-rotateAngle, MathHelper.Tau / 8));
                capEndNormal = Vector3.Transform(capEndNormal, Matrix4X4.CreateRotation(direction, MathHelper.Tau / (steps * 2) + MathHelper.Tau / (steps) * i));
                capEndNormal = (capEndNormal * scale).GetNormal();
                Vector3 capEndOffset = capEndNormal * radius;
                capEndOffset *= scale;
                Vector3 capEnd = endPos + capEndOffset;
                capEndIndices[i] = (uint)colorVertexData.Count;
                colorVertexData.Add(new ColorVertexData(capEnd, capEndNormal, color));
            }

            uint tipStartIndex = (uint)colorVertexData.Count;
            Vector3 tipOffset = directionNormal * radius;
            tipOffset *= scale;
            colorVertexData.Add(new ColorVertexData(startPos - tipOffset, -directionNormal, color));
            uint tipEndIndex = (uint)colorVertexData.Count;
            colorVertexData.Add(new ColorVertexData(endPos + tipOffset, directionNormal, color));

            for (int i = 0; i < steps; i++)
            {
                // create tube polys
                indexData.Add(tubeStartIndices[i]);
                indexData.Add(tubeEndIndices[i]);
                indexData.Add(tubeEndIndices[(i + 1) % steps]);

                indexData.Add(tubeStartIndices[i]);
                indexData.Add(tubeEndIndices[(i + 1) % steps]);
                indexData.Add(tubeStartIndices[(i + 1) % steps]);

                // create start cap polys
                indexData.Add(tubeStartIndices[i]);
                indexData.Add(capStartIndices[i]);
                indexData.Add(capStartIndices[(i + 1) % steps]);

                indexData.Add(tubeStartIndices[i]);
                indexData.Add(capStartIndices[(i + 1) % steps]);
                indexData.Add(tubeStartIndices[(i + 1) % steps]);

                // create end cap polys
                indexData.Add(tubeEndIndices[i]);
                indexData.Add(capEndIndices[i]);
                indexData.Add(capEndIndices[(i + 1) % steps]);

                indexData.Add(tubeEndIndices[i]);
                indexData.Add(capEndIndices[(i + 1) % steps]);
                indexData.Add(tubeEndIndices[(i + 1) % steps]);

                // create start tip polys
                indexData.Add(tipStartIndex);
                indexData.Add(capStartIndices[i]);
                indexData.Add(capStartIndices[(i + 1) % steps]);

                // create end tip polys
                indexData.Add(tipEndIndex);
                indexData.Add(capEndIndices[i]);
                indexData.Add(capEndIndices[(i + 1) % steps]);
            }
        }

        static public void CreatePointer(VectorPOD<ColorVertexData> colorVertexData, VectorPOD<uint> indexData, Vector3 startPos, Vector3 endPos, double radius, int steps, RGBA_Bytes color)
        {
            Vector3 direction = endPos - startPos;
            Vector3 directionNormal = direction.GetNormal();
            Vector3 startSweepDirection = Vector3.GetPerpendicular(startPos, endPos).GetNormal();

            uint[] tubeStartIndices = new uint[steps];

            for (int i = 0; i < steps; i++)
            {
                // create tube ends verts
                Vector3 tubeNormal = Vector3.Transform(startSweepDirection, Matrix4X4.CreateRotation(direction, MathHelper.Tau / (steps * 2) + MathHelper.Tau / (steps) * i));
                Vector3 offset = Vector3.Transform(startSweepDirection * radius, Matrix4X4.CreateRotation(direction, MathHelper.Tau / (steps * 2) + MathHelper.Tau / (steps) * i));
                Vector3 tubeStart = startPos + offset;
                tubeStartIndices[i] = (uint)colorVertexData.Count;
                colorVertexData.Add(new ColorVertexData(tubeStart, tubeNormal, color));
                Vector3 tubeEnd = endPos + offset;
            }

            uint tipEndIndex = (uint)colorVertexData.Count;
            colorVertexData.Add(new ColorVertexData(endPos, directionNormal, color));

            for (int i = 0; i < steps; i++)
            {
                // create tube polys
                indexData.Add(tubeStartIndices[i]);
                indexData.Add(tubeStartIndices[(i + 1) % steps]);

                indexData.Add(tipEndIndex);
            }
        }
    }

    public class RenderFeatureRetract : RenderFeatureBase
    {
        public static double RetractionDrawRadius = 1;

        float extrusionAmount;
        float mmPerSecond;
        Vector3Float position;
        public RenderFeatureRetract(Vector3 position, double extrusionAmount, double mmPerSecond)
        {
            this.extrusionAmount = (float)extrusionAmount;
            this.mmPerSecond = (float)mmPerSecond;
            this.position = new Vector3Float(position);
        }

        private double Radius(double layerScale)
        {
            double radius = RetractionDrawRadius * layerScale;
            double area = Math.PI * radius * radius;
            area *= Math.Abs(extrusionAmount);
            radius = Math.Sqrt(area / Math.PI);
            return radius;
        }

        public override void CreateRender3DData(VectorPOD<ColorVertexData> colorVertexData, VectorPOD<uint> indexData, Affine transform, double layerScale, RenderType renderType)
        {
            if ((renderType & RenderType.Retractions) == RenderType.Retractions)
            {
                Vector3 position = new Vector3(this.position);
                if (extrusionAmount > 0)
                {
                    // unretraction
                    CreatePointer(colorVertexData, indexData, position + new Vector3(0, 0, 1.3), position + new Vector3(0, 0, .3), Radius(1), 5, RGBA_Bytes.Blue);
                }
                else
                {
                    // retraction
                    CreatePointer(colorVertexData, indexData, position + new Vector3(0, 0, .3), position + new Vector3(0, 0, 1.3), Radius(1), 5, RGBA_Bytes.Red);
                }
            }
        }

        public override void Render(Graphics2D graphics2D, Affine transform, double layerScale, RenderType renderType)
        {
            if ((renderType & RenderType.Retractions) == RenderType.Retractions)
            {
                Vector2 position = new Vector2(this.position.x, this.position.y);
                transform.transform(ref position);
                double radius = Radius(layerScale);
                Ellipse extrusion = new Ellipse(position, radius);

                if (extrusionAmount > 0)
                {
                    // unretraction
                    graphics2D.Render(extrusion, new RGBA_Bytes(RGBA_Bytes.Blue, 200));
                }
                else
                {
                    // retraction
                    graphics2D.Render(extrusion, new RGBA_Bytes(RGBA_Bytes.Red, 200));
                }
            }
        }
    }

    public class RenderFeatureTravel : RenderFeatureBase
    {
        protected Vector3Float start;
        protected Vector3Float end;
        protected float travelSpeed;

        public RenderFeatureTravel(Vector3 start, Vector3 end, double travelSpeed)
        {
            this.start = new Vector3Float(start);
            this.end = new Vector3Float(end);
            this.travelSpeed = (float)travelSpeed;
        }

        public override void CreateRender3DData(VectorPOD<ColorVertexData> colorVertexData, VectorPOD<uint> indexData, Affine transform, double layerScale, RenderType renderType)
        {
            if ((renderType & RenderType.Moves) == RenderType.Moves)
            {
                CreateCylinder(colorVertexData, indexData, new Vector3(start), new Vector3(end), .1, 6, GCodeRenderer.TravelColor, .2);
            }
        }

        public override void Render(Graphics2D graphics2D, Affine transform, double layerScale, RenderType renderType)
        {
            if ((renderType & RenderType.Moves) == RenderType.Moves)
            {
                double movementLineWidth = 0.35 * layerScale;
                RGBA_Bytes movementColor = new RGBA_Bytes(10, 190, 15);

                PathStorage pathStorage = new PathStorage();
                VertexSourceApplyTransform transformedPathStorage = new VertexSourceApplyTransform(pathStorage, transform);
                Stroke stroke = new Stroke(transformedPathStorage, movementLineWidth);

                stroke.line_cap(LineCap.Round);
                stroke.line_join(LineJoin.Round);

                pathStorage.Add(start.x, start.y, ShapePath.FlagsAndCommand.CommandMoveTo);
                if (end.x != start.x || end.y != start.y)
                {
                    pathStorage.Add(end.x, end.y, ShapePath.FlagsAndCommand.CommandLineTo);
                }
                else
                {
                    pathStorage.Add(end.x + .01, end.y, ShapePath.FlagsAndCommand.CommandLineTo);
                }
                graphics2D.Render(stroke, 0, movementColor);
            }
        }
    }

    public class ExtrusionColors
    {
        SortedDictionary<float, RGBA_Bytes> speedColorLookup = new SortedDictionary<float, RGBA_Bytes>();

        public RGBA_Bytes GetColorForSpeed(float speed)
        {
            if (speed > 0)
            {
                double startColor = 223.0 / 360.0;
                double endColor = 5.0 / 360.0;
                double delta = startColor - endColor;

                if (!speedColorLookup.ContainsKey(speed))
                {
                    RGBA_Bytes color = RGBA_Floats.FromHSL(startColor, .99, .49).GetAsRGBA_Bytes();
                    speedColorLookup.Add(speed, color);

                    if (speedColorLookup.Count > 1)
                    {
                        double step = delta / (speedColorLookup.Count - 1);
                        for (int index = 0; index < speedColorLookup.Count; index++)
                        {
                            double offset = step * index;
                            double fixedColor = startColor - offset;
                            KeyValuePair<float, RGBA_Bytes> keyValue = speedColorLookup.ElementAt(index);
                            speedColorLookup[keyValue.Key] = RGBA_Floats.FromHSL(fixedColor, .99, .49).GetAsRGBA_Bytes();
                        }
                    }
                }

                return speedColorLookup[speed];
            }

            return RGBA_Bytes.Black;
        }
    }

    public class RenderFeatureExtrusion : RenderFeatureTravel
    {
        float extrusionVolumeMm3;
        float layerHeight;
        RGBA_Bytes color;

        public RenderFeatureExtrusion(Vector3 start, Vector3 end, double travelSpeed, double totalExtrusionMm, double filamentDiameterMm, double layerHeight, RGBA_Bytes color)
            : base(start, end, travelSpeed)
        {
            this.color = color;
            double fillamentRadius = filamentDiameterMm / 2;
            double areaSquareMm = (fillamentRadius * fillamentRadius) * Math.PI;

            this.extrusionVolumeMm3 = (float)(areaSquareMm * totalExtrusionMm);
            this.layerHeight = (float)layerHeight;
        }

        public override void CreateRender3DData(VectorPOD<ColorVertexData> colorVertexData, VectorPOD<uint> indexData, Affine transform, double layerScale, RenderType renderType)
        {
            if ((renderType & RenderType.Extrusions) == RenderType.Extrusions)
            {
                double area = extrusionVolumeMm3 / ((end - start).Length);
                double radius = Math.Sqrt(area / Math.PI);
#if false
                CreateCylinder(colorVertexData, indexData, new Vector3(start), new Vector3(end), radius, 6, GCodeRenderer.ExtrusionColor, layerHeight);
#else
                CreateCylinder(colorVertexData, indexData, new Vector3(start), new Vector3(end), radius, 6, color, layerHeight);
#endif
            }
        }

        public override void Render(Graphics2D graphics2D, Affine transform, double layerScale, RenderType renderType)
        {
            if ((renderType & RenderType.Extrusions) == RenderType.Extrusions)
            {
                double extrusionLineWidths = 0.2 * layerScale;
                RGBA_Bytes extrusionColor = RGBA_Bytes.Black;
                //extrusionColor = color;

                PathStorage pathStorage = new PathStorage();
                VertexSourceApplyTransform transformedPathStorage = new VertexSourceApplyTransform(pathStorage, transform);
                Stroke stroke = new Stroke(transformedPathStorage, extrusionLineWidths);

                stroke.line_cap(LineCap.Round);
                stroke.line_join(LineJoin.Round);

                pathStorage.Add(start.x, start.y, ShapePath.FlagsAndCommand.CommandMoveTo);
                pathStorage.Add(end.x, end.y, ShapePath.FlagsAndCommand.CommandLineTo);

                graphics2D.Render(stroke, 0, extrusionColor);
            }
        }
    }

    public class GCodeRenderer
    {
        VectorPOD<ColorVertexData> colorVertexData = new VectorPOD<ColorVertexData>();
        VectorPOD<uint> vertexIndexArray = new VectorPOD<uint>();
        List<int> layerStartIndex = new List<int>();
        List<List<int>> featureStartIndex = new List<List<int>>();
        List<List<RenderFeatureBase>> renderFeatures = new List<List<RenderFeatureBase>>();
        int TotalRenderFeatures = 0;

        public static RGBA_Bytes ExtrusionColor = RGBA_Bytes.White;
        public static RGBA_Bytes TravelColor = RGBA_Bytes.Green;

        GCodeFile gCodeFileToDraw;

        ExtrusionColors extrusionColors;

        public GCodeRenderer(GCodeFile gCodeFileToDraw)
        {
            this.gCodeFileToDraw = gCodeFileToDraw;

            for (int i = 0; i < gCodeFileToDraw.NumChangesInZ; i++)
            {
                renderFeatures.Add(new List<RenderFeatureBase>());
            }
        }

        void CreateFeaturesForLayerIfRequired(int layerToCreate)
        {
            if (extrusionColors == null)
            {
                extrusionColors = new ExtrusionColors();
                for (int layerIndex = 0; layerIndex < gCodeFileToDraw.NumChangesInZ; layerIndex++)
                {
                    for (int i = 1; i < gCodeFileToDraw.Count; i++)
                    {
                        PrinterMachineInstruction prevInstruction = gCodeFileToDraw.Instruction(i-1);
                        PrinterMachineInstruction instruction = gCodeFileToDraw.Instruction(i);
                        if (instruction.EPosition > prevInstruction.EPosition)
                        {
                            extrusionColors.GetColorForSpeed((float)instruction.FeedRate);
                        }
                    }
                }
            }

            if (renderFeatures[layerToCreate].Count > 0)
            {
                return;
            }

            List<RenderFeatureBase> renderFeaturesForLayer = renderFeatures[layerToCreate];

            int startRenderIndex = gCodeFileToDraw.IndexOfChangeInZ[layerToCreate];
            int endRenderIndex = gCodeFileToDraw.Count - 1;
            if (layerToCreate < gCodeFileToDraw.IndexOfChangeInZ.Count - 1)
            {
                endRenderIndex = gCodeFileToDraw.IndexOfChangeInZ[layerToCreate + 1];
            }

            for (int i = startRenderIndex; i < endRenderIndex; i++ )
            {
                PrinterMachineInstruction currentInstruction = gCodeFileToDraw.Instruction(i);
                PrinterMachineInstruction previousInstruction = currentInstruction;
                if (i > 0)
                {
                    previousInstruction = gCodeFileToDraw.Instruction(i - 1);
                }

                if (currentInstruction.Position == previousInstruction.Position)
                {
                    if (Math.Abs(currentInstruction.EPosition - previousInstruction.EPosition) > 0)
                    {
                        // this is a retraction
                        renderFeaturesForLayer.Add(new RenderFeatureRetract(currentInstruction.Position, currentInstruction.EPosition - previousInstruction.EPosition, currentInstruction.FeedRate));
                    }
                    if (currentInstruction.Line.StartsWith("G10"))
                    {
                        renderFeaturesForLayer.Add(new RenderFeatureRetract(currentInstruction.Position, -1, currentInstruction.FeedRate));
                    }
                    else if (currentInstruction.Line.StartsWith("G11"))
                    {
                        renderFeaturesForLayer.Add(new RenderFeatureRetract(currentInstruction.Position, 1, currentInstruction.FeedRate));
                    }
                }
                else
                {
                    if (gCodeFileToDraw.IsExtruding(i))
                    {
                        double layerThickness = gCodeFileToDraw.GetLayerHeight();
                        if (layerToCreate == 0)
                        {
                            layerThickness = gCodeFileToDraw.GetFirstLayerHeight();
                        }
                        renderFeaturesForLayer.Add(new RenderFeatureExtrusion(previousInstruction.Position, currentInstruction.Position, currentInstruction.FeedRate, currentInstruction.EPosition - previousInstruction.EPosition, gCodeFileToDraw.GetFilamentDiamter(), layerThickness, extrusionColors.GetColorForSpeed((float)currentInstruction.FeedRate)));
                    }
                    else
                    {
                        renderFeaturesForLayer.Add(new RenderFeatureTravel(previousInstruction.Position, currentInstruction.Position, currentInstruction.FeedRate));
                    }
                }
            }

            TotalRenderFeatures += renderFeaturesForLayer.Count;
        }

        public int GetNumFeatures(int layerToCountFeaturesOn)
        {
            CreateFeaturesForLayerIfRequired(layerToCountFeaturesOn);
            return renderFeatures[layerToCountFeaturesOn].Count;
        }

        public void Render(Graphics2D graphics2D, int activeLayerIndex, Affine transform, double layerScale, RenderType renderType,
            double featureToStartOnRatio0To1, double featureToEndOnRatio0To1)
        {
            if (renderFeatures.Count > 0)
            {
                CreateFeaturesForLayerIfRequired(activeLayerIndex);

                int featuresOnLayer = renderFeatures[activeLayerIndex].Count;
                int endFeature = (int)(featuresOnLayer * featureToEndOnRatio0To1 + .5);
                endFeature = Math.Max(0, Math.Min(endFeature, featuresOnLayer));

                int startFeature = (int)(featuresOnLayer * featureToStartOnRatio0To1 + .5);
                startFeature = Math.Max(0, Math.Min(startFeature, featuresOnLayer));

                // try to make sure we always draw at least one feature
                if (endFeature <= startFeature)
                {
                    endFeature = Math.Min(startFeature + 1, featuresOnLayer);
                }
                if (startFeature >= endFeature)
                {
                    // This can only happen if the sart and end are set to the last feature
                    // Try to set the start feture to one from the end
                    startFeature = Math.Max(endFeature - 1, 0);
                }

                for (int i = startFeature; i < endFeature; i++)
                {
                    RenderFeatureBase feature = renderFeatures[activeLayerIndex][i];
                    feature.Render(graphics2D, transform, layerScale, renderType);
                }
            }
        }

#if USE_OPENGL
        void Create3DData(Affine transform, double layerScale, RenderType renderType, int lastLayerIndex)
        {
            colorVertexData.Clear();
            vertexIndexArray.Clear();
            layerStartIndex.Clear();
            layerStartIndex.Capacity = gCodeFileToDraw.NumChangesInZ;
            featureStartIndex.Clear();
            layerStartIndex.Capacity = gCodeFileToDraw.NumChangesInZ;

            bool canOnlyShowOneLayer = TotalRenderFeatures > MAX_RENDER_FEATURES_TO_ALLOW_3D;
            if (canOnlyShowOneLayer)
            {
                layerStartIndex.Add(vertexIndexArray.Count);

                for (int layerIndex = 0; layerIndex < lastLayerIndex+1; layerIndex++)
                {
                    featureStartIndex.Add(new List<int>());
                }

                for (int i = 0; i < renderFeatures[lastLayerIndex].Count; i++)
                {
                    featureStartIndex[lastLayerIndex].Add(vertexIndexArray.Count);
                    RenderFeatureBase feature = renderFeatures[lastLayerIndex][i];
                    feature.CreateRender3DData(colorVertexData, vertexIndexArray, transform, layerScale, renderType);
                }

                singleLayerIndex = lastLayerIndex;
            }
            else
            {
                for (int layerIndex = 0; layerIndex < gCodeFileToDraw.NumChangesInZ; layerIndex++)
                {
                    layerStartIndex.Add(vertexIndexArray.Count);
                    featureStartIndex.Add(new List<int>());

                    for (int i = 0; i < renderFeatures[layerIndex].Count; i++)
                    {
                        featureStartIndex[layerIndex].Add(vertexIndexArray.Count);
                        RenderFeatureBase feature = renderFeatures[layerIndex][i];
                        feature.CreateRender3DData(colorVertexData, vertexIndexArray, transform, layerScale, renderType);
                    }
                }
            }
        }

        static readonly int MAX_RENDER_FEATURES_TO_ALLOW_3D = 250000;
        VertexBuffer vertexBuffer;
        RenderType lastRenderType = RenderType.None;
        int singleLayerIndex = 0;
        public void Render3D(int startLayerIndex, int endLayerIndex, Affine transform, double layerScale, RenderType renderType,
            double featureToStartOnRatio0To1, double featureToEndOnRatio0To1)
        {
            for (int layerIndex = 0; layerIndex < gCodeFileToDraw.NumChangesInZ; layerIndex++)
            {
                CreateFeaturesForLayerIfRequired(layerIndex);
            }

            if (renderFeatures.Count > 0)
            {
                bool canOnlyShowOneLayer = TotalRenderFeatures > MAX_RENDER_FEATURES_TO_ALLOW_3D;

                // If its the first render or we change what we are trying to render then create vertex data.
                if (colorVertexData.Count == 0 || lastRenderType != renderType
                    || (canOnlyShowOneLayer && endLayerIndex-1 != singleLayerIndex))
                {
                    Create3DData(transform, layerScale, renderType, endLayerIndex-1);
                    
                    vertexBuffer = new VertexBuffer();
                    vertexBuffer.SetVertexData(colorVertexData.Array);
                    vertexBuffer.SetIndexData(vertexIndexArray.Array);

                    lastRenderType = renderType;
                }

                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.PushAttrib(AttribMask.EnableBit);
                GL.Enable(EnableCap.PolygonSmooth);

                //GL.InterleavedArrays(InterleavedArrayFormat.C4fN3fV3f, 0, colorVertexData.Array);

                // draw all the layers from start to end-2
                if (endLayerIndex - 1 > startLayerIndex && !canOnlyShowOneLayer)
                {
                    int ellementCount = layerStartIndex[endLayerIndex - 1] - layerStartIndex[startLayerIndex];

                    vertexBuffer.renderRange(layerStartIndex[startLayerIndex], ellementCount);
                }

                // draw the partial layer of end-1 from startratio to endratio
                {
                    int layerIndex = endLayerIndex - 1;
                    int featuresOnLayer = renderFeatures[layerIndex].Count;
                    int startFeature = (int)(featuresOnLayer * featureToStartOnRatio0To1 + .5);
                    startFeature = Math.Max(0, Math.Min(startFeature, featuresOnLayer));

                    int endFeature = (int)(featuresOnLayer * featureToEndOnRatio0To1 + .5);
                    endFeature = Math.Max(0, Math.Min(endFeature, featuresOnLayer));

                    // try to make sure we always draw at least one feature
                    if (endFeature <= startFeature)
                    {
                        endFeature = Math.Min(startFeature + 1, featuresOnLayer);
                    }
                    if (startFeature >= endFeature)
                    {
                        // This can only happen if the sart and end are set to the last feature
                        // Try to set the start feture to one from the end
                        startFeature = Math.Max(endFeature - 1, 0);
                    }

                    if (endFeature > startFeature)
                    {
                        int ellementCount = featureStartIndex[layerIndex][endFeature - 1] - featureStartIndex[layerIndex][startFeature];

                        vertexBuffer.renderRange(featureStartIndex[layerIndex][startFeature], ellementCount);
                    }
                }

                GL.PopAttrib();
            }
        }
#endif
    }

#if USE_OPENGL
    public class VertexBuffer
    {
        public int myVertexId;
        public int myIndexId;
        public BeginMode myMode = BeginMode.Triangles;
        public uint myVertexLength;
        public uint myIndexLength;
        public VertexBuffer()
        {
            GL.GenBuffers(1, out myVertexId);
            GL.GenBuffers(1, out myIndexId);
        }

        ~VertexBuffer()
        {
            if (myVertexId != -1)
            {
                int holdVertexId = myVertexId;
                int holdIndexId = myIndexId;
                UiThread.RunOnIdle( (state) => 
                {
                    GL.DeleteBuffers(1, ref holdVertexId);
                    GL.DeleteBuffers(1, ref holdIndexId);
                } );
            }
        }

        public void SetVertexData(ColorVertexData[] data)
        {
            SetVertexData(data, (uint)data.Length);
        }

        public void SetVertexData(ColorVertexData[] data, uint count)
        {
            myVertexLength = count;
            GL.BindBuffer(BufferTarget.ArrayBuffer, myVertexId);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(data.Length * ColorVertexData.Stride), data, BufferUsageHint.StaticDraw);
        }

        public void SetIndexData(uint[] data)
        {
            SetIndexData(data, (ushort)data.Length);
        }

        public void SetIndexData(uint[] data, uint count)
        {
            myIndexLength = count;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, myIndexId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(data.Length * sizeof(uint)), data, BufferUsageHint.DynamicDraw);
        }
        
        public void renderRange(int offset, int count)
        {
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.VertexArray);

            GL.EnableClientState(ArrayCap.IndexArray);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, myVertexId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, myIndexId);
            
            GL.ColorPointer(4, ColorPointerType.Float, ColorVertexData.Stride, new IntPtr(0));
            GL.NormalPointer(NormalPointerType.Float, ColorVertexData.Stride, new IntPtr(4 * 4));
            GL.VertexPointer(3, VertexPointerType.Float, ColorVertexData.Stride, new IntPtr(4 * 4 + 3 * 4));

            GL.DrawRangeElements(myMode, 0, myIndexLength, count, DrawElementsType.UnsignedInt, new IntPtr(offset * 4));
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.ColorArray);
        }
    }
#endif
}
