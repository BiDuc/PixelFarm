﻿namespace SampleWinForms
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.txtInputChar = new System.Windows.Forms.TextBox();
            this.chkBorder = new System.Windows.Forms.CheckBox();
            this.chkFillBackground = new System.Windows.Forms.CheckBox();
            this.cmbRenderChoices = new System.Windows.Forms.ComboBox();
            this.lstFontSizes = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkShowControlPoints = new System.Windows.Forms.CheckBox();
            this.chkShowTess = new System.Windows.Forms.CheckBox();
            this.chkShowGrid = new System.Windows.Forms.CheckBox();
            this.txtGridSize = new System.Windows.Forms.TextBox();
            this.chkYGridFitting = new System.Windows.Forms.CheckBox();
            this.chkDrawCentroidBone = new System.Windows.Forms.CheckBox();
            this.chkXGridFitting = new System.Windows.Forms.CheckBox();
            this.chkLcdTechnique = new System.Windows.Forms.CheckBox();
            this.cmdBuildMsdfTexture = new System.Windows.Forms.Button();
            this.cmbPositionTech = new System.Windows.Forms.ComboBox();
            this.lstFontList = new System.Windows.Forms.ListBox();
            this.chkGsubEnableLigature = new System.Windows.Forms.CheckBox();
            this.lstHintList = new System.Windows.Forms.ListBox();
            this.chkShowSampleTextBox = new System.Windows.Forms.CheckBox();
            this.sampleTextBox1 = new SampleWinForms.SampleTextBox();
            this.lstGlyphSnapX = new System.Windows.Forms.ListBox();
            this.lstGlyphSnapY = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkDrawGlyphBone = new System.Windows.Forms.CheckBox();
            this.chkDynamicOutline = new System.Windows.Forms.CheckBox();
            this.txtLeftXControl = new System.Windows.Forms.TextBox();
            this.chkMinorOffset = new System.Windows.Forms.CheckBox();
            this.chkDrawTriangles = new System.Windows.Forms.CheckBox();
            this.chkDrawRegenerateOutline = new System.Windows.Forms.CheckBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.chkDrawLineHubConn = new System.Windows.Forms.CheckBox();
            this.chkDrawPerpendicularLine = new System.Windows.Forms.CheckBox();
            this.lstEdgeOffset = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(436, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(81, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "Render!";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // txtInputChar
            // 
            this.txtInputChar.Location = new System.Drawing.Point(0, -2);
            this.txtInputChar.Name = "txtInputChar";
            this.txtInputChar.Size = new System.Drawing.Size(168, 20);
            this.txtInputChar.TabIndex = 1;
            this.txtInputChar.Text = "I";
            // 
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Location = new System.Drawing.Point(937, 41);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(57, 17);
            this.chkBorder.TabIndex = 3;
            this.chkBorder.Text = "Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            // 
            // chkFillBackground
            // 
            this.chkFillBackground.AutoSize = true;
            this.chkFillBackground.Location = new System.Drawing.Point(937, 64);
            this.chkFillBackground.Name = "chkFillBackground";
            this.chkFillBackground.Size = new System.Drawing.Size(101, 17);
            this.chkFillBackground.TabIndex = 4;
            this.chkFillBackground.Text = "Fill BackGround";
            this.chkFillBackground.UseVisualStyleBackColor = true;
            // 
            // cmbRenderChoices
            // 
            this.cmbRenderChoices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRenderChoices.FormattingEnabled = true;
            this.cmbRenderChoices.Location = new System.Drawing.Point(523, 12);
            this.cmbRenderChoices.Name = "cmbRenderChoices";
            this.cmbRenderChoices.Size = new System.Drawing.Size(224, 21);
            this.cmbRenderChoices.TabIndex = 7;
            // 
            // lstFontSizes
            // 
            this.lstFontSizes.FormattingEnabled = true;
            this.lstFontSizes.Location = new System.Drawing.Point(528, 167);
            this.lstFontSizes.Name = "lstFontSizes";
            this.lstFontSizes.Size = new System.Drawing.Size(121, 212);
            this.lstFontSizes.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(525, 151);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Size in Points";
            // 
            // chkShowControlPoints
            // 
            this.chkShowControlPoints.AutoSize = true;
            this.chkShowControlPoints.Checked = true;
            this.chkShowControlPoints.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowControlPoints.Location = new System.Drawing.Point(937, 86);
            this.chkShowControlPoints.Name = "chkShowControlPoints";
            this.chkShowControlPoints.Size = new System.Drawing.Size(121, 17);
            this.chkShowControlPoints.TabIndex = 12;
            this.chkShowControlPoints.Text = "Show Control Points";
            this.chkShowControlPoints.UseVisualStyleBackColor = true;
            // 
            // chkShowTess
            // 
            this.chkShowTess.AutoSize = true;
            this.chkShowTess.Location = new System.Drawing.Point(811, 173);
            this.chkShowTess.Name = "chkShowTess";
            this.chkShowTess.Size = new System.Drawing.Size(110, 17);
            this.chkShowTess.TabIndex = 13;
            this.chkShowTess.Text = "Show Tesselation";
            this.chkShowTess.UseVisualStyleBackColor = true;
            // 
            // chkShowGrid
            // 
            this.chkShowGrid.AutoSize = true;
            this.chkShowGrid.Location = new System.Drawing.Point(665, 173);
            this.chkShowGrid.Name = "chkShowGrid";
            this.chkShowGrid.Size = new System.Drawing.Size(75, 17);
            this.chkShowGrid.TabIndex = 14;
            this.chkShowGrid.Text = "Show Grid";
            this.chkShowGrid.UseVisualStyleBackColor = true;
            // 
            // txtGridSize
            // 
            this.txtGridSize.Location = new System.Drawing.Point(735, 173);
            this.txtGridSize.Name = "txtGridSize";
            this.txtGridSize.Size = new System.Drawing.Size(51, 20);
            this.txtGridSize.TabIndex = 15;
            this.txtGridSize.Text = "5";
            // 
            // chkYGridFitting
            // 
            this.chkYGridFitting.AutoSize = true;
            this.chkYGridFitting.Checked = true;
            this.chkYGridFitting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkYGridFitting.Location = new System.Drawing.Point(667, 225);
            this.chkYGridFitting.Name = "chkYGridFitting";
            this.chkYGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkYGridFitting.TabIndex = 16;
            this.chkYGridFitting.Text = "Y Grid Auto Fitting";
            this.chkYGridFitting.UseVisualStyleBackColor = true;
            // 
            // chkDrawCentroidBone
            // 
            this.chkDrawCentroidBone.AutoSize = true;
            this.chkDrawCentroidBone.Checked = true;
            this.chkDrawCentroidBone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawCentroidBone.Location = new System.Drawing.Point(827, 223);
            this.chkDrawCentroidBone.Name = "chkDrawCentroidBone";
            this.chkDrawCentroidBone.Size = new System.Drawing.Size(121, 17);
            this.chkDrawCentroidBone.TabIndex = 19;
            this.chkDrawCentroidBone.Text = "Draw Centroid Bone";
            this.chkDrawCentroidBone.UseVisualStyleBackColor = true;
            // 
            // chkXGridFitting
            // 
            this.chkXGridFitting.AutoSize = true;
            this.chkXGridFitting.Location = new System.Drawing.Point(667, 248);
            this.chkXGridFitting.Name = "chkXGridFitting";
            this.chkXGridFitting.Size = new System.Drawing.Size(111, 17);
            this.chkXGridFitting.TabIndex = 20;
            this.chkXGridFitting.Text = "X Grid Auto Fitting";
            this.chkXGridFitting.UseVisualStyleBackColor = true;
            // 
            // chkLcdTechnique
            // 
            this.chkLcdTechnique.AutoSize = true;
            this.chkLcdTechnique.Location = new System.Drawing.Point(667, 271);
            this.chkLcdTechnique.Name = "chkLcdTechnique";
            this.chkLcdTechnique.Size = new System.Drawing.Size(95, 17);
            this.chkLcdTechnique.TabIndex = 21;
            this.chkLcdTechnique.Text = "LcdTechnique";
            this.chkLcdTechnique.UseVisualStyleBackColor = true;
            // 
            // cmdBuildMsdfTexture
            // 
            this.cmdBuildMsdfTexture.Location = new System.Drawing.Point(937, 135);
            this.cmdBuildMsdfTexture.Name = "cmdBuildMsdfTexture";
            this.cmdBuildMsdfTexture.Size = new System.Drawing.Size(121, 28);
            this.cmdBuildMsdfTexture.TabIndex = 22;
            this.cmdBuildMsdfTexture.Text = "Make MsdfTexture";
            this.cmdBuildMsdfTexture.UseVisualStyleBackColor = true;
            this.cmdBuildMsdfTexture.Click += new System.EventHandler(this.cmdBuildMsdfTexture_Click);
            // 
            // cmbPositionTech
            // 
            this.cmbPositionTech.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPositionTech.FormattingEnabled = true;
            this.cmbPositionTech.Location = new System.Drawing.Point(523, 39);
            this.cmbPositionTech.Name = "cmbPositionTech";
            this.cmbPositionTech.Size = new System.Drawing.Size(224, 21);
            this.cmbPositionTech.TabIndex = 23;
            // 
            // lstFontList
            // 
            this.lstFontList.FormattingEnabled = true;
            this.lstFontList.Location = new System.Drawing.Point(800, 12);
            this.lstFontList.Name = "lstFontList";
            this.lstFontList.Size = new System.Drawing.Size(121, 121);
            this.lstFontList.TabIndex = 25;
            // 
            // chkGsubEnableLigature
            // 
            this.chkGsubEnableLigature.AutoSize = true;
            this.chkGsubEnableLigature.Checked = true;
            this.chkGsubEnableLigature.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGsubEnableLigature.Location = new System.Drawing.Point(938, 14);
            this.chkGsubEnableLigature.Name = "chkGsubEnableLigature";
            this.chkGsubEnableLigature.Size = new System.Drawing.Size(136, 17);
            this.chkGsubEnableLigature.TabIndex = 26;
            this.chkGsubEnableLigature.Text = "GSUB: Enable Ligature";
            this.chkGsubEnableLigature.UseVisualStyleBackColor = true;
            // 
            // lstHintList
            // 
            this.lstHintList.FormattingEnabled = true;
            this.lstHintList.Location = new System.Drawing.Point(523, 71);
            this.lstHintList.Name = "lstHintList";
            this.lstHintList.Size = new System.Drawing.Size(224, 69);
            this.lstHintList.TabIndex = 27;
            // 
            // chkShowSampleTextBox
            // 
            this.chkShowSampleTextBox.AutoSize = true;
            this.chkShowSampleTextBox.Location = new System.Drawing.Point(665, 146);
            this.chkShowSampleTextBox.Name = "chkShowSampleTextBox";
            this.chkShowSampleTextBox.Size = new System.Drawing.Size(133, 17);
            this.chkShowSampleTextBox.TabIndex = 39;
            this.chkShowSampleTextBox.Text = "Show Sample TextBox";
            this.chkShowSampleTextBox.UseVisualStyleBackColor = true;
            this.chkShowSampleTextBox.CheckedChanged += new System.EventHandler(this.chkShowSampleTextBox_CheckedChanged);
            // 
            // sampleTextBox1
            // 
            this.sampleTextBox1.BackColor = System.Drawing.Color.Silver;
            this.sampleTextBox1.Location = new System.Drawing.Point(12, 71);
            this.sampleTextBox1.Name = "sampleTextBox1";
            this.sampleTextBox1.Size = new System.Drawing.Size(505, 774);
            this.sampleTextBox1.TabIndex = 40;
            this.sampleTextBox1.Visible = false;
            // 
            // lstGlyphSnapX
            // 
            this.lstGlyphSnapX.FormattingEnabled = true;
            this.lstGlyphSnapX.Location = new System.Drawing.Point(666, 320);
            this.lstGlyphSnapX.Name = "lstGlyphSnapX";
            this.lstGlyphSnapX.Size = new System.Drawing.Size(120, 69);
            this.lstGlyphSnapX.TabIndex = 43;
            // 
            // lstGlyphSnapY
            // 
            this.lstGlyphSnapY.FormattingEnabled = true;
            this.lstGlyphSnapY.Location = new System.Drawing.Point(666, 421);
            this.lstGlyphSnapY.Name = "lstGlyphSnapY";
            this.lstGlyphSnapY.Size = new System.Drawing.Size(120, 69);
            this.lstGlyphSnapY.TabIndex = 44;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(664, 306);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 45;
            this.label1.Text = "SnapX";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(664, 405);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 46;
            this.label3.Text = "SnapY";
            // 
            // chkDrawGlyphBone
            // 
            this.chkDrawGlyphBone.AutoSize = true;
            this.chkDrawGlyphBone.Checked = true;
            this.chkDrawGlyphBone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawGlyphBone.Location = new System.Drawing.Point(827, 252);
            this.chkDrawGlyphBone.Name = "chkDrawGlyphBone";
            this.chkDrawGlyphBone.Size = new System.Drawing.Size(109, 17);
            this.chkDrawGlyphBone.TabIndex = 47;
            this.chkDrawGlyphBone.Text = "Draw Glyph Bone";
            this.chkDrawGlyphBone.UseVisualStyleBackColor = true;
            // 
            // chkDynamicOutline
            // 
            this.chkDynamicOutline.AutoSize = true;
            this.chkDynamicOutline.Location = new System.Drawing.Point(806, 306);
            this.chkDynamicOutline.Name = "chkDynamicOutline";
            this.chkDynamicOutline.Size = new System.Drawing.Size(130, 17);
            this.chkDynamicOutline.TabIndex = 51;
            this.chkDynamicOutline.Text = "Show DynamicOutline";
            this.chkDynamicOutline.UseVisualStyleBackColor = true;
            // 
            // txtLeftXControl
            // 
            this.txtLeftXControl.Location = new System.Drawing.Point(788, 380);
            this.txtLeftXControl.Name = "txtLeftXControl";
            this.txtLeftXControl.Size = new System.Drawing.Size(217, 20);
            this.txtLeftXControl.TabIndex = 52;
            this.txtLeftXControl.Text = "0";
            // 
            // chkMinorOffset
            // 
            this.chkMinorOffset.AutoSize = true;
            this.chkMinorOffset.Location = new System.Drawing.Point(788, 357);
            this.chkMinorOffset.Name = "chkMinorOffset";
            this.chkMinorOffset.Size = new System.Drawing.Size(80, 17);
            this.chkMinorOffset.TabIndex = 53;
            this.chkMinorOffset.Text = "Offset to Fit";
            this.chkMinorOffset.UseVisualStyleBackColor = true;
            // 
            // chkDrawTriangles
            // 
            this.chkDrawTriangles.AutoSize = true;
            this.chkDrawTriangles.Checked = true;
            this.chkDrawTriangles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawTriangles.Location = new System.Drawing.Point(827, 196);
            this.chkDrawTriangles.Name = "chkDrawTriangles";
            this.chkDrawTriangles.Size = new System.Drawing.Size(94, 17);
            this.chkDrawTriangles.TabIndex = 54;
            this.chkDrawTriangles.Text = "DrawTriangles";
            this.chkDrawTriangles.UseVisualStyleBackColor = true;
            // 
            // chkDrawRegenerateOutline
            // 
            this.chkDrawRegenerateOutline.AutoSize = true;
            this.chkDrawRegenerateOutline.Location = new System.Drawing.Point(826, 329);
            this.chkDrawRegenerateOutline.Name = "chkDrawRegenerateOutline";
            this.chkDrawRegenerateOutline.Size = new System.Drawing.Size(157, 17);
            this.chkDrawRegenerateOutline.TabIndex = 55;
            this.chkDrawRegenerateOutline.Text = "Draw Regenerated Outlines";
            this.chkDrawRegenerateOutline.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(528, 496);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(393, 349);
            this.treeView1.TabIndex = 56;
            // 
            // chkDrawLineHubConn
            // 
            this.chkDrawLineHubConn.AutoSize = true;
            this.chkDrawLineHubConn.Checked = true;
            this.chkDrawLineHubConn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawLineHubConn.Location = new System.Drawing.Point(827, 275);
            this.chkDrawLineHubConn.Name = "chkDrawLineHubConn";
            this.chkDrawLineHubConn.Size = new System.Drawing.Size(122, 17);
            this.chkDrawLineHubConn.TabIndex = 57;
            this.chkDrawLineHubConn.Text = "Draw LineHub Conn";
            this.chkDrawLineHubConn.UseVisualStyleBackColor = true;
            // 
            // chkDrawPerpendicularLine
            // 
            this.chkDrawPerpendicularLine.AutoSize = true;
            this.chkDrawPerpendicularLine.Checked = true;
            this.chkDrawPerpendicularLine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawPerpendicularLine.Location = new System.Drawing.Point(952, 196);
            this.chkDrawPerpendicularLine.Name = "chkDrawPerpendicularLine";
            this.chkDrawPerpendicularLine.Size = new System.Drawing.Size(142, 17);
            this.chkDrawPerpendicularLine.TabIndex = 58;
            this.chkDrawPerpendicularLine.Text = "Draw Perpendicular Line";
            this.chkDrawPerpendicularLine.UseVisualStyleBackColor = true;
            // 
            // lstEdgeOffset
            // 
            this.lstEdgeOffset.FormattingEnabled = true;
            this.lstEdgeOffset.Location = new System.Drawing.Point(528, 386);
            this.lstEdgeOffset.Name = "lstEdgeOffset";
            this.lstEdgeOffset.Size = new System.Drawing.Size(120, 95);
            this.lstEdgeOffset.TabIndex = 60;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1117, 857);
            this.Controls.Add(this.lstEdgeOffset);
            this.Controls.Add(this.chkDrawPerpendicularLine);
            this.Controls.Add(this.chkDrawLineHubConn);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.chkDrawRegenerateOutline);
            this.Controls.Add(this.chkDrawTriangles);
            this.Controls.Add(this.chkMinorOffset);
            this.Controls.Add(this.txtLeftXControl);
            this.Controls.Add(this.chkDynamicOutline);
            this.Controls.Add(this.chkDrawGlyphBone);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstGlyphSnapY);
            this.Controls.Add(this.lstGlyphSnapX);
            this.Controls.Add(this.sampleTextBox1);
            this.Controls.Add(this.chkShowSampleTextBox);
            this.Controls.Add(this.lstHintList);
            this.Controls.Add(this.chkGsubEnableLigature);
            this.Controls.Add(this.lstFontList);
            this.Controls.Add(this.cmbPositionTech);
            this.Controls.Add(this.cmdBuildMsdfTexture);
            this.Controls.Add(this.chkLcdTechnique);
            this.Controls.Add(this.chkXGridFitting);
            this.Controls.Add(this.chkDrawCentroidBone);
            this.Controls.Add(this.chkYGridFitting);
            this.Controls.Add(this.txtGridSize);
            this.Controls.Add(this.chkShowGrid);
            this.Controls.Add(this.chkShowTess);
            this.Controls.Add(this.chkShowControlPoints);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstFontSizes);
            this.Controls.Add(this.cmbRenderChoices);
            this.Controls.Add(this.chkFillBackground);
            this.Controls.Add(this.chkBorder);
            this.Controls.Add(this.txtInputChar);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtInputChar;
        private System.Windows.Forms.CheckBox chkBorder;
        private System.Windows.Forms.CheckBox chkFillBackground;
        private System.Windows.Forms.ComboBox cmbRenderChoices;
        private System.Windows.Forms.ListBox lstFontSizes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkShowControlPoints;
        private System.Windows.Forms.CheckBox chkShowTess;
        private System.Windows.Forms.CheckBox chkShowGrid;
        private System.Windows.Forms.TextBox txtGridSize;
        private System.Windows.Forms.CheckBox chkYGridFitting;
        private System.Windows.Forms.CheckBox chkDrawCentroidBone;
        private System.Windows.Forms.CheckBox chkXGridFitting;
        private System.Windows.Forms.CheckBox chkLcdTechnique;
        private System.Windows.Forms.Button cmdBuildMsdfTexture;
        private System.Windows.Forms.ComboBox cmbPositionTech;
       
        private System.Windows.Forms.ListBox lstFontList;
        private System.Windows.Forms.CheckBox chkGsubEnableLigature;
        private System.Windows.Forms.ListBox lstHintList;
        private System.Windows.Forms.CheckBox chkShowSampleTextBox;
        private SampleTextBox sampleTextBox1;
        private System.Windows.Forms.ListBox lstGlyphSnapX;
        private System.Windows.Forms.ListBox lstGlyphSnapY;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkDrawGlyphBone;
        private System.Windows.Forms.CheckBox chkDynamicOutline;
        private System.Windows.Forms.TextBox txtLeftXControl;
        private System.Windows.Forms.CheckBox chkMinorOffset;
        private System.Windows.Forms.CheckBox chkDrawTriangles;
        private System.Windows.Forms.CheckBox chkDrawRegenerateOutline;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.CheckBox chkDrawLineHubConn;
        private System.Windows.Forms.CheckBox chkDrawPerpendicularLine;
        private System.Windows.Forms.ListBox lstEdgeOffset;
    }
}

