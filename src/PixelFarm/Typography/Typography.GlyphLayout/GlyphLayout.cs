﻿//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace Typography.TextLayout
{


    /// <summary>
    /// scaled glyph plan to specfic font size.
    /// offsetX,offsetY,advanceX are adjusted to fit with specific font size    
    /// </summary>
    public struct PxScaledGlyphPlan
    {
        public readonly short input_cp_offset;
        public readonly ushort glyphIndex;
        public PxScaledGlyphPlan(short input_cp_offset, ushort glyphIndex, float advanceW, float offsetX, float offsetY)
        {
            this.input_cp_offset = input_cp_offset;
            this.glyphIndex = glyphIndex;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.AdvanceX = advanceW;
        }
        public float AdvanceX { get; private set; }
        /// <summary>
        /// x offset from current position
        /// </summary>
        public float OffsetX { get; private set; }
        /// <summary>
        /// y offset from current position
        /// </summary>
        public float OffsetY { get; private set; }

        public bool AdvanceMoveForward { get { return this.AdvanceX > 0; } }

#if DEBUG
        public override string ToString()
        {
            return " adv:" + AdvanceX;
        }
#endif
    }


    /// <summary>
    /// expandable list of glyph plan
    /// </summary>
    public class PxScaledGlyphPlanList
    {
        List<PxScaledGlyphPlan> _glyphPlans = new List<PxScaledGlyphPlan>();
        float _accumAdvanceX;

        public void Clear()
        {
            _glyphPlans.Clear();
            _accumAdvanceX = 0;
        }
        public void Append(PxScaledGlyphPlan glyphPlan)
        {
            _glyphPlans.Add(glyphPlan);
            _accumAdvanceX += glyphPlan.AdvanceX;
        }
        public float AccumAdvanceX { get { return _accumAdvanceX; } }

        public PxScaledGlyphPlan this[int index]
        {
            get
            {
                return _glyphPlans[index];
            }
        }
        public int Count
        {
            get
            {
                return _glyphPlans.Count;
            }
        }

#if DEBUG
        public PxScaledGlyphPlanList()
        {

        }
#endif
    }


    /// <summary>
    /// unscaled glyph-plan
    /// </summary>
    public struct UnscaledGlyphPlan
    {
        public readonly short input_cp_offset;
        public readonly ushort glyphIndex;
        public UnscaledGlyphPlan(short input_cp_offset, ushort glyphIndex, short advanceW, short offsetX, short offsetY)
        {
            this.input_cp_offset = input_cp_offset;
            this.glyphIndex = glyphIndex;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.AdvanceX = advanceW;
        }
        public short AdvanceX { get; private set; }
        /// <summary>
        /// x offset from current position
        /// </summary>
        public short OffsetX { get; private set; }
        /// <summary>
        /// y offset from current position
        /// </summary>
        public short OffsetY { get; private set; }

        public bool AdvanceMoveForward { get { return this.AdvanceX > 0; } }

#if DEBUG
        public override string ToString()
        {
            return " adv:" + AdvanceX;
        }
#endif
    }




    public interface IUnscaledGlyphPlanList
    {
        void Append(UnscaledGlyphPlan glyphPlan);
        int Count { get; }
        UnscaledGlyphPlan this[int index] { get; }
    }
    public struct GlyphPlanSequence
    {
        //
        public static GlyphPlanSequence Empty = new GlyphPlanSequence();
        //
        readonly IUnscaledGlyphPlanList glyphBuffer;
        internal readonly int startAt;
        internal readonly ushort len;
        public GlyphPlanSequence(IUnscaledGlyphPlanList glyphBuffer, int startAt, int len)
        {
            this.glyphBuffer = glyphBuffer;
            this.startAt = startAt;
            this.len = (ushort)len;
        }
        public UnscaledGlyphPlan this[int index]
        {
            get
            {
                if (index >= 0 && index < (startAt + len))
                {
                    return glyphBuffer[startAt + index];
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }
        public int Count
        {
            get
            {
                if (glyphBuffer != null)
                {
                    return len;
                }
                else
                {
                    return 0;
                }
            }
        }
        public float CalculateWidth()
        {
            if (glyphBuffer == null) return 0;
            //
            IUnscaledGlyphPlanList plans = glyphBuffer;
            int end = startAt + len;
            float width = 0;
            for (int i = startAt; i < end; ++i)
            {
                width += plans[i].AdvanceX;
            }
            return width;
        }
        public bool IsEmpty()
        {
            return glyphBuffer == null;
        }


        /// <summary>
        /// generate scaled from unscale glyph size to specific scale
        /// </summary>
        /// <param name="glyphPositions"></param>
        /// <param name="pxscale"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void GenerateUnscaledGlyphPlans(IGlyphPositions glyphPositions, IUnscaledGlyphPlanList outputGlyphPlanList)
        {
            //user can implement this with some 'PixelScaleEngine'

            int finalGlyphCount = glyphPositions.Count;
            for (int i = 0; i < finalGlyphCount; ++i)
            {
                short input_offset, offsetX, offsetY, advW; //all from pen-pos
                ushort glyphIndex = glyphPositions.GetGlyph(i, out input_offset, out offsetX, out offsetY, out advW);

                outputGlyphPlanList.Append(new UnscaledGlyphPlan(
                    input_offset,
                    glyphIndex,
                    advW,
                    offsetX,
                    offsetY
                    ));
            }
        }
    }

    public enum PositionTechnique
    {
        None,
        /// <summary>
        /// use kerning table (old)
        /// </summary>
        Kerning, //old technique-- TODO: review and remove this 
        /// <summary>
        /// use openfont gpos table
        /// </summary>
        OpenFont,
    }

    class GlyphLayoutPlanCollection
    {
        Dictionary<GlyphLayoutPlanKey, GlyphLayoutPlanContext> collection = new Dictionary<GlyphLayoutPlanKey, GlyphLayoutPlanContext>();
        /// <summary>
        /// get glyph layout plan or create if not exists
        /// </summary>
        /// <param name="typeface"></param>
        /// <param name="scriptLang"></param>
        /// <returns></returns>
        public GlyphLayoutPlanContext GetPlanOrCreate(Typeface typeface, ScriptLang scriptLang)
        {
            GlyphLayoutPlanKey key = new GlyphLayoutPlanKey(typeface, scriptLang.internalName);
            GlyphLayoutPlanContext context;
            if (!collection.TryGetValue(key, out context))
            {
                var glyphSubstitution = (typeface.GSUBTable != null) ? new GlyphSubstitution(typeface, scriptLang.shortname) : null;
                var glyphPosition = (typeface.GPOSTable != null) ? new GlyphSetPosition(typeface, scriptLang.shortname) : null;
                collection.Add(key, context = new GlyphLayoutPlanContext(glyphSubstitution, glyphPosition));
            }
            return context;
        }

    }
    struct GlyphLayoutPlanKey
    {
        public Typeface t;
        public int scriptInternameName;
        public GlyphLayoutPlanKey(Typeface t, int scriptInternameName)
        {
            this.t = t;
            this.scriptInternameName = scriptInternameName;
        }
    }
    struct GlyphLayoutPlanContext
    {
        public readonly GlyphSubstitution _glyphSub;
        public readonly GlyphSetPosition _glyphPos;
        public GlyphLayoutPlanContext(GlyphSubstitution _glyphSub, GlyphSetPosition glyphPos)
        {
            this._glyphSub = _glyphSub;
            this._glyphPos = glyphPos;
        }
    }



    struct CodePointFromUserChar
    {
        public readonly int codePoint;
        public readonly ushort user_char_offset;
        public CodePointFromUserChar(ushort user_char_offset, int codePoint)
        {
            this.user_char_offset = user_char_offset;
            this.codePoint = codePoint;
        }
    }


    //TODO: rename this to ShapingEngine ?

    /// <summary>
    /// text span's glyph layout engine, 
    /// </summary>
    public class GlyphLayout
    {

        GlyphLayoutPlanCollection _layoutPlanCollection = new GlyphLayoutPlanCollection();
        Typeface _typeface;
        ScriptLang _scriptLang;
        GlyphSubstitution _gsub;
        GlyphSetPosition _gpos;
        bool _needPlanUpdate;

        GlyphIndexList _inputGlyphs = new GlyphIndexList();//reusable input glyph
        GlyphPosStream _glyphPositions = new GlyphPosStream();


        public GlyphLayout()
        {
            PositionTechnique = PositionTechnique.OpenFont;
            EnableLigature = true;
            EnableComposition = true;
            ScriptLang = ScriptLangs.Latin;
        }
        public IGlyphPositions ResultUnscaledGlyphPositions
        {
            get { return _glyphPositions; }
        }
        public PositionTechnique PositionTechnique { get; set; }
        public ScriptLang ScriptLang
        {
            get { return _scriptLang; }
            set
            {
                if (_scriptLang != value)
                {
                    _needPlanUpdate = true;
                }
                _scriptLang = value;
            }
        }


        public bool EnableLigature { get; set; }
        public bool EnableComposition { get; set; }
        public Typeface Typeface
        {
            get { return _typeface; }
            set
            {
                if (_typeface != value)
                {
                    _typeface = value;
                    _needPlanUpdate = true;
                }
            }
        }


        //not thread-safe*** 

        List<CodePointFromUserChar> _reusableCodePointFromUserCharList = new List<CodePointFromUserChar>();

        /// <summary>
        /// do glyph shaping and glyph out
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        public void Layout(
            char[] str,
            int startAt,
            int len)
        {
            if (_needPlanUpdate)
            {
                UpdateLayoutPlan();
            }


            //[A]
            // this is important!
            // -----------------------
            //  from @samhocevar's PR: (https://github.com/LayoutFarm/Typography/pull/56/commits/b71c7cf863531ebf5caa478354d3249bde40b96e)
            // In many places, "char" is not a valid type to handle characters, because it
            // only supports 16 bits.In order to handle the full range of Unicode characters,
            // we need to use "int".
            // This allows characters such as 🙌 or 𐐷 or to be treated as single codepoints even
            // though they are encoded as two "char"s in a C# string.
            _reusableCodePointFromUserCharList.Clear();
            for (int i = 0; i < len; ++i)
            {
                char ch = str[startAt + i];
                int codepoint = ch;
                if (ch >= 0xd800 && ch <= 0xdbff && i + 1 < len)
                {
                    char nextCh = str[startAt + i + 1];
                    if (nextCh >= 0xdc00 && nextCh <= 0xdfff)
                    {
                        //please note: 
                        //num of codepoint may be less than  original user input char 
                        ++i;
                        codepoint = char.ConvertToUtf32(ch, nextCh);
                    }
                }
                _reusableCodePointFromUserCharList.Add(new CodePointFromUserChar((ushort)i, codepoint));
            }

            //
            //[B]
            // convert codepoint-list to input glyph-list 
            // clear before use
            _inputGlyphs.Clear();
            int codePointCount = _reusableCodePointFromUserCharList.Count;
            for (int i = 0; i < codePointCount; ++i)
            {
                CodePointFromUserChar cp = _reusableCodePointFromUserCharList[i];
                ushort glyphIndex = _typeface.LookupIndex(cp.codePoint);

                if (i + 1 < codePointCount)
                {
                    // Maybe this is a UVS sequence; in that case,
                    //***SKIP*** the second codepoint
                    CodePointFromUserChar nextCp = _reusableCodePointFromUserCharList[i + 1];
                    ushort variationGlyphIndex = _typeface.LookupIndex(cp.codePoint, nextCp.codePoint);
                    if (variationGlyphIndex > 0)
                    {
                        //user glyph index from next codepoint
                        glyphIndex = variationGlyphIndex;
                        //but record as current code point i
                        _inputGlyphs.AddGlyph(i, glyphIndex);

                        ++i; //skip
                        continue;//*** 
                    }
                }
                _inputGlyphs.AddGlyph(i, glyphIndex);
            }

            //[C]
            //----------------------------------------------  
            //glyph substitution            
            if (_gsub != null & len > 0)
            {
                //TODO: review perf here
                _gsub.EnableLigation = this.EnableLigature;
                _gsub.EnableComposition = this.EnableComposition;
                _gsub.DoSubstitution(_inputGlyphs);
            }

            //----------------------------------------------  
            //after glyph substitution,
            //number of input glyph MAY changed (increase or decrease).***
            //so count again.
            int finalGlyphCount = _inputGlyphs.Count;
            //----------------------------------------------  

            //[D]
            //glyph position
            _glyphPositions.Clear();
            _glyphPositions.Typeface = _typeface;
            for (int i = 0; i < finalGlyphCount; ++i)
            {
                //at this stage _inputGlyphs and _glyphPositions 
                //has member 1:1
                ushort glyIndex, input_codepointOffset, input_mapLen;
                _inputGlyphs.GetGlyphIndexAndMap(i, out glyIndex, out input_codepointOffset, out input_mapLen);
                //
                Glyph orgGlyph = _typeface.GetGlyphByIndex(glyIndex);
                //this is original value WITHOUT fit-to-grid adjust
                _glyphPositions.AddGlyph((short)input_codepointOffset, glyIndex, orgGlyph);
            }

            PositionTechnique posTech = this.PositionTechnique;
            if (_gpos != null && len > 1 && posTech == PositionTechnique.OpenFont)
            {
                _gpos.DoGlyphPosition(_glyphPositions);
            }
            //----------------------------------------------  
            //at this point, all positions are layouted at its original scale ***
            //then we will scale it to target scale later 
            //----------------------------------------------   
        }


        /// <summary>
        /// generate map from user codepoint buffer to output glyph index, from latest layout result
        /// </summary>
        /// <param name="outputUserCharToGlyphIndexMapList"></param>
        public void CreateMapFromUserCharToGlyphIndices(List<UserCodePointToGlyphIndex> outputUserCharToGlyphIndexMapList)
        {
            //1. get map from user-input-codepoint to glyph-index 
            _inputGlyphs.CreateMapFromUserCodePointToGlyphIndices(outputUserCharToGlyphIndexMapList);

            ////TODO:
            ////2. 
            ////since some user-input-codepoints may be skiped in codepoint-to-glyph index lookup (see this.Layout(), [A])    
            //int j = outputUserCharToGlyphIndexMapList.Count;
            //for (int i = 0; i < j; ++i)
            //{ 
            //    UserCodePointToGlyphIndex userCodePointToGlyphIndex = outputUserCharToGlyphIndexMapList[i];
            //    CodePointFromUserChar codePointFromUserChar = _reusableCodePointFromUserCharList[userCodePointToGlyphIndex.userCodePointIndex]; 
            //}
        }
        void UpdateLayoutPlan()
        {
            GlyphLayoutPlanContext context = _layoutPlanCollection.GetPlanOrCreate(this._typeface, this._scriptLang);
            this._gpos = context._glyphPos;
            this._gsub = context._glyphSub;
            _needPlanUpdate = false;
        }
    }




    public static class GlyphLayoutExtensions
    {

#if DEBUG
        public static float dbugSnapToFitInteger(float value)
        {
            int floor_value = (int)value;
            return (value - floor_value >= (1f / 2f)) ? floor_value + 1 : floor_value;
        }
        public static float dbugSnapHalf(float value)
        {
            int floor_value = (int)value;
            //round to int 0, 0.5,1.0
            return (value - floor_value >= (2f / 3f)) ? floor_value + 1 : //else->
                   (value - floor_value >= (1f / 3f)) ? floor_value + 0.5f : floor_value;
        }
        static int dbugSnapUpper(float value)
        {
            int floor_value = (int)value;
            return floor_value + 1;
        }

#endif


        /// <summary>
        /// generate scaled from unscale glyph size to specific scale
        /// </summary>
        /// <param name="glyphPositions"></param>
        /// <param name="pxscale"></param>
        /// <param name="outputGlyphPlanList"></param>
        public static void GenerateGlyphPlans(IGlyphPositions glyphPositions,
            float pxscale,
            bool snapToGrid,
            PxScaledGlyphPlanList outputGlyphPlanList)
        {
            //user can implement this with some 'PixelScaleEngine' 
            if (snapToGrid)
            {

                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    short input_offset, offsetX, offsetY, advW; //all from pen-pos
                    ushort glyphIndex = glyphPositions.GetGlyph(i, out input_offset, out offsetX, out offsetY, out advW);

                    outputGlyphPlanList.Append(new PxScaledGlyphPlan(
                        input_offset,
                        glyphIndex,
                        (short)Math.Round(advW * pxscale),
                        (short)Math.Round(offsetX * pxscale),
                        (short)Math.Round(offsetY * pxscale)
                        ));
                }
            }
            else
            {
                //not snap to grid
                //scaled but not snap to grid
                int finalGlyphCount = glyphPositions.Count;
                for (int i = 0; i < finalGlyphCount; ++i)
                {
                    short input_offset, offsetX, offsetY, advW; //all from pen-pos
                    ushort glyphIndex = glyphPositions.GetGlyph(i, out input_offset, out offsetX, out offsetY, out advW);

                    outputGlyphPlanList.Append(new PxScaledGlyphPlan(
                        input_offset,
                        glyphIndex,
                        advW * pxscale,
                        offsetX * pxscale,
                        offsetY * pxscale
                        ));
                }
            }

        }

    }

    /// <summary>
    /// glyph position stream
    /// </summary>
    class GlyphPosStream : IGlyphPositions
    {
        List<GlyphPos> _glyphPosList = new List<GlyphPos>();

        Typeface _typeface;
        public GlyphPosStream() { }

        public int Count
        {
            get
            {
                return _glyphPosList.Count;
            }
        }
        public void Clear()
        {
            _typeface = null;
            _glyphPosList.Clear();
        }
        public Typeface Typeface
        {
            get { return this._typeface; }
            set { this._typeface = value; }
        }
        public void AddGlyph(short o_offset, ushort glyphIndex, Glyph glyph)
        {
            if (!glyph.HasOriginalAdvancedWidth)
            {
                glyph.OriginalAdvanceWidth = _typeface.GetHAdvanceWidthFromGlyphIndex(glyphIndex);
            }
            _glyphPosList.Add(new GlyphPos(o_offset, glyphIndex, glyph.GlyphClass, glyph.OriginalAdvanceWidth));
        }
        public void AppendGlyphOffset(int index, short appendOffsetX, short appendOffsetY)
        {
            GlyphPos existing = _glyphPosList[index];
            existing.xoffset += appendOffsetX;
            existing.yoffset += appendOffsetY;
            _glyphPosList[index] = existing;
        }
        public GlyphPos this[int index]
        {

            get
            {
                return _glyphPosList[index];
            }
        }
        public GlyphClassKind GetGlyphClassKind(int index)
        {
            return _glyphPosList[index].classKind;
        }
        public ushort GetGlyph(int index, out ushort advW)
        {
            GlyphPos pos = _glyphPosList[index];
            advW = (ushort)pos.advanceW;
            return pos.glyphIndex;
        }
        public ushort GetGlyph(int index, out short inputOffset, out short offsetX, out short offsetY, out short advW)
        {
            GlyphPos pos = _glyphPosList[index];
            offsetX = pos.xoffset;
            offsetY = pos.yoffset;
            advW = pos.advanceW;
            inputOffset = pos.o_offset;
            return pos.glyphIndex;
        }
        public void GetOffset(int index, out short offsetX, out short offsetY)
        {
            GlyphPos pos = _glyphPosList[index];
            offsetX = pos.xoffset;
            offsetY = pos.yoffset;
        }

        public void AppendGlyphAdvance(int index, short appendAdvX, short appendAdvY)
        {
            GlyphPos pos = _glyphPosList[index];
            pos.advanceW += appendAdvX;//TODO: review for appendY
            _glyphPosList[index] = pos;
        }

    }

    struct GlyphPos
    {
        public readonly short o_offset; //original user offset
        public readonly ushort glyphIndex;
        public short xoffset;
        public short yoffset;
        public short advanceW; // actually this value is ushort, TODO: review here
        public readonly GlyphClassKind glyphClass;

        public GlyphPos(short o_offset,
            ushort glyphIndex,
            GlyphClassKind glyphClass,
            ushort orgAdvanced
            )
        {
            this.o_offset = o_offset;
            this.glyphClass = glyphClass;
            this.glyphIndex = glyphIndex;
            this.advanceW = (short)orgAdvanced;
            xoffset = yoffset = 0;
        }
        public GlyphClassKind classKind
        {
            get { return glyphClass; }
        }

        public short OffsetX { get { return xoffset; } }
        public short OffsetY { get { return yoffset; } }
#if DEBUG
        public override string ToString()
        {
            return glyphIndex.ToString() + "(" + xoffset + "," + yoffset + ")";
        }
#endif
    }
}
