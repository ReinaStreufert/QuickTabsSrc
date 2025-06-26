using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public class IconPicker : Control
    {
        public override Color BackColor { get => DrawingConstants.TabEditorBackColor; set => base.BackColor = value; }

        private ObservableCollection<MultiColorBitmap> icons = new ObservableCollection<MultiColorBitmap>();
        public Collection<MultiColorBitmap> Icons { get => icons; }
        public int IconSize { get; set; } = DrawingConstants.SmallIconSize;
        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }
        public event EventHandler SelectedIndexChanged;
        private int hoveredIndex = -1;

        public IconPicker()
        {
            this.DoubleBuffered = true;
            icons.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => collectionChanged();
        }

        private void collectionChanged()
        {
            if (selectedIndex >= icons.Count)
            {
                SelectedIndex = icons.Count - 1;
            }
            this.Size = new Size(this.Size.Height * icons.Count, this.Size.Height);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredIndex = -1;
            this.Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.X < 0 || e.Y < 0)
            {
                return;
            }
            hoveredIndex = (int)Math.Floor(e.X / (float)this.Size.Height);
            this.Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (hoveredIndex >= 0 && hoveredIndex < icons.Count)
            {
                selectedIndex = hoveredIndex;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int cellSize = this.Size.Height;
            using (SolidBrush highlightBrush = new SolidBrush(DrawingConstants.HighlightColor))
            {
                for (int i = 0; i < icons.Count; i++)
                {
                    int x = i * cellSize;
                    int centerX = x + (cellSize / 2);
                    int centerY = cellSize / 2;
                    int iconSize = IconSize;
                    int iconX = centerX - iconSize / 2;
                    int iconY = centerY - iconSize / 2;
                    MultiColorBitmap icon = icons[i];
                    Color iconColor;
                    if (selectedIndex == i)
                    {
                        iconColor = DrawingConstants.ContrastColor;
                    } else
                    {
                        iconColor = DrawingConstants.FadedGray;
                    }
                    g.DrawImage(icon[iconColor], iconX, iconY, iconSize, iconSize);
                    if (hoveredIndex == i)
                    {
                        g.FillRectangle(highlightBrush, x, 0, cellSize, cellSize);
                    }
                }
            }
        }
    }
}
