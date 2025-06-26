using QuickTabs.Forms;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public class LogoPanel : Panel
    {
        public override Color BackColor { get => DrawingConstants.EmptySpaceBackColor; set => base.BackColor = value; }

        private bool suspendLogoDraw = false;
        public bool SuspendLogoDraw
        {
            get
            {
                return suspendLogoDraw;
            }
            set
            {
                suspendLogoDraw = value;
                if (!value)
                {
                    this.Invalidate();
                }
            }
        }
        private Bitmap darkModelogoPattern;
        private Bitmap lightModelogoPattern;
        
        public LogoPanel()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (SuspendLogoDraw)
            {
                return;
            }
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (DrawingConstants.CurrentTheme == Enums.Theme.DarkMode)
            {
                g.DrawImage(darkModelogoPattern, 0, 0);
            } else
            {
                g.DrawImage(lightModelogoPattern, 0, 0);
            }
            
            g.ResetTransform();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            darkModelogoPattern = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            lightModelogoPattern = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            drawLogoPattern(darkModelogoPattern, DrawingConstants.DarkModeLogoPatternColor);
            drawLogoPattern(lightModelogoPattern, DrawingConstants.LightModeLogoPatternColor);
        }
        private void drawLogoPattern(Bitmap bitmap, Color color)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                int width = bitmap.Width;
                int height = bitmap.Height;
                for (int y = DrawingConstants.LogoPatternSpacingSmall; y < height + 100; y += DrawingConstants.LogoPatternSpacingSmall)
                {
                    for (int x = 0; x < width + 100; x += DrawingConstants.LogoPatternSpacingLarge)
                    {
                        Matrix matrix = new Matrix(Matrix3x2.Identity);
                        matrix.RotateAt(DrawingConstants.LogoPatternRotation, new PointF(x, y));
                        g.Transform = matrix;
                        g.DrawImage(DrawingIcons.QuickTabsLogo[color], x, y, DrawingConstants.LogoPatternSize.Width, DrawingConstants.LogoPatternSize.Height);
                    }
                }
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
        }
    }
}
