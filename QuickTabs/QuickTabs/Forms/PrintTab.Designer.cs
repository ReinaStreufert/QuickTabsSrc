namespace QuickTabs.Forms
{
    partial class PrintTab
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
            this.label6 = new System.Windows.Forms.Label();
            this.printerSelect = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.scaleInput = new System.Windows.Forms.NumericUpDown();
            this.percentText = new System.Windows.Forms.Label();
            this.previewZoomInput = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.previewPageInput = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.useColorCheck = new System.Windows.Forms.CheckBox();
            this.doubleSidedCheck = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.paperSizeSelect = new System.Windows.Forms.ComboBox();
            this.landscapeCheck = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.topMarginInput = new System.Windows.Forms.NumericUpDown();
            this.bottomMarginInput = new System.Windows.Forms.NumericUpDown();
            this.rightMarginInput = new System.Windows.Forms.NumericUpDown();
            this.leftMarginInput = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.copiesInput = new System.Windows.Forms.NumericUpDown();
            this.printButton = new System.Windows.Forms.Button();
            this.includeCoverCheck = new System.Windows.Forms.CheckBox();
            this.systemDialogLink = new System.Windows.Forms.LinkLabel();
            this.previewLocation = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.scaleInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.previewZoomInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.previewPageInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.topMarginInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomMarginInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightMarginInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftMarginInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.copiesInput)).BeginInit();
            this.SuspendLayout();
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 29);
            this.label6.TabIndex = 15;
            this.label6.Text = "Printer";
            // 
            // printerSelect
            // 
            this.printerSelect.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.printerSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.printerSelect.DropDownWidth = 500;
            this.printerSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.printerSelect.FormattingEnabled = true;
            this.printerSelect.Location = new System.Drawing.Point(18, 45);
            this.printerSelect.Name = "printerSelect";
            this.printerSelect.Size = new System.Drawing.Size(306, 37);
            this.printerSelect.TabIndex = 16;
            this.printerSelect.SelectedIndexChanged += new System.EventHandler(this.printerSelect_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(333, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 29);
            this.label1.TabIndex = 18;
            this.label1.Text = "Preview";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label2.Location = new System.Drawing.Point(12, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 29);
            this.label2.TabIndex = 19;
            this.label2.Text = "Scale";
            // 
            // scaleInput
            // 
            this.scaleInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.scaleInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.scaleInput.ForeColor = System.Drawing.Color.White;
            this.scaleInput.Increment = new decimal(new int[] {
            14,
            0,
            0,
            0});
            this.scaleInput.Location = new System.Drawing.Point(18, 125);
            this.scaleInput.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.scaleInput.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.scaleInput.Name = "scaleInput";
            this.scaleInput.Size = new System.Drawing.Size(90, 35);
            this.scaleInput.TabIndex = 20;
            this.scaleInput.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.scaleInput.ValueChanged += new System.EventHandler(this.scaleInput_ValueChanged);
            // 
            // percentText
            // 
            this.percentText.AutoSize = true;
            this.percentText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.percentText.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.percentText.Location = new System.Drawing.Point(114, 126);
            this.percentText.Name = "percentText";
            this.percentText.Size = new System.Drawing.Size(35, 29);
            this.percentText.TabIndex = 21;
            this.percentText.Text = "%";
            // 
            // previewZoomInput
            // 
            this.previewZoomInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.previewZoomInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.previewZoomInput.ForeColor = System.Drawing.Color.White;
            this.previewZoomInput.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.previewZoomInput.Location = new System.Drawing.Point(543, 10);
            this.previewZoomInput.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.previewZoomInput.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.previewZoomInput.Name = "previewZoomInput";
            this.previewZoomInput.Size = new System.Drawing.Size(90, 29);
            this.previewZoomInput.TabIndex = 22;
            this.previewZoomInput.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.previewZoomInput.ValueChanged += new System.EventHandler(this.previewZoomInput_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(454, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 29);
            this.label3.TabIndex = 23;
            this.label3.Text = "Zoom";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label4.Location = new System.Drawing.Point(641, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 29);
            this.label4.TabIndex = 24;
            this.label4.Text = "Page";
            // 
            // previewPageInput
            // 
            this.previewPageInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.previewPageInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.previewPageInput.ForeColor = System.Drawing.Color.White;
            this.previewPageInput.Location = new System.Drawing.Point(723, 10);
            this.previewPageInput.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.previewPageInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.previewPageInput.Name = "previewPageInput";
            this.previewPageInput.Size = new System.Drawing.Size(90, 29);
            this.previewPageInput.TabIndex = 25;
            this.previewPageInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.previewPageInput.ValueChanged += new System.EventHandler(this.previewPageInput_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label5.Location = new System.Drawing.Point(12, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 29);
            this.label5.TabIndex = 26;
            this.label5.Text = "Options";
            // 
            // useColorCheck
            // 
            this.useColorCheck.AutoSize = true;
            this.useColorCheck.Checked = true;
            this.useColorCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useColorCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.useColorCheck.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.useColorCheck.Location = new System.Drawing.Point(18, 201);
            this.useColorCheck.Name = "useColorCheck";
            this.useColorCheck.Size = new System.Drawing.Size(148, 33);
            this.useColorCheck.TabIndex = 27;
            this.useColorCheck.Text = "Use color";
            this.useColorCheck.UseVisualStyleBackColor = true;
            this.useColorCheck.CheckedChanged += new System.EventHandler(this.useColorCheck_CheckedChanged);
            // 
            // doubleSidedCheck
            // 
            this.doubleSidedCheck.AutoSize = true;
            this.doubleSidedCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.doubleSidedCheck.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.doubleSidedCheck.Location = new System.Drawing.Point(18, 244);
            this.doubleSidedCheck.Name = "doubleSidedCheck";
            this.doubleSidedCheck.Size = new System.Drawing.Size(241, 33);
            this.doubleSidedCheck.TabIndex = 28;
            this.doubleSidedCheck.Text = "Print double sided";
            this.doubleSidedCheck.UseVisualStyleBackColor = true;
            this.doubleSidedCheck.CheckedChanged += new System.EventHandler(this.doubleSidedCheck_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label7.Location = new System.Drawing.Point(12, 370);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(137, 29);
            this.label7.TabIndex = 29;
            this.label7.Text = "Paper size";
            // 
            // paperSizeSelect
            // 
            this.paperSizeSelect.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.paperSizeSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.paperSizeSelect.DropDownWidth = 500;
            this.paperSizeSelect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.paperSizeSelect.FormattingEnabled = true;
            this.paperSizeSelect.Location = new System.Drawing.Point(18, 406);
            this.paperSizeSelect.Name = "paperSizeSelect";
            this.paperSizeSelect.Size = new System.Drawing.Size(306, 37);
            this.paperSizeSelect.TabIndex = 30;
            this.paperSizeSelect.SelectedIndexChanged += new System.EventHandler(this.paperSizeSelect_SelectedIndexChanged);
            // 
            // landscapeCheck
            // 
            this.landscapeCheck.AutoSize = true;
            this.landscapeCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.landscapeCheck.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.landscapeCheck.Location = new System.Drawing.Point(18, 287);
            this.landscapeCheck.Name = "landscapeCheck";
            this.landscapeCheck.Size = new System.Drawing.Size(163, 33);
            this.landscapeCheck.TabIndex = 31;
            this.landscapeCheck.Text = "Landscape";
            this.landscapeCheck.UseVisualStyleBackColor = true;
            this.landscapeCheck.CheckedChanged += new System.EventHandler(this.landscapeCheck_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label8.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label8.Location = new System.Drawing.Point(12, 450);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(152, 29);
            this.label8.TabIndex = 32;
            this.label8.Text = "Margins (in)";
            // 
            // topMarginInput
            // 
            this.topMarginInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.topMarginInput.DecimalPlaces = 2;
            this.topMarginInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.topMarginInput.ForeColor = System.Drawing.Color.White;
            this.topMarginInput.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.topMarginInput.Location = new System.Drawing.Point(126, 492);
            this.topMarginInput.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.topMarginInput.Name = "topMarginInput";
            this.topMarginInput.Size = new System.Drawing.Size(90, 35);
            this.topMarginInput.TabIndex = 33;
            this.topMarginInput.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.topMarginInput.ValueChanged += new System.EventHandler(this.marginInputChanged);
            // 
            // bottomMarginInput
            // 
            this.bottomMarginInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.bottomMarginInput.DecimalPlaces = 2;
            this.bottomMarginInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.bottomMarginInput.ForeColor = System.Drawing.Color.White;
            this.bottomMarginInput.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.bottomMarginInput.Location = new System.Drawing.Point(126, 535);
            this.bottomMarginInput.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.bottomMarginInput.Name = "bottomMarginInput";
            this.bottomMarginInput.Size = new System.Drawing.Size(90, 35);
            this.bottomMarginInput.TabIndex = 34;
            this.bottomMarginInput.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.bottomMarginInput.ValueChanged += new System.EventHandler(this.marginInputChanged);
            // 
            // rightMarginInput
            // 
            this.rightMarginInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.rightMarginInput.DecimalPlaces = 2;
            this.rightMarginInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rightMarginInput.ForeColor = System.Drawing.Color.White;
            this.rightMarginInput.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.rightMarginInput.Location = new System.Drawing.Point(222, 514);
            this.rightMarginInput.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.rightMarginInput.Name = "rightMarginInput";
            this.rightMarginInput.Size = new System.Drawing.Size(90, 35);
            this.rightMarginInput.TabIndex = 35;
            this.rightMarginInput.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.rightMarginInput.ValueChanged += new System.EventHandler(this.marginInputChanged);
            // 
            // leftMarginInput
            // 
            this.leftMarginInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.leftMarginInput.DecimalPlaces = 2;
            this.leftMarginInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.leftMarginInput.ForeColor = System.Drawing.Color.White;
            this.leftMarginInput.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.leftMarginInput.Location = new System.Drawing.Point(30, 514);
            this.leftMarginInput.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.leftMarginInput.Name = "leftMarginInput";
            this.leftMarginInput.Size = new System.Drawing.Size(90, 35);
            this.leftMarginInput.TabIndex = 36;
            this.leftMarginInput.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.leftMarginInput.ValueChanged += new System.EventHandler(this.marginInputChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label9.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label9.Location = new System.Drawing.Point(11, 575);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 29);
            this.label9.TabIndex = 37;
            this.label9.Text = "Copies";
            // 
            // copiesInput
            // 
            this.copiesInput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.copiesInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.copiesInput.ForeColor = System.Drawing.Color.White;
            this.copiesInput.Location = new System.Drawing.Point(18, 611);
            this.copiesInput.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.copiesInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.copiesInput.Name = "copiesInput";
            this.copiesInput.Size = new System.Drawing.Size(90, 35);
            this.copiesInput.TabIndex = 38;
            this.copiesInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.copiesInput.ValueChanged += new System.EventHandler(this.copiesInput_ValueChanged);
            // 
            // printButton
            // 
            this.printButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(154)))), ((int)(((byte)(231)))));
            this.printButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.printButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.printButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.printButton.Location = new System.Drawing.Point(18, 654);
            this.printButton.Name = "printButton";
            this.printButton.Size = new System.Drawing.Size(306, 56);
            this.printButton.TabIndex = 39;
            this.printButton.Text = "Print";
            this.printButton.UseVisualStyleBackColor = false;
            this.printButton.Click += new System.EventHandler(this.printButton_Click);
            // 
            // includeCoverCheck
            // 
            this.includeCoverCheck.AutoSize = true;
            this.includeCoverCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.includeCoverCheck.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.includeCoverCheck.Location = new System.Drawing.Point(18, 330);
            this.includeCoverCheck.Name = "includeCoverCheck";
            this.includeCoverCheck.Size = new System.Drawing.Size(228, 33);
            this.includeCoverCheck.TabIndex = 41;
            this.includeCoverCheck.Text = "Include title page";
            this.includeCoverCheck.UseVisualStyleBackColor = true;
            this.includeCoverCheck.CheckedChanged += new System.EventHandler(this.includeCoverCheck_CheckedChanged);
            // 
            // systemDialogLink
            // 
            this.systemDialogLink.ActiveLinkColor = System.Drawing.Color.DodgerBlue;
            this.systemDialogLink.AutoSize = true;
            this.systemDialogLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.systemDialogLink.LinkColor = System.Drawing.Color.White;
            this.systemDialogLink.Location = new System.Drawing.Point(13, 713);
            this.systemDialogLink.Name = "systemDialogLink";
            this.systemDialogLink.Size = new System.Drawing.Size(230, 29);
            this.systemDialogLink.TabIndex = 42;
            this.systemDialogLink.TabStop = true;
            this.systemDialogLink.Text = "System print options";
            this.systemDialogLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.systemDialogLink_LinkClicked);
            // 
            // previewLocation
            // 
            this.previewLocation.Location = new System.Drawing.Point(339, 44);
            this.previewLocation.Name = "previewLocation";
            this.previewLocation.Size = new System.Drawing.Size(400, 200);
            this.previewLocation.TabIndex = 43;
            // 
            // PrintTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.WindowText;
            this.ClientSize = new System.Drawing.Size(1188, 783);
            this.Controls.Add(this.previewLocation);
            this.Controls.Add(this.systemDialogLink);
            this.Controls.Add(this.includeCoverCheck);
            this.Controls.Add(this.printButton);
            this.Controls.Add(this.copiesInput);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.leftMarginInput);
            this.Controls.Add(this.rightMarginInput);
            this.Controls.Add(this.bottomMarginInput);
            this.Controls.Add(this.topMarginInput);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.landscapeCheck);
            this.Controls.Add(this.paperSizeSelect);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.doubleSidedCheck);
            this.Controls.Add(this.useColorCheck);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.previewPageInput);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.previewZoomInput);
            this.Controls.Add(this.percentText);
            this.Controls.Add(this.scaleInput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.printerSelect);
            this.Controls.Add(this.label6);
            this.Name = "PrintTab";
            this.ShowIcon = false;
            this.Text = "Print";
            ((System.ComponentModel.ISupportInitialize)(this.scaleInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.previewZoomInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.previewPageInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.topMarginInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomMarginInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightMarginInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftMarginInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.copiesInput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label6;
        private ComboBox printerSelect;
        private Label label1;
        private Label label2;
        private NumericUpDown scaleInput;
        private Label percentText;
        private NumericUpDown previewZoomInput;
        private Label label3;
        private Label label4;
        private NumericUpDown previewPageInput;
        private Label label5;
        private CheckBox useColorCheck;
        private CheckBox doubleSidedCheck;
        private Label label7;
        private ComboBox paperSizeSelect;
        private CheckBox landscapeCheck;
        private Label label8;
        private NumericUpDown topMarginInput;
        private NumericUpDown bottomMarginInput;
        private NumericUpDown rightMarginInput;
        private NumericUpDown leftMarginInput;
        private Label label9;
        private NumericUpDown copiesInput;
        private Button printButton;
        private CheckBox includeCoverCheck;
        private LinkLabel systemDialogLink;
        private Panel previewLocation;
    }
}