using QuickTabs.Controls;
using QuickTabs.Properties;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs.Forms
{
    public partial class Splash : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;
        private const int logoWidth = 480;

        protected override CreateParams CreateParams
        {
            get
            {
                // add the drop shadow flag for automatically drawing 
                // a drop shadow around the form 
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private Task iconLoader;
        private Timer checkCompleteTimer = new Timer();
        private bool firstTick = true;

        public Splash(Task iconLoader)
        {
            InitializeComponent();
            failedLabel.Visible = false;
            startAnyway.Visible = false;
            exit.Visible = false;
            this.iconLoader = iconLoader;
            checkCompleteTimer.Interval = 100;
            checkCompleteTimer.Tick += CheckCompleteTimer_Tick;
        }

        private void CheckCompleteTimer_Tick(object? sender, EventArgs e)
        {
            if (firstTick)
            {
                firstTick = false;
                AudioEngine.Initialize();
            }
            if (iconLoader.IsCompletedSuccessfully)
            {
                Editor editor = new Editor();
                editor.FormClosed += Editor_FormClosed;
                editor.Show();
                checkCompleteTimer.Stop();
                this.Visible = false;
            } else if (iconLoader.IsFaulted)
            {
                failedLabel.Visible = true;
                startAnyway.Visible = true;
                exit.Visible = true;
                checkCompleteTimer.Stop();
            }
        }

        private void Editor_FormClosed(object? sender, FormClosedEventArgs e)
        {
            AudioEngine.Stop();
            this.Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Bitmap logo = Resources.logo;
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;
            float logoScale = logo.Height / (float)logo.Width;
            int logoHeight = (int)(logoWidth * logoScale);
            int centerX = width / 2;
            int centerY = height / 2;
            g.DrawImage(logo, centerX - logoWidth / 2, centerY - logoHeight / 2, logoWidth, logoHeight);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Invalidate();
            checkCompleteTimer.Start();
        }

        private void startAnyway_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Editor editor = new Editor();
            editor.FormClosed += Editor_FormClosed;
            editor.Show();
        }

        private void exit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AudioEngine.Stop();
            this.Close();
        }
    }
}
