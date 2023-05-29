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
    internal class LogoPanel : Panel
    {
        private bool suspendPaint = false;
        public bool SuspendPaint
        {
            get
            {
                return suspendPaint;
            }
            set
            {
                suspendPaint = value;
                if (!value)
                {
                    this.Invalidate();
                }
            }
        }
        private Bitmap logoPattern;
        
        public LogoPanel()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (SuspendPaint)
            {
                return;
            }
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            g.DrawImage(logoPattern, 0, 0);
            g.ResetTransform();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (SuspendPaint)
            {
                return;
            }
            base.OnPaint(e);
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            logoPattern = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(logoPattern))
            {
                int width = logoPattern.Width;
                int height = logoPattern.Height;
                for (int y = DrawingConstants.LogoPatternSpacingSmall; y < height + 100; y += DrawingConstants.LogoPatternSpacingSmall)
                {
                    for (int x = 0; x < width + 100; x += DrawingConstants.LogoPatternSpacingLarge)
                    {
                        Matrix matrix = new Matrix(Matrix3x2.Identity);
                        matrix.RotateAt(DrawingConstants.LogoPatternRotation, new PointF(x, y));
                        g.Transform = matrix;
                        g.DrawImage(DrawingIcons.QuickTabsLogo[DrawingConstants.LogoPatternColor], x, y, DrawingConstants.LogoPatternSize.Width, DrawingConstants.LogoPatternSize.Height);
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
