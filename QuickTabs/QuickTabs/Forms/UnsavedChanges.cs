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
    public partial class UnsavedChanges : Form
    {
        public bool Continue { get; private set; } = false;
        public string Verb { get; set; } = "";
        public string CustomMessage { get; set; } = "";
        public UnsavedChanges()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Continue = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (CustomMessage == "")
            {
                label1.Text = "You have made unsaved changes. Are you sure you would like to " + Verb + " without saving?";
            } else
            {
                label1.Text = CustomMessage;
            }
        }
    }
}
