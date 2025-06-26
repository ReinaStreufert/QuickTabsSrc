using NAudio.Dsp;
using Newtonsoft.Json.Linq;
using QuickTabs.Configuration;
using QuickTabs.Enums;
using QuickTabs.Songwriting;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public class TabEditor : Control
    {
        public override Color BackColor { get => DrawingConstants.TabEditorBackColor; set => base.BackColor = value; }

        public int MaxHeight { get; set; }
        private Song song { get; set; }
        public Song Song
        {
            get
            {
                return song;
            }
            set
            {
                song = value;
                trackView.Song = value;
            }
        }
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
                selectionChanged = true;
                selection = value;
                SelectionChanged?.Invoke();
            }
        }
        private MusicalTimespan playCursor;
        public MusicalTimespan PlayCursor
        {
            get
            {
                return playCursor;
            }
            set
            {
                playCursor = value;
                MusicalTimespan throwaway;
                selection = new Selection(Song.FocusedTab.FindClosestBeatIndexToTime(value, out throwaway), 1);
                if (scrollbarShown && !autoscrollBroken)
                    scrollSelectionIntoView();
                SelectionChanged?.Invoke();
            }
        }
        public void QuietlySelect(Selection newSelection)
        {
            if (PlayMode)
            {
                return;
            }
            selection = newSelection;
        }
        public event Action SelectionChanged;
        private bool playMode = false;
        public bool PlayMode
        {
            get
            {
                return playMode;
            }
            set
            {
                playMode = value;
                if (value)
                {
                    this.Cursor = Cursors.Default;
                    autoscrollBroken = false;
                }
                else
                {
                    updateUI();
                    Invalidate();
                }
            }
        }

        protected List<UIRow> tabUI = new List<UIRow>();
        private List<NoteEnd> noteEnds = new List<NoteEnd>();
        private UIStep currentlyHighlighted = null;
        private Point selectionStartPoint;
        private Selection oldSelection;
        private Track oldFocusedTrack;
        private NoteDragInfo noteDragInfo = null;
        private bool selectionDrag = false;
        private VScrollBar scrollBar = new VScrollBar();
        private bool scrollbarShown = false;
        private bool selectionChanged = false; // whether the selection has been updated since the last UI update
        private bool autoscrollBroken = false;
        private Dictionary<Step, UIStep> stepDictionary = new Dictionary<Step, UIStep>();
        private TrackView trackView = new TrackView();

        protected virtual bool PrintMode
        {
            get
            {
                return false;
            }
        }
        protected virtual bool PrintFocusedTrackOnly
        {
            get
            {
                return false;
            }
        }

        public TabEditor()
        {
            this.DoubleBuffered = true;
            scrollBar.Scroll += scrollBar_Scroll;
            if (!PrintMode)
            {
                trackView.Location = new Point(0, 0);
                trackView.Size = new Size(DrawingConstants.TrackViewWidth, 0);
                trackView.ParentEditor = this;
                this.Controls.Add(trackView);
            }
            
            ShortcutManager.AddShortcut(Keys.None, Keys.A, () => { setRelativeSelection(-1); });
            ShortcutManager.AddShortcut(Keys.None, Keys.D, () => { setRelativeSelection(1); });
            ShortcutManager.AddShortcut(Keys.Shift, Keys.A, () => { lengthenSelection(-1); });
            ShortcutManager.AddShortcut(Keys.Shift, Keys.D, () => { lengthenSelection(1); });
        }

        protected void updateUI()
        {
            if (Song == null)
            {
                return;
            }
            //Songwriting.Tab tab = Song.FocusedTab;
            tabUI.Clear();
            stepDictionary.Clear();
            int usableWidth = this.Width - DrawingConstants.MediumMargin - DrawingConstants.LeftMargin;
            if (scrollbarShown)
            {
                usableWidth -= SystemInformation.VerticalScrollBarWidth;
            }
            if (!PrintMode)
            {
                usableWidth -= DrawingConstants.TrackViewWidth;
            }
            MusicalTimespan measureLength = Song.TimeSignature.MeasureLength;

            foreach (Track track in Song.Tracks)
            {
                if (PrintMode && PrintFocusedTrackOnly)
                {
                    if (track != Song.FocusedTrack)
                    {
                        continue;
                    }
                }
                Tab tab = track.Tab;
                int rowsPerStaff = tab.Tuning.Count;

                UIRow currentRow = new UIRow();
                currentRow.Track = track;
                MusicalTimespan sinceLastMeasureBar = MusicalTimespan.Zero;
                int stepIndex = 0;
                int playCursorLocation = -1;
                if (playMode)
                {
                    if (track == Song.FocusedTrack)
                    {
                        playCursorLocation = selection.SelectionStart;
                    } else
                    {
                        MusicalTimespan throwaway;
                        playCursorLocation = tab.FindClosestBeatIndexToTime(playCursor, out throwaway);
                    }
                }
                foreach (Step step in tab)
                {
                    if ((currentRow.Steps.Count + 1) * DrawingConstants.StepWidth >= usableWidth)
                    {
                        tabUI.Add(currentRow);
                        currentRow = new UIRow();
                        currentRow.Track = track;
                    }

                    switch (step.Type)
                    {
                        case StepType.SectionHead:
                            SectionHead sectionHead = (SectionHead)step;
                            if (currentRow.Steps.Count > 0)
                            {
                                tabUI.Add(currentRow);
                                currentRow = new UIRow();
                                currentRow.Track = track;
                                currentRow.Head = sectionHead.Name;
                            }
                            else
                            {
                                currentRow.Head = sectionHead.Name;
                            }
                            currentRow.Steps.Add(new UIStep(UIStepType.MeasureBar, null));
                            sinceLastMeasureBar = MusicalTimespan.Zero;
                            break;
                        case StepType.Beat:
                            Beat beat = (Beat)step;
                            UIStep uiStep = new UIStep(UIStepType.Beat, beat);
                            uiStep.Row = currentRow;
                            stepDictionary[beat] = uiStep;
                            if (playMode)
                            {
                                if (stepIndex == playCursorLocation)
                                {
                                    uiStep.Selected = true;
                                }
                            } else if (track == Song.FocusedTrack && Selection != null && Selection.Contains(stepIndex))
                            {
                                uiStep.Selected = true;
                            }
                            currentRow.Steps.Add(uiStep);
                            sinceLastMeasureBar += beat.BeatDivision;
                            break;
                        case StepType.Annotation:
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
                    if (sinceLastMeasureBar >= measureLength)
                    {
                        if ((currentRow.Steps.Count + 2) * DrawingConstants.StepWidth < usableWidth)
                        {
                            currentRow.Steps.Add(new UIStep(UIStepType.MeasureBar, null));
                            sinceLastMeasureBar = MusicalTimespan.Zero;
                        }
                        else
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
                            currentRow.Track = track;
                            currentRow.Steps.Add(new UIStep(UIStepType.MeasureBar, null));
                            sinceLastMeasureBar = MusicalTimespan.Zero;
                        }
                    }

                    stepIndex++;
                }
                tabUI.Add(currentRow);
            }
            if (PrintMode)
            {
                return;
            }
            int[] trackHeights = new int[Song.Tracks.Count];
            int contentHeight = DrawingConstants.MediumMargin;
            trackHeights[0] = DrawingConstants.MediumMargin;
            Track lastRowTrack = Song.Tracks[0];
            int trackHeightsIndex = 0;
            foreach (UIRow row in tabUI)
            {
                int tallRowHeight = tuningRowHeight(row.Track.Tab.Tuning);
                if (row.Track != lastRowTrack)
                {
                    trackHeightsIndex++;
                    lastRowTrack = row.Track;
                    trackHeights[trackHeightsIndex] = tallRowHeight;
                } else
                {
                    trackHeights[trackHeightsIndex] += tallRowHeight;
                }
                contentHeight += tallRowHeight;
            }
            bool trackViewUpdate = false;
            bool trackViewInvalOnlyUpdate = false;
            if (trackView.TrackHeights == null || !trackView.TrackHeights.SequenceEqual(trackHeights))
            {
                trackView.TrackHeights = trackHeights;
                trackViewUpdate = true;
            }
            
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
                    if (trackView.ParentScroll != scrollBar.Value)
                    {
                        trackView.ParentScroll = scrollBar.Value;
                        trackViewInvalOnlyUpdate = true;
                    }
                }
            }
            if (this.Height != trackView.Height)
            {
                trackView.Height = this.Height;
                trackViewUpdate = true;
            }
            if (trackViewUpdate)
            {
                trackView.UpdateUI();
                trackView.Invalidate();
            } else if (trackViewInvalOnlyUpdate)
            {
                trackView.Invalidate();
            }
            if (selectionChanged)
            {
                selectionChanged = false;
                if (scrollbarShown && selection != null)
                {
                    scrollSelectionIntoView();
                }
            }
        }
        protected static int tuningRowHeight(Tuning tuning)
        {
            return DrawingConstants.RowHeight * (tuning.Count + 3);
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
            if (trackView.ParentScroll > 0)
            {
                trackView.ParentScroll = 0;
                trackView.Invalidate();
            }
        }
        private void scrollBar_Scroll(object? sender, ScrollEventArgs e)
        {
            if (scrollbarShown)
            {
                handleUserScroll(e.NewValue);
            }
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
                handleUserScroll(newValue);
            }
        }
        private void handleUserScroll(int newValue)
        {
            scrollBar.Value = newValue;
            if (playMode)
            {
                if (autoscrollBroken)
                {
                    if (isSelectionInView())
                    {
                        autoscrollBroken = false;
                    }
                } else
                {
                    if (!isSelectionInView())
                    {
                        autoscrollBroken = true;
                    }
                }
            }
            this.Invalidate();
            this.Update();
            trackView.ParentScroll = scrollBar.Value;
            trackView.Invalidate();
            trackView.Update(); // force immediate redraw for smooth synced scrolling with the editor
        }
        private bool tryGetNoteEndFromPoint(Point point, out NoteEnd noteEnd)
        {
            point.X -= DrawingConstants.TrackViewWidth;
            if (scrollbarShown)
            {
                point.Y += scrollBar.Value;
            }
            int matchRadius = DrawingConstants.RowHeight / 3;
            foreach (NoteEnd checkNoteEnd in noteEnds)
            {
                int xDist = Math.Abs(checkNoteEnd.Location.X - point.X);
                int yDist = Math.Abs(checkNoteEnd.Location.Y - point.Y);
                if (xDist <= matchRadius && yDist <= matchRadius)
                {
                    noteEnd = checkNoteEnd;
                    return true;
                }
            }
            noteEnd = null;
            return false;
        }
        private bool tryGetStepFromPoint(Point point, out UIStep step)
        {
            point.X -= DrawingConstants.TrackViewWidth;
            if (scrollbarShown)
            {
                point.Y += scrollBar.Value;
            }
            if (point.X < DrawingConstants.MediumMargin + DrawingConstants.LeftMargin)
            {
                step = null;
                return false;
            }
            //int tallRowHeight = tuningRowHeight(Song.FocusedTab.Tuning);
            //int rowIndex = (int)Math.Floor((float)point.Y / tallRowHeight);
            UIRow row = null;
            int currentY = DrawingConstants.MediumMargin;
            for (int i = 0; i < tabUI.Count; i++)
            {
                UIRow checkRow = tabUI[i];
                int rowHeight = tuningRowHeight(checkRow.Track.Tab.Tuning);
                if (point.Y >= currentY && point.Y <= currentY + rowHeight)
                {
                    row = checkRow;
                    break;
                }
                currentY += rowHeight;
            }
            if (row == null)
            {
                step = null;
                return false;
            }
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
            /*foreach (UIRow row in tabUI)
            {
                foreach (UIStep uiStep in row.Steps)
                {
                    if (uiStep.AssociatedStep == step)
                    {
                        return uiStep;
                    }
                }
            }
            throw new IndexOutOfRangeException();*/
            return stepDictionary[step];            
        }
        private bool isSelectionInView()
        {
            int selectionStartRow = rowIndexFromTabStep(Song.FocusedTab[selection.SelectionStart]);
            int selectionEndRow = rowIndexFromTabStep(Song.FocusedTab[selection.SelectionStart + selection.SelectionLength - 1]);
            int selectionStartY = DrawingConstants.MediumMargin - scrollBar.Value;
            for (int i = 0; i < selectionStartRow; i++)
            {
                selectionStartY += tuningRowHeight(tabUI[i].Track.Tab.Tuning);
            }
            int selectionEndY = DrawingConstants.MediumMargin - scrollBar.Value;
            for (int i = 0; i <= selectionEndRow; i++)
            {
                selectionEndY += tuningRowHeight(tabUI[i].Track.Tab.Tuning);
            }
            if (selectionEndY - selectionStartY > this.Height)
            {
                return true;
            }
            if (selectionStartY < 0 || selectionEndY > this.Height)
            {
                return false;
            }
            return true;
        }
        private void scrollSelectionIntoView()
        {
            // detect if any part of the selection is off the screen, if so set scrolling
            selectionChanged = false;
            int selectionStartRow = rowIndexFromTabStep(Song.FocusedTab[selection.SelectionStart]);
            int selectionEndRow = rowIndexFromTabStep(Song.FocusedTab[selection.SelectionStart + selection.SelectionLength - 1]);
            int selectionStartY = DrawingConstants.MediumMargin - scrollBar.Value;
            for (int i = 0; i < selectionStartRow; i++)
            {
                selectionStartY += tuningRowHeight(tabUI[i].Track.Tab.Tuning);
            }
            int selectionEndY = DrawingConstants.MediumMargin - scrollBar.Value;
            for (int i = 0; i <= selectionEndRow; i++)
            {
                selectionEndY += tuningRowHeight(tabUI[i].Track.Tab.Tuning);
            }
            if (selectionEndY - selectionStartY > this.Height)
            {
                return;
            }
            if (selectionStartY < 0)
            {
                scrollBar.Value = scrollBar.Value + selectionStartY;
                if (selectionDrag)
                {
                    selectionStartPoint = new Point(selectionStartPoint.X, selectionStartPoint.Y - selectionStartY);
                }
                this.Invalidate();
            }
            if (selectionEndY > this.Height)
            {
                scrollBar.Value = scrollBar.Value + (selectionEndY - this.Height);
                if (selectionDrag)
                {
                    selectionStartPoint = new Point(selectionStartPoint.X, selectionStartPoint.Y - (selectionEndY - this.Height));
                }
                this.Invalidate();
            }
            trackView.ParentScroll = scrollBar.Value;
            trackView.Invalidate();
        }
        private int rowIndexFromTabStep(Step step)
        {
            for (int i = 0; i < tabUI.Count; i++)
            {
                UIRow row = tabUI[i];
                foreach (UIStep uiStep in row.Steps)
                {
                    if (uiStep.AssociatedStep == step)
                    {
                        return i;
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
                    while (Song.FocusedTab[newStart].Type != Enums.StepType.Beat)
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
                    if (newStart >= Song.FocusedTab.Count)
                    {
                        return;
                    }
                    while (Song.FocusedTab[newStart].Type != Enums.StepType.Beat)
                    {
                        newStart++;
                        if (newStart >= Song.FocusedTab.Count)
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
                    while (Song.FocusedTab[newStart].Type != Enums.StepType.Beat)
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
                    if (newStart + newLength - 1 >= Song.FocusedTab.Count)
                    {
                        return;
                    }
                    while (Song.FocusedTab[newStart + newLength - 1].Type != Enums.StepType.Beat)
                    {
                        newLength++;
                        if (newStart + newLength - 1 >= Song.FocusedTab.Count)
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
            if (e.X < 0 || e.Y < 0)
            {
                return;
            }

            if (noteDragInfo != null)
            {
                if (currentlyHighlighted != null)
                {
                    currentlyHighlighted.Highlighted = false;
                    currentlyHighlighted = null;
                    this.Invalidate();
                }
                if (updateNoteDragFromPoint(e.Location))
                {
                    this.Invalidate();
                }
                return;
            }
            NoteEnd noteEnd;
            if (tryGetNoteEndFromPoint(e.Location, out noteEnd))
            {
                this.Cursor = Cursors.SizeWE;
            } else
            {
                this.Cursor = Cursors.Default;
            }

            UIStep uiStep;
            if (tryGetStepFromPoint(e.Location, out uiStep) && uiStep.Type == UIStepType.Beat)
            {
                if (selectionDrag)
                {
                    if (updateSelectionFromPoints(selectionStartPoint, e.Location))
                    {
                        if (selection != null && scrollbarShown)
                        {
                            scrollSelectionIntoView();
                        }
                        this.Invalidate();
                    }
                }

                if (currentlyHighlighted != null)
                {
                    currentlyHighlighted.Highlighted = false;
                }
                uiStep.Highlighted = true;
                if (currentlyHighlighted != uiStep)
                {
                    currentlyHighlighted = uiStep;
                    this.Invalidate();
                }
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
                int oldValue = scrollBar.Value;
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
            NoteEnd noteEnd;
            if (tryGetNoteEndFromPoint(e.Location, out noteEnd))
            {
                noteDragInfo = new NoteDragInfo();
                noteDragInfo.Beat = noteEnd.Beat;
                noteDragInfo.Fret = noteEnd.Fret;
                noteDragInfo.Track = noteEnd.Track;
                noteDragInfo.OriginX = e.Location.X;
                noteDragInfo.OriginSustainTime = noteEnd.Beat[noteEnd.Fret];
            } else
            {
                selectionStartPoint = e.Location;
                oldSelection = selection;
                oldFocusedTrack = song.FocusedTrack;
                selectionDrag = true;
            }
            this.Focus(); // closes context menu dropdown
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (noteDragInfo != null)
            {
                this.Cursor = Cursors.Default;
                SelectionChanged?.Invoke();
                if (noteDragInfo.OriginSustainTime != noteDragInfo.Beat[noteDragInfo.Fret])
                {
                    History.PushState(Song, selection);
                }
                noteDragInfo = null;
            } else if (selectionDrag)
            {
                selectionDrag = false;
                if (PlayMode)
                {
                    return;
                }
                if (updateSelectionFromPoints(selectionStartPoint, e.Location))
                {
                    if (scrollbarShown && selection != null)
                    {
                        scrollSelectionIntoView();
                    }
                    this.Invalidate();
                }
                if (oldFocusedTrack != Song.FocusedTrack)
                {
                    History.PushState(Song, selection, false);
                    SelectionChanged?.Invoke();
                } else
                {
                    if (selection == null || oldSelection == null)
                    {
                        if (selection != oldSelection)
                        {
                            History.PushState(Song, selection, false);
                            SelectionChanged?.Invoke();
                        }
                    }
                    else
                    {
                        if (selection.SelectionStart != oldSelection.SelectionStart || selection.SelectionLength != oldSelection.SelectionLength)
                        {
                            History.PushState(Song, selection, false);
                            SelectionChanged?.Invoke();
                        }
                    }
                }
                if (selection != null && selection.SelectionLength == 1 && QTPersistence.Current.EnablePreviewPlay)
                {
                    BeatPlayer beatPlayer = new BeatPlayer((Beat)(Song.FocusedTab[selection.SelectionStart]), Song.FocusedTrack);
                    beatPlayer.BPM = Song.Tempo;
                    beatPlayer.Tuning = Song.FocusedTab.Tuning;
                    beatPlayer.Start();
                }
            }
        }
        private Beat findPreviousBeat(Beat beat)
        {
            Beat lastBeat = beat;
            foreach (Step step in Song.FocusedTab)
            {
                if (step.Type == StepType.Beat)
                {
                    Beat enumBeat = (Beat)step;
                    if (beat == enumBeat)
                    {
                        return lastBeat;
                    } else
                    {
                        lastBeat = enumBeat;
                    }
                }
            }
            throw new IndexOutOfRangeException("beat not found in tab");
        }
        private bool updateNoteDragFromPoint(Point p) // output is whether anything changed
        {
            UIStep dragDestStep;
            if (tryGetStepFromPoint(p, out dragDestStep) && dragDestStep.Type == UIStepType.Beat)
            {
                if (dragDestStep.Row.Track != noteDragInfo.Track)
                {
                    return false;
                }
                Tab tab = noteDragInfo.Track.Tab;
                Beat dragDestBeat = (Beat)dragDestStep.AssociatedStep;
                MusicalTimespan originTimePosition = tab.FindBeatTime(noteDragInfo.Beat, 0);
                MusicalTimespan dragTimePosition = tab.FindBeatTime(dragDestBeat, 1);
                MusicalTimespan suggestedLength = (dragTimePosition - originTimePosition);// + (findPreviousBeat(dragDestBeat).BeatDivision) + (dragDestBeat.BeatDivision / 2);
                MusicalTimespan newLength;
                if (suggestedLength < noteDragInfo.Beat.BeatDivision)
                {
                    newLength = noteDragInfo.Beat.BeatDivision;
                } else
                {
                    newLength = suggestedLength;
                }
                MusicalTimespan currentValue = noteDragInfo.Beat[noteDragInfo.Fret];
                if (currentValue == newLength)
                {
                    return false;
                }
                noteDragInfo.Beat[noteDragInfo.Fret] = newLength;
                if (Control.ModifierKeys.HasFlag(Keys.Shift))
                {
                    MusicalTimespan difference = newLength - currentValue;
                    for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
                    {
                        if (Song.FocusedTab[i].Type == StepType.Beat)
                        {
                            Beat transformBeat = (Beat)Song.FocusedTab[i];
                            foreach (KeyValuePair<Fret, MusicalTimespan> note in transformBeat)
                            {
                                if (transformBeat == noteDragInfo.Beat && note.Key == noteDragInfo.Fret)
                                {
                                    continue;
                                }
                                MusicalTimespan transformedValue = note.Value + difference;
                                if (transformedValue < transformBeat.BeatDivision)
                                {
                                    transformedValue = transformBeat.BeatDivision;
                                }
                                transformBeat[note.Key] = transformedValue;
                            }
                        }
                    }
                }
                return true;
            } else
            {
                return false;
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
                    if (i < Song.FocusedTab.Count)
                    {
                        if (Song.FocusedTab[i].Type == StepType.Beat)
                        {
                            uiStepFromTabStep(Song.FocusedTab[i]).Selected = false;
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
                if (earliestStep.Row.Track != latestStep.Row.Track)
                {
                    return false;
                }
                Track lOldFocusedTrack = Song.FocusedTrack;
                Song.FocusedTrack = earliestStep.Row.Track;
                for (int i = earliestStep.AssociatedStep.IndexWithinTab; i <= latestStep.AssociatedStep.IndexWithinTab; i++)
                {
                    if (Song.FocusedTab[i].Type == StepType.Beat)
                    {
                        UIStep uiStep = uiStepFromTabStep(Song.FocusedTab[i]);
                        uiStep.Selected = true;
                    }
                }
                Selection lOldSelection = Selection;
                selection = new Selection(earliestStep.AssociatedStep.IndexWithinTab, latestStep.AssociatedStep.IndexWithinTab - earliestStep.AssociatedStep.IndexWithinTab + 1);
                if (lOldSelection == null)
                {
                    return true;
                }
                if (lOldFocusedTrack != song.FocusedTrack)
                {
                    return true;
                }
                if (lOldSelection.SelectionStart == Selection.SelectionStart && lOldSelection.SelectionLength == Selection.SelectionLength)
                {
                    return false;
                }
                return true;
            } else
            {
                selection = null;
                return !selectionWasNull;
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            updateUI();
            Invalidate();
        }
        public void RefreshTrackView()
        {
            trackView.UpdateUI();
            trackView.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            if (!PrintMode)
            {
                int translateY = 0;
                if (scrollbarShown)
                {
                    translateY = -scrollBar.Value;
                }
                g.TranslateTransform(DrawingConstants.TrackViewWidth, translateY);
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
            using (Pen fadedPen = new Pen(DrawingConstants.HighlightColor, DrawingConstants.PenWidth * 2))
            using (Pen backPen = new Pen(DrawingConstants.ContrastColor, DrawingConstants.PenWidth))
            using (Pen forePen = new Pen(DrawingConstants.HighlightBlue, DrawingConstants.BoldPenWidth))
            using (Pen seperatorPen = new Pen(DrawingConstants.FadedGray, DrawingConstants.SeperatorLineWidth))
            using (Font boldFont = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font twoDigitFont = new Font(DrawingConstants.Montserrat, DrawingConstants.TwoDigitTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
            {
                int startX = DrawingConstants.MediumMargin;
                int startY = DrawingConstants.MediumMargin;

                List<HeldString> currentlyHeldStrings = new List<HeldString>();
                noteEnds.Clear();
                MusicalTimespan midlineCounter = MusicalTimespan.Zero;
                MusicalTimespan midlineInterval = QTPersistence.Current.MidlineInterval;
                Track lastTrack = tabUI[0].Track;
                foreach (UIRow row in tabUI)
                {
                    int rowWidth = DrawingConstants.LeftMargin + (row.Steps.Count * DrawingConstants.StepWidth);
                    Tuning tuning = row.Track.Tab.Tuning;
                    int stringCount = tuning.Count;

                    // seperator
                    if (row.Track != lastTrack)
                    {
                        g.DrawLine(seperatorPen, 0, startY, this.Width, startY);
                        if (currentlyHeldStrings.Count > 0)
                        {
                            foreach (HeldString heldString in currentlyHeldStrings)
                            {
                                NoteEnd noteEnd = new NoteEnd();
                                noteEnd.Fret = heldString.Fret;
                                noteEnd.Beat = heldString.Beat;
                                noteEnd.Location = new Point(heldString.LastEndX, heldString.LastEndY);
                                noteEnd.Track = lastTrack;
                                noteEnds.Add(noteEnd);
                            }
                            currentlyHeldStrings.Clear();
                        }
                        lastTrack = row.Track;
                    }

                    // heading
                    if (row.Head != "")
                    {
                        g.DrawString(row.Head, boldFont, textBrush, startX, startY + DrawingConstants.RowHeight - DrawingConstants.SmallTextYOffset);
                    }

                    // strings
                    for (int i = 0; i < stringCount; i++)
                    {
                        int x = startX + DrawingConstants.StringOffsetForLetters;
                        int y = startY + (i * DrawingConstants.RowHeight) + DrawingConstants.RowHeight * 2;
                        g.DrawLine(backPen, x, y, (startX + rowWidth) - (DrawingConstants.StepWidth), y);
                        SizeF textSize = g.MeasureString(tuning[i], boldFont);
                        g.DrawString(tuning[i], boldFont, textBrush, startX - textSize.Width + DrawingConstants.StringOffsetForLetters, y - DrawingConstants.SmallTextYOffset);
                    }
                    // steps
                    float selectRectStart = -1;
                    int selectRectLength = 0;
                    for (int i = 0; i < row.Steps.Count; i++)
                    {
                        UIStep uiStep = row.Steps[i];
                        int x = DrawingConstants.MediumMargin + DrawingConstants.LeftMargin + (i * DrawingConstants.StepWidth);
                        switch (uiStep.Type)
                        {
                            case UIStepType.Beat:
                                Beat beat = (Beat)uiStep.AssociatedStep;
                                if (QTPersistence.Current.ViewMidLines && midlineCounter.IsDivisibleBy(midlineInterval))
                                {
                                    g.DrawLine(fadedPen, x, startY + DrawingConstants.RowHeight * 2, x, startY + ((stringCount - 1) * DrawingConstants.RowHeight + DrawingConstants.RowHeight * 2));
                                }
                                midlineCounter += beat.BeatDivision;
                                HeldString[] pairs = currentlyHeldStrings.ToArray();
                                foreach (HeldString hold in pairs)
                                {
                                    int heldString = hold.Fret.String;
                                    int y = startY + (heldString * DrawingConstants.RowHeight) + DrawingConstants.RowHeight * 2;
                                    if (hold.TimeLeft < beat.BeatDivision)
                                    {
                                        NoteEnd noteEnd = new NoteEnd();
                                        noteEnd.Fret = hold.Fret;
                                        noteEnd.Beat = hold.Beat;
                                        noteEnd.Track = row.Track;
                                        noteEnd.Location = new Point(hold.LastEndX, hold.LastEndY);
                                        noteEnds.Add(noteEnd);
                                        currentlyHeldStrings.Remove(hold);
                                        continue;
                                    }
                                    g.DrawLine(forePen, x - DrawingConstants.StepWidth / 2F, y, x + DrawingConstants.StepWidth / 2F, y);
                                    hold.TimeLeft -= beat.BeatDivision;
                                    hold.LastEndX = (int)Math.Round(x + DrawingConstants.StepWidth / 2F);
                                    hold.LastEndY = y;
                                }
                                foreach (KeyValuePair<Fret,MusicalTimespan> heldFret in beat)
                                {
                                    int y = startY + (heldFret.Key.String * DrawingConstants.RowHeight) + DrawingConstants.RowHeight * 2;
                                    g.FillRectangle(backBrush, x - DrawingConstants.StepWidth / 2F, y - DrawingConstants.RowHeight / 2F, DrawingConstants.StepWidth, DrawingConstants.RowHeight);
                                    Font usedFont = boldFont;
                                    if (heldFret.Key.Space > 9)
                                    {
                                        usedFont = twoDigitFont;
                                    } else
                                    {
                                        usedFont = boldFont;
                                    }
                                    string text = heldFret.Key.Space.ToString();
                                    // MeasureString is not accurate enough for centering on the staff. It returns slightly off values for different DPI settings while MeasureCharacterRanges seems to be off by the same *consistent* amount which is better here
                                    StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                                    stringFormat.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(0, text.Length) });
                                    Region[] charRanges = g.MeasureCharacterRanges(heldFret.Key.Space.ToString(), usedFont, new RectangleF(0, 0, 100F, 100F), stringFormat);
                                    RectangleF lastCharBounds = charRanges.Last().GetBounds(g);
                                    SizeF textSize = new SizeF(lastCharBounds.Right, lastCharBounds.Bottom);
                                    g.DrawString(text, usedFont, textBrush, x - textSize.Width / 2 + DrawingConstants.FretTextXOffset, y - textSize.Height / 2);
                                    if (heldFret.Value > beat.BeatDivision)
                                    {
                                        HeldString heldString = new HeldString();
                                        heldString.TimeLeft = heldFret.Value - beat.BeatDivision;
                                        heldString.Beat = beat;
                                        heldString.Fret = heldFret.Key;
                                        heldString.LastEndX = (int)Math.Round(x + DrawingConstants.StepWidth / 2F);
                                        heldString.LastEndY = y;
                                        currentlyHeldStrings.Add(heldString);
                                    } else
                                    {
                                        NoteEnd noteEnd = new NoteEnd();
                                        noteEnd.Fret = heldFret.Key;
                                        noteEnd.Beat = beat;
                                        noteEnd.Track = row.Track;
                                        noteEnd.Location = new Point((int)Math.Round(x + DrawingConstants.StepWidth / 2F), y);
                                        noteEnds.Add(noteEnd);
                                    }
                                }
                                break;
                            case UIStepType.Comment:
                                break;
                            case UIStepType.MeasureBar:
                                midlineCounter = MusicalTimespan.Zero;
                                g.DrawLine(backPen, x, startY + DrawingConstants.RowHeight * 2, x, startY + ((stringCount - 1) * DrawingConstants.RowHeight + DrawingConstants.RowHeight * 2));
                                break;
                        }
                        if (uiStep.Highlighted)
                        {
                            g.FillRectangle(higlightBrush, x - DrawingConstants.StepWidth / 2F - 1, startY + DrawingConstants.RowHeight * 1.5F, DrawingConstants.StepWidth, DrawingConstants.RowHeight * stringCount);
                        }
                        if (uiStep.Selected)
                        {
                            if (selectRectStart < 0)
                            {
                                selectRectStart = x - DrawingConstants.StepWidth / 2F - 1;
                                selectRectLength = 1;
                            } else
                            {
                                selectRectLength++;
                            }
                            //g.FillRectangle(selectionBrush, x - DrawingConstants.StepWidth / 2F - 1, startY + DrawingConstants.RowHeight / 2F, DrawingConstants.StepWidth, DrawingConstants.RowHeight * stringCount);
                        } else if (selectRectStart > 0)
                        {
                            g.FillRectangle(selectionBrush, selectRectStart, startY + DrawingConstants.RowHeight * 1.5F, DrawingConstants.StepWidth * selectRectLength, DrawingConstants.RowHeight * stringCount);
                            selectRectStart = -1;
                        }
                    }
                    if (selectRectStart > 0)
                    {
                        g.FillRectangle(selectionBrush, selectRectStart, startY + DrawingConstants.RowHeight * 1.5F, DrawingConstants.StepWidth * selectRectLength, DrawingConstants.RowHeight * stringCount);
                        selectRectStart = -1;
                        selectRectLength = 0;
                    }
                    // spaces
                    int spaceY = startY + DrawingConstants.RowHeight * (stringCount + 2);
                    int leftSpaceX = -1;
                    int rightSpaceX = -1;
                    for (int i = 0; i < row.Steps.Count; i++)
                    {
                        int x = DrawingConstants.MediumMargin + DrawingConstants.LeftMargin + (i * DrawingConstants.StepWidth);
                        if (row.Steps[i].Type == UIStepType.Beat)
                        {
                            g.DrawLine(forePen, x, spaceY + DrawingConstants.PenWidth / 2F, x, spaceY - DrawingConstants.RowHeight / 2);
                            if (leftSpaceX == -1)
                            {
                                leftSpaceX = x;
                            }
                            rightSpaceX = x;
                        }
                    }
                    g.DrawLine(forePen, leftSpaceX - DrawingConstants.PenWidth / 2F, spaceY, rightSpaceX + DrawingConstants.PenWidth / 2F, spaceY);
                    startY += tuningRowHeight(row.Track.Tab.Tuning);
                }
                if (currentlyHeldStrings.Count > 0)
                {
                    foreach (HeldString heldString in currentlyHeldStrings)
                    {
                        NoteEnd noteEnd = new NoteEnd();
                        noteEnd.Fret = heldString.Fret;
                        noteEnd.Beat = heldString.Beat;
                        noteEnd.Location = new Point(heldString.LastEndX, heldString.LastEndY);
                        noteEnd.Track = lastTrack;
                        noteEnds.Add(noteEnd);
                    }
                    currentlyHeldStrings.Clear();
                }
            }
        }

        protected class UIRow
        {
            public string Head = "";
            public List<UIStep> Steps { get; set; } = new List<UIStep>();
            public Track Track { get; set; }
        }
        protected class UIStep
        {
            public UIStepType Type { get; set; }
            public Step AssociatedStep { get; set; }
            public UIRow Row { get; set; }
            public bool Highlighted { get; set; } = false;
            public bool Selected { get; set; } = false;
            public UIStep(UIStepType type, Step associatedStep)
            {
                Type = type;
                AssociatedStep = associatedStep;
            }
        }
        private class HeldString
        {
            public Fret Fret { get; set; }
            public MusicalTimespan TimeLeft { get; set; }
            public Beat Beat { get; set; }
            public int LastEndX { get; set; }
            public int LastEndY { get; set; }
        }
        private class NoteEnd
        {
            public Fret Fret { get; set; }
            public Point Location { get; set; }
            public Beat Beat { get; set; }
            public Track Track { get; set; }
        }
        private class NoteDragInfo
        {
            public Beat Beat { get; set; }
            public Fret Fret { get; set; }
            public MusicalTimespan OriginSustainTime { get; set; }
            public int OriginX { get; set; }
            public Track Track { get; set; }
        }
        protected enum UIStepType
        {
            Beat,
            MeasureBar,
            Comment
        }
    }
}
