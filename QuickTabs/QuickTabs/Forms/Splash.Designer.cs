namespace QuickTabs.Forms
{
    partial class Splash
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
            this.failedLabel = new System.Windows.Forms.Label();
            this.startAnyway = new System.Windows.Forms.LinkLabel();
            this.exit = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // failedLabel
            // 
            this.failedLabel.AutoSize = true;
            this.failedLabel.Font = new System.Drawing.Font("Montserrat", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.failedLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.failedLabel.Location = new System.Drawing.Point(48, 345);
            this.failedLabel.Name = "failedLabel";
            this.failedLabel.Size = new System.Drawing.Size(471, 44);
            this.failedLabel.TabIndex = 0;
            this.failedLabel.Text = "Could not load some icons.";
            // 
            // startAnyway
            // 
            this.startAnyway.AutoSize = true;
            this.startAnyway.Font = new System.Drawing.Font("Montserrat", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.startAnyway.LinkColor = System.Drawing.Color.White;
            this.startAnyway.Location = new System.Drawing.Point(538, 345);
            this.startAnyway.Name = "startAnyway";
            this.startAnyway.Size = new System.Drawing.Size(101, 44);
            this.startAnyway.TabIndex = 1;
            this.startAnyway.TabStop = true;
            this.startAnyway.Text = "Start";
            this.startAnyway.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.startAnyway_LinkClicked);
            // 
            // exit
            // 
            this.exit.AutoSize = true;
            this.exit.Font = new System.Drawing.Font("Montserrat", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.exit.LinkColor = System.Drawing.Color.White;
            this.exit.Location = new System.Drawing.Point(666, 345);
            this.exit.Name = "exit";
            this.exit.Size = new System.Drawing.Size(83, 44);
            this.exit.TabIndex = 2;
            this.exit.TabStop = true;
            this.exit.Text = "Exit";
            this.exit.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.exit_LinkClicked);
            // 
            // Splash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.exit);
            this.Controls.Add(this.startAnyway);
            this.Controls.Add(this.failedLabel);
            this.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Splash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Splash";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label failedLabel;
        private LinkLabel startAnyway;
        private LinkLabel exit;
    }
}