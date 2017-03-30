﻿//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
using Typography.TextLayout;

namespace Typography.Rendering
{
    public class InstalledFont
    {

        public InstalledFont(string fontName, string fontSubFamily, string fontPath)
        {
            FontName = fontName;
            FontSubFamily = fontSubFamily;
            FontPath = fontPath;
        }

        public string FontName { get; set; }
        public string FontSubFamily { get; set; }
        public string FontPath { get; set; }

#if DEBUG
        public override string ToString()
        {
            return FontName + " " + FontSubFamily;
        }
#endif
    }


    public interface IFontface
    {
        string FontName { get; }
        string FontSubFamily { get; }
    }

    public class FontRequest
    {
        public string FontName { get; set; }
        public InstalledFontStyle Style { get; set; }
    }


    /// <summary>
    /// base TextPrinter class for developer only, 
    /// </summary>
    public abstract class DevTextPrinterBase
    {
        HintTechnique _hintTech;
        public DevTextPrinterBase()
        {
            FontSizeInPoints = 14;//
            ScriptLang = Typography.OpenFont.ScriptLangs.Latin;//default?
        }

        ///// <summary>
        ///// directly set request font stream to current printer
        ///// </summary>
        //public abstract FontStreamSource FontStreamSource
        //{
        //    get;
        //    set;
        //}


        public abstract Typography.TextLayout.GlyphLayout GlyphLayoutMan { get; }
        public abstract Typography.OpenFont.Typeface Typeface { get; set; }
        public bool FillBackground { get; set; }
        public bool DrawOutline { get; set; }
        public float FontAscendingPx { get; set; }
        public float FontDescedingPx { get; set; }
        public float FontLineGapPx { get; set; }
        public float FontLineSpacingPx { get; set; }

        public HintTechnique HintTechnique
        {
            get { return _hintTech; }
            set
            {
                this._hintTech = value;
            }
        }


        float _fontSizeInPoints;
        public float FontSizeInPoints
        {
            get { return _fontSizeInPoints; }
            set
            {
                if (_fontSizeInPoints != value)
                {
                    _fontSizeInPoints = value;
                    OnFontSizeChanged();
                }
            }
        }

        protected virtual void OnFontSizeChanged() { }
        public Typography.OpenFont.ScriptLang ScriptLang { get; set; }
        public Typography.TextLayout.PositionTechnique PositionTechnique { get; set; }
        public bool EnableLigature { get; set; }
        /// <summary>
        /// draw string at (xpos,ypos) of baseline 
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public abstract void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos);
        /// <summary>
        /// draw glyph plan list at (xpos,ypos) of baseline
        /// </summary>
        /// <param name="glyphPlanList"></param>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public abstract void DrawFromGlyphPlans(List<GlyphPlan> glyphPlanList, int startAt, int len, float xpos, float ypos);

        /// <summary>
        /// draw caret at xpos,ypos (sample only)
        /// </summary>
        /// <param name="xpos"></param>
        /// <param name="ypos"></param>
        public abstract void DrawCaret(float xpos, float ypos);


        //----------------------------------------------------
        //helper methods
        public void DrawString(char[] textBuffer, float xpos, float ypos)
        {
            DrawString(textBuffer, 0, textBuffer.Length, xpos, ypos);
        }
        public void DrawFromGlyphPlans(List<GlyphPlan> glyphPlanList, float xpos, float ypos)
        {
            DrawFromGlyphPlans(glyphPlanList, 0, glyphPlanList.Count, xpos, ypos);
        }

    }

}