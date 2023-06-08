using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class TabPrinter : TabEditor
    {
        public delegate void PageCountSet(int count);

        public PrintDocument Document { get; } = new PrintDocument();
        public float Scale { get; set; } = 1.0F;
        public event PageCountSet OnPageCountSet;

        private List<List<UIRow>> pages;
        private int currentPageIndex = 0;
        private float calculatedScale;

        public TabPrinter()
        {
            Document.BeginPrint += Document_BeginPrint;
            Document.PrintPage += Document_PrintPage;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            throw new InvalidOperationException("TabPrinter cannot be added to a control. It is not a real control.");
        }

        private void Document_BeginPrint(object sender, PrintEventArgs e)
        {
            currentPageIndex = 0;
            // my approach to scaling here is to get updateUI to break correctly without setting the DrawingConstants values, then do the scale as a transform when we draw
            calculatedScale = Scale / DrawingConstants.CurrentScale; // factor the screen dpi out of the drawing constant scales

            // first calculate layout (row breaks) for the entire tab so we can calculate page breaks.
            this.MaxHeight = int.MaxValue;
            PageSettings pageSettings = Document.DefaultPageSettings;
            int marginWidth = pageSettings.Margins.Left + pageSettings.Margins.Right;
            int marginHeight = pageSettings.Margins.Top + pageSettings.Margins.Bottom;
            int printAreaWidth = (int)((pageSettings.Bounds.Width - marginWidth) / calculatedScale); // ex: if were drawing at half scale (0.5), there is now double the available space for breaking calculations
            int printAreaHeight = (int)((pageSettings.Bounds.Height - marginHeight) / calculatedScale);
            this.Size = new Size(printAreaWidth, 0);
            updateUI();

            int rowY = 0;
            int tallRowHeight = DrawingConstants.RowHeight * (Song.Tab.Tuning.Count + 2); // +2 is for heading + spacing line
            pages = new List<List<UIRow>>();
            List<UIRow> currentPage = new List<UIRow>();
            for (int i = 0; i < tabUI.Count; i++)
            {
                UIRow currentRow = tabUI[i];
                rowY += tallRowHeight;
                if (rowY < printAreaHeight)
                {
                    currentPage.Add(currentRow);
                } else
                {
                    pages.Add(currentPage);
                    currentPage = new List<UIRow>();
                    rowY = tallRowHeight;
                    currentPage.Add(currentRow);
                }
            }
            pages.Add(currentPage);
            OnPageCountSet?.Invoke(pages.Count);
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs e)
        {
            tabUI = pages[currentPageIndex];
            e.Graphics.TranslateTransform(e.MarginBounds.X, e.MarginBounds.Y);
            e.Graphics.ScaleTransform(calculatedScale, calculatedScale);
            PaintEventArgs paintEventArgs = new PaintEventArgs(e.Graphics, new Rectangle(0, 0, e.MarginBounds.Width, e.MarginBounds.Height));
            Enums.Theme originalTheme = DrawingConstants.CurrentTheme;
            DrawingConstants.SetTheme(Enums.Theme.LightMode);
            this.OnPaint(paintEventArgs);
            DrawingConstants.SetTheme(originalTheme);

            currentPageIndex++;
            e.HasMorePages = (currentPageIndex < pages.Count);
        }
    }
}
