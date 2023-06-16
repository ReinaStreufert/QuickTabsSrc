using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection measureSection;

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
        private int countBeatsInSection(int stepIndex)
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
        }
        private int findNextMeasureAlignedStepIndex(int stepIndex, bool ignoreLastSectionHead)
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
            int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
            int beatCounter = 0;
            for (int i = 0; i < stepIndex; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    beatCounter++;
                    if (beatCounter >= beatsPerMeasure)
                    {
                        beatCounter = 0;
                    }
                }
            }
            int beatsFromStepIndex = (beatsPerMeasure - beatCounter);
            int result = stepIndex;
            while (beatsFromStepIndex > 0)
            {
                result++;
                if (result >= Song.Tab.Count)
                {
                    beatsFromStepIndex--;
                }
                else
                {
                    if (Song.Tab[result].Type == Enums.StepType.Beat)
                    {
                        beatsFromStepIndex--;
                    }
                    else if (beatsFromStepIndex == 1 && ignoreLastSectionHead)
                    {
                        beatsFromStepIndex--;
                    }
                }
            }
            return result;
        }
        private void addMeasureClick()
        {
            int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
            int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart, true);
            if (nextMeasure >= Song.Tab.Count)
            {
                Song.Tab.SetLength(Song.Tab.Count + beatsPerMeasure);
            }
            else
            {
                Song.Tab.InsertBeats(nextMeasure, beatsPerMeasure);
            }
            editor.Selection = new Selection(nextMeasure, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void removeMeasureClick()
        {
            int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
            if (countBeatsInSection(editor.Selection.SelectionStart) > beatsPerMeasure)
            {
                int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart - beatsPerMeasure, false);
                Song.Tab.RemoveBeats(nextMeasure, beatsPerMeasure);
                if (nextMeasure - beatsPerMeasure < 0)
                {
                    editor.Selection = new Selection(nextMeasure, 1);
                }
                else
                {
                    editor.Selection = new Selection(nextMeasure - beatsPerMeasure, 1);
                }
                editor.Refresh();
                Fretboard.Refresh();
            }
            History.PushState(Song, editor.Selection);
        }
        private void addSectionClick()
        {
            int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
            int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart, false);
            if (nextMeasure >= Song.Tab.Count)
            {
                Song.Tab.SetLength(Song.Tab.Count + beatsPerMeasure + 1);
            }
            else
            {
                if (Song.Tab[nextMeasure - 1].Type == Enums.StepType.SectionHead)
                {
                    return;
                }
                Song.Tab.InsertBeats(nextMeasure, 1);
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
    }
}
