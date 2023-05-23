using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class ContextMenu : Control
    {
        protected List<ContextSection> Sections { get; set; } = new List<ContextSection>();
        protected MultiColorBitmap Logo { get; set; }
        private List<UIElement> ui = new List<UIElement>();
        private Button hoveredButton = null;

        protected void updateUI()
        {
            ui.Clear();
            int startY = DrawingConstants.LargeMargin;
            int x = DrawingConstants.MediumMargin;
            int usableHeight = this.Height - DrawingConstants.LargeMargin * 2;
            Image image = new Image();
            image.Bitmap = Logo;
            int imageWidth = (int)(Logo.Size.Width * (usableHeight / (float)Logo.Size.Height));
            image.Location = new Rectangle(x, startY, imageWidth, usableHeight);
            x += imageWidth;
            ui.Add(image);
            foreach (ContextSection section in Sections)
            {
                Seperator seperator = new Seperator();
                seperator.Location = new Rectangle(x, startY, DrawingConstants.SectionSpacing, usableHeight);
                ui.Add(seperator);
                x += DrawingConstants.SectionSpacing;
                Label label = new Label();
                label.Text = section.SectionName;
                label.Location = new Rectangle(x, startY, 0, 0);
                ui.Add(label);
                foreach (ContextItem item in section)
                {
                    Button button = new Button();
                    button.Icon = item.Icon;
                    button.ContextItem = item;
                    int buttonSize = (int)(usableHeight - DrawingConstants.SmallTextSizePx);
                    button.Location = new Rectangle(x, startY + (int)DrawingConstants.SmallTextSizePx, buttonSize, buttonSize);
                    x += buttonSize;
                    ui.Add(button);
                }
            }
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
                    break;
                }
            }
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredButton != null)
            {
                hoveredButton.Hovered = false;
                hoveredButton = null;
                this.Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (hoveredButton != null)
            {
                hoveredButton.ContextItem.InvokeClick();
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            using (Font boldFont = new Font("Montserrat", DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            using (SolidBrush gray = new SolidBrush(DrawingConstants.FadedGray))
            using (Pen seperatorPen = new Pen(Color.White, DrawingConstants.SeperatorLineWidth))
            using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
            using (SolidBrush white = new SolidBrush(Color.White))
            {
                foreach (UIElement uiElement in ui)
                {
                    Type type = uiElement.GetType();
                    if (type == typeof(Image))
                    {
                        Image image = (Image)uiElement;
                        g.DrawImage(image.Bitmap[Color.White], image.Location);
                    }
                    else if (type == typeof(Label))
                    {
                        Label label = (Label)uiElement;
                        g.DrawString(label.Text, boldFont, white, label.Location.X, label.Location.Y);
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
                            g.DrawImage(button.Icon[Color.White], centerX - DrawingConstants.SmallIconSize / 2, centerY - DrawingConstants.SmallIconSize / 2, DrawingConstants.SmallIconSize, DrawingConstants.SmallIconSize);
                        } else
                        {
                            g.DrawImage(button.Icon[DrawingConstants.FadedGray], centerX - DrawingConstants.SmallIconSize / 2, centerY - DrawingConstants.SmallIconSize / 2, DrawingConstants.SmallIconSize, DrawingConstants.SmallIconSize);
                        }
                    }
                }
            }
        }

        public ContextMenu()
        {
            this.BackColor = Color.FromArgb(0x0, 0x0, 0x0);
            this.DoubleBuffered = true;
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
    }
    internal class ContextSection : IEnumerable<ContextItem>
    {
        public delegate void RadioChangeEvent(ContextItem selectedItem);

        public string SectionName { get; set; }
        public ToggleType ToggleType { get; set; }
        public event RadioChangeEvent RadioChange;
        private List<ContextItem> items { get; set; } = new List<ContextItem>();
        public void AddItem(ContextItem item)
        {
            items.Add(item);
            if (ToggleType == ToggleType.Togglable)
            {
                item.Click += () =>
                {
                    item.Selected = !item.Selected;
                };
            } else if (ToggleType == ToggleType.Radio)
            {
                item.Click += () =>
                {
                    foreach (ContextItem itemToClear in items)
                    {
                        itemToClear.Selected = false;
                    }
                    item.Selected = true;
                    RadioChange?.Invoke(item);
                };
            }
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
    internal class ContextItem
    {
        public MultiColorBitmap Icon { get; set; }
        public bool Selected { get; set; }
        public event Action Click;

        public ContextItem(MultiColorBitmap icon)
        {
            Icon = icon;
        }

        public void InvokeClick()
        {
            Click?.Invoke();
        }
    }
    enum ToggleType
    {
        NotTogglable,
        Togglable,
        Radio
    }
}
