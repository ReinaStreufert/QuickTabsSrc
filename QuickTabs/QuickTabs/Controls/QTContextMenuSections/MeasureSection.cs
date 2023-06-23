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
    internal partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection measureSection;
        private ContextItem removeMeasure;
        private ContextItem removeSection;
        private ContextItem addSection;
        private ContextItem division16;
        private ContextItem division8;
        private ContextItem division4;
        private ContextItem division2;
        private ContextItem division1;

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
            {
                ContextSection divisionSubsection = new ContextSection();
                divisionSubsection.SectionName = "";
                divisionSubsection.ToggleType = ToggleType.Radio;
                division16 = new ContextItem(DrawingIcons.SixteenthNote, "Sixteenth note");
                division16.Selected = false;
                division16.Click += () => setMeasureDivision(new MusicalTimespan(1, 16));
                divisionSubsection.AddItem(division16);
                division8 = new ContextItem(DrawingIcons.EighthNote, "Eighth note");
                division8.Selected = true;
                division8.Click += () => setMeasureDivision(new MusicalTimespan(1, 8));
                divisionSubsection.AddItem(division8);
                division4 = new ContextItem(DrawingIcons.QuarterNote, "Quarter note");
                division4.Selected = false;
                division4.Click += () => setMeasureDivision(new MusicalTimespan(1, 4));
                divisionSubsection.AddItem(division4);
                division2 = new ContextItem(DrawingIcons.HalfNote, "Half note");
                division2.Selected = false;
                division2.Click += () => setMeasureDivision(new MusicalTimespan(1, 2));
                divisionSubsection.AddItem(division2);
                division1 = new ContextItem(DrawingIcons.WholeNote, "Whole note");
                division1.Selected = false;
                division1.Click += () => setMeasureDivision(new MusicalTimespan(1, 1));
                divisionSubsection.AddItem(division1);
                beatDivision.Submenu = divisionSubsection;
            }
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
        private int findSectionHead(int stepIndex)
        {
            int sectionHead = 0;
            for (int i = stepIndex; i >= 0; i--)
            {
                if (Song.Tab[i].Type == Enums.StepType.SectionHead)
                {
                    sectionHead = i;
                    break;
                }
            }
            return sectionHead;
        }
        /*private int countBeatsInSection(int stepIndex)
        {
            int sectionHead = findSectionHead(stepIndex);
            int beatCount = 0;
            for (int i = sectionHead + 1; i < Song.Tab.Count; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    beatCount++;
                }
                else if (Song.Tab[i].Type == Enums.StepType.SectionHead)
                {
                    return beatCount;
                }
            }
            return beatCount;
        }*/
        private int countMeasuresInSection(int stepIndex)
        {
            MusicalTimespan measureLength = Song.TimeSignature.MeasureLength;
            MusicalTimespan beatCounter = MusicalTimespan.Zero;
            int sectionHead = findSectionHead(stepIndex);
            int measureCount = 0;
            for (int i = sectionHead + 1; i < Song.Tab.Count; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)Song.Tab[i];
                    beatCounter += beat.BeatDivision;
                    if (beatCounter >= measureLength)
                    {
                        measureCount++;
                        beatCounter = MusicalTimespan.Zero;
                    }
                } else if (Song.Tab[i].Type == Enums.StepType.SectionHead)
                {
                    return measureCount;
                }
            }
            return measureCount;
        }
        private int findFirstBeatInMeasure(int startPoint, bool roundForward)
        {
            MusicalTimespan measureLength = Song.TimeSignature.MeasureLength;
            MusicalTimespan beatCounter = MusicalTimespan.Zero;
            int lastMeasureStart = -1;
            if (roundForward)
            {
                startPoint++;
            }

            for (int i = 0; i < Song.Tab.Count; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    if (lastMeasureStart == -1)
                    {
                        lastMeasureStart = i;
                    }
                    Beat beat = (Beat)Song.Tab[i];
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
                }
            }
            return Song.Tab.Count;
        }
        /*private int findNextMeasureAlignedStepIndex(int stepIndex, bool ignoreLastSectionHead)
        {
            if (stepIndex < 0)
            {
                for (int i = 0; i < Song.Tab.Count; i++)
                {
                    if (Song.Tab[i].Type == Enums.StepType.Beat)
                    {
                        return i;
                    }
                }
            }
            //int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
            MusicalTimespan measureLength = Song.TimeSignature.MeasureLength;
            MusicalTimespan beatCounter = MusicalTimespan.Zero;
            for (int i = 0; i < stepIndex; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)Song.Tab[i];
                    beatCounter += beat.BeatDivision;
                    if (beatCounter >= measureLength)
                    {
                        beatCounter = MusicalTimespan.Zero;
                    }
                }
            }
            MusicalTimespan beatsFromStepIndex = (measureLength - beatCounter);
            MusicalTimespan lastDivision = ((Beat)Song.Tab[stepIndex]).BeatDivision;
            int result = stepIndex;
            while (beatsFromStepIndex > MusicalTimespan.Zero)
            {
                result++;
                if (result >= Song.Tab.Count)
                {
                    beatsFromStepIndex -= lastDivision;
                }
                else
                {
                    if (Song.Tab[result].Type == Enums.StepType.Beat)
                    {
                        lastDivision = ((Beat)Song.Tab[result]).BeatDivision;
                        beatsFromStepIndex -= lastDivision;
                    }
                    else if (beatsFromStepIndex == lastDivision && ignoreLastSectionHead)
                    {
                        beatsFromStepIndex -= lastDivision;
                    }
                }
            }
            return result;
        }*/
        private void addMeasureClick()
        {
            int currentMeasure = findFirstBeatInMeasure(editor.Selection.SelectionStart, false);
            MusicalTimespan division = ((Beat)Song.Tab[currentMeasure]).BeatDivision;
            int beatsPerMeasure = Song.TimeSignature.MeasureLength / division;
            int nextMeasure = findFirstBeatInMeasure(editor.Selection.SelectionStart, true);
            if (nextMeasure >= Song.Tab.Count)
            {
                Song.Tab.SetLength(Song.Tab.Count + beatsPerMeasure, division);
            }
            else
            {
                Song.Tab.InsertBeats(nextMeasure, beatsPerMeasure, division);
            }
            editor.Selection = new Selection(nextMeasure, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void removeMeasureClick()
        {
            if (countMeasuresInSection(editor.Selection.SelectionStart) > 1)
            {
                int currentMeasure = findFirstBeatInMeasure(editor.Selection.SelectionStart, false);
                MusicalTimespan division = ((Beat)Song.Tab[currentMeasure]).BeatDivision;
                int beatsPerMeasure = Song.TimeSignature.MeasureLength / division;
                Song.Tab.RemoveBeats(currentMeasure, beatsPerMeasure);
                if (currentMeasure - 1 < 1)
                {
                    editor.Selection = new Selection(currentMeasure, 1);
                }
                else
                {
                    editor.Selection = new Selection(findFirstBeatInMeasure(currentMeasure - 1, false), 1);
                }
                editor.Refresh();
                Fretboard.Refresh();
            }
            History.PushState(Song, editor.Selection);
        }
        private void addSectionClick()
        {
            int nextMeasure = findFirstBeatInMeasure(editor.Selection.SelectionStart, true);
            int beatsPerMeasure = Song.TimeSignature.MeasureLength / Song.TimeSignature.DefaultDivision;
            if (nextMeasure >= Song.Tab.Count)
            {
                Song.Tab.SetLength(Song.Tab.Count + beatsPerMeasure + 1, Song.TimeSignature.DefaultDivision);
            }
            else
            {
                if (Song.Tab[nextMeasure - 1].Type == Enums.StepType.SectionHead)
                {
                    return;
                }
                Song.Tab.InsertBeats(nextMeasure, 1, Song.TimeSignature.DefaultDivision);
            }
            SectionHead sectionHead = new SectionHead();
            sectionHead.Name = "Untitled Section";
            sectionHead.IndexWithinTab = nextMeasure;
            Song.Tab[nextMeasure] = sectionHead;
            editor.Selection = new Selection(nextMeasure + 1, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void removeSectionClick()
        {
            if (editor.Selection.SelectionStart - 1 == 0)
            {
                return;
            }
            if (Song.Tab[editor.Selection.SelectionStart - 1].Type == Enums.StepType.SectionHead)
            {
                Song.Tab.RemoveBeats(editor.Selection.SelectionStart - 1, 1);
                editor.Selection = new Selection(editor.Selection.SelectionStart - 1, 1);
                editor.Refresh();
                Fretboard.Refresh();
                History.PushState(Song, editor.Selection);
            }
        }
        private void renameSectionClick()
        {
            using (SectionName sectionNameForm = new SectionName())
            {
                SectionHead head = (SectionHead)Song.Tab[findSectionHead(editor.Selection.SelectionStart)];
                sectionNameForm.Name = head.Name;
                sectionNameForm.ShowDialog();
                if (head.Name != sectionNameForm.Name)
                {
                    head.Name = sectionNameForm.Name;
                    editor.Refresh();
                    History.PushState(Song, editor.Selection);
                }
            }
        }
        private void setMeasureDivision(MusicalTimespan newDivision)
        {
            int currentMeasure = findFirstBeatInMeasure(editor.Selection.SelectionStart, false);
            MusicalTimespan oldDivision = ((Beat)Song.Tab[currentMeasure]).BeatDivision;
            int oldMeasureLength = Song.TimeSignature.MeasureLength / oldDivision;
            int newMeasureLength = Song.TimeSignature.MeasureLength / newDivision;
            float sourceMapFactor = oldDivision.DivideByF(newDivision);
            Beat[] newMeasureBeats = new Beat[newMeasureLength];
            int lostOrMovedSteps = 0;
            for (int i = 0; i < oldMeasureLength; i++)
            {
                bool lossCounted = false;
                int scaledI = (int)Math.Floor(i * sourceMapFactor);
                Beat copy = ((Beat)Song.Tab[currentMeasure + i]).Copy();
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
                }
            }
            if (lostOrMovedSteps > 0)
            {
                if (QTSettings.Current.ScaleAskAboutLoss)
                {
                    using (PersistentAreYouSure areYouSure = new PersistentAreYouSure())
                    {
                        areYouSure.Text = "Division scaling";
                        areYouSure.Message = "This action will delete or move " + lostOrMovedSteps + " step(s). Are you sure you would like to continue? This change can always be undone with Ctrl+Z.";
                        areYouSure.ShowDialog();
                        if (!areYouSure.Continue)
                        {
                            return;
                        } else
                        {
                            if (areYouSure.StopAsking)
                            {
                                QTSettings.Current.ScaleAskAboutLoss = false;
                                QTSettings.Current.Save();
                            }
                        }
                    }
                }
            }
            int measureLengthDifference = newMeasureLength - oldMeasureLength;
            if (measureLengthDifference > 0)
            {
                Song.Tab.InsertBeats(currentMeasure, measureLengthDifference, newDivision);
            } else if (measureLengthDifference < 0)
            {
                Song.Tab.RemoveBeats(currentMeasure, -measureLengthDifference);
            }
            for (int i = 0; i < newMeasureLength; i++)
            {
                if (newMeasureBeats[i] != null)
                {
                    Song.Tab[currentMeasure + i] = newMeasureBeats[i];
                } else
                {
                    Beat beat = new Beat();
                    beat.BeatDivision = newDivision;
                    beat.SustainTime = newDivision;
                    Song.Tab[currentMeasure + i] = beat;
                }
            }
            editor.Selection = new Selection(currentMeasure, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
    }
}
