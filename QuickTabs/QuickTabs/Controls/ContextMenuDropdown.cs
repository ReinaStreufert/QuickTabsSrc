using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal partial class ContextMenu
    {
        private class ContextMenuDropdown : Control
        {
            public ContextSection Section { get; set; }

            private int hoveredItem = -1;
            private Action closeDropdown;

            public ContextMenuDropdown(Action closeDropdown)
            {
                this.BackColor = Color.Black;
                //this.Size = new Size(100, 400);
                this.DoubleBuffered = true;
                this.closeDropdown = closeDropdown;
            }

            private void updateUI()
            {
                this.Size = new Size(DrawingConstants.DropdownWidth, Section.Count() * DrawingConstants.DropdownRowHeight);
            }

            private int getItemFromY(int y)
            {
                return (int)Math.Floor(y / (float)DrawingConstants.DropdownRowHeight);
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                int newHover = getItemFromY(e.Y);
                if (newHover >= Section.Count())
                {
                    newHover = -1;
                }
                if (newHover != hoveredItem)
                {
                    hoveredItem = newHover;
                    this.Invalidate();
                }
            }
            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                hoveredItem = -1;
                this.Invalidate();
            }
            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (hoveredItem >= 0 && hoveredItem < Section.Count())
                {
                    ContextItem item = Section[hoveredItem];
                    if (item.Selected || Section.ToggleType != ToggleType.NotTogglable)
                    {
                        item.InvokeClick();
                        if (!item.DontCloseDropdown)
                        {
                            closeDropdown();
                        }
                    }
                }
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;

                int width = this.Width;

                using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
                using (Font boldFont = new Font("Montserrat", DrawingConstants.SmallTextSizePx, FontStyle.Regular, GraphicsUnit.Pixel))
                using (SolidBrush white = new SolidBrush(Color.White))
                using (SolidBrush fadedGray = new SolidBrush(DrawingConstants.FadedGray))
                {
                    int i = 0;
                    foreach (ContextItem item in Section)
                    {
                        int yStart = i * DrawingConstants.DropdownRowHeight;
                        int centerY = yStart + DrawingConstants.DropdownRowHeight / 2;
                        int centerIconX = DrawingConstants.DropdownIconArea / 2;
                        int textStartX = DrawingConstants.DropdownIconArea;
                        int centerCheckX = this.Width - centerIconX;
                        if (i == hoveredItem)
                        {
                            g.FillRectangle(hoverBrush, 0, yStart, width, DrawingConstants.DropdownRowHeight);
                        }
                        Bitmap usedIcon;
                        if (item.Selected)
                        {
                            usedIcon = item.Icon[Color.White];
                        } else
                        {
                            usedIcon = item.Icon[DrawingConstants.FadedGray];
                        }
                        g.DrawImage(usedIcon, centerIconX - DrawingConstants.MediumIconSize / 2, centerY - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        SizeF textSize = g.MeasureString(item.CollapsedText, boldFont);
                        SolidBrush usedBrush;
                        if (item.Selected)
                        {
                            usedBrush = white;
                        } else
                        {
                            if (Section.ToggleType == ToggleType.NotTogglable)
                            {
                                usedBrush = fadedGray;
                            } else
                            {
                                usedBrush = white;
                            }
                        }
                        g.DrawString(item.CollapsedText, boldFont, usedBrush, textStartX, centerY - textSize.Height / 2);
                        if (item.Selected && Section.ToggleType != ToggleType.NotTogglable)
                        {
                            g.DrawImage(DrawingIcons.Check[Color.White], centerCheckX - DrawingConstants.MediumIconSize / 2, centerY - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        }
                        i++;
                    }
                }
            }

            public override void Refresh()
            {
                updateUI();
                this.Invalidate();
                base.Refresh();
            }
        }
    }
}
