using QuickTabs.Enums;
using QuickTabs.Songwriting;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class TabEditor : Control
    {
        public override Color BackColor { get => DrawingConstants.TabEditorBackColor; set => base.BackColor = value; }

        public int MaxHeight { get; set; }
        public Song Song { get; set; }
        private Selection selection;
        public Selection Selection
        {
            get
            {
                return selection;
            }
            set
            {
                if (PlayMode)
                {
                    return;
                }
                selection = value;
                SelectionChanged?.Invoke();
            }
        }
        public int PlayCursor
        {
            get
            {
                return selection.SelectionStart;
            }
            set
            {
                selection = new Selection(value, 1);
                SelectionChanged?.Invoke();
            }
        }
        public void QuietlySelect(Selection newSelection)
        {
            selection = newSelection;
        }
        public event Action SelectionChanged;
        public bool PlayMode { get; set; } = false;

        private List<UIRow> tabUI = new List<UIRow>();
        private UIStep currentlyHighlighted = null;
        private Point selectionStartPoint;
        private Selection oldSelection;
        private bool mouseDown = false;
        private VScrollBar scrollBar = new VScrollBar();
        private bool scrollbarShown = false;

        public TabEditor()
        {
            this.DoubleBuffered = true;
            scrollBar.Scroll += scrollBar_Scroll;
            ShortcutManager.AddShortcut(Keys.None, Keys.A, () => { setRelativeSelection(-1); });
            ShortcutManager.AddShortcut(Keys.None, Keys.D, () => { setRelativeSelection(1); });
            ShortcutManager.AddShortcut(Keys.Shift, Keys.A, () => { lengthenSelection(-1); });
            ShortcutManager.AddShortcut(Keys.Shift, Keys.D, () => { lengthenSelection(1); });
        }

        private void updateUI()
        {
            if (Song == null)
            {
                return;
            }
            Songwriting.Tab tab = Song.Tab;
            tabUI.Clear();

            int usableWidth = this.Width - DrawingConstants.MediumMargin * 2 - DrawingConstants.LeftMargin;
            if (scrollbarShown)
            {
                usableWidth -= SystemInformation.VerticalScrollBarWidth;
            }
            int rowsPerStaff = tab.Tuning.Count;
            int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;

            UIRow currentRow = new UIRow();
            int sinceLastMeasureBar = 0;
            int stepIndex = 0;
            foreach (Step step in tab)
            {
                if ((currentRow.Steps.Count + 1) * DrawingConstants.StepWidth >= usableWidth)
                {
                    tabUI.Add(currentRow);
                    currentRow = new UIRow();
                }

                switch (step.Type)
                {
                    case StepType.SectionHead:
                        SectionHead sectionHead = (SectionHead)step;
                        if (currentRow.Steps.Count > 0)
                        {
                            tabUI.Add(currentRow);
                            currentRow = new UIRow();
                            currentRow.Head = sectionHead.Name;
                        } else
                        {
                            currentRow.Head = sectionHead.Name;
                        }
                        currentRow.Steps.Add(new UIStep(UIStepType.MeasureBar, null));
                        sinceLastMeasureBar = 0;
                        break;
                    case StepType.Beat:
                        Beat beat = (Beat)step;
                        UIStep uiStep = new UIStep(UIStepType.Beat, beat);
                        if (Selection != null && Selection.Contains(stepIndex))
                        {
                            uiStep.Selected = true;
                        }
                        currentRow.Steps.Add(uiStep);
                        break;
                    case StepType.Comment:
                        Comment comment = (Comment)step;
                        uiStep = new UIStep(UIStepType.Comment, comment);
                        if (Selection != null && Selection.Contains(stepIndex))
                        {
                            uiStep.Selected = true;
                        }
                        currentRow.Steps.Add(uiStep);
                        break;
                }
                if (step.Type == StepType.SectionHead)
                {
                    stepIndex++;
                    continue;
                }
                sinceLastMeasureBar++;
                if (sinceLastMeasureBar >= beatsPerMeasure)
                {
                    if ((currentRow.Steps.Count + 2) * DrawingConstants.StepWidth < usableWidth)
                    {
                        currentRow.Steps.Add(new UIStep(UIStepType.MeasureBar, null));
                        sinceLastMeasureBar = 0;
                    } else
                    {
                        if (stepIndex + 1 < tab.Count && tab[stepIndex + 1].Type == StepType.SectionHead)
                        {
                            stepIndex++;
                            continue;
                        }
                        if (stepIndex + 1 >= tab.Count)
                        {
                            stepIndex++;
                            continue;
                        }
                        tabUI.Add(currentRow);
                        currentRow = new UIRow();
                        currentRow.Steps.Add(new UIStep(UIStepType.MeasureBar, null));
                        sinceLastMeasureBar = 0;
                    }
                }

                stepIndex++;
            }
            tabUI.Add(currentRow);
            int tallRowHeight = DrawingConstants.RowHeight * (Song.Tab.Tuning.Count + 2); // +2 is for heading + spacing line
            int contentHeight = (tallRowHeight * tabUI.Count) + DrawingConstants.MediumMargin;
            if (contentHeight < MaxHeight)
            {
                this.Height = contentHeight;
                if (scrollbarShown)
                {
                    hideScrollbar();
                }
            } else
            {
                this.Height = MaxHeight;
                if (!scrollbarShown)
                {
                    showScrollbar(contentHeight);
                } else if (scrollBar.Maximum != contentHeight - this.Height)
                {
                    scrollBar.Maximum = (contentHeight - this.Height) + DrawingConstants.ScrollbarLargeChange;
                    if (scrollBar.Value >= (contentHeight - this.Height))
                    {
                        scrollBar.Value = (contentHeight - this.Height);
                    }
                }
            }
        }
        private void showScrollbar(int contentHeight)
        {
            scrollBar.Location = new Point(this.Width - SystemInformation.VerticalScrollBarWidth, 0);
            scrollBar.Size = new Size(SystemInformation.VerticalScrollBarWidth, this.Height);
            scrollBar.Value = 0;
            scrollBar.Minimum = 0;
            scrollBar.Maximum = (contentHeight - this.Height) + DrawingConstants.ScrollbarLargeChange;
            scrollBar.SmallChange = DrawingConstants.ScrollbarSmallChange;
            scrollBar.LargeChange = DrawingConstants.ScrollbarLargeChange;
            this.Controls.Add(scrollBar);
            scrollbarShown = true;
            updateUI();
        }
        private void hideScrollbar()
        {
            this.Controls.Remove(scrollBar);
            scrollbarShown = false;
            updateUI();
        }
        private void scrollBar_Scroll(object? sender, ScrollEventArgs e)
        {
            this.Invalidate();
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (scrollbarShown)
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
        private bool tryGetStepFromPoint(Point point, out UIStep step)
        {
            if (scrollbarShown)
            {
                point.Y += scrollBar.Value;
            }
            if (point.X < DrawingConstants.MediumMargin + DrawingConstants.LeftMargin)
            {
                step = null;
                return false;
            }
            int tallRowHeight = DrawingConstants.RowHeight * (Song.Tab.Tuning.Count + 2); // +2 is for heading + spacing line
            int rowIndex = (int)Math.Floor((float)point.Y / tallRowHeight);
            if (rowIndex >= tabUI.Count)
            {
                step = null;
                return false;
            }
            UIRow row = tabUI[rowIndex];
            int stepIndex = (int)Math.Round(((float)point.X - (DrawingConstants.MediumMargin + DrawingConstants.LeftMargin)) / DrawingConstants.StepWidth);
            if (stepIndex >= row.Steps.Count)
            {
                step = null;
                return false;
            } else
            {
                step = row.Steps[stepIndex];
                return true;
            }
        }
        private UIStep uiStepFromTabStep(Step step)
        {
            foreach (UIRow row in tabUI)
            {
                foreach (UIStep uiStep in row.Steps)
                {
                    if (uiStep.AssociatedStep == step)
                    {
                        return uiStep;
                    }
                }
            }
            throw new IndexOutOfRangeException();
        }
        private void setRelativeSelection(int direction)
        {
            if (PlayMode)
            {
                return;
            }
            if (selection != null)
            {
                int newStart;
                if (direction < 0)
                {
                    newStart = selection.SelectionStart - 1;
                    while (Song.Tab[newStart].Type != Enums.StepType.Beat)
                    {
                        newStart--;
                        if (newStart < 0)
                        {
                            return;
                        }
                    }
                }
                else if (direction > 0)
                {
                    newStart = selection.SelectionStart + selection.SelectionLength;
                    if (newStart >= Song.Tab.Count)
                    {
                        return;
                    }
                    while (Song.Tab[newStart].Type != Enums.StepType.Beat)
                    {
                        newStart++;
                        if (newStart >= Song.Tab.Count)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
                Selection = new Selection(newStart, 1);
                updateUI();
                this.Invalidate();
                History.PushState(Song, selection, false);
            }
        }
        private void lengthenSelection(int direction)
        {
            if (PlayMode)
            {
                return;
            }
            if (selection != null)
            {
                int newStart;
                int newLength = selection.SelectionLength + 1;
                if (direction < 0)
                {
                    newStart = selection.SelectionStart - 1;
                    while (Song.Tab[newStart].Type != Enums.StepType.Beat)
                    {
                        newStart--;
                        if (newStart < 0)
                        {
                            return;
                        }
                    }
                    newLength = (selection.SelectionStart + selection.SelectionLength - 1) - newStart + 1;
                }
                else if (direction > 0)
                {
                    newStart = selection.SelectionStart;
                    newLength = selection.SelectionLength + 1;
                    if (newStart + newLength - 1 >= Song.Tab.Count)
                    {
                        return;
                    }
                    while (Song.Tab[newStart + newLength - 1].Type != Enums.StepType.Beat)
                    {
                        newLength++;
                        if (newStart + newLength - 1 >= Song.Tab.Count)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
                Selection = new Selection(newStart, newLength);
                updateUI();
                this.Invalidate();
                History.PushState(Song, Selection, false);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (PlayMode)
            {
                return;
            }

            UIStep uiStep;
            if (e.X < 0 || e.Y < 0)
            {
                return;
            }
            if (tryGetStepFromPoint(e.Location, out uiStep) && uiStep.Type == UIStepType.Beat)
            {
                if (mouseDown)
                {
                    if (updateSelectionFromPoints(selectionStartPoint, e.Location))
                    {
                        this.Invalidate();
                    }
                }

                if (currentlyHighlighted != null)
                {
                    currentlyHighlighted.Highlighted = false;
                }
                uiStep.Highlighted = true;
                currentlyHighlighted = uiStep;
                this.Invalidate();
            } else
            {
                if (currentlyHighlighted != null)
                {
                    currentlyHighlighted.Highlighted = false;
                    currentlyHighlighted = null;
                    this.Invalidate();
                }
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (currentlyHighlighted != null)
            {
                currentlyHighlighted.Highlighted = false;
                currentlyHighlighted = null;
                this.Invalidate();
            }
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (scrollbarShown)
            {
                scrollBar.Location = new Point(this.Width - SystemInformation.VerticalScrollBarWidth, 0);
                scrollBar.Height = this.Height;
            }
            updateUI();
            this.Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (PlayMode)
            {
                return;
            }
            selectionStartPoint = e.Location;
            oldSelection = selection;
            mouseDown = true;
            this.Focus();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown = false;
            if (PlayMode)
            {
                return;
            }
            if (updateSelectionFromPoints(selectionStartPoint, e.Location))
            {
                this.Invalidate();
            }
            if (selection == null || oldSelection == null)
            {
                if (selection != oldSelection)
                {
                    History.PushState(Song, selection, false);
                }
            } else
            {
                if (selection.SelectionStart != oldSelection.SelectionStart || selection.SelectionLength != oldSelection.SelectionLength)
                {
                    History.PushState(Song, selection, false);
                }
            }
            if (selection != null && selection.SelectionLength == 1)
            {
                BeatPlayer beatPlayer = new BeatPlayer((Beat)(Song.Tab[selection.SelectionStart]));
                beatPlayer.BPM = Song.Tempo;
                beatPlayer.Tuning = Song.Tab.Tuning;
                beatPlayer.Start();
            }
        }
        private bool updateSelectionFromPoints(Point p1, Point p2) // output is whether anything changed.
        {
            bool selectionWasNull = (Selection == null);
            // clear current selection
            if (Selection != null)
            {
                for (int i = Selection.SelectionStart; i < Selection.SelectionStart + Selection.SelectionLength; i++)
                {
                    if (i < Song.Tab.Count)
                    {
                        if (Song.Tab[i].Type == StepType.Beat)
                        {
                            uiStepFromTabStep(Song.Tab[i]).Selected = false;
                        }
                    }
                }
            }

            UIStep p1Step;
            UIStep p2Step;
            if (tryGetStepFromPoint(p1, out p1Step) && tryGetStepFromPoint(p2, out p2Step))
            {
                if (p1Step.Type == UIStepType.MeasureBar || p2Step.Type == UIStepType.MeasureBar)
                {
                    Selection = null;
                    return !selectionWasNull;
                }
                UIStep earliestStep;
                UIStep latestStep;
                if (p1Step.AssociatedStep.IndexWithinTab < p2Step.AssociatedStep.IndexWithinTab)
                {
                    earliestStep = p1Step;
                    latestStep = p2Step;
                } else
                {
                    earliestStep = p2Step;
                    latestStep = p1Step;
                }
                for (int i = earliestStep.AssociatedStep.IndexWithinTab; i <= latestStep.AssociatedStep.IndexWithinTab; i++)
                {
                    if (Song.Tab[i].Type == StepType.Beat)
                    {
                        UIStep uiStep = uiStepFromTabStep(Song.Tab[i]);
                        uiStep.Selected = true;
                    }
                }
                Selection oldSelection = Selection;
                Selection = new Selection(earliestStep.AssociatedStep.IndexWithinTab, latestStep.AssociatedStep.IndexWithinTab - earliestStep.AssociatedStep.IndexWithinTab + 1);
                if (oldSelection == null)
                {
                    return true;
                }
                if (oldSelection.SelectionStart == Selection.SelectionStart && oldSelection.SelectionLength == Selection.SelectionLength)
                {
                    return false;
                }
                return true;
            } else
            {
                Selection = null;
                return !selectionWasNull;
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            updateUI();
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            if (scrollbarShown)
            {
                g.TranslateTransform(0, -scrollBar.Value);
            }

            Color selectionColor = DrawingConstants.EditModeSelectionColor;
            if (PlayMode)
            {
                selectionColor = DrawingConstants.PlayModeSelectionColor;
            }
            using (SolidBrush backBrush = new SolidBrush(BackColor))
            using (SolidBrush higlightBrush = new SolidBrush(DrawingConstants.HighlightColor))
            using (SolidBrush selectionBrush = new SolidBrush(selectionColor))
            using (SolidBrush textBrush = new SolidBrush(DrawingConstants.ContrastColor))
            using (Pen backPen = new Pen(DrawingConstants.ContrastColor, DrawingConstants.PenWidth))
            using (Pen forePen = new Pen(DrawingConstants.HighlightBlue, DrawingConstants.BoldPenWidth))
            using (Font font = new Font("Montserrat", DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font boldFont = new Font("Montserrat", DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font twoDigitFont = new Font("Montserrat", DrawingConstants.TwoDigitTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                int stringCount = Song.Tab.Tuning.Count;
                int tallRowHeight = DrawingConstants.RowHeight * (Song.Tab.Tuning.Count + 2);

                int startX = DrawingConstants.MediumMargin;
                int startY = DrawingConstants.MediumMargin;

                Dictionary<int[], int> currentlyHeldStrings = new Dictionary<int[], int>();
                foreach (UIRow row in tabUI)
                {
                    int rowWidth = DrawingConstants.LeftMargin + (row.Steps.Count * DrawingConstants.StepWidth);

                    // heading
                    if (row.Head != "")
                    {
                        g.DrawString(row.Head, font, textBrush, startX, startY - DrawingConstants.SmallTextYOffset);
                    }

                    // strings
                    for (int i = 0; i < stringCount; i++)
                    {
                        int x = startX + DrawingConstants.StringOffsetForLetters;
                        int y = startY + (i * DrawingConstants.RowHeight) + DrawingConstants.RowHeight;
                        g.DrawLine(backPen, x, y, (startX + rowWidth) - (DrawingConstants.StepWidth), y);
                        //g.FillRectangle(textBrush, x, y, 10, 10);
                        SizeF textSize = g.MeasureString(Song.Tab.Tuning[i], boldFont);
                        g.DrawString(Song.Tab.Tuning[i], boldFont, textBrush, startX - textSize.Width + DrawingConstants.StringOffsetForLetters, y - DrawingConstants.SmallTextYOffset);
                    }
                    // steps
                    for (int i = 0; i < row.Steps.Count; i++)
                    {
                        UIStep uiStep = row.Steps[i];
                        int x = DrawingConstants.MediumMargin + DrawingConstants.LeftMargin + (i * DrawingConstants.StepWidth);
                        switch (uiStep.Type)
                        {
                            case UIStepType.Beat:
                                Beat beat = (Beat)uiStep.AssociatedStep;
                                KeyValuePair<int[], int>[] pairs = currentlyHeldStrings.ToArray();
                                foreach (KeyValuePair<int[], int> hold in pairs)
                                {
                                    if (hold.Value > 0)
                                    {
                                        foreach (int heldString in hold.Key)
                                        {
                                            int y = startY + (heldString * DrawingConstants.RowHeight) + DrawingConstants.RowHeight;
                                            g.DrawLine(forePen, x - DrawingConstants.StepWidth / 2F, y, x + DrawingConstants.StepWidth / 2F, y);
                                        }
                                        currentlyHeldStrings[hold.Key]--;
                                        if (currentlyHeldStrings[hold.Key] <= 0)
                                        {
                                            currentlyHeldStrings.Remove(hold.Key);
                                        }
                                    }
                                }
                                if (beat.NoteLength > 1 && beat.HeldCount > 0)
                                {
                                    int[] newHold = new int[beat.HeldCount];
                                    int ii = 0;
                                    foreach (Fret fret in beat)
                                    {
                                        newHold[ii] = fret.String;
                                        ii++;
                                    }
                                    currentlyHeldStrings[newHold] = beat.NoteLength - 1;
                                }
                                foreach (Fret heldFret in beat)
                                {
                                    int y = startY + (heldFret.String * DrawingConstants.RowHeight) + DrawingConstants.RowHeight;
                                    g.FillRectangle(backBrush, x - DrawingConstants.StepWidth / 2F, y - DrawingConstants.RowHeight / 2F, DrawingConstants.StepWidth, DrawingConstants.RowHeight);
                                    Font usedFont = boldFont;
                                    if (heldFret.Space > 9)
                                    {
                                        usedFont = twoDigitFont;
                                    } else
                                    {
                                        usedFont = boldFont;
                                    }
                                    string text = heldFret.Space.ToString();
                                    // MeasureString is not accurate enough for centering on the staff. It returns slightly off values for different DPI settings while MeasureCharacterRanges seems to be off by the same *consistent* amount which is better here
                                    StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                                    stringFormat.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, text.Length) });
                                    Region[] charRanges = g.MeasureCharacterRanges(heldFret.Space.ToString(), usedFont, new RectangleF(0, 0, 100F, 100F), stringFormat);
                                    RectangleF lastCharBounds = charRanges.Last().GetBounds(g);
                                    SizeF textSize = new SizeF(lastCharBounds.Right, lastCharBounds.Bottom);
                                    g.DrawString(text, usedFont, textBrush, x - textSize.Width / 2 + DrawingConstants.FretTextXOffset, y - textSize.Height / 2);
                                }
                                break;
                            case UIStepType.Comment:
                                break;
                            case UIStepType.MeasureBar:
                                g.DrawLine(backPen, x, startY + DrawingConstants.RowHeight, x, startY + ((stringCount - 1) * DrawingConstants.RowHeight + DrawingConstants.RowHeight));
                                break;
                        }
                        if (uiStep.Highlighted)
                        {
                            g.FillRectangle(higlightBrush, x - DrawingConstants.StepWidth / 2F - 1, startY + DrawingConstants.RowHeight / 2F, DrawingConstants.StepWidth, DrawingConstants.RowHeight * stringCount);
                        }
                        if (uiStep.Selected)
                        {
                            g.FillRectangle(selectionBrush, x - DrawingConstants.StepWidth / 2F - 1, startY + DrawingConstants.RowHeight / 2F, DrawingConstants.StepWidth, DrawingConstants.RowHeight * stringCount);
                        }
                    }
                    // spaces
                    int spaceY = startY + DrawingConstants.RowHeight * (stringCount + 1);
                    List<Point> points = new List<Point>();
                    for (int i = 0; i < row.Steps.Count; i++)
                    {
                        if (row.Steps[i].Type == UIStepType.Beat)
                        {
                            points.Add(new Point(DrawingConstants.MediumMargin + DrawingConstants.LeftMargin + (i * DrawingConstants.StepWidth), spaceY));
                            points.Add(new Point(DrawingConstants.MediumMargin + DrawingConstants.LeftMargin + (i * DrawingConstants.StepWidth), spaceY - DrawingConstants.RowHeight / 2));
                            points.Add(new Point(DrawingConstants.MediumMargin + DrawingConstants.LeftMargin + (i * DrawingConstants.StepWidth), spaceY));
                        }
                    }
                    if (points.Count > 1)
                    {
                        g.DrawLines(forePen, points.ToArray());
                    }
                    startY += tallRowHeight;
                }
            }
        }

        private class UIRow
        {
            public string Head = "";
            public List<UIStep> Steps { get; set; } = new List<UIStep>();
        }
        private class UIStep
        {
            public UIStepType Type { get; set; }
            public Step AssociatedStep { get; set; }
            public bool Highlighted { get; set; } = false;
            public bool Selected { get; set; } = false;
            public UIStep(UIStepType type, Step associatedStep)
            {
                Type = type;
                AssociatedStep = associatedStep;
            }
        }
        private enum UIStepType
        {
            Beat,
            MeasureBar,
            Comment
        }
    }
}
