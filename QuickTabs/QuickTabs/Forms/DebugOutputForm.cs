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
    public partial class DebugOutputForm : Form
    {
        public string Output;
        public DebugOutputForm()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
        }
        public void DebugOutputForm_OnShown(object Sender, EventArgs e)
        {
            label1.Text = Output;
        }

        private void DebugOutputForm_SizeChanged(object sender, EventArgs e)
        {
            label1.Size = this.ClientSize;
        }
    }
}
