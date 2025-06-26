using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public class IconToggle : Control
    {
        public override Color BackColor { get => DrawingConstants.UIAreaBackColor; set => base.BackColor = value; }

        public MultiColorBitmap Icon { get; set; }
        public int IconSize { get; set; } = DrawingConstants.SmallIconSize;
        private bool toggleState = false;
        public bool ToggleState
        {
            get
            {
                return toggleState;
            }
            set
            {
                toggleState = value;
                Invalidate();
            }
        }
        public event EventHandler ToggleStateChanged;

        public IconToggle()
        {
            this.DoubleBuffered = true;
        }
        public IconToggle(MultiColorBitmap icon, bool initialState)
        {
            this.DoubleBuffered = true;
            Icon = icon;
            toggleState = initialState;
        }

        private bool hovered = false;

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            hovered = true;
            Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hovered = false;
            Invalidate();
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            toggleState = !toggleState;
            Invalidate();
            ToggleStateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int iconX = this.Width / 2 - IconSize / 2;
            int iconY = this.Height / 2 - IconSize / 2;
            Color iconColor = DrawingConstants.FadedGray;
            if (toggleState)
            {
                iconColor = DrawingConstants.ContrastColor;
            }
            g.DrawImage(Icon[iconColor], iconX, iconY, IconSize, IconSize);
            if (hovered)
            {
                using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
                {
                    g.FillRectangle(hoverBrush, 0, 0, this.Width, this.Height);
                }
            }
        }
    }
}
