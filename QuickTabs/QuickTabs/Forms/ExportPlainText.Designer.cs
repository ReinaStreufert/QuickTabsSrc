namespace QuickTabs.Forms
{
    partial class ExportPlainText
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.previewBox = new System.Windows.Forms.TextBox();
            this.includeName = new System.Windows.Forms.CheckBox();
            this.includeTempo = new System.Windows.Forms.CheckBox();
            this.includeTs = new System.Windows.Forms.CheckBox();
            this.includeTuning = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.enableWrap = new System.Windows.Forms.CheckBox();
            this.wrapEveryText = new System.Windows.Forms.Label();
            this.wrapEvery = new System.Windows.Forms.NumericUpDown();
            this.measuresText = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.staffStyle = new System.Windows.Forms.ComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.wrapEvery)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(242, 33);
            this.label2.TabIndex = 2;
            this.label2.Text = "Metadata Options";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(341, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 33);
            this.label1.TabIndex = 3;
            this.label1.Text = "Preview";
            // 
            // previewBox
            // 
            this.previewBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.previewBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.previewBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.previewBox.Location = new System.Drawing.Point(347, 45);
            this.previewBox.Multiline = true;
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(772, 629);
            this.previewBox.TabIndex = 4;
            this.previewBox.WordWrap = false;
            // 
            // includeName
            // 
            this.includeName.AutoSize = true;
            this.includeName.Checked = true;
            this.includeName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.includeName.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.includeName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.includeName.Location = new System.Drawing.Point(19, 46);
            this.includeName.Name = "includeName";
            this.includeName.Size = new System.Drawing.Size(211, 37);
            this.includeName.TabIndex = 5;
            this.includeName.Text = "Include name";
            this.includeName.UseVisualStyleBackColor = true;
            this.includeName.CheckedChanged += new System.EventHandler(this.includeName_CheckedChanged);
            // 
            // includeTempo
            // 
            this.includeTempo.AutoSize = true;
            this.includeTempo.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.includeTempo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.includeTempo.Location = new System.Drawing.Point(19, 89);
            this.includeTempo.Name = "includeTempo";
            this.includeTempo.Size = new System.Drawing.Size(222, 37);
            this.includeTempo.TabIndex = 6;
            this.includeTempo.Text = "Include tempo";
            this.includeTempo.UseVisualStyleBackColor = true;
            this.includeTempo.CheckedChanged += new System.EventHandler(this.includeTempo_CheckedChanged);
            // 
            // includeTs
            // 
            this.includeTs.AutoSize = true;
            this.includeTs.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.includeTs.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.includeTs.Location = new System.Drawing.Point(19, 132);
            this.includeTs.Name = "includeTs";
            this.includeTs.Size = new System.Drawing.Size(317, 37);
            this.includeTs.TabIndex = 7;
            this.includeTs.Text = "Include time signature";
            this.includeTs.UseVisualStyleBackColor = true;
            this.includeTs.CheckedChanged += new System.EventHandler(this.includeTs_CheckedChanged);
            // 
            // includeTuning
            // 
            this.includeTuning.AutoSize = true;
            this.includeTuning.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.includeTuning.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.includeTuning.Location = new System.Drawing.Point(19, 175);
            this.includeTuning.Name = "includeTuning";
            this.includeTuning.Size = new System.Drawing.Size(293, 37);
            this.includeTuning.TabIndex = 8;
            this.includeTuning.Text = "Include exact tuning";
            this.includeTuning.UseVisualStyleBackColor = true;
            this.includeTuning.CheckedChanged += new System.EventHandler(this.includeTuning_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(14, 329);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 33);
            this.label3.TabIndex = 9;
            this.label3.Text = "Wrap";
            // 
            // enableWrap
            // 
            this.enableWrap.AutoSize = true;
            this.enableWrap.Checked = true;
            this.enableWrap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableWrap.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.enableWrap.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.enableWrap.Location = new System.Drawing.Point(19, 365);
            this.enableWrap.Name = "enableWrap";
            this.enableWrap.Size = new System.Drawing.Size(249, 37);
            this.enableWrap.TabIndex = 10;
            this.enableWrap.Text = "Enable wrapping";
            this.enableWrap.UseVisualStyleBackColor = true;
            this.enableWrap.CheckedChanged += new System.EventHandler(this.enableWrap_CheckedChanged);
            // 
            // wrapEveryText
            // 
            this.wrapEveryText.AutoSize = true;
            this.wrapEveryText.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.wrapEveryText.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.wrapEveryText.Location = new System.Drawing.Point(12, 412);
            this.wrapEveryText.Name = "wrapEveryText";
            this.wrapEveryText.Size = new System.Drawing.Size(150, 33);
            this.wrapEveryText.TabIndex = 11;
            this.wrapEveryText.Text = "Wrap every";
            // 
            // wrapEvery
            // 
            this.wrapEvery.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.wrapEvery.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.wrapEvery.ForeColor = System.Drawing.Color.White;
            this.wrapEvery.Location = new System.Drawing.Point(235, 414);
            this.wrapEvery.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.wrapEvery.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.wrapEvery.Name = "wrapEvery";
            this.wrapEvery.Size = new System.Drawing.Size(90, 37);
            this.wrapEvery.TabIndex = 12;
            this.wrapEvery.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.wrapEvery.ValueChanged += new System.EventHandler(this.wrapEvery_ValueChanged);
            // 
            // measuresText
            // 
            this.measuresText.AutoSize = true;
            this.measuresText.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.measuresText.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.measuresText.Location = new System.Drawing.Point(202, 460);
            this.measuresText.Name = "measuresText";
            this.measuresText.Size = new System.Drawing.Size(132, 33);
            this.measuresText.TabIndex = 13;
            this.measuresText.Text = "measures";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label6.Location = new System.Drawing.Point(14, 234);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 33);
            this.label6.TabIndex = 14;
            this.label6.Text = "Staff style";
            // 
            // staffStyle
            // 
            this.staffStyle.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.staffStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.staffStyle.DropDownWidth = 500;
            this.staffStyle.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.staffStyle.FormattingEnabled = true;
            this.staffStyle.Items.AddRange(new object[] {
            "QuickTabs style",
            "Standard spaced (hides note length)"});
            this.staffStyle.Location = new System.Drawing.Point(19, 270);
            this.staffStyle.Name = "staffStyle";
            this.staffStyle.Size = new System.Drawing.Size(306, 41);
            this.staffStyle.TabIndex = 15;
            this.staffStyle.SelectedIndexChanged += new System.EventHandler(this.staffStyle_SelectedIndexChanged);
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(154)))), ((int)(((byte)(231)))));
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.Font = new System.Drawing.Font("Montserrat", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.saveButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.saveButton.Location = new System.Drawing.Point(19, 506);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(306, 56);
            this.saveButton.TabIndex = 16;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = false;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // ExportPlainText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(1117, 674);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.staffStyle);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.measuresText);
            this.Controls.Add(this.wrapEvery);
            this.Controls.Add(this.wrapEveryText);
            this.Controls.Add(this.enableWrap);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.includeTuning);
            this.Controls.Add(this.includeTs);
            this.Controls.Add(this.includeTempo);
            this.Controls.Add(this.includeName);
            this.Controls.Add(this.previewBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Name = "ExportPlainText";
            this.ShowIcon = false;
            this.Text = "Export plain text";
            ((System.ComponentModel.ISupportInitialize)(this.wrapEvery)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label2;
        private Label label1;
        private TextBox previewBox;
        private CheckBox includeName;
        private CheckBox includeTempo;
        private CheckBox includeTs;
        private CheckBox includeTuning;
        private Label label3;
        private CheckBox enableWrap;
        private Label wrapEveryText;
        private NumericUpDown wrapEvery;
        private Label measuresText;
        private Label label6;
        private ComboBox staffStyle;
        private Button saveButton;
    }
}