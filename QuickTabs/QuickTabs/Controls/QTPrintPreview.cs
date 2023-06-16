using QuickTabs.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class QTPrintPreview : Panel
    {
        public override Color BackColor { get => DrawingConstants.UIControlBackColor; set => base.BackColor = value; }

        public TabPrinter.DocumentPreview PreviewSource
        {
            get
            {
                return pagePreview.PreviewSource;
            }
            set
            {
                pagePreview.PreviewSource = value;
            }
        }
        public int Page
        {
            get
            {
                return pagePreview.Page;
            }
            set
            {
                pagePreview.Page = value;
                InvalidatePreview();
            }
        }
        public float Zoom
        {
            get
            {
                return pagePreview.Zoom;
            }
            set
            {
                pagePreview.Zoom = value;
                InvalidatePreview();
            }
        }

        private QTPagePreview pagePreview;

        public QTPrintPreview(TabPrinter.DocumentPreview Preview)
        {
            pagePreview = new QTPagePreview();
            pagePreview.PreviewSource = Preview;
            Controls.Add(pagePreview);
            this.DoubleBuffered = true;
            this.AutoScroll = true;
            InvalidatePreview();
        }

        private void centerPagePreview()
        {
            int centerX = this.Width / 2;
            int centerY = this.Height / 2;
            int x = centerX - pagePreview.Width / 2;
            int y = centerY - pagePreview.Height / 2;
            if (pagePreview.Width > this.Width)
            {
                x = 0;
            }
            if (pagePreview.Height > this.Height)
            {
                y = 0;
            }
            pagePreview.Location = new Point(x, y);
            this.Invalidate();
        }

        public void InvalidatePreview()
        {
            if (this.VScroll)
            {
                this.VerticalScroll.Value = 0;
            }
            if (this.HScroll)
            {
                this.HorizontalScroll.Value = 0;
            }
            pagePreview.RefreshPage();
            centerPagePreview();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            centerPagePreview();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            using (SolidBrush brush = new SolidBrush(DrawingConstants.FadedGray))
            {
                int width = DrawingConstants.PrintPreviewOutlineWidth;
                Rectangle rect = new Rectangle(pagePreview.Location.X - width, pagePreview.Location.Y - width, pagePreview.Width + width * 2, pagePreview.Height + width * 2);
                g.FillRectangle(brush, rect);
            }
        }

        private class QTPagePreview : Control
        {
            public override Color BackColor { get => Color.White; set => base.BackColor = value; }

            public TabPrinter.DocumentPreview PreviewSource { get; set; }
            private int page = 0;
            public int Page
            {
                get
                {
                    return page;
                }
                set
                {
                    if (value < 0 || value >= PreviewSource.PageCount)
                    {
                        throw new ArgumentOutOfRangeException();
                    } else
                    {
                        page = value;
                    }
                }
            }
            public float Zoom { get; set; } = 0.5F;

            public void RefreshPage()
            {
                if (page >= PreviewSource.PageCount)
                {
                    page = PreviewSource.PageCount - 1;
                }
                this.Size = PreviewSource.GetSize(Zoom);
                this.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                PreviewSource.Draw(page, e.Graphics, Zoom);
            }
        }
    }
}
