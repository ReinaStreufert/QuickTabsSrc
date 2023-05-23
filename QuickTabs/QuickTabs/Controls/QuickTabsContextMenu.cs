using QuickTabs.Controls.Tools;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class QuickTabsContextMenu : ContextMenu
    {
        public Song Song { get; set; }
        private TabEditor editor;
        public TabEditor Editor
        {
            get
            {
                return editor;
            }
            set
            {
                editor = value;
                value.SelectionChanged += selectionChanged;
            }
        }
        public Fretboard Fretboard { get; set; }

        private ContextSection fileSection;
        private ContextSection viewSection;
        private ContextSection measureSection;
        private ContextItem removeMeasure;
        private ContextItem removeSection;
        private ContextItem addSection;
        public QuickTabsContextMenu()
        {
            Logo = DrawingIcons.QuickTabsLogo;

            fileSection = new ContextSection();
            fileSection.SectionName = "File";
            fileSection.ToggleType = ToggleType.NotTogglable;
            ContextItem open = new ContextItem(DrawingIcons.OpenFile);
            open.Selected = true;
            fileSection.AddItem(open);
            ContextItem save = new ContextItem(DrawingIcons.SaveFile);
            save.Selected = true;
            fileSection.AddItem(save);
            ContextItem saveAs = new ContextItem(DrawingIcons.SaveFileAs);
            saveAs.Selected = true;
            fileSection.AddItem(saveAs);
            ContextItem newFile = new ContextItem(DrawingIcons.NewFile);
            newFile.Selected = true;
            fileSection.AddItem(newFile);
            ContextItem editMetadata = new ContextItem(DrawingIcons.EditMetadata);
            editMetadata.Selected = true;
            fileSection.AddItem(editMetadata);
            Sections.Add(fileSection);

            viewSection = new ContextSection();
            viewSection.SectionName = "View";
            viewSection.ToggleType = ToggleType.Togglable;
            ContextItem fretCounts = new ContextItem(DrawingIcons.Counter);
            fretCounts.Selected = true;
            viewSection.AddItem(fretCounts);
            ContextItem dots = new ContextItem(DrawingIcons.Dots);
            dots.Selected = true;
            viewSection.AddItem(dots);
            Sections.Add(viewSection);

            measureSection = new ContextSection();
            measureSection.SectionName = "Measure";
            measureSection.ToggleType = ToggleType.NotTogglable;
            ContextItem addMeasure = new ContextItem(DrawingIcons.Plus);
            addMeasure.Selected = true;
            addMeasure.Click += addMeasureClick;
            measureSection.AddItem(addMeasure);
            removeMeasure = new ContextItem(DrawingIcons.Minus);
            removeMeasure.Selected = true;
            removeMeasure.Click += removeMeasureClick;
            measureSection.AddItem(removeMeasure);
            addSection = new ContextItem(DrawingIcons.PlusSection);
            addSection.Selected = true;
            addSection.Click += addSectionClick;
            measureSection.AddItem(addSection);
            removeSection = new ContextItem(DrawingIcons.MinusSection);
            removeSection.Selected = false;
            removeSection.Click += removeSectionClick;
            measureSection.AddItem(removeSection);
            ContextItem renameSection = new ContextItem(DrawingIcons.Rename);
            renameSection.Selected = true;
            renameSection.Click += renameSectionClick;
            measureSection.AddItem(renameSection);

            updateUI();
        }
        private void selectionChanged()
        {
            if (editor.Selection != null)
            {
                if (!Sections.Contains(measureSection))
                {
                    Sections.Add(measureSection);
                }
                if (editor.Selection.SelectionStart - 1 != 0 && Song.Tab[editor.Selection.SelectionStart - 1].Type == Enums.StepType.SectionHead)
                {
                    removeSection.Selected = true;
                } else
                {
                    removeSection.Selected = false;
                }
                if (countBeatsInSection(editor.Selection.SelectionStart) > 8)
                {
                    removeMeasure.Selected = true;
                } else
                {
                    removeMeasure.Selected = false;
                }
                int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart, false);
                if (nextMeasure - 1 < Song.Tab.Count)
                {
                    if (Song.Tab[nextMeasure - 1].Type == Enums.StepType.SectionHead)
                    {
                        addSection.Selected = false;
                    } else
                    {
                        addSection.Selected = true;
                    }
                }
            } else
            {
                if (Sections.Contains(measureSection))
                {
                    Sections.Remove(measureSection);
                }
            }
            updateUI();
            Invalidate();
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
                } else if (Song.Tab[i].Type == Enums.StepType.SectionHead)
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
            int beatCounter = 0;
            for (int i = 0; i < stepIndex; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    beatCounter++;
                    if (beatCounter > 7)
                    {
                        beatCounter = 0;
                    }
                }
            }
            int beatsFromStepIndex = (8 - beatCounter);
            int result = stepIndex;
            while (beatsFromStepIndex > 0)
            {
                result++;
                if (result >= Song.Tab.Count)
                {
                    beatsFromStepIndex--;
                } else
                {
                    if (Song.Tab[result].Type == Enums.StepType.Beat)
                    {
                        beatsFromStepIndex--;
                    } else if (beatsFromStepIndex == 1 && ignoreLastSectionHead)
                    {
                        beatsFromStepIndex--;
                    }
                }
            }
            return result;
        }
        private void addMeasureClick()
        {
            int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart, true);
            if (nextMeasure >= Song.Tab.Count)
            {
                Song.Tab.SetLength(Song.Tab.Count + 8);
            } else
            {
                Song.Tab.InsertBeats(nextMeasure, 8);
            }
            editor.Selection = new Selection(nextMeasure, 1);
            editor.Refresh();
            Fretboard.Refresh();
        }
        private void removeMeasureClick()
        {
            if (countBeatsInSection(editor.Selection.SelectionStart) > 8)
            {
                int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart - 8, false);
                Song.Tab.RemoveBeats(nextMeasure, 8);
                if (nextMeasure - 8 < 0)
                {
                    editor.Selection = new Selection(nextMeasure, 1);
                } else
                {
                    editor.Selection = new Selection(nextMeasure - 8, 1);
                }
                editor.Refresh();
                Fretboard.Refresh();
            }
        }
        private void addSectionClick()
        {
            int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart, false);
            if (nextMeasure >= Song.Tab.Count)
            {
                Song.Tab.SetLength(Song.Tab.Count + 9);
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
            }
        }
        private void renameSectionClick()
        {
            using (SectionName sectionNameForm = new SectionName())
            {
                SectionHead head = (SectionHead)Song.Tab[findSectionHead(editor.Selection.SelectionStart)];
                sectionNameForm.Name = head.Name;
                sectionNameForm.ShowDialog();
                head.Name = sectionNameForm.Name;
                editor.Refresh();
            }
        }
    }
}
