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
        public bool IncludeCover { get; set; } = false;
        public event PageCountSet OnPageCountSet;

        private List<List<UIRow>> pages;
        private int currentPageIndex = -1;
        private float calculatedScale;

        private const float headTextSize = 52; // pt
        private const float subTextSize = 24; // pt

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
            currentPageIndex = -1;
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
            int pageCount = pages.Count;
            if (IncludeCover)
            {
                pageCount++;
            }
            OnPageCountSet?.Invoke(pageCount);
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs e)
        {
            if (currentPageIndex < 0)
            {
                currentPageIndex = 0;
                if (IncludeCover)
                {
                    drawCover(e);
                    return;
                }
            }
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

        private void drawCover(PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using (Font headFont = new Font(DrawingConstants.Montserrat, headTextSize, FontStyle.Bold, GraphicsUnit.Point))
            using (Font subFont = new Font(DrawingConstants.Montserrat, subTextSize, FontStyle.Regular, GraphicsUnit.Point))
            using (SolidBrush black = new SolidBrush(Color.Black))
            {
                int width = e.MarginBounds.Width;
                int height = e.MarginBounds.Height;
                g.TranslateTransform(e.MarginBounds.X, e.MarginBounds.Y);
                string headText = Song.Name;
                string subText = Song.TimeSignature.T1 + "/" + Song.TimeSignature.T2 + " • " + Song.Tempo + " BPM •";
                for (int i = Song.Tab.Tuning.Count - 1; i >= 0; i--)
                {
                    subText += " " + Song.Tab.Tuning.GetMusicalNote(i).ToString();
                }
                SizeF headSize;
                SizeF subSize;
                float headLineHeight = headFont.GetHeight(g);
                float subLineHeight = subFont.GetHeight(g);
                string[] headLines = wrapText(g, headText, width, headFont, out headSize);
                string[] subLines = wrapText(g, subText, width, subFont, out subSize);
                SizeF contentSize = new SizeF(Math.Max(headSize.Width, subSize.Width), headSize.Height + subLineHeight + subSize.Height);
                float startY = (height / 2) - contentSize.Height / 2;
                for (int i = 0; i < headLines.Length; i++)
                {
                    float y = startY + (i * headLineHeight);
                    string text = headLines[i];
                    SizeF textSize = g.MeasureString(text, headFont);
                    float x = (width / 2) - (textSize.Width / 2);
                    g.DrawString(text, headFont, black, x, y);
                }
                startY += headSize.Height + subLineHeight;
                for (int i = 0; i < subLines.Length; i++)
                {
                    float y = startY + (i * subLineHeight);
                    string text = subLines[i];
                    SizeF textSize = g.MeasureString(text, subFont);
                    float x = (width / 2) - (textSize.Width / 2);
                    g.DrawString(text, subFont, black, x, y);
                }
            }
            e.HasMorePages = true;
        }

        private string[] wrapText(Graphics g, string text, float maxWidth, Font font, out SizeF size)
        {
            string[] words = text.Split(' ');
            string currentLine = words[0];
            List<string> lines = new List<string>();
            SizeF totalSize = new SizeF(0, 0);
            for (int i = 1; i < words.Length; i++)
            {
                string potentialLine = currentLine + " " + words[i];
                SizeF lineSize = g.MeasureString(potentialLine, font);
                if (lineSize.Width <= maxWidth)
                {
                    currentLine = potentialLine;
                    if (lineSize.Width > totalSize.Width)
                    {
                        totalSize = new SizeF(lineSize.Width, 1);
                    }
                } else
                {
                    lines.Add(currentLine);
                    currentLine = words[i];
                }
            }
            lines.Add(currentLine);
            size = new SizeF(totalSize.Width, font.GetHeight(g) * lines.Count);
            return lines.ToArray();
        }
    }
}
