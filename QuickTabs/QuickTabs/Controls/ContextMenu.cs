﻿using QuickTabs.Enums;
using QuickTabs.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public partial class ContextMenu : Control
    {
        public ContextMenuStyle Style { get; set; } = ContextMenuStyle.Responsive;
        protected List<ContextSection> Sections { get; set; } = new List<ContextSection>();
        protected MultiColorBitmap Logo { get; set; }
        private List<UIElement> ui = new List<UIElement>();
        private Button hoveredButton = null;
        private CollapsedSection hoveredSection = null;
        private CollapsedSection selectedSection = null;
        private ContextMenuStyle absoluteStyle = ContextMenuStyle.ShowAllItems;
        private ToolTip toolTip;
        private Button toolTipLocation = null;
        private ContextMenuDropdown dropdownControl;

        public override Color BackColor { get => DrawingConstants.UIAreaBackColor; set => base.BackColor = value; }

        public ContextMenu()
        {
            this.DoubleBuffered = true;
            dropdownControl = new ContextMenuDropdown(() => { dropdownControlLostFocus(null, null); });
            toolTip = new ToolTip();
        }

        protected void updateUI()
        {
            ui.Clear();
            if (Style == ContextMenuStyle.Responsive)
            {
                int fullWidth = getFullWidth();
                if (fullWidth < this.Width)
                {
                    absoluteStyle = ContextMenuStyle.ShowAllItems;
                    if (selectedSection != null)
                    {
                        closeDropdown();
                        selectedSection = null;
                    }
                }
                else
                {
                    absoluteStyle = ContextMenuStyle.Collapsed;
                    if (toolTip.Active)
                    {
                        toolTip.Hide(this);
                    }
                }
            } else
            {
                absoluteStyle = Style;
            }
            int startY = DrawingConstants.LargeMargin;
            int x = DrawingConstants.MediumMargin;
            int usableHeight = this.Height - DrawingConstants.LargeMargin * 2;
            if (Style != ContextMenuStyle.Collapsed && Logo != null)
            {
                Image image = new Image();
                image.Bitmap = Logo;
                int imageWidth = (int)(Logo.Size.Width * (usableHeight / (float)Logo.Size.Height));
                image.Location = new Rectangle(x, startY, imageWidth, usableHeight);
                x += imageWidth;
                ui.Add(image);
            }
            if (absoluteStyle == ContextMenuStyle.ShowAllItems)
            {
                bool first = true;
                foreach (ContextSection section in Sections)
                {
                    if (!first || (Logo != null && Style != ContextMenuStyle.Collapsed))
                    {
                        Seperator seperator = new Seperator();
                        seperator.Location = new Rectangle(x, startY, DrawingConstants.SectionSpacing, usableHeight);
                        ui.Add(seperator);
                        x += DrawingConstants.SectionSpacing;
                    }
                    if (first)
                    {
                        first = false;
                    }
                    Label label = new Label();
                    label.Text = section.SectionName;
                    label.Location = new Rectangle(x, startY, 0, 0);
                    Size sectionNameSize = TextRenderer.MeasureText(section.SectionName, new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel));
                    int minEndX = x + sectionNameSize.Width;
                    ui.Add(label);
                    foreach (ContextItem item in section)
                    {
                        Button button = new Button();
                        button.Icon = item.Icon;
                        button.ContextItem = item;
                        int buttonSize = (int)(usableHeight - DrawingConstants.SmallTextSizePx);
                        button.Location = new Rectangle(x, startY + (int)DrawingConstants.SmallTextSizePx, buttonSize, buttonSize);
                        x += buttonSize;
                        if (hoveredButton != null && item == hoveredButton.ContextItem)
                        {
                            button.Hovered = true;
                            hoveredButton = button;
                        }
                        ui.Add(button);
                    }
                    if (x < minEndX)
                    {
                        x = minEndX;
                    }
                }
            } else if (absoluteStyle == ContextMenuStyle.Collapsed)
            {
                if (Logo != null && Style != ContextMenuStyle.Collapsed)
                {
                    Seperator seperator = new Seperator();
                    seperator.Location = new Rectangle(x, startY, DrawingConstants.SectionSpacing, usableHeight);
                    ui.Add(seperator);
                    x += DrawingConstants.SectionSpacing;
                }
                int centerY = startY + usableHeight / 2;
                foreach (ContextSection section in Sections)
                {
                    CollapsedSection collapsedSection = new CollapsedSection();
                    collapsedSection.Section = section;
                    Size textSize;
                    using (Font font = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
                        textSize = TextRenderer.MeasureText(section.SectionName, font);
                    Size collapsedSectionSize = new Size(textSize.Width + DrawingConstants.MediumMargin, this.Height);
                    collapsedSection.Location = new Rectangle(x, centerY - collapsedSectionSize.Height / 2, collapsedSectionSize.Width, collapsedSectionSize.Height);
                    if (hoveredSection != null && section == hoveredSection.Section)
                    {
                        collapsedSection.Hovered = true;
                        hoveredSection = collapsedSection;
                    }
                    x += collapsedSectionSize.Width;
                    ui.Add(collapsedSection);
                    if (selectedSection != null && selectedSection.Section == section)
                    {
                        selectedSection = collapsedSection;
                    }
                }
            }
            if (selectedSection != null)
            {
                if (Sections.Contains(selectedSection.Section))
                {
                    dropdownControl.Refresh();
                } else
                {
                    selectedSection = null;
                    closeDropdown();
                }
            }
        }

        private int getFullWidth()
        {
            int fullWidth = DrawingConstants.MediumMargin;
            int usableHeight = this.Height - DrawingConstants.LargeMargin * 2;
            int imageWidth = 0; 
            if (Logo != null)
            {
                imageWidth = (int)(Logo.Size.Width * (usableHeight / (float)Logo.Size.Height));
            }
            int buttonSize = (int)(usableHeight - DrawingConstants.SmallTextSizePx);

            fullWidth += imageWidth;
            foreach (ContextSection section in Sections)
            {
                fullWidth += DrawingConstants.SectionSpacing;
                fullWidth += section.Count() * buttonSize;
            }
            if (Logo == null)
            {
                fullWidth -= DrawingConstants.SectionSpacing;
            }
            return fullWidth;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            updateUI();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (absoluteStyle == ContextMenuStyle.ShowAllItems)
            {
                Button oldHoveredButton = hoveredButton;
                if (hoveredButton != null)
                {
                    hoveredButton.Hovered = false;
                }
                hoveredButton = null;
                foreach (Button button in ui.OfType<Button>())
                {
                    if (button.Location.Contains(e.Location))
                    {
                        button.Hovered = true;
                        hoveredButton = button;
                        if (toolTipLocation != hoveredButton)
                        {
                            toolTipLocation = hoveredButton;
                            toolTip.Show(hoveredButton.ContextItem.CollapsedText, this, hoveredButton.Location.X, hoveredButton.Location.Bottom);
                        }
                        break;
                    }
                }
                if (hoveredButton == null && toolTip.Active)
                {
                    toolTipLocation = null;
                    toolTip.Hide(this);
                }
                if (oldHoveredButton != hoveredButton)
                {
                    this.Invalidate();
                }
            } else if (absoluteStyle == ContextMenuStyle.Collapsed)
            {
                CollapsedSection oldHoveredSection = hoveredSection;
                if (hoveredSection != null)
                {
                    hoveredSection.Hovered = false;
                }
                hoveredSection = null;
                foreach (CollapsedSection collapsedSection in ui.OfType<CollapsedSection>())
                {
                    if (collapsedSection.Location.Contains(e.Location))
                    {
                        collapsedSection.Hovered = true;
                        hoveredSection = collapsedSection;
                        if (selectedSection != null && selectedSection != hoveredSection)
                        {
                            selectedSection = hoveredSection;
                            updateDropdown();
                        }
                        break;
                    }
                }
                if (oldHoveredSection != hoveredSection)
                {
                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredButton != null)
            {
                hoveredButton.Hovered = false;
                hoveredButton = null;
                if (toolTip.Active)
                {
                    toolTip.Hide(this);
                }
                this.Invalidate();
            }
            if (hoveredSection != null)
            {
                hoveredSection.Hovered = false;
                hoveredSection = null;
                this.Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (hoveredButton != null)
            {
                ContextItem contextItem = hoveredButton.ContextItem;
                contextItem.InvokeClick();
                if (selectedSection != null)
                {
                    selectedSection = null;
                    closeDropdown();
                } else if (contextItem.Submenu != null)
                {
                    // is this a hacky way to add this feature? absolutely yes. but its my birthday and god damnit,
                    // its just the easiest way to do it. sue me.
                    selectedSection = new CollapsedSection();
                    selectedSection.Location = hoveredButton.Location;
                    selectedSection.Section = contextItem.Submenu;
                    updateDropdown();
                }
                this.Invalidate();
                return;
            }
            if (hoveredSection != null)
            {
                if (selectedSection == hoveredSection)
                {
                    selectedSection = null;
                    closeDropdown();
                    
                } else
                {
                    selectedSection = hoveredSection;
                    updateDropdown();
                }
                this.Invalidate();
            } else if (selectedSection != null)
            {
                selectedSection = null;
                closeDropdown();
                this.Invalidate();
            }
        }

        private void closeDropdown()
        {
            dropdownControl.LostFocus -= dropdownControlLostFocus;
            this.Parent.Controls.Remove(dropdownControl);
        }

        private void updateDropdown()
        {
            int dropdownRight = this.Location.X + selectedSection.Location.X + DrawingConstants.DropdownWidth;
            if (dropdownRight >= this.Parent.ClientSize.Width)
            {
                dropdownControl.Location = new Point(this.Location.X + selectedSection.Location.Right - DrawingConstants.DropdownWidth, this.Location.Y + this.Height);
            } else
            {
                dropdownControl.Location = new Point(this.Location.X + selectedSection.Location.X, this.Location.Y + this.Height);
            }
            dropdownControl.Section = selectedSection.Section;
            if (!this.Parent.Controls.Contains(dropdownControl))
            {
                dropdownControl.LostFocus += dropdownControlLostFocus;
                this.Parent.Controls.Add(dropdownControl);
                this.Parent.Controls.SetChildIndex(dropdownControl, 0);
                dropdownControl.Focus();
            }
            dropdownControl.Refresh();
        }

        private void dropdownControlLostFocus(object? sender, EventArgs e)
        {
            selectedSection = null;
            closeDropdown();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            if (selectedSection != null)
            {
                dropdownControl.Invalidate();
            }
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            using (Font boldFont = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            //using (Font largeFont = new Font("Montserrat", DrawingConstants.MediumTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush gray = new SolidBrush(DrawingConstants.FadedGray))
            using (Pen seperatorPen = new Pen(DrawingConstants.ContrastColor, DrawingConstants.SeperatorLineWidth))
            using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
            using (SolidBrush contrast = new SolidBrush(DrawingConstants.ContrastColor))
            using (SolidBrush fadedGray = new SolidBrush(DrawingConstants.FadedGray))
            {
                foreach (UIElement uiElement in ui)
                {
                    Type type = uiElement.GetType();
                    if (type == typeof(Image))
                    {
                        Image image = (Image)uiElement;
                        g.DrawImage(image.Bitmap[DrawingConstants.ContrastColor], image.Location);
                    }
                    else if (type == typeof(Label))
                    {
                        Label label = (Label)uiElement;
                        g.DrawString(label.Text, boldFont, contrast, label.Location.X, label.Location.Y);
                    } else if (type == typeof(Seperator))
                    {
                        g.DrawLine(seperatorPen, uiElement.Location.X + (uiElement.Location.Width / 2), uiElement.Location.Y, uiElement.Location.X + (uiElement.Location.Width / 2), uiElement.Location.Y + uiElement.Location.Height);
                    } else if (type == typeof(Button))
                    {
                        Button button = (Button)uiElement;
                        if (button.Hovered)
                        {
                            g.FillRectangle(hoverBrush, button.Location);
                        }
                        int centerX = button.Location.X + (button.Location.Width / 2);
                        int centerY = button.Location.Y + (button.Location.Height / 2);
                        if (button.ContextItem.Selected)
                        {
                            g.DrawImage(button.Icon[DrawingConstants.ContrastColor], centerX - DrawingConstants.MediumIconSize / 2, centerY - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        } else
                        {
                            g.DrawImage(button.Icon[DrawingConstants.FadedGray], centerX - DrawingConstants.MediumIconSize / 2, centerY - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        }
                    } else if (type == typeof(CollapsedSection))
                    {
                        CollapsedSection collapsedSection = (CollapsedSection)uiElement;
                        if (collapsedSection.Hovered)
                        {
                            g.FillRectangle(hoverBrush, collapsedSection.Location);
                        }
                        int centerX = collapsedSection.Location.X + (collapsedSection.Location.Width / 2);
                        int centerY = collapsedSection.Location.Y + (collapsedSection.Location.Height / 2);
                        SizeF textSize = g.MeasureString(collapsedSection.Section.SectionName, boldFont);
                        if (selectedSection == null || selectedSection == collapsedSection)
                        {
                            g.DrawString(collapsedSection.Section.SectionName, boldFont, contrast, centerX - textSize.Width / 2, centerY - textSize.Height / 2);
                        } else
                        {
                            g.DrawString(collapsedSection.Section.SectionName, boldFont, fadedGray, centerX - textSize.Width / 2, centerY - textSize.Height / 2);
                        }
                    }
                }
            }
        }

        private abstract class UIElement
        {
            public Rectangle Location;
        }
        private class Image : UIElement
        {
            public MultiColorBitmap Bitmap { get; set; }
        }
        private class Label : UIElement
        {
            public string Text { get; set; }
        }
        private class Seperator : UIElement { }
        private class Button : UIElement
        {
            public ContextItem ContextItem { get; set; }
            public MultiColorBitmap Icon { get; set; }
            public bool Hovered { get; set; } = false;
        }
        private class CollapsedSection : UIElement
        {
            public ContextSection Section { get; set; }
            public bool Hovered { get; set; } = false;
        }
    }
    public class ContextSection : IEnumerable<ContextItem>
    {
        public delegate void RadioChangeEvent(ContextItem selectedItem);

        public string SectionName { get; set; }
        public ToggleType ToggleType { get; set; }
        public event RadioChangeEvent RadioChange;
        private List<ContextItem> items { get; set; } = new List<ContextItem>();

        public void AddItem(ContextItem item)
        {
            items.Add(item);
            if (ToggleType == ToggleType.Togglable && !item.ExcludeFromToggle)
            {
                item.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) =>
                {
                    if (!e.Cancel)
                    {
                        item.Selected = !item.Selected;
                    }
                };
            } else if (ToggleType == ToggleType.Radio && !item.ExcludeFromToggle)
            {
                item.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) =>
                {
                    if (!e.Cancel)
                    {
                        foreach (ContextItem itemToClear in items)
                        {
                            itemToClear.Selected = false;
                        }
                        item.Selected = true;
                        RadioChange?.Invoke(item);
                    }
                };
            }
        }
        public void ClearItems()
        {
            items.Clear();
        }
        public ContextItem this[int i]
        {
            get
            {
                return items[i];
            }
        }
        public IEnumerator<ContextItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
    public class ContextItem
    {
        public class ContextItemClickEventArgs { public bool Cancel { get; set; } = false; }
        public delegate void ContextItemClick(ContextItem sender, ContextItemClickEventArgs e);

        public string CollapsedText { get; set; }
        public MultiColorBitmap Icon { get; set; }
        public bool Selected { get; set; }
        public bool DontCloseDropdown { get; set; } = false;
        public bool ExcludeFromToggle { get; set; } = false;
        public ContextSection Submenu { get; set; } = null;
        public event ContextItemClick Click;

        public ContextItem(MultiColorBitmap icon, string collapsedText)
        {
            Icon = icon;
            CollapsedText = collapsedText;
        }

        public void InvokeClick()
        {
            Click?.Invoke(this, new ContextItemClickEventArgs());
        }
    }
    public enum ToggleType
    {
        NotTogglable,
        Togglable,
        Radio
    }
}
