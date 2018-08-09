﻿//BSD, 2014-present, WinterDev
//MattersHackers
//AGG 2.4

using System;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.Drawing;
using PaintLab.Svg;

namespace PixelFarm.CpuBlit
{


    public class MyTestSprite : BasicSprite
    {
        SpriteShape _spriteShape;

        float _posX, _posY;
        float _mouseDownX, _mouseDownY;

        byte alpha;
        public MyTestSprite(SpriteShape spriteShape)
        {
            _spriteShape = spriteShape;

            this.Width = 500;
            this.Height = 500;
            AlphaValue = 255;
        }


        public SpriteShape SpriteShape
        {
            get { return _spriteShape; }
            set { _spriteShape = value; }
        }

        //public bool AutoFlipY
        //{
        //    get;
        //    set;
        //}
        public int SharpenRadius
        {
            get;
            set;
        }

        public byte AlphaValue
        {
            get { return this.alpha; }
            set
            {
                this.alpha = value;
                //change alpha value
                _spriteShape.ApplyNewAlpha(value);
                //int j = lionShape.NumPaths;
                //var colorBuffer = lionShape.Colors;
                //for (int i = lionShape.NumPaths - 1; i >= 0; --i)
                //{
                //    colorBuffer[i] = colorBuffer[i].NewFromChangeAlpha(alpha);
                //}
            }
        }
        bool recreatePathAgain = true;

        public bool JustMove { get; set; }

        public override bool Move(int mouseX, int mouseY)
        {

            if (JustMove)
            {
                _posX += mouseX - _mouseDownX;
                _posY += mouseY - _mouseDownY;

                _mouseDownX = mouseX;
                _mouseDownY = mouseY;
                return true;
            }
            else
            {
                bool result = base.Move(mouseX, mouseY);
                recreatePathAgain = true;
                return result;
            }
        }

        static class VgHitChainPool
        {
            //
            //
            [System.ThreadStatic]
            static System.Collections.Generic.Stack<SvgHitChain> s_hitChains = new System.Collections.Generic.Stack< SvgHitChain>();

            public static void GetFreeHitTestChain(out SvgHitChain hitTestArgs)
            {
                if (s_hitChains.Count > 0)
                {
                    hitTestArgs = s_hitChains.Pop();
                }
                else
                {
                    hitTestArgs = new SvgHitChain();
                }
            }
            public static void ReleaseHitTestChain(ref SvgHitChain hitTestArgs)
            {
                hitTestArgs.Clear();
                s_hitChains.Push(hitTestArgs);
                hitTestArgs = null;
            }
        }
        public bool HitTest(float x, float y, bool withSubPathTest)
        {
            RectD bounds = _spriteShape.Bounds;
            bounds.Offset(_posX, _posY);
            if (bounds.Contains(x, y))
            {
                _mouseDownX = x;
                _mouseDownY = y;
                x -= _posX; //offset x to the coordinate of the sprite
                y -= _posY;
                if (withSubPathTest)
                {
                    VgHitChainPool.GetFreeHitTestChain(out SvgHitChain svgHitChain);
                    svgHitChain.SetHitTestPos(x, y);
                    svgHitChain.WithSubPartTest = withSubPathTest;
                    _spriteShape.HitTestOnSubPart(svgHitChain);
                    VgHitChainPool.ReleaseHitTestChain(ref svgHitChain);
                    //_hitTestArgs.Clear();
                    //_hitTestArgs.X = x;
                    //_hitTestArgs.Y = y;
                    //_hitTestArgs.WithSubPartTest = withSubPathTest;
                    //_spriteShape.HitTestOnSubPart(_hitTestArgs);
                    //return _hitTestArgs.Result;
                }


                //                //find capture point relative to the bounds

                //                _capY = (float)bounds.Top - y;
                //#if DEBUG
                //                //Console.WriteLine("hit");
                //#endif
                return true;
            }
            else
            {
                _mouseDownX = _mouseDownY = 0;
            }
            return false;
        }

        public override void Render(PixelFarm.Drawing.Painter p)
        {
            if (recreatePathAgain)
            {
                recreatePathAgain = false;

                var transform = Affine.NewMatix(
                        AffinePlan.Translate(-_spriteShape.Center.x, -_spriteShape.Center.y),
                        AffinePlan.Scale(spriteScale, spriteScale),
                        AffinePlan.Rotate(angle + Math.PI),
                        AffinePlan.Skew(skewX / 1000.0, skewY / 1000.0),
                        AffinePlan.Translate(Width / 2, Height / 2)
                );
                //create vertextStore again from original path



                //temp fix

                //-----------------------
                //(1) reset to original shape
                //_spriteShape.ResetTransform();
                //SvgRenderVx renderVx = _spriteShape.GetRenderVx();
                //int count = renderVx.VgCmdCount;
                //for (int i = 0; i < count; ++i)
                //{
                //    VgCmd vx = renderVx.GetVgCmd(i);
                //    if (vx.Name != VgCommandName.Path)
                //    {
                //        continue;
                //    }
                //    VgCmdPath path = (VgCmdPath)vx;
                //    using (VxsContext.Temp(out VertexStore tmp))
                //    {
                //        transform.TransformToVxs(path.Vxs, tmp);
                //        path.SetVxsAsOriginal(tmp.CreateTrim());
                //    }
                //}
                //_spriteShape.UpdateBounds();
                //-----------------------

                //(2) or just transform when draw => not affect its org shape
                VgRenderVx renderVx = _spriteShape.GetRenderVx();
                //renderVx.PrefixCommand = new VgCmdAffineTransform(transform);










                //if (AutoFlipY)
                //{
                //    //flip the lion
                //    PixelFarm.Agg.Transform.Affine aff = PixelFarm.Agg.Transform.Affine.NewMatix(
                //      PixelFarm.Agg.Transform.AffinePlan.Scale(-1, -1),
                //      PixelFarm.Agg.Transform.AffinePlan.Translate(0, 600));
                //    //
                //    var v2 = new VertexStore();
                //    myvxs = transform.TransformToVxs(myvxs, v2);
                //}

            }
            //---------------------------------------------------------------------------------------------
            {

                float ox = p.OriginX;
                float oy = p.OriginY;
                p.SetOrigin(ox + _posX, oy + _posY);

                _spriteShape.Paint(p);


                //#if DEBUG
                //                RectD bounds = lionShape.Bounds;
                //                bounds.Offset(_posX, _posY);
                //                //draw lion bounds
                //                var savedStrokeColor = p.StrokeColor;
                //                var savedFillColor = p.FillColor;
                //                var savedSmoothMode = p.SmoothingMode;

                //                p.SmoothingMode = SmoothingMode.HighSpeed;
                //                p.StrokeColor = Color.Black;
                //                p.DrawRect(bounds.Left, bounds.Top - bounds.Height, bounds.Width, bounds.Height);

                //                p.StrokeColor = Color.Red;
                //                p.DrawRect(_mouseDownX, _mouseDownY, 4, 4);


                //                //restore
                //                p.SmoothingMode = savedSmoothMode;
                //                p.StrokeColor = savedStrokeColor;
                //                p.FillColor = savedFillColor;


                //#endif 
                p.SetOrigin(ox, oy);

                //int j = lionShape.NumPaths;
                //int[] pathList = lionShape.PathIndexList;
                //Drawing.Color[] colors = lionShape.Colors;
                ////graphics2D.UseSubPixelRendering = true; 
                //for (int i = 0; i < j; ++i)
                //{
                //    p.FillColor = colors[i];
                //    p.Fill(new VertexStoreSnap(myvxs, pathList[i]));
                //}
            }
            //test 
            if (SharpenRadius > 0)
            {
                //p.DoFilter(new RectInt(0, p.Height, p.Width, 0), 2);
                //PixelFarm.Agg.Imaging.SharpenFilterARGB.Sharpen()
            }
        }

        public SpriteShape GetSpriteShape()
        {
            return _spriteShape;
        }
    }
}