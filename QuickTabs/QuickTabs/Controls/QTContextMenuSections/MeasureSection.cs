using QuickTabs.Configuration;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection measureSection;
        private ContextSection divisionSubsection;
        private ContextItem removeMeasure;
        private ContextItem removeSection;
        private ContextItem addSection;
        private TimeSignature currentDivisionTimeSignature = new TimeSignature(0, 0);
        private MusicalTimespan currentSelectedDivisionButton;

        private void setupMeasureSection()
        {
            measureSection = new ContextSection();
            measureSection.SectionName = "Measure";
            measureSection.ToggleType = ToggleType.NotTogglable;
            ContextItem addMeasure = new ContextItem(DrawingIcons.AddMeasure, "Add measure");
            addMeasure.Selected = true;
            addMeasure.Click += addMeasureClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.None, Keys.Oemplus, addMeasureClick));
            measureSection.AddItem(addMeasure);
            removeMeasure = new ContextItem(DrawingIcons.RemoveMeasure, "Remove measure");
            removeMeasure.Selected = true;
            removeMeasure.Click += removeMeasureClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.None, (Keys)189, removeMeasureClick));
            measureSection.AddItem(removeMeasure);
            ContextItem beatDivision = new ContextItem(DrawingIcons.Division, "Beat division");
            beatDivision.Selected = true;
            beatDivision.DontCloseDropdown = true;
            divisionSubsection = new ContextSection();
            divisionSubsection.SectionName = "";
            divisionSubsection.ToggleType = ToggleType.Radio;
            beatDivision.Submenu = divisionSubsection;
            measureSection.AddItem(beatDivision);
            addSection = new ContextItem(DrawingIcons.AddSection, "Add or split section");
            addSection.Selected = true;
            addSection.Click += addSectionClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, Keys.Oemplus, addSectionClick));
            measureSection.AddItem(addSection);
            removeSection = new ContextItem(DrawingIcons.RemoveSection, "Collate section");
            removeSection.Selected = false;
            removeSection.Click += removeSectionClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, (Keys)189, removeSectionClick));
            measureSection.AddItem(removeSection);
            ContextItem renameSection = new ContextItem(DrawingIcons.Rename, "Rename section...");
            renameSection.Selected = true;
            renameSection.Click += renameSectionClick;
            measureSection.AddItem(renameSection);
        }
        private bool refreshDivisionButtons()
        {
            bool updateRequired = false;
            MusicalTimespan selectionDivision = ((Beat)Song.FocusedTab[editor.Selection.SelectionStart]).BeatDivision;
            if (currentDivisionTimeSignature != Song.TimeSignature || currentSelectedDivisionButton != selectionDivision)
            {
                updateRequired = true;
            }
            if (updateRequired)
            {
                divisionSubsection.ClearItems();
                foreach (NoteLengthTableRow noteLengthInfo in DrawingIcons.NoteLengthValueTable)
                {
                    if (Song.TimeSignature.MeasureLength.IsDivisibleBy(noteLengthInfo.Timespan))
                    {
                        ContextItem item = new ContextItem(noteLengthInfo.NoteLengthIcon, noteLengthInfo.FullName);
                        item.Selected = (noteLengthInfo.Timespan == selectionDivision);
                        item.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) =>
                        {
                            e.Cancel = true;//!setMeasureDivision(noteLengthInfo.Timespan);
                            setMeasureDivision(noteLengthInfo.Timespan);
                            // setMeasureDivision will set the selection ultimately triggering
                            // refreshDivisionButtons again and THEN the click event handler will run
                            // and try to clear selection on all new correct context items and set a now non-added item to be
                            // selected. this was in fact fucking impossible to debug but i figured it out finally.
                        };
                        divisionSubsection.AddItem(item);
                    }
                }
            }
            return updateRequired;
        }
        private int findSectionHead(int stepIndex, Tab tab = null)
        {
            if (tab == null)
            {
                tab = Song.FocusedTab;
            }
            int sectionHead = 0;
            for (int i = stepIndex; i >= 0; i--)
            {
                if (tab[i].Type == Enums.StepType.SectionHead)
                {
                    sectionHead = i;
                    break;
                }
            }
            return sectionHead;
        }
        private bool checkMultipleSections()
        {
            for (int i = 1; i < Song.FocusedTab.Count; i++)
            {
                if (Song.FocusedTab[i].Type == Enums.StepType.SectionHead)
                {
                    return true;
                }
            }
            return false;
        }
        private int countMeasuresInSection(int stepIndex)
        {
            MusicalTimespan measureLength = Song.TimeSignature.MeasureLength;
            MusicalTimespan beatCounter = MusicalTimespan.Zero;
            int sectionHead = findSectionHead(stepIndex);
            int measureCount = 0;
            for (int i = sectionHead + 1; i < Song.FocusedTab.Count; i++)
            {
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)Song.FocusedTab[i];
                    beatCounter += beat.BeatDivision;
                    if (beatCounter >= measureLength)
                    {
                        measureCount++;
                        beatCounter = MusicalTimespan.Zero;
                    }
                } else if (Song.FocusedTab[i].Type == Enums.StepType.SectionHead)
                {
                    return measureCount;
                }
            }
            return measureCount;
        }
        private int findRelativeMeasure(int startPoint, RelativeMeasureMode mode)
        {
            MusicalTimespan measureLength = Song.TimeSignature.MeasureLength;
            MusicalTimespan beatCounter = MusicalTimespan.Zero;
            int lastMeasureStart = -1;
            bool roundForward;
            if (mode == RelativeMeasureMode.ReturnFirstMeasureAfterStartPoint || mode == RelativeMeasureMode.ReturnSectionHeadOrFirstMeasureAfterStartPoint)
            {
                roundForward = true;
            } else
            {
                roundForward = false;
            }
            if (roundForward)
            {
                startPoint++;
            }

            for (int i = 0; i < Song.FocusedTab.Count; i++)
            {
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    if (lastMeasureStart == -1)
                    {
                        lastMeasureStart = i;
                    }
                    Beat beat = (Beat)Song.FocusedTab[i];
                    if (beatCounter >= measureLength)
                    {
                        beatCounter = MusicalTimespan.Zero;
                        lastMeasureStart = i;
                        if (roundForward && i >= startPoint)
                        {
                            return lastMeasureStart;
                        }
                    }
                    if (!roundForward && i == startPoint)
                    {
                        return lastMeasureStart;
                    }
                    beatCounter += beat.BeatDivision;
                } else if (Song.FocusedTab[i].Type == Enums.StepType.SectionHead)
                {
                    if (mode == RelativeMeasureMode.ReturnSectionHeadOrFirstMeasureAfterStartPoint)
                    {
                        if (beatCounter >= measureLength && i >= startPoint)
                        {
                            return i;
                        }
                    } else if (mode == RelativeMeasureMode.ReturnLastMeasureBeforeStartPoint)
                    {
                        if (i == startPoint)
                        {
                            return lastMeasureStart;
                        }
                    }
                }
            }
            if (roundForward)
            {
                return Song.FocusedTab.Count;
            } else
            {
                return lastMeasureStart;
            }
        }
        private enum RelativeMeasureMode
        {
            ReturnLastMeasureBeforeStartPoint,
            ReturnFirstMeasureAfterStartPoint,
            ReturnSectionHeadOrFirstMeasureAfterStartPoint
        }
        private delegate void MirrorCallback(Tab tab, params int[] positions);
        private void mirrorModification(MirrorCallback callback, params int[] positions)
        {
            MusicalTimespan[] timePositions = new MusicalTimespan[positions.Length];
            bool[] shiftToSectionHead = new bool[positions.Length];
            for (int i = 0; i < timePositions.Length; i++)
            {
                int val = positions[i];
                timePositions[i] = Song.FocusedTab.FindIndexTime(val);
                if (val < Song.FocusedTab.Count)
                {
                    shiftToSectionHead[i] = (Song.FocusedTab[val].Type == Enums.StepType.SectionHead);
                } else
                {
                    shiftToSectionHead[i] = false;
                }
            }
            MusicalTimespan throwaway;
            foreach (Track track in Song.Tracks)
            {
                Tab tab = track.Tab;
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = tab.FindClosestBeatIndexToTime(timePositions[i], out throwaway);
                    if (shiftToSectionHead[i])
                    {
                        positions[i]--; // FindIndexTime -> FindClosestBeatIndexToTime will always come back a beat even if the original index was a section head.
                    }
                }
                if (track == Song.FocusedTrack)
                {
                    callback(tab, positions);
                } else
                {
                    callback(tab, positions);
                }
            }
        }
        private void addMeasureClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => addMeasureClick();
        private void addMeasureClick()
        {
            int nextMeasureVal = findRelativeMeasure(editor.Selection.SelectionStart, RelativeMeasureMode.ReturnSectionHeadOrFirstMeasureAfterStartPoint);
            int currentMeasureVal = findRelativeMeasure(editor.Selection.SelectionStart, RelativeMeasureMode.ReturnLastMeasureBeforeStartPoint);
            mirrorModification((Tab tab, int[] args) =>
            {
                int nextMeasure = args[0];
                int currentMeasure = args[1];
                MusicalTimespan division = ((Beat)tab[currentMeasure]).BeatDivision;
                int beatsPerMeasure = Song.TimeSignature.MeasureLength / division;

                if (nextMeasure >= tab.Count)
                {
                    tab.SetLength(tab.Count + beatsPerMeasure, division);
                }
                else
                {
                    tab.InsertBeats(nextMeasure, beatsPerMeasure, division);
                }
            }, nextMeasureVal, currentMeasureVal);
            editor.Selection = new Selection(nextMeasureVal, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void removeMeasureClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => removeMeasureClick();
        private void removeMeasureClick()
        {
            int measuresInSection = countMeasuresInSection(editor.Selection.SelectionStart);
            if (checkMultipleSections() || measuresInSection > 1)
            {
                int currentMeasureVal = findRelativeMeasure(editor.Selection.SelectionStart, RelativeMeasureMode.ReturnLastMeasureBeforeStartPoint);
                mirrorModification((Tab tab, int[] args) =>
                {
                    int currentMeasure = args[0];
                    MusicalTimespan division = ((Beat)tab[currentMeasure]).BeatDivision;
                    int beatsPerMeasure = Song.TimeSignature.MeasureLength / division;
                    if (measuresInSection > 1)
                    {
                        tab.RemoveBeats(currentMeasure, beatsPerMeasure);
                    }
                    else
                    {
                        tab.RemoveBeats(currentMeasure - 1, beatsPerMeasure + 1);
                    }
                }, currentMeasureVal);
                if (currentMeasureVal - 1 < 1)
                {
                    editor.Selection = new Selection(currentMeasureVal, 1);
                }
                else
                {
                    int selectionStart = findRelativeMeasure(currentMeasureVal - 1, RelativeMeasureMode.ReturnLastMeasureBeforeStartPoint);
                    editor.Selection = new Selection(selectionStart, 1);
                }
                editor.Refresh();
                Fretboard.Refresh();
            }
            History.PushState(Song, editor.Selection);
        }
        private void addSectionClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => addSectionClick();
        private void addSectionClick()
        {
            int nextMeasureVal = findRelativeMeasure(editor.Selection.SelectionStart, RelativeMeasureMode.ReturnSectionHeadOrFirstMeasureAfterStartPoint);
            int beatsPerMeasure = Song.TimeSignature.MeasureLength / Song.TimeSignature.DefaultDivision;
            mirrorModification((Tab tab, int[] args) =>
            {
                int nextMeasure = args[0];
                if (nextMeasure >= tab.Count)
                {
                    tab.SetLength(tab.Count + beatsPerMeasure + 1, Song.TimeSignature.DefaultDivision);
                }
                else
                {
                    if (tab[nextMeasure].Type == Enums.StepType.SectionHead)
                    {
                        tab.InsertBeats(nextMeasure, beatsPerMeasure + 1, Song.TimeSignature.DefaultDivision);
                    }
                    else
                    {
                        tab.InsertBeats(nextMeasure, 1, Song.TimeSignature.DefaultDivision);
                    }
                }
                SectionHead sectionHead = new SectionHead();
                sectionHead.Name = "Untitled Section";
                sectionHead.IndexWithinTab = nextMeasure;
                tab[nextMeasure] = sectionHead;
            }, nextMeasureVal);
            editor.Selection = new Selection(nextMeasureVal + 1, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void removeSectionClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => removeSectionClick();
        private void removeSectionClick()
        {
            int sectionHeadVal = findSectionHead(editor.Selection.SelectionStart);
            if (sectionHeadVal > 0)
            {
                mirrorModification((Tab tab, int[] args) =>
                {
                    int selectionStart = args[0];
                    int sectionHead = findSectionHead(selectionStart, tab);
                    tab.RemoveBeats(sectionHead, 1);
                }, editor.Selection.SelectionStart);
                editor.Selection = new Selection(sectionHeadVal, 1);
                editor.Refresh();
                Fretboard.Refresh();
                History.PushState(Song, editor.Selection);
            }
        }
        private void renameSectionClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => renameSectionClick();
        private void renameSectionClick()
        {
            using (SectionName sectionNameForm = new SectionName())
            {
                SectionHead focusedHead = (SectionHead)Song.FocusedTab[findSectionHead(editor.Selection.SelectionStart)];
                sectionNameForm.Name = focusedHead.Name;
                sectionNameForm.ShowDialog();
                string newName = sectionNameForm.Name;
                if (focusedHead.Name != newName)
                {
                    mirrorModification((Tab tab, int[] args) =>
                    {
                        int selectionStart = args[0];
                        SectionHead head = (SectionHead)tab[findSectionHead(selectionStart, tab)];
                        head.Name = newName;
                    }, editor.Selection.SelectionStart);
                    editor.Refresh();
                    History.PushState(Song, editor.Selection);
                }
            }
        }
        private bool setMeasureDivision(MusicalTimespan newDivision)
        {
            if (!Song.TimeSignature.MeasureLength.IsDivisibleBy(newDivision))
            {
                using (GenericMessage message = new GenericMessage())
                {
                    message.Text = "Set measure division";
                    message.Message = "Division is not available in this time signature.";
                    message.ShowDialog();
                    return false;
                }
            }
            int currentMeasure = findRelativeMeasure(editor.Selection.SelectionStart, RelativeMeasureMode.ReturnLastMeasureBeforeStartPoint);
            MusicalTimespan oldDivision = ((Beat)Song.FocusedTab[currentMeasure]).BeatDivision;
            int oldMeasureLength = Song.TimeSignature.MeasureLength / oldDivision;
            int newMeasureLength = Song.TimeSignature.MeasureLength / newDivision;
            float sourceMapFactor = oldDivision.DivideByF(newDivision);
            Beat[] newMeasureBeats = new Beat[newMeasureLength];
            int lostOrMovedSteps = 0;
            for (int i = 0; i < oldMeasureLength; i++)
            {
                bool lossCounted = false;
                int scaledI = (int)Math.Floor(i * sourceMapFactor);
                Beat copy = ((Beat)Song.FocusedTab[currentMeasure + i]).Copy();
                copy.BeatDivision = newDivision;
                MusicalTimespan originalPosition = (oldDivision * i);
                MusicalTimespan newPosition = (newDivision * scaledI);
                if (copy.HeldCount > 0 && (newPosition != originalPosition)) // check if scaledI is precisely the same spot in the measure as it was or if it was moved slightly
                {
                    lostOrMovedSteps++;
                    lossCounted = true;
                }
                if (newMeasureBeats[scaledI] == null || newMeasureBeats[scaledI].HeldCount == 0)
                {
                    newMeasureBeats[scaledI] = copy;
                } else
                {
                    if (copy.HeldCount > 0 && !lossCounted /*dont count the same beat as lossy twice*/)
                    {
                        lostOrMovedSteps++;
                    }
                    if (QTPersistence.Current.ScaleMergeSteps)
                    {
                        Beat inTheWay = newMeasureBeats[scaledI];
                        Beat mergeBeat = new Beat();
                        mergeBeat.BeatDivision = newDivision;
                        bool[] setStrings = new bool[Song.FocusedTab.Tuning.Count];
                        foreach (KeyValuePair<Fret, MusicalTimespan> held in inTheWay)
                        {
                            mergeBeat[held.Key] = held.Value;
                            setStrings[held.Key.String] = true;
                        }
                        foreach (KeyValuePair<Fret, MusicalTimespan> held in copy)
                        {
                            if (setStrings[held.Key.String])
                            {
                                continue;
                            }
                            mergeBeat[held.Key] = held.Value;
                        }
                        newMeasureBeats[scaledI] = mergeBeat;
                    }
                }
            }
            if (lostOrMovedSteps > 0)
            {
                if (QTPersistence.Current.ScaleAskAboutLoss)
                {
                    using (PersistentAreYouSure areYouSure = new PersistentAreYouSure())
                    {
                        areYouSure.Text = "Division scaling";
                        areYouSure.Message = "This action will delete or move " + lostOrMovedSteps + " step(s). Are you sure you would like to continue? This change can always be undone with Ctrl+Z.";
                        areYouSure.ShowDialog();
                        if (!areYouSure.Continue)
                        {
                            return false;
                        } else
                        {
                            if (areYouSure.StopAsking)
                            {
                                QTPersistence.Current.ScaleAskAboutLoss = false;
                                QTPersistence.Current.Save();
                            }
                        }
                    }
                }
            }
            int measureLengthDifference = newMeasureLength - oldMeasureLength;
            if (measureLengthDifference > 0)
            {
                Song.FocusedTab.InsertBeats(currentMeasure, measureLengthDifference, newDivision);
            } else if (measureLengthDifference < 0)
            {
                Song.FocusedTab.RemoveBeats(currentMeasure, -measureLengthDifference);
            }
            for (int i = 0; i < newMeasureLength; i++)
            {
                if (newMeasureBeats[i] != null)
                {
                    Song.FocusedTab[currentMeasure + i] = newMeasureBeats[i];
                } else
                {
                    Beat beat = new Beat();
                    beat.BeatDivision = newDivision;
                    Song.FocusedTab[currentMeasure + i] = beat;
                }
            }
            editor.Selection = new Selection(currentMeasure, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
            return true;
        }
    }
}
