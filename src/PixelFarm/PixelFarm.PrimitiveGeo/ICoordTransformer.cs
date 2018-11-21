﻿//BSD, 2014-present, WinterDev
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
//
// Affine transformation classes.
//
//----------------------------------------------------------------------------
//#ifndef AGG_TRANS_AFFINE_INCLUDED
//#define AGG_TRANS_AFFINE_INCLUDED

//#include <math.h>
//#include "agg_basics.h"


namespace PixelFarm.CpuBlit.VertexProcessing
{
    public interface ICoordTransformer
    {
        void Transform(ref double x, ref double y);
        ICoordTransformer MultiplyWith(ICoordTransformer another);
        ICoordTransformer CreateInvert();
        CoordTransformerKind Kind { get; }
    }


    public enum CoordTransformerKind
    {
        Unknown,
        Affine3x2,
        Perspective,
        Bilinear,
        TransformChain,
    }


    //Manual Serializer/ Deserializer
    //---------------------------
    //TODO: move this class to another file
    /// <summary>
    /// ICoordTransform Serializer/ Deseralizer
    /// </summary>
    public static class ICoordTransformRW
    {
        /// <summary>
        /// serialize coord-transform-chain to specific stream
        /// </summary>
        /// <param name="coordTx"></param>
        /// <param name="writer"></param>
        public static void Write(ICoordTransformer coordTx, System.IO.BinaryWriter writer)
        {
            //write transformation matrix to binary stream
            CoordTransformerKind txKind = coordTx.Kind;
            switch (txKind)
            {
                case CoordTransformerKind.Unknown:
                default:
                    throw new System.NotSupportedException();
                case CoordTransformerKind.Affine3x2:
                    {
                        Affine aff = (Affine)coordTx;
                        writer.Write((ushort)txKind); //type
                        AffineMat affMat = aff.GetInternalMat();
                        //write elements
                        writer.Write(affMat.sx); writer.Write(affMat.shy);
                        writer.Write(affMat.shx); writer.Write(affMat.sy);
                        writer.Write(affMat.tx); writer.Write(affMat.ty);

                    }
                    break;
                case CoordTransformerKind.Bilinear:
                    {
                        Bilinear binTx = (Bilinear)coordTx;
                        writer.Write((ushort)txKind);
                        //write elements
                        BilinearMat binMat = binTx.GetInternalElements();
                        writer.Write(binMat.rc00); writer.Write(binMat.rc01);
                        writer.Write(binMat.rc10); writer.Write(binMat.rc11);
                        writer.Write(binMat.rc20); writer.Write(binMat.rc21);
                        writer.Write(binMat.rc30); writer.Write(binMat.rc31);

                    }
                    break;
                case CoordTransformerKind.Perspective:
                    {
                        Perspective perTx = (Perspective)coordTx;
                        writer.Write((ushort)txKind);
                        PerspectiveMat perMat = perTx.GetInternalElements();
                        writer.Write(perMat.sx); writer.Write(perMat.shx); writer.Write(perMat.w0);
                        writer.Write(perMat.shy); writer.Write(perMat.sy); writer.Write(perMat.w1);
                        writer.Write(perMat.tx); writer.Write(perMat.ty); writer.Write(perMat.w2);

                        //sx, shy, w0,
                        //shx, sy, w1,
                        //tx, ty, w2; 
                    }
                    break;
                case CoordTransformerKind.TransformChain:
                    {
                        CoordTransformationChain chainTx = (CoordTransformationChain)coordTx;
                        writer.Write((ushort)txKind);
                        //*** left, right
                        Write(chainTx.Left, writer);
                        Write(chainTx.Right, writer);
                    }
                    break;
            }
        }



        /// <summary>
        /// read back, read  coord-transform-chain  back from specific stream
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ICoordTransformer Read(System.IO.BinaryReader reader)
        {

            CoordTransformerKind txKind = (CoordTransformerKind)reader.ReadUInt16();
            switch (txKind)
            {
                default:
                    throw new System.NotSupportedException();
                case CoordTransformerKind.Affine3x2:
                    {
                        double sx = reader.ReadDouble(); double shy = reader.ReadDouble();
                        double shx = reader.ReadDouble(); double sy = reader.ReadDouble();
                        double tx = reader.ReadDouble(); double ty = reader.ReadDouble();
                        return new Affine(sx, shy, shx, sy, tx, ty);
                    }

                case CoordTransformerKind.Bilinear:
                    {
                        return new Bilinear(
                             reader.ReadDouble(), reader.ReadDouble(),
                             reader.ReadDouble(), reader.ReadDouble(),
                             reader.ReadDouble(), reader.ReadDouble(),
                             reader.ReadDouble(), reader.ReadDouble()
                        );
                    }
                case CoordTransformerKind.Perspective:
                    {
                        return new Perspective(
                             reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
                             reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
                             reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble()
                            );
                    }
                case CoordTransformerKind.TransformChain:
                    {
                        return new CoordTransformationChain(
                            Read(reader), //left
                            Read(reader) // right
                            );
                    }
            }
        }
    }
}