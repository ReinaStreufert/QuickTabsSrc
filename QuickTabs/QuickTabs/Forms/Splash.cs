using QuickTabs.Controls;
using QuickTabs.Properties;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs.Forms
{
    public partial class Splash : Form
    {
        private int logoWidth = 550;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
         );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     // variables for box shadow
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS                           // struct for box shadow
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private readonly Color darkGray = Color.FromArgb(0x1E, 0x1E, 0x1E);

        public override Color BackColor 
        {
            get 
            { 
                if (QTSettings.Current.ViewDarkMode)
                {
                    return darkGray;
                } else
                {
                    return Color.White;
                }
            }
            set => base.BackColor = value; 
        }

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    Console.WriteLine("H!!");
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 2,
                            leftWidth = 2,
                            rightWidth = 2,
                            topHeight = 2
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        private Task iconLoader;
        private Timer checkCompleteTimer = new Timer();
        private MultiColorBitmap logoSource;
        private bool firstTick = true;
        private bool editorStarted = false;

        public Splash(Task iconLoader)
        {
            InitializeComponent();
            Updater.UpdateStarted += Updater_UpdateStarted;
            Updater.UpdateFailed += Updater_UpdateFailed;
            logoSource = new MultiColorBitmap(Resources.logo);
            logoSource.AddColor(Color.White);
            logoSource.AddColor(darkGray);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            logoWidth = (int)(logoWidth * (this.DeviceDpi / 192.0F));
            failedLabel.Visible = false;
            startAnyway.Visible = false;
            exit.Visible = false;
            this.iconLoader = iconLoader;
            checkCompleteTimer.Interval = 100;
            checkCompleteTimer.Tick += CheckCompleteTimer_Tick;
        }

        private void Updater_UpdateFailed()
        {
            this.Invoke(() =>
            {
                if (!editorStarted)
                {
                    checkCompleteTimer.Start();
                }
            });
        }

        private void Updater_UpdateStarted()
        {
            this.Invoke(() =>
            {
                checkCompleteTimer.Stop();
            });
        }

        private void CheckCompleteTimer_Tick(object? sender, EventArgs e)
        {
            if (firstTick)
            {
                firstTick = false;
                DrawingConstants.LoadFonts();
                failedLabel.Font = new Font(DrawingConstants.Montserrat, 12, FontStyle.Bold, GraphicsUnit.Point);
                startAnyway.Font = new Font(DrawingConstants.Montserrat, 12, FontStyle.Bold, GraphicsUnit.Point);
                exit.Font = new Font(DrawingConstants.Montserrat, 12, FontStyle.Bold, GraphicsUnit.Point);
                AudioEngine.Initialize();
            }
            if (iconLoader.IsCompletedSuccessfully && !Updater.IsUpdating)
            {
                startEditor();
            } else if (iconLoader.IsFaulted)
            {
                failedLabel.Visible = true;
                startAnyway.Visible = true;
                exit.Visible = true;
                checkCompleteTimer.Stop();
            }
        }

        private void startEditor()
        {
            editorStarted = true;
            Editor editor = new Editor();
            editor.FormClosed += Editor_FormClosed;
            editor.Show();
            checkCompleteTimer.Stop();
            this.Visible = false;
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
            Bitmap logo;
            if (QTSettings.Current.ViewDarkMode)
            {
                logo = logoSource[Color.White];
            } else
            {
                logo = logoSource[darkGray];
            }
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
            if (m_aeroEnabled)
            {
                var v = 2;
                DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                MARGINS margins = new MARGINS()
                {
                    bottomHeight = 2,
                    leftWidth = 2,
                    rightWidth = 2,
                    topHeight = 2
                };
                DwmExtendFrameIntoClientArea(this.Handle, ref margins);

            }
            this.Invalidate();
            checkCompleteTimer.Start();
            this.Activate();
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
