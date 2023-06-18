namespace QuickTabs.Forms
{
    partial class DocumentProperties
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nameInput = new System.Windows.Forms.TextBox();
            this.tempoInput = new System.Windows.Forms.NumericUpDown();
            this.ts1Input = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.ts2Input = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tuningPickerContainer = new System.Windows.Forms.Panel();
            this.ts1DisabledLabel = new System.Windows.Forms.Label();
            this.ts2DisabledLabel = new System.Windows.Forms.Label();
            this.tuningPresetLink = new System.Windows.Forms.LinkLabel();
            this.tapTempoLink = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.tempoInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ts1Input)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ts2Input)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(523, 61);
            this.label1.TabIndex = 0;
            this.label1.Text = "Document Properties";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label2.Location = new System.Drawing.Point(24, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 29);
            this.label2.TabIndex = 1;
            this.label2.Text = "Song name";
            // 
            // nameInput
            // 
            this.nameInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.nameInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.nameInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.nameInput.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.nameInput.Location = new System.Drawing.Point(28, 118);
            this.nameInput.Name = "nameInput";
            this.nameInput.Size = new System.Drawing.Size(605, 35);
            this.nameInput.TabIndex = 2;
            // 
            // tempoInput
            // 
            this.tempoInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.tempoInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tempoInput.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.tempoInput.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.tempoInput.Location = new System.Drawing.Point(639, 118);
            this.tempoInput.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.tempoInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tempoInput.Name = "tempoInput";
            this.tempoInput.Size = new System.Drawing.Size(103, 35);
            this.tempoInput.TabIndex = 3;
            this.tempoInput.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // ts1Input
            // 
            this.ts1Input.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ts1Input.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ts1Input.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ts1Input.Location = new System.Drawing.Point(748, 118);
            this.ts1Input.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.ts1Input.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ts1Input.Name = "ts1Input";
            this.ts1Input.Size = new System.Drawing.Size(91, 35);
            this.ts1Input.TabIndex = 4;
            this.ts1Input.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.ts1Input.ValueChanged += new System.EventHandler(this.ts1Input_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(842, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 29);
            this.label3.TabIndex = 5;
            this.label3.Text = "/";
            // 
            // ts2Input
            // 
            this.ts2Input.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ts2Input.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ts2Input.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ts2Input.Location = new System.Drawing.Point(869, 117);
            this.ts2Input.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.ts2Input.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ts2Input.Name = "ts2Input";
            this.ts2Input.Size = new System.Drawing.Size(91, 35);
            this.ts2Input.TabIndex = 6;
            this.ts2Input.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.ts2Input.ValueChanged += new System.EventHandler(this.ts2Input_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label4.Location = new System.Drawing.Point(632, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 29);
            this.label4.TabIndex = 7;
            this.label4.Text = "BPM";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label5.Location = new System.Drawing.Point(742, 81);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(188, 29);
            this.label5.TabIndex = 8;
            this.label5.Text = "Time signature";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label6.Location = new System.Drawing.Point(27, 164);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 29);
            this.label6.TabIndex = 10;
            this.label6.Text = "Tuning";
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(154)))), ((int)(((byte)(231)))));
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.saveButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.saveButton.Location = new System.Drawing.Point(688, 315);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(272, 56);
            this.saveButton.TabIndex = 11;
            this.saveButton.Text = "Save All";
            this.saveButton.UseVisualStyleBackColor = false;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.White;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.cancelButton.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.cancelButton.Location = new System.Drawing.Point(405, 315);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(272, 56);
            this.cancelButton.TabIndex = 12;
            this.cancelButton.Text = "Discard Changes";
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // tuningPickerContainer
            // 
            this.tuningPickerContainer.Location = new System.Drawing.Point(28, 200);
            this.tuningPickerContainer.Name = "tuningPickerContainer";
            this.tuningPickerContainer.Size = new System.Drawing.Size(932, 109);
            this.tuningPickerContainer.TabIndex = 13;
            // 
            // ts1DisabledLabel
            // 
            this.ts1DisabledLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ts1DisabledLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ts1DisabledLabel.Location = new System.Drawing.Point(748, 117);
            this.ts1DisabledLabel.Name = "ts1DisabledLabel";
            this.ts1DisabledLabel.Size = new System.Drawing.Size(91, 38);
            this.ts1DisabledLabel.TabIndex = 14;
            this.ts1DisabledLabel.Text = "4";
            this.ts1DisabledLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ts2DisabledLabel
            // 
            this.ts2DisabledLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ts2DisabledLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ts2DisabledLabel.Location = new System.Drawing.Point(869, 117);
            this.ts2DisabledLabel.Name = "ts2DisabledLabel";
            this.ts2DisabledLabel.Size = new System.Drawing.Size(91, 38);
            this.ts2DisabledLabel.TabIndex = 15;
            this.ts2DisabledLabel.Text = "4";
            this.ts2DisabledLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tuningPresetLink
            // 
            this.tuningPresetLink.AutoSize = true;
            this.tuningPresetLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tuningPresetLink.LinkColor = System.Drawing.Color.White;
            this.tuningPresetLink.Location = new System.Drawing.Point(137, 164);
            this.tuningPresetLink.Name = "tuningPresetLink";
            this.tuningPresetLink.Size = new System.Drawing.Size(95, 29);
            this.tuningPresetLink.TabIndex = 16;
            this.tuningPresetLink.TabStop = true;
            this.tuningPresetLink.Text = "Presets";
            this.tuningPresetLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.tuningPresetLink_LinkClicked);
            // 
            // tapTempoLink
            // 
            this.tapTempoLink.ActiveLinkColor = System.Drawing.Color.DodgerBlue;
            this.tapTempoLink.AutoSize = true;
            this.tapTempoLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tapTempoLink.LinkColor = System.Drawing.Color.White;
            this.tapTempoLink.Location = new System.Drawing.Point(692, 164);
            this.tapTempoLink.Name = "tapTempoLink";
            this.tapTempoLink.Size = new System.Drawing.Size(56, 29);
            this.tapTempoLink.TabIndex = 17;
            this.tapTempoLink.TabStop = true;
            this.tapTempoLink.Text = "Tap";
            this.tapTempoLink.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tapTempoLink_LinkClicked);
            // 
            // DocumentProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(985, 398);
            this.Controls.Add(this.tapTempoLink);
            this.Controls.Add(this.tuningPresetLink);
            this.Controls.Add(this.ts2DisabledLabel);
            this.Controls.Add(this.ts1DisabledLabel);
            this.Controls.Add(this.tuningPickerContainer);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ts2Input);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ts1Input);
            this.Controls.Add(this.tempoInput);
            this.Controls.Add(this.nameInput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DocumentProperties";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Document Properties";
            ((System.ComponentModel.ISupportInitialize)(this.tempoInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ts1Input)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ts2Input)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox nameInput;
        private NumericUpDown tempoInput;
        private NumericUpDown ts1Input;
        private Label label3;
        private NumericUpDown ts2Input;
        private Label label4;
        private Label label5;
        private Label label6;
        private Button saveButton;
        private Button cancelButton;
        private Panel tuningPickerContainer;
        private Label ts1DisabledLabel;
        private Label ts2DisabledLabel;
        private LinkLabel tuningPresetLink;
        private LinkLabel tapTempoLink;
    }
}