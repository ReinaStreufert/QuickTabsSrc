using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickTabs.Forms
{
    public partial class PersistentAreYouSure : Form
    {
        public string Message { get; set; }
        public bool StopAsking { get; private set; } = false;
        public bool Continue { get; private set; } = false;

        public PersistentAreYouSure()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            messageLabel.Text = Message;
        }

        private void dontAskCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (dontAskCheckbox.Checked)
            {
                cancelButton.Visible = false;
            } else
            {
                cancelButton.Visible = true;
            }
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            Continue = true;
            StopAsking = dontAskCheckbox.Checked;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
