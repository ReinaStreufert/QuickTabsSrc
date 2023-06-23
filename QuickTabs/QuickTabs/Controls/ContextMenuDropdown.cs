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
            public override Color BackColor { get => DrawingConstants.UIAreaBackColor; set => base.BackColor = value; }

            public ContextSection Section { get; set; }

            private int hoveredItem = -1;
            private Action closeDropdown;
            private ContextMenuDropdown subDropdown = null;

            public ContextMenuDropdown(Action closeDropdown)
            {
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
                    if (newHover > -1)
                    {
                        if (Section[hoveredItem].Submenu != null)
                        {
                            updateSubDropdown();
                        } else if (subDropdown != null)
                        {
                            closeSubDropdown();
                        }
                    } else if (subDropdown != null)
                    {
                        closeSubDropdown();
                    }
                    this.Invalidate();
                }
            }
            private void closeSubDropdown()
            {
                subDropdown.Parent.Controls.Remove(subDropdown);
                subDropdown.Dispose();
                subDropdown = null;
            }
            private void updateSubDropdown()
            {
                if (subDropdown == null)
                {
                    subDropdown = new ContextMenuDropdown(closeDropdown);
                    this.Parent.Controls.Add(subDropdown);
                    this.Parent.Controls.SetChildIndex(subDropdown, 0);
                }
                subDropdown.Location = new Point(this.Location.X + this.Width, this.Location.Y + DrawingConstants.DropdownRowHeight * hoveredItem);
                subDropdown.Section = Section[hoveredItem].Submenu;
                subDropdown.Refresh();
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
            protected override void OnParentChanged(EventArgs e)
            {
                base.OnParentChanged(e);
                if (Parent == null)
                {
                    if (subDropdown != null)
                    {
                        closeSubDropdown();
                    }
                }
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (subDropdown != null)
                {
                    subDropdown.Invalidate();
                }
                Graphics g = e.Graphics;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                int width = this.Width;

                using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
                using (Font font = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Regular, GraphicsUnit.Pixel))
                using (SolidBrush contrast = new SolidBrush(DrawingConstants.ContrastColor))
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
                            usedIcon = item.Icon[DrawingConstants.ContrastColor];
                        } else
                        {
                            usedIcon = item.Icon[DrawingConstants.FadedGray];
                        }
                        g.DrawImage(usedIcon, centerIconX - DrawingConstants.MediumIconSize / 2, centerY - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        SizeF textSize = g.MeasureString(item.CollapsedText, font);
                        SolidBrush usedBrush;
                        if (item.Selected)
                        {
                            usedBrush = contrast;
                        } else
                        {
                            if (Section.ToggleType == ToggleType.NotTogglable)
                            {
                                usedBrush = fadedGray;
                            } else
                            {
                                usedBrush = contrast;
                            }
                        }
                        g.DrawString(item.CollapsedText, font, usedBrush, textStartX, centerY - textSize.Height / 2);
                        if (item.Selected && Section.ToggleType != ToggleType.NotTogglable)
                        {
                            g.DrawImage(DrawingIcons.Check[DrawingConstants.ContrastColor], centerCheckX - DrawingConstants.MediumIconSize / 2, centerY - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        } else if (item.Selected && item.Submenu != null)
                        {
                            g.DrawImage(DrawingIcons.MenuRight[DrawingConstants.ContrastColor], centerCheckX - DrawingConstants.MediumIconSize / 2, centerY - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        }
                        i++;
                    }
                }
            }

            public override void Refresh()
            {
                updateUI();
                if (subDropdown != null)
                {
                    closeSubDropdown();
                }
                this.Invalidate();
                base.Refresh();
            }
        }
    }
}
