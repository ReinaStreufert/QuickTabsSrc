using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public class ButtonListClickEventArgs : EventArgs
    {
        public int ButtonIndex;
        public ButtonListClickEventArgs(int buttonIndex) => ButtonIndex = buttonIndex;
    }
    public delegate void ButtonListClickEventHandler(object sender, ButtonListClickEventArgs e);

    public class ButtonList : Control, IMessageFilter
    {
        public override Color BackColor { get => DrawingConstants.UIAreaBackColor; set => base.BackColor = value; }

        public string[] Buttons { get; set; }
        public event ButtonListClickEventHandler ButtonClick;

        private VScrollBar scrollBar = null;
        private int hoveredIndex = -1;
        private int contentHeight;

        public ButtonList(string[] buttons)
        {
            Buttons = buttons;
            this.DoubleBuffered = true;
            Application.AddMessageFilter(this);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) Application.RemoveMessageFilter(this);
            base.Dispose(disposing);
        }
        public bool PreFilterMessage(ref Message m) // this is a dumbass solution to winforms scrolling both this control and the parent one when you use the wheel
        {
            const int WM_MOUSEWHEEL = 0x020A;
            //const int WHEEL_DELTA = 120;
            if (m.HWnd == this.Handle && m.Msg == WM_MOUSEWHEEL)
            {
                Point posOnScreen = new Point(m.LParam.ToInt32());
                Point pos = PointToClient(posOnScreen);
                int wParam = unchecked((int)(long)m.WParam);
                int delta = (short)((wParam >> 16) & 0xFFFF);
                var args = new MouseEventArgs(MouseButtons.None, 0, pos.X, pos.Y, delta);
                this.OnMouseWheel(args);
                return true;
            }
            return false;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            contentHeight = Buttons.Length * DrawingConstants.DropdownRowHeight;
            if (contentHeight > this.Height)
            {
                if (scrollBar == null)
                {
                    scrollBar = new VScrollBar();
                    scrollBar.Location = new Point(this.Width - SystemInformation.VerticalScrollBarWidth, 1);
                    scrollBar.Size = new Size(SystemInformation.VerticalScrollBarWidth, this.Height - 2);
                    scrollBar.Value = 0;
                    scrollBar.Minimum = 0;
                    scrollBar.Maximum = (contentHeight - this.Height) + DrawingConstants.ScrollbarLargeChange;
                    scrollBar.SmallChange = DrawingConstants.ScrollbarSmallChange;
                    scrollBar.LargeChange = DrawingConstants.ScrollbarLargeChange;
                    scrollBar.Scroll += scrollBar_Scroll;
                    this.Controls.Add(scrollBar);
                }
            } else
            {
                if (scrollBar != null)
                {
                    this.Controls.Remove(scrollBar);
                    scrollBar.Scroll -= scrollBar_Scroll;
                    scrollBar.Dispose();
                    scrollBar = null;
                }
            }
            this.Invalidate();
        }

        private void scrollBar_Scroll(object? sender, ScrollEventArgs e)
        {
            this.Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (scrollBar != null)
            {
                int newValue = scrollBar.Value - e.Delta;
                if (newValue < scrollBar.Minimum)
                {
                    newValue = scrollBar.Minimum;
                }
                if (newValue > scrollBar.Maximum - scrollBar.LargeChange)
                {
                    newValue = scrollBar.Maximum - scrollBar.LargeChange;
                }
                scrollBar.Value = newValue;
                this.Invalidate();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int y = e.Y;
            if (scrollBar != null)
            {
                y += scrollBar.Value;
            }
            if (e.X < 0 || y < 0)
            {
                return;
            }
            int ptIndex = (int)Math.Floor(y / (float)DrawingConstants.DropdownRowHeight);
            int oldHovered = hoveredIndex;
            if (ptIndex < Buttons.Length)
            {
                hoveredIndex = ptIndex;
            } else
            {
                hoveredIndex = -1;
            }
            if (hoveredIndex != oldHovered)
            {
                this.Invalidate();
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredIndex > -1)
            {
                hoveredIndex = -1;
                this.Invalidate();
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (hoveredIndex > -1 && hoveredIndex < Buttons.Length)
            {
                ButtonClick?.Invoke(this, new ButtonListClickEventArgs(hoveredIndex));
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int yAdd = 0;
            int width = this.Width;
            if (scrollBar != null)
            {
                yAdd = -scrollBar.Value;
                width -= SystemInformation.VerticalScrollBarWidth;
            }
            using (SolidBrush contentBackBrush = new SolidBrush(DrawingConstants.UIControlBackColor))
            using (SolidBrush textBrush = new SolidBrush(DrawingConstants.ContrastColor))
            using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
            using (Font boldFont = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font regFont = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Pen borderPen = new Pen(DrawingConstants.FadedGray, DrawingConstants.ButtonListBorderWidth))
            {
                if (contentHeight >= Height)
                {
                    g.FillRectangle(contentBackBrush, 0, 0, width, this.Height);
                } else
                {
                    g.FillRectangle(contentBackBrush, 0, 0, width, contentHeight);
                }
                string[] items = Buttons;
                for (int i = 0; i < items.Length; i++)
                {
                    int y = (i * DrawingConstants.DropdownRowHeight) + yAdd;
                    if (y + DrawingConstants.DropdownRowHeight < 0 || y > Height)
                    {
                        continue;
                    }
                    string text = items[i];
                    float centerY = y + DrawingConstants.DropdownRowHeight / 2F;
                    SizeF textSize = g.MeasureString(text, boldFont);
                    g.DrawString(text, boldFont, textBrush, DrawingConstants.MediumMargin, centerY - textSize.Height / 2);
                    if (hoveredIndex == i)
                    {
                        g.FillRectangle(hoverBrush, 0, y, width, DrawingConstants.DropdownRowHeight);
                        const string hoverText = "Click to open";
                        SizeF hoverTextSize = g.MeasureString(hoverText, regFont);
                        g.DrawString(hoverText, regFont, textBrush, width - DrawingConstants.MediumMargin - hoverTextSize.Width, centerY - hoverTextSize.Height / 2);
                    }
                }
                if (items.Length == 0)
                {
                    float centerX = width / 2F;
                    float centerY = Height / 2F;
                    const string emptyText = "No items to show";
                    SizeF emptyTextSize = g.MeasureString(emptyText, regFont);
                    g.DrawString(emptyText, regFont, textBrush, centerX - emptyTextSize.Width / 2, centerY - emptyTextSize.Height / 2);
                }
                g.DrawLine(borderPen, 0, 0, this.Width - 1, 0);
                g.DrawLine(borderPen, this.Width - 1, 0, this.Width - 1, this.Height - 1);
                g.DrawLine(borderPen, this.Width - 1, this.Height - 1, 0, this.Height - 1);
                g.DrawLine(borderPen, 0, this.Height - 1, 0, 0);
            }
        }
    }
}
