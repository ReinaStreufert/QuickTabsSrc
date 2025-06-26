namespace QuickTabs.Forms
{
    partial class CrashRecovery
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
            this.messageLabel = new System.Windows.Forms.Label();
            this.exceptionBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.recoverCloseButton = new System.Windows.Forms.Button();
            this.discardCloseButton = new System.Windows.Forms.Button();
            this.recoveryAvailableLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // messageLabel
            // 
            this.messageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.messageLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.messageLabel.Location = new System.Drawing.Point(12, 9);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(880, 126);
            this.messageLabel.TabIndex = 1;
            this.messageLabel.Text = "QuickTabs had to abort your last session due to an unrecoverable error. Details o" +
    "f the error are given below. If you had unsaved data, QuickTabs attempted to sav" +
    "e it and you may recover it below.";
            // 
            // exceptionBox
            // 
            this.exceptionBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.exceptionBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.exceptionBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.exceptionBox.Location = new System.Drawing.Point(18, 143);
            this.exceptionBox.Multiline = true;
            this.exceptionBox.Name = "exceptionBox";
            this.exceptionBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.exceptionBox.Size = new System.Drawing.Size(867, 352);
            this.exceptionBox.TabIndex = 6;
            this.exceptionBox.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(12, 106);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 29);
            this.label1.TabIndex = 5;
            this.label1.Text = "Error info";
            // 
            // recoverCloseButton
            // 
            this.recoverCloseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(154)))), ((int)(((byte)(231)))));
            this.recoverCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.recoverCloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.recoverCloseButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.recoverCloseButton.Location = new System.Drawing.Point(610, 501);
            this.recoverCloseButton.Name = "recoverCloseButton";
            this.recoverCloseButton.Size = new System.Drawing.Size(275, 56);
            this.recoverCloseButton.TabIndex = 7;
            this.recoverCloseButton.Text = "Recover and close";
            this.recoverCloseButton.UseVisualStyleBackColor = false;
            this.recoverCloseButton.Click += new System.EventHandler(this.recoverCloseButton_Click);
            // 
            // discardCloseButton
            // 
            this.discardCloseButton.BackColor = System.Drawing.Color.White;
            this.discardCloseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.discardCloseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.discardCloseButton.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.discardCloseButton.Location = new System.Drawing.Point(328, 501);
            this.discardCloseButton.Name = "discardCloseButton";
            this.discardCloseButton.Size = new System.Drawing.Size(276, 56);
            this.discardCloseButton.TabIndex = 8;
            this.discardCloseButton.Text = "Ignore and close";
            this.discardCloseButton.UseVisualStyleBackColor = false;
            this.discardCloseButton.Click += new System.EventHandler(this.discardCloseButton_Click);
            // 
            // recoveryAvailableLabel
            // 
            this.recoveryAvailableLabel.AutoSize = true;
            this.recoveryAvailableLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.999999F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.recoveryAvailableLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.recoveryAvailableLabel.Location = new System.Drawing.Point(13, 515);
            this.recoveryAvailableLabel.Name = "recoveryAvailableLabel";
            this.recoveryAvailableLabel.Size = new System.Drawing.Size(292, 29);
            this.recoveryAvailableLabel.TabIndex = 9;
            this.recoveryAvailableLabel.Text = "Data recovery: available";
            // 
            // CrashRecovery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(904, 564);
            this.Controls.Add(this.recoveryAvailableLabel);
            this.Controls.Add(this.discardCloseButton);
            this.Controls.Add(this.recoverCloseButton);
            this.Controls.Add(this.exceptionBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.messageLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CrashRecovery";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Crash Recovery";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label messageLabel;
        private TextBox exceptionBox;
        private Label label1;
        private Button recoverCloseButton;
        private Button discardCloseButton;
        private Label recoveryAvailableLabel;
    }
}