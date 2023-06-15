namespace QuickTabs.Forms
{
    partial class Installer
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.installPathInput = new System.Windows.Forms.TextBox();
            this.chooseButton = new System.Windows.Forms.Button();
            this.startShortcutCheck = new System.Windows.Forms.CheckBox();
            this.installButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::QuickTabs.Properties.Resources.logo;
            this.pictureBox1.Location = new System.Drawing.Point(132, 33);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(544, 223);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(39, 278);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 45);
            this.label1.TabIndex = 1;
            this.label1.Text = "Install Path";
            // 
            // installPathInput
            // 
            this.installPathInput.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.installPathInput.Location = new System.Drawing.Point(49, 326);
            this.installPathInput.Name = "installPathInput";
            this.installPathInput.Size = new System.Drawing.Size(542, 50);
            this.installPathInput.TabIndex = 2;
            // 
            // chooseButton
            // 
            this.chooseButton.BackColor = System.Drawing.Color.DodgerBlue;
            this.chooseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chooseButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chooseButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.chooseButton.Location = new System.Drawing.Point(597, 326);
            this.chooseButton.Name = "chooseButton";
            this.chooseButton.Size = new System.Drawing.Size(151, 50);
            this.chooseButton.TabIndex = 3;
            this.chooseButton.Text = "Choose";
            this.chooseButton.UseVisualStyleBackColor = false;
            this.chooseButton.Click += new System.EventHandler(this.chooseButton_Click);
            // 
            // startShortcutCheck
            // 
            this.startShortcutCheck.AutoSize = true;
            this.startShortcutCheck.Checked = true;
            this.startShortcutCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.startShortcutCheck.Font = new System.Drawing.Font("Segoe UI Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.startShortcutCheck.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.startShortcutCheck.Location = new System.Drawing.Point(49, 382);
            this.startShortcutCheck.Name = "startShortcutCheck";
            this.startShortcutCheck.Size = new System.Drawing.Size(328, 49);
            this.startShortcutCheck.TabIndex = 4;
            this.startShortcutCheck.Text = "Create start shortcut";
            this.startShortcutCheck.UseVisualStyleBackColor = true;
            // 
            // installButton
            // 
            this.installButton.BackColor = System.Drawing.Color.DodgerBlue;
            this.installButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.installButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.installButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.installButton.Location = new System.Drawing.Point(49, 437);
            this.installButton.Name = "installButton";
            this.installButton.Size = new System.Drawing.Size(699, 50);
            this.installButton.TabIndex = 5;
            this.installButton.Text = "Install and start";
            this.installButton.UseVisualStyleBackColor = false;
            this.installButton.Click += new System.EventHandler(this.installButton_Click);
            // 
            // Installer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(800, 523);
            this.Controls.Add(this.installButton);
            this.Controls.Add(this.startShortcutCheck);
            this.Controls.Add(this.chooseButton);
            this.Controls.Add(this.installPathInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Installer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Install QuickTabs";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private TextBox installPathInput;
        private Button chooseButton;
        private CheckBox startShortcutCheck;
        private Button installButton;
    }
}