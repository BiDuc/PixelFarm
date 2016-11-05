﻿//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System.Collections.Generic;
using NOpenType.Tables;
namespace NOpenType
{
    public class Typeface
    {
        readonly Bounds _bounds;
        readonly ushort _unitsPerEm;
        readonly Glyph[] _glyphs;
        readonly CharacterMap[] _cmaps;
        readonly HorizontalMetrics _horizontalMetrics;
        readonly NameEntry _nameEntry;

        Kern _kern;

        internal Typeface(
            NameEntry nameEntry,
            Bounds bounds,
            ushort unitsPerEm,
            Glyph[] glyphs,
            CharacterMap[] cmaps,
            HorizontalMetrics horizontalMetrics,
            OS2Table os2Table)
        {
            _nameEntry = nameEntry;
            _bounds = bounds;
            _unitsPerEm = unitsPerEm;
            _glyphs = glyphs;
            _cmaps = cmaps;
            _horizontalMetrics = horizontalMetrics;
            OS2Table = os2Table;
        }
        internal Kern KernTable
        {
            get { return _kern; }
            set { this._kern = value; }
        }
        internal Gasp GaspTable
        {
            get;
            set;
        }
        internal OS2Table OS2Table
        {
            get;
            set;
        }

        /// <summary>
        /// OS2 sTypoAscender, in font designed unit
        /// </summary>
        public short Ascender
        {
            get
            {
                return OS2Table.sTypoAscender;
            }
        }
        
        /// <summary>
        /// OS2 sTypoDescender, in font designed unit
        /// </summary>
        public short Descender
        {
            get
            {
                return OS2Table.sTypoDescender;
            }
        }
        /// <summary>
        /// OS2 Linegap
        /// </summary>
        public short LineGap
        {
            get
            {
                return OS2Table.sTypoLineGap;
            }
        }
     



        public string Name
        {
            get { return _nameEntry.FontName; }
        }
        public string FontSubFamily
        {
            get { return _nameEntry.FontSubFamily; }
        }
        public int LookupIndex(char character)
        {
            // TODO: What if there are none or several tables?
            return _cmaps[0].CharacterToGlyphIndex(character);
        }

        public Glyph Lookup(char character)
        {
            return _glyphs[LookupIndex(character)];
        }
        public Glyph GetGlyphByIndex(int glyphIndex)
        {
            return _glyphs[glyphIndex];
        }
        public ushort GetAdvanceWidth(char character)
        {
            return _horizontalMetrics.GetAdvanceWidth(LookupIndex(character));
        }
        public ushort GetAdvanceWidthFromGlyphIndex(int glyphIndex)
        {
            return _horizontalMetrics.GetAdvanceWidth(glyphIndex);
        }
        public short GetKernDistance(ushort leftGlyphIndex, ushort rightGlyphIndex)
        {
            return _kern.GetKerningDistance(leftGlyphIndex, rightGlyphIndex);
        }
        public Bounds Bounds { get { return _bounds; } }
        public ushort UnitsPerEm { get { return _unitsPerEm; } }
        public Glyph[] Glyphs { get { return _glyphs; } }


        const int pointsPerInch = 72;
        public float CalculateScale(float sizeInPointUnit, int resolution = 96)
        {
            return ((sizeInPointUnit * resolution) / (pointsPerInch * this.UnitsPerEm));
        }

        GDEF GDEFTable
        {
            get;
            set;
        }
        GSUB GSUBTable
        {
            get;
            set;
        }
        GPOS GPOSTable
        {
            get;
            set;
        }
        BASE BaseTable
        {
            get;
            set;
        }

        //-------------------------------------------------------

        public void Lookup(char[] buffer, List<int> output)
        {
            //do shaping here?
            //1. do look up and substitution 
            int j = buffer.Length;
            for (int i = 0; i < j; ++i)
            {
                output.Add(LookupIndex(buffer[i]));
            }
            //tmp disable here
            //check for glyph substitution
            //this.GSUBTable.CheckSubstitution(output[1]);
        }
        //-------------------------------------------------------
        //experiment
        internal void LoadOpenTypeLayoutInfo(GDEF gdefTable, GSUB gsubTable, GPOS gposTable, BASE baseTable)
        {

            //***
            this.GDEFTable = gdefTable;
            this.GSUBTable = gsubTable;
            this.GPOSTable = gposTable;
            this.BaseTable = baseTable;
            //---------------------------
            //1. fill glyph definition            
            if (gdefTable != null)
            {
                gdefTable.FillGlyphData(this.Glyphs);
            }
        }
    }

    //------------------------------------------------------------------------------------------------------
    namespace Extensions
    {

        public static class TypefaceExtensions
        {
            public static bool DoseSupportUnicode(
                this Typeface typeface,
                UnicodeLangBits unicodeLangBits)
            {
                if (typeface.OS2Table == null)
                {
                    return false;
                }
                //-----------------------------
                long bits = (long)unicodeLangBits;
                int bitpos = (int)(bits >> 32);

                if (bitpos == 0)
                {
                    return true; //default
                }
                else if (bitpos < 32)
                {
                    //use range 1
                    return (typeface.OS2Table.ulUnicodeRange1 & (1 << bitpos)) != 0;
                }
                else if (bitpos < 64)
                {
                    return (typeface.OS2Table.ulUnicodeRange2 & (1 << (bitpos - 32))) != 0;
                }
                else if (bitpos < 96)
                {
                    return (typeface.OS2Table.ulUnicodeRange3 & (1 << (bitpos - 64))) != 0;
                }
                else if (bitpos < 128)
                {
                    return (typeface.OS2Table.ulUnicodeRange4 & (1 << (bitpos - 96))) != 0;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
        public static class UnicodeLangBitsExtension
        {
            public static UnicodeRangeInfo ToUnicodeRangeInfo(this UnicodeLangBits unicodeLangBits)
            {
                long bits = (long)unicodeLangBits;
                int bitpos = (int)(bits >> 32);
                int lower32 = (int)(bits & 0xFFFFFFFF);
                return new UnicodeRangeInfo(bitpos,
                    lower32 >> 16,
                    lower32 & 0xFFFF);
            }
        }

    }
}
