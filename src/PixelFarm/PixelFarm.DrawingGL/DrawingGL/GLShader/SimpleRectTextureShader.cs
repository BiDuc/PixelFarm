﻿//MIT, 2016-present, WinterDev

using OpenTK.Graphics.ES20;
namespace PixelFarm.DrawingGL
{
    abstract class SimpleRectTextureShader : ShaderBase
    {
        protected ShaderVtxAttrib3f a_position;
        protected ShaderVtxAttrib2f a_texCoord;
        protected ShaderUniformMatrix4 u_matrix;
        protected ShaderUniformVar1 s_texture;
        protected static readonly ushort[] indices = new ushort[] { 0, 1, 2, 3 };
        public SimpleRectTextureShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {
        }

        int _orthoviewVersion = -1;
        internal void CheckViewMatrix()
        {
            int version = 0;
            if (_orthoviewVersion != (version = _shareRes.OrthoViewVersion))
            {
                _orthoviewVersion = version;
                u_matrix.SetData(_shareRes.OrthoView.data);
            }
        }
        //-----------------------------------------
        protected float _latestBmpW;
        protected float _latestBmpH;
        protected bool _latestBmpYFlipped;

        /// <summary>
        /// load glbmp before draw
        /// </summary>
        /// <param name="bmp"></param>
        public void LoadGLBitmap(GLBitmap bmp)
        {

            //load before use with RenderSubImage
            SetCurrent();
            CheckViewMatrix();
            //-------------------------------------------------------------------------------------
            // Bind the texture...
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, bmp.GetServerTextureId());
            // Set the texture sampler to texture unit to 0     
            s_texture.SetValue(0);
            _latestBmpW = bmp.Width;
            _latestBmpH = bmp.Height;
            _latestBmpYFlipped = bmp.IsYFlipped;
        }
        internal void SetAssociatedTextureInfo(GLBitmap bmp)
        {
            _latestBmpW = bmp.Width;
            _latestBmpH = bmp.Height;
            _latestBmpYFlipped = bmp.IsYFlipped;
        }
        internal unsafe void UnsafeDrawSubImages(float* srcDestList, int arrLen, float scale)
        {


            //-------------------------------------------------------------------------------------
            OnSetVarsBeforeRenderer();
            //-------------------------------------------------------------------------------------          
            float orgBmpW = _latestBmpW;
            float orgBmpH = _latestBmpH;
            for (int i = 0; i < arrLen;)
            {

                float srcLeft = srcDestList[i];
                float srcTop = srcDestList[i + 1];
                float srcW = srcDestList[i + 2];
                float srcH = srcDestList[i + 3];
                float targetLeft = srcDestList[i + 4];
                float targetTop = srcDestList[i + 5];

                i += 6;//***
                //-------------------------------
                float srcBottom = srcTop + srcH;
                float srcRight = srcLeft + srcW;

                unsafe
                {
                    if (!_latestBmpYFlipped)
                    {
                        float* imgVertices = stackalloc float[5 * 4];
                        {
                            imgVertices[0] = targetLeft; imgVertices[1] = targetTop; imgVertices[2] = 0; //coord 0 (left,top)
                            imgVertices[3] = srcLeft / orgBmpW; imgVertices[4] = srcBottom / orgBmpH; //texture coord 0  (left,bottom)

                            //---------------------
                            imgVertices[5] = targetLeft; imgVertices[6] = targetTop - (srcH * scale); imgVertices[7] = 0; //coord 1 (left,bottom)
                            imgVertices[8] = srcLeft / orgBmpW; imgVertices[9] = srcTop / orgBmpH; //texture coord 1  (left,top)

                            //---------------------
                            imgVertices[10] = targetLeft + (srcW * scale); imgVertices[11] = targetTop; imgVertices[12] = 0; //coord 2 (right,top)
                            imgVertices[13] = srcRight / orgBmpW; imgVertices[14] = srcBottom / orgBmpH; //texture coord 2  (right,bottom)

                            //---------------------
                            imgVertices[15] = targetLeft + (srcW * scale); imgVertices[16] = targetTop - (srcH * scale); imgVertices[17] = 0; //coord 3 (right, bottom)
                            imgVertices[18] = srcRight / orgBmpW; imgVertices[19] = srcTop / orgBmpH; //texture coord 3 (right,top)
                        }
                        a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                        a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                    }
                    else
                    {
                        float* imgVertices = stackalloc float[5 * 4];
                        {
                            imgVertices[0] = targetLeft;        /**/imgVertices[1] = targetTop;                   /**/imgVertices[2] = 0; //coord 0 (left,top)                                                                                                       
                            imgVertices[3] = srcLeft / orgBmpW; /**/imgVertices[4] = srcTop / orgBmpH; /**/                               //texture coord 0 (left,top)

                            //---------------------
                            imgVertices[5] = targetLeft;        /**/imgVertices[6] = targetTop - (srcH * scale);  /**/imgVertices[7] = 0; //coord 1 (left,bottom)
                            imgVertices[8] = srcLeft / orgBmpW; /**/imgVertices[9] = srcBottom / orgBmpH;         /**/                     //texture coord 1 (left,bottom)

                            //---------------------
                            imgVertices[10] = targetLeft + (srcW * scale); /**/imgVertices[11] = targetTop;       /**/imgVertices[12] = 0; //coord 2 (right,top)
                            imgVertices[13] = srcRight / orgBmpW;          /**/imgVertices[14] = srcTop / orgBmpH;                         //texture coord 2 (right,top)

                            //---------------------
                            imgVertices[15] = targetLeft + (srcW * scale); /**/imgVertices[16] = targetTop - (srcH * scale);    /**/imgVertices[17] = 0; //coord 3 (right, bottom)
                            imgVertices[18] = srcRight / orgBmpW;          /**/imgVertices[19] = srcBottom / orgBmpH;               //texture coord 3  (right,bottom)
                        }
                        a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                        a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                    }
                }
                GL.DrawElements(BeginMode.TriangleStrip, 4, DrawElementsType.UnsignedShort, indices);
            }
        }

        public void DrawWithVBO(TextureCoordVboBuilder vboBuilder)
        {
            float[] vboList = vboBuilder._buffer.UnsafeInternalArray;
            ushort[] indexList = vboBuilder._indexList.UnsafeInternalArray;

            SetCurrent();
            CheckViewMatrix();
            //-------------------------------------------------------------------------------------       
            unsafe
            {
                fixed (float* imgVertices = &vboList[0])
                {
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
            }
            GL.DrawElements(BeginMode.TriangleStrip, vboBuilder._indexList.Count, DrawElementsType.UnsignedShort, indexList);
        }

        public void Render(GLBitmap bmp, float left, float top, float w, float h)
        {
            Render(bmp.GetServerTextureId(), left, top, w, h, bmp.IsYFlipped);
        }
        public void Render(GLBitmap bmp,
            float left_top_x, float left_top_y,
            float right_top_x, float right_top_y,
            float right_bottom_x, float right_bottom_y,
            float left_bottom_x, float left_bottom_y, bool flipY = false)

        {
            Render(bmp.GetServerTextureId(),
                left_top_x, left_top_y,
                right_top_x, right_top_y,
                right_bottom_x, right_bottom_y,
                left_bottom_x, left_bottom_y, flipY);
        }
        public void Render(int textureId,
            float left_top_x, float left_top_y,
            float right_top_x, float right_top_y,
            float right_bottom_x, float right_bottom_y,
            float left_bottom_x, float left_bottom_y, bool flipY = false)
        {

            bool isFlipped = flipY;
            unsafe
            {
                //user's coord
                //(left,top) ----- (right,top)
                //  |                   |
                //  |                   |
                //  |                   |
                //(left,bottom) ---(right,bottom)

                // 
                //(0,1) ------------ (1,1)
                //  |                   |
                //  |   texture-img     |
                //  |                   |
                //(0,0) -------------(1,0)


                if (isFlipped)
                {
                    //since this is fliped in Y axis
                    //so we map 
                    //| user's coord    | texture-img |
                    //----------------------------------
                    //| left            | left
                    //| right           | right 
                    //----------------------------------
                    //| top             | bottom
                    //| bottom          | top
                    //----------------------------------

                    float* imgVertices = stackalloc float[5 * 4];
                    {

                        imgVertices[0] = left_top_x; imgVertices[1] = left_top_y; imgVertices[2] = 0; //coord 0 (left,top)
                        imgVertices[3] = 0; imgVertices[4] = 0; //texture coord 0 (left,bottom)
                        //---------------------
                        imgVertices[5] = left_bottom_x; imgVertices[6] = left_bottom_y; imgVertices[7] = 0; //coord 1 (left,bottom)
                        imgVertices[8] = 0; imgVertices[9] = 1; //texture coord 1  (left,top)

                        //---------------------
                        imgVertices[10] = right_top_x; imgVertices[11] = right_top_y; imgVertices[12] = 0; //coord 2 (right,top)
                        imgVertices[13] = 1; imgVertices[14] = 0; //texture coord 2  (right,bottom)

                        //---------------------
                        imgVertices[15] = right_bottom_x; imgVertices[16] = right_bottom_y; imgVertices[17] = 0; //coord 3 (right, bottom)
                        imgVertices[18] = 1; imgVertices[19] = 1; //texture coord 3 (right,top)
                    }
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
                else
                {    //since this is NOT fliped in Y axis
                    //so we map 
                    //| user's coord    | texture-img |
                    //----------------------------------
                    //| left            | left
                    //| right           | right 
                    //----------------------------------
                    //| top             | top
                    //| bottom          | bottom
                    //----------------------------------
                    float* imgVertices = stackalloc float[5 * 4];
                    {
                        imgVertices[0] = left_top_x; imgVertices[1] = left_top_y; imgVertices[2] = 0; //coord 0 (left,top)                                                                                                       
                        imgVertices[3] = 0; imgVertices[4] = 1; //texture coord 0 (left,top)

                        //---------------------
                        imgVertices[5] = left_bottom_x; imgVertices[6] = left_bottom_y; imgVertices[7] = 0; //coord 1 (left,bottom)
                        imgVertices[8] = 0; imgVertices[9] = 0; //texture coord 1 (left,bottom)

                        //---------------------
                        imgVertices[10] = right_top_x; imgVertices[11] = right_top_y; imgVertices[12] = 0; //coord 2 (right,top)
                        imgVertices[13] = 1; imgVertices[14] = 1; //texture coord 2 (right,top)

                        //---------------------
                        imgVertices[15] = right_bottom_x; imgVertices[16] = right_bottom_y; imgVertices[17] = 0; //coord 3 (right, bottom)
                        imgVertices[18] = 1; imgVertices[19] = 0; //texture coord 3  (right,bottom)
                    }
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
            }

            SetCurrent();
            CheckViewMatrix();
            //-------------------------------------------------------------------------------------
            // Bind the texture...
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            // Set the texture sampler to texture unit to 0     
            s_texture.SetValue(0);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, 4, DrawElementsType.UnsignedShort, indices);



        }


        public void Render(int textureId, float left, float top, float w, float h, bool isFlipped = false)
        {
            SetCurrent();
            CheckViewMatrix();
            unsafe
            {
                //user's coord
                //(left,top) ----- (right,top)
                //  |                   |
                //  |                   |
                //  |                   |
                //(left,bottom) ---(right,bottom)

                // 
                //(0,1) ------------ (1,1)
                //  |                   |
                //  |   texture-img     |
                //  |                   |
                //(0,0) -------------(1,0)


                if (isFlipped)
                {
                    //since this is fliped in Y axis
                    //so we map 
                    //| user's coord    | texture-img |
                    //----------------------------------
                    //| left            | left
                    //| right           | right 
                    //----------------------------------
                    //| top             | bottom
                    //| bottom          | top
                    //----------------------------------

                    float* imgVertices = stackalloc float[5 * 4];
                    {

                        imgVertices[0] = left; imgVertices[1] = top; imgVertices[2] = 0; //coord 0 (left,top)
                        imgVertices[3] = 0; imgVertices[4] = 0; //texture coord 0 (left,bottom)
                        //---------------------
                        imgVertices[5] = left; imgVertices[6] = top - h; imgVertices[7] = 0; //coord 1 (left,bottom)
                        imgVertices[8] = 0; imgVertices[9] = 1; //texture coord 1  (left,top)

                        //---------------------
                        imgVertices[10] = left + w; imgVertices[11] = top; imgVertices[12] = 0; //coord 2 (right,top)
                        imgVertices[13] = 1; imgVertices[14] = 0; //texture coord 2  (right,bottom)

                        //---------------------
                        imgVertices[15] = left + w; imgVertices[16] = top - h; imgVertices[17] = 0; //coord 3 (right, bottom)
                        imgVertices[18] = 1; imgVertices[19] = 1; //texture coord 3 (right,top)
                    }
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
                else
                {    //since this is NOT fliped in Y axis
                    //so we map 
                    //| user's coord    | texture-img |
                    //----------------------------------
                    //| left            | left
                    //| right           | right 
                    //----------------------------------
                    //| top             | top
                    //| bottom          | bottom
                    //----------------------------------
                    float* imgVertices = stackalloc float[5 * 4];
                    {
                        imgVertices[0] = left; imgVertices[1] = top; imgVertices[2] = 0; //coord 0 (left,top)                                                                                                       
                        imgVertices[3] = 0; imgVertices[4] = 1; //texture coord 0 (left,top)

                        //---------------------
                        imgVertices[5] = left; imgVertices[6] = top - h; imgVertices[7] = 0; //coord 1 (left,bottom)
                        imgVertices[8] = 0; imgVertices[9] = 0; //texture coord 1 (left,bottom)

                        //---------------------
                        imgVertices[10] = left + w; imgVertices[11] = top; imgVertices[12] = 0; //coord 2 (right,top)
                        imgVertices[13] = 1; imgVertices[14] = 1; //texture coord 2 (right,top)

                        //---------------------
                        imgVertices[15] = left + w; imgVertices[16] = top - h; imgVertices[17] = 0; //coord 3 (right, bottom)
                        imgVertices[18] = 1; imgVertices[19] = 0; //texture coord 3  (right,bottom)
                    }
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
            }

            SetCurrent();
            CheckViewMatrix();
            //-------------------------------------------------------------------------------------
            // Bind the texture...
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            // Set the texture sampler to texture unit to 0     
            s_texture.SetValue(0);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, 4, DrawElementsType.UnsignedShort, indices);
        }
        protected bool BuildProgram(string vs, string fs)
        {
            //NOTE: during development, 
            //new shader source may not recompile if you don't clear cache or disable cache feature
            //like...
            //EnableProgramBinaryCache = false;

            //---------------------
            if (!LoadCompiledShader())
            {
                if (!_shaderProgram.Build(vs, fs))
                {
                    return false;
                }
                SaveCompiledShader();
            }

            //-----------------------
            a_position = _shaderProgram.GetAttrV3f("a_position");
            a_texCoord = _shaderProgram.GetAttrV2f("a_texCoord");
            u_matrix = _shaderProgram.GetUniformMat4("u_mvpMatrix");
            s_texture = _shaderProgram.GetUniform1("s_texture");
            OnProgramBuilt();
            return true;
        }
        protected virtual void OnSetVarsBeforeRenderer()
        {
        }
        protected virtual void OnProgramBuilt()
        {
        }
    }

    /// <summary>
    /// for 32 bits texture/image  in BGR format (Windows GDI,with no alpha)d, we can specific A component laterd
    /// </summary>
    class BGRImageTextureShader : SimpleRectTextureShader
    {
        ShaderUniformVar1 u_alpha;//** alpha component to apply with original img
        public BGRImageTextureShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {
            Alpha = 1f;//default 
            //--------------------------------------------------------------------------
            string vs = @"
                attribute vec4 a_position;
                attribute vec2 a_texCoord;
                uniform mat4 u_mvpMatrix;   
                

                varying vec2 v_texCoord;
                void main()
                {
                    gl_Position = u_mvpMatrix* a_position;
                    v_texCoord =  a_texCoord;
                 }	 
                ";
            //in fs, angle on windows 
            //we need to switch color component ***
            //because we store value in memory as BGRA
            //and gl expect input in RGBA
            string fs = @"
                      precision mediump float;
                      varying vec2 v_texCoord;
                      uniform sampler2D s_texture;
                      uniform float u_alpha;
                      void main()
                      {
                         vec4 c = texture2D(s_texture, v_texCoord);                            
                         gl_FragColor =  vec4(c[2],c[1],c[0],u_alpha);
                      }
                ";
            BuildProgram(vs, fs);
        }
        protected override void OnProgramBuilt()
        {
            u_alpha = _shaderProgram.GetUniform1("u_alpha");
        }
        protected override void OnSetVarsBeforeRenderer()
        {
            u_alpha.SetValue(Alpha);
        }

        float _alpha;
        /// <summary>
        /// 00-1.0f
        /// </summary>
        public float Alpha
        {
            get => _alpha;
            set
            {
                //clamp 0-1
                if (_alpha < 0)
                {
                    _alpha = 0;
                }
                else if (_alpha > 1)
                {
                    _alpha = 1;
                }
                else
                {
                    _alpha = value;
                }
            }
        }
    }


    /// <summary>
    /// for 32 bits texture/image in BGRA format (eg. CpuBlit's ActualBitmap)
    /// </summary>
    class BGRAImageTextureShader : SimpleRectTextureShader
    {

        public BGRAImageTextureShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {
            //--------------------------------------------------------------------------
            string vs = @"
                attribute vec4 a_position;
                attribute vec2 a_texCoord;
                uniform mat4 u_mvpMatrix;                  
                varying vec2 v_texCoord;
                void main()
                {
                    gl_Position = u_mvpMatrix* a_position;
                    v_texCoord =  a_texCoord;
                 }	 
                ";
            //in fs, angle on windows 
            //we need to switch color component ***
            //because we store value in memory as BGRA
            //and gl expect input in RGBA
            string fs = @"
                      precision mediump float;
                      varying vec2 v_texCoord;
                      uniform sampler2D s_texture;
                      void main()
                      {
                         vec4 c = texture2D(s_texture, v_texCoord);                            
                         gl_FragColor =  vec4(c[2],c[1],c[0],c[3]);
                      }
                ";
            BuildProgram(vs, fs);
        }
    }



    /// <summary>
    /// for 32 bits texture/image  in RGBA format
    /// </summary>
    class RGBATextureShader : SimpleRectTextureShader
    {
        public RGBATextureShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {
            //--------------------------------------------------------------------------
            string vs = @"
                attribute vec4 a_position;
                attribute vec2 a_texCoord;
                uniform mat4 u_mvpMatrix; 
                varying vec2 v_texCoord;
                void main()
                {
                    gl_Position = u_mvpMatrix* a_position;
                    v_texCoord =  a_texCoord;
                 }	 
                ";
            //this case we not need to swap 
            string fs = @"
                      precision mediump float;
                      varying vec2 v_texCoord;
                      uniform sampler2D s_texture;
                      void main()
                      {
                         gl_FragColor = texture2D(s_texture, v_texCoord);        
                      }
                ";
            BuildProgram(vs, fs);
        }
    }


    class BGRAImageTextureWithWhiteTransparentShader : SimpleRectTextureShader
    {
        public BGRAImageTextureWithWhiteTransparentShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {
            string vs = @"
                attribute vec4 a_position;
                attribute vec2 a_texCoord;
                uniform mat4 u_mvpMatrix; 
                varying vec2 v_texCoord;
                void main()
                {
                    gl_Position = u_mvpMatrix* a_position;
                    v_texCoord =  a_texCoord;
                 }	 
                ";
            //in fs, angle on windows 
            //we need to switch color component
            //because we store value in memory as BGRA
            //and gl expect input in RGBA
            string fs = @"
                      precision mediump float;
                      varying vec2 v_texCoord;
                      uniform sampler2D s_texture;
                      void main()
                      {
                         vec4 c = texture2D(s_texture, v_texCoord); 
                         if((c[2] ==1.0) && (c[1]==1.0) && (c[0]== 1.0) && (c[3] == 1.0)){
                            discard;
                         }else{                                                   
                            gl_FragColor =  vec4(c[2],c[1],c[0],c[3]);  
                         }
                      }
                ";
            BuildProgram(vs, fs);
        }

    }


    //old
    //class GlyphImageStecilShader : SimpleRectTextureShader
    //{
    //    //similar to GdiImageTextureWithWhiteTransparentShader
    //    float _color_a = 1f;
    //    float _color_r;
    //    float _color_g;
    //    float _color_b;

    //    ShaderUniformVar4 _d_color; //drawing color
    //    public GlyphImageStecilShader(ShaderSharedResource shareRes)
    //        : base(shareRes)
    //    {
    //        string vs = @"
    //            attribute vec4 a_position;
    //            attribute vec2 a_texCoord;
    //            uniform mat4 u_mvpMatrix; 
    //            varying vec2 v_texCoord;
    //            void main()
    //            {
    //                gl_Position = u_mvpMatrix* a_position;
    //                v_texCoord =  a_texCoord;
    //             }	 
    //            ";
    //        //in fs, angle on windows 
    //        //we need to switch color component
    //        //because we store value in memory as BGRA
    //        //and gl expect input in RGBA
    //        string fs = @"
    //                  precision mediump float;
    //                  varying vec2 v_texCoord;
    //                  uniform sampler2D s_texture;
    //                  uniform vec4 d_color;
    //                  void main()
    //                  {
    //                     vec4 c = texture2D(s_texture, v_texCoord); 
    //                     if((c[2] ==1.0) && (c[1]==1.0) && (c[0]== 1.0) && (c[3] == 1.0)){
    //                        discard;
    //                     }else{                                                 

    //                        gl_FragColor =  vec4(d_color[0],d_color[1],d_color[2],c[3]);  
    //                     }
    //                  }
    //            ";
    //        BuildProgram(vs, fs);
    //    }
    //    public void SetColor(PixelFarm.Drawing.Color c)
    //    {
    //        _color_a = c.A / 255f;
    //        _color_r = c.R / 255f;
    //        _color_g = c.G / 255f;
    //        _color_b = c.B / 255f;
    //    }
    //    protected override void OnProgramBuilt()
    //    {
    //        _d_color = shaderProgram.GetUniform4("d_color");
    //    }
    //    protected override void OnSetVarsBeforeRenderer()
    //    {
    //        _d_color.SetValue(_color_r, _color_g, _color_b, _color_a);
    //    }
    //}


    class GlyphImageStecilShader : SimpleRectTextureShader
    {

        //we share the texture with the subpixel-lcd effect texture
        //which is 32 bits bitmap.
        //backgrond color= black,
        //font =white + subpixel value

        //see the glyph texture example at https://github.com/PaintLab/PixelFarm/issues/16




        float _color_a = 1f;
        float _color_r;
        float _color_g;
        float _color_b;

        ShaderUniformVar4 _d_color;
        public GlyphImageStecilShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {
            string vs = @"
                attribute vec4 a_position;
                attribute vec2 a_texCoord;
                uniform mat4 u_mvpMatrix; 
                varying vec2 v_texCoord;
                void main()
                {
                    gl_Position = u_mvpMatrix* a_position;
                    v_texCoord =  a_texCoord;
                 }	 
                ";
            //in fs, angle on windows 
            //we need to switch color component
            //because we store value in memory as BGRA
            //and gl expect input in RGBA
            string fs = @"
                      precision mediump float;
                      varying vec2 v_texCoord;
                      uniform sampler2D s_texture;
                      uniform vec4 d_color;
                      void main()
                      {
                         vec4 c = texture2D(s_texture, v_texCoord); 
                         gl_FragColor =  vec4(d_color[0],d_color[1],d_color[2],c[1]);  
                      }
                ";
            BuildProgram(vs, fs);
        }
        public void SetColor(PixelFarm.Drawing.Color c)
        {
            _color_a = c.A / 255f;
            _color_r = c.R / 255f;
            _color_g = c.G / 255f;
            _color_b = c.B / 255f;
        }
        protected override void OnProgramBuilt()
        {
            _d_color = _shaderProgram.GetUniform4("d_color");
        }
        protected override void OnSetVarsBeforeRenderer()
        {
            _d_color.SetValue(_color_r, _color_g, _color_b, _color_a);
        }

    }


    class ImageTextureWithSubPixelRenderingShader : SimpleRectTextureShader
    {
        //this shader is designed for subpixel shader

        ShaderUniformVar1 _c_compo;
        ShaderUniformVar1 _isBigEndian;
        ShaderUniformVar1 _c_intensity;
        ShaderUniformVar4 _d_color; //drawing color
        public ImageTextureWithSubPixelRenderingShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {

            string vs = @"
                attribute vec4 a_position;
                attribute vec2 a_texCoord;
                uniform mat4 u_mvpMatrix; 
                varying vec2 v_texCoord;
                void main()
                {
                    gl_Position = u_mvpMatrix* a_position;
                    v_texCoord =  a_texCoord;
                 }	 
                ";
            //in fs, angle on windows 
            //we need to switch color component
            //because we store value in memory as BGRA
            //and gl expect input in RGBA


            string fs = @"
                      precision mediump float;

                      uniform sampler2D s_texture;
                      uniform int isBigEndian;
                      uniform int c_compo;
                      uniform vec4 d_color; 
                      varying vec2 v_texCoord; 
                      void main()
                      {   
                         vec4 c= texture2D(s_texture,v_texCoord);    
                         if(c_compo==0){ 
                            gl_FragColor = vec4(0,0,d_color[2],(c[0]* d_color[3]) );
                         }else if(c_compo==1){ 
                            gl_FragColor = vec4(0,d_color[1],0,(c[1]* d_color[3]) );
                         }else{ 
                            gl_FragColor = vec4(d_color[0],0,0,(c[2]* d_color[3]) );
                         } 
                      }
                ";
            BuildProgram(vs, fs);
        }
        public bool IsBigEndian { get; set; }

        float _color_a = 1f;
        float _color_r;
        float _color_g;
        float _color_b;
        int _use_color_compo;//0,1,2

        public void SetColor(PixelFarm.Drawing.Color c)
        {
            _color_a = c.A / 255f;
            _color_r = c.R / 255f;
            _color_g = c.G / 255f;
            _color_b = c.B / 255f;
        }
        public void SetCompo(int compo)
        {
            switch (compo)
            {
                case 0:
                case 1:
                case 2:
                    _use_color_compo = compo;
                    break;
                default:
                    throw new System.NotSupportedException();

            }
        }
        public void SetIntensity(float intensity)
        {

        }
        protected override void OnProgramBuilt()
        {
            _isBigEndian = _shaderProgram.GetUniform1("isBigEndian");
            _d_color = _shaderProgram.GetUniform4("d_color");
            _c_compo = _shaderProgram.GetUniform1("c_compo");
            _c_intensity = _shaderProgram.GetUniform1("c_intensity");
        }
        protected override void OnSetVarsBeforeRenderer()
        {
            _isBigEndian.SetValue(IsBigEndian);
            _d_color.SetValue(_color_r, _color_g, _color_b, _color_a);
            _c_compo.SetValue(_use_color_compo);
        }

        public void NewDrawSubImage4FromCurrentLoadedVBO(int elemCount, float x, float y)
        {
            SetCurrent();
            CheckViewMatrix();
            //-------------------------------------------------------------------------------------          
            a_position.LoadLatest(5, 0);
            a_texCoord.LoadLatest(5, 3 * 4);

            MyMat4 backup = _shareRes.OrthoView;
            MyMat4 mm2 = _shareRes.OrthoView * MyMat4.translate(new OpenTK.Vector3(x, y, 0));

            ////version 1
            ////1. B , yellow  result
            GL.ColorMask(false, false, true, false);
            this.SetCompo(0);
            OnSetVarsBeforeRenderer();
            u_matrix.SetData(mm2.data);


            GL.DrawElements(BeginMode.TriangleStrip, elemCount, DrawElementsType.UnsignedShort, 0);

            ////2. G , magenta result
            GL.ColorMask(false, true, false, false);
            this.SetCompo(1);
            OnSetVarsBeforeRenderer();
            // u_matrix.SetData(mm2.data);
            GL.DrawElements(BeginMode.TriangleStrip, elemCount, DrawElementsType.UnsignedShort, 0);

            //1. R , cyan result 
            GL.ColorMask(true, false, false, false);//     
            this.SetCompo(2);
            OnSetVarsBeforeRenderer();
            //u_matrix.SetData(mm2.data);
            GL.DrawElements(BeginMode.TriangleStrip, elemCount, DrawElementsType.UnsignedShort, 0);

            //restore
            GL.ColorMask(true, true, true, true);

            u_matrix.SetData(backup.data);
        }

        /// <summary>
        /// DrawElements, use vertex-buffer and index-list
        /// </summary>
        /// <param name="vboList"></param>
        /// <param name="indexList"></param>
        public void DrawSubImages(TextureCoordVboBuilder vboBuilder)
        {
            SetCurrent();
            CheckViewMatrix();
            //-------------------------------------------------------------------------------------          

            float[] vboList = vboBuilder._buffer.UnsafeInternalArray; //***
            unsafe
            {
                fixed (float* imgVertices = &vboList[0])
                {
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
            }

            //SHARED ARRAY 
            ushort[] indexList = vboBuilder._indexList.UnsafeInternalArray; //***
            int count1 = vboBuilder._indexList.Count; //***

            //version 1
            //1. B , yellow  result
            GL.ColorMask(false, false, true, false);
            this.SetCompo(0);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, count1, DrawElementsType.UnsignedShort, indexList);

            //2. G , magenta result
            GL.ColorMask(false, true, false, false);
            this.SetCompo(1);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, count1, DrawElementsType.UnsignedShort, indexList);

            //1. R , cyan result 
            GL.ColorMask(true, false, false, false);//     
            this.SetCompo(2);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, count1, DrawElementsType.UnsignedShort, indexList);

            //restore
            GL.ColorMask(true, true, true, true);
        }

        public void DrawSubImageWithLcdSubPix(float srcLeft, float srcTop, float srcW, float srcH, float targetLeft, float targetTop)
        {

            SetCurrent();
            CheckViewMatrix();
            //-------------------------------------------------------------------------------------          
            float orgBmpW = _latestBmpW;
            float orgBmpH = _latestBmpH;
            float scale = 1;

            //-------------------------------
            float srcBottom = srcTop + srcH;
            float srcRight = srcLeft + srcW;

            unsafe
            {
                if (!_latestBmpYFlipped)
                {

                    float* imgVertices = stackalloc float[5 * 4];
                    {
                        imgVertices[0] = targetLeft; imgVertices[1] = targetTop; imgVertices[2] = 0; //coord 0 (left,top)
                        imgVertices[3] = srcLeft / orgBmpW; imgVertices[4] = srcBottom / orgBmpH; //texture coord 0  (left,bottom)

                        //---------------------
                        imgVertices[5] = targetLeft; imgVertices[6] = targetTop - (srcH * scale); imgVertices[7] = 0; //coord 1 (left,bottom)
                        imgVertices[8] = srcLeft / orgBmpW; imgVertices[9] = srcTop / orgBmpH; //texture coord 1  (left,top)

                        //---------------------
                        imgVertices[10] = targetLeft + (srcW * scale); imgVertices[11] = targetTop; imgVertices[12] = 0; //coord 2 (right,top)
                        imgVertices[13] = srcRight / orgBmpW; imgVertices[14] = srcBottom / orgBmpH; //texture coord 2  (right,bottom)

                        //---------------------
                        imgVertices[15] = targetLeft + (srcW * scale); imgVertices[16] = targetTop - (srcH * scale); imgVertices[17] = 0; //coord 3 (right, bottom)
                        imgVertices[18] = srcRight / orgBmpW; imgVertices[19] = srcTop / orgBmpH; //texture coord 3 (right,top)
                    }
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
                else
                {
                    float* imgVertices = stackalloc float[5 * 4];
                    {
                        imgVertices[0] = targetLeft; imgVertices[1] = targetTop; imgVertices[2] = 0; //coord 0 (left,top)                                                                                                       
                        imgVertices[3] = srcLeft / orgBmpW; imgVertices[4] = srcTop / orgBmpH; //texture coord 0 (left,top)

                        //---------------------
                        imgVertices[5] = targetLeft; imgVertices[6] = targetTop - (srcH * scale); imgVertices[7] = 0; //coord 1 (left,bottom)
                        imgVertices[8] = srcLeft / orgBmpW; imgVertices[9] = srcBottom / orgBmpH; //texture coord 1 (left,bottom)

                        //---------------------
                        imgVertices[10] = targetLeft + (srcW * scale); imgVertices[11] = targetTop; imgVertices[12] = 0; //coord 2 (right,top)
                        imgVertices[13] = srcRight / orgBmpW; imgVertices[14] = srcTop / orgBmpH; //texture coord 2 (right,top)

                        //---------------------
                        imgVertices[15] = targetLeft + (srcW * scale); imgVertices[16] = targetTop - (srcH * scale); imgVertices[17] = 0; //coord 3 (right, bottom)
                        imgVertices[18] = srcRight / orgBmpW; imgVertices[19] = srcBottom / orgBmpH; //texture coord 3  (right,bottom)
                    }
                    a_position.UnsafeLoadMixedV3f(imgVertices, 5);
                    a_texCoord.UnsafeLoadMixedV2f(imgVertices + 3, 5);
                }
            }

            ////version 1
            ////1. B , yellow  result
            GL.ColorMask(false, false, true, false);
            this.SetCompo(0);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, 4, DrawElementsType.UnsignedShort, indices);

            ////2. G , magenta result
            GL.ColorMask(false, true, false, false);
            this.SetCompo(1);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, 4, DrawElementsType.UnsignedShort, indices);

            //1. R , cyan result 
            GL.ColorMask(true, false, false, false);//     
            this.SetCompo(2);
            OnSetVarsBeforeRenderer();
            GL.DrawElements(BeginMode.TriangleStrip, 4, DrawElementsType.UnsignedShort, indices);
            //restore
            GL.ColorMask(true, true, true, true);
        }
    }


    //--------------------------------------------------------
    static class SimpleRectTextureShaderExtensions
    {

        public static void DrawSubImage(this SimpleRectTextureShader shader, float srcLeft, float srcTop, float srcW, float srcH, float targetLeft, float targetTop)
        {

            unsafe
            {
                float* srcDestList = stackalloc float[6];
                {
                    srcDestList[0] = srcLeft;
                    srcDestList[1] = srcTop;
                    srcDestList[2] = srcW;
                    srcDestList[3] = srcH;
                    srcDestList[4] = targetLeft;
                    srcDestList[5] = targetTop;
                }
                shader.UnsafeDrawSubImages(srcDestList, 6, 1);
            }
        }
        public static void DrawSubImage(this SimpleRectTextureShader shader, GLBitmap bmp,
            float srcLeft, float srcTop,
            float srcW, float srcH,
            float targetLeft, float targetTop,
            float scale = 1)
        {

            unsafe
            {
                float* srcDestList = stackalloc float[6];
                {
                    srcDestList[0] = srcLeft;
                    srcDestList[1] = srcTop;
                    srcDestList[2] = srcW;
                    srcDestList[3] = srcH;
                    srcDestList[4] = targetLeft;
                    srcDestList[5] = targetTop;
                }
                shader.LoadGLBitmap(bmp);
                shader.UnsafeDrawSubImages(srcDestList, 6, scale);
            }
        }
        public static void DrawSubImages(this SimpleRectTextureShader shader, float[] srcDestList, float scale)
        {
            unsafe
            {
                fixed (float* head = &srcDestList[0])
                {
                    shader.UnsafeDrawSubImages(head, srcDestList.Length, scale);
                }
            }
        }
        public static void DrawSubImages(this SimpleRectTextureShader shader, GLBitmap bmp, float[] srcDestList, float scale)
        {
            shader.LoadGLBitmap(bmp);
            shader.DrawSubImages(srcDestList, scale);
        }
    }

}