﻿using System;
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
    public partial class InstallerMessage : Form
    {
        public string Message { get; set; }

        public InstallerMessage()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            messageLabel.Text = Message;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
