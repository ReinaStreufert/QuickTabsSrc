using QuickTabs.Controls.Tools;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs.Controls
{
    internal partial class QuickTabsContextMenu : ContextMenu
    {
        public Song Song { get; set; }
        private TabEditor editor;
        public Editor EditorForm { get; set; }
        public SequencePlayer SequencePlayer { get; set; }
        public TabEditor Editor
        {
            get
            {
                return editor;
            }
            set
            {
                editor = value;
                value.SelectionChanged += UpdateAvailableContent;
            }
        }
        public Fretboard Fretboard { get; set; }

        // these shortcuts get enabled and disabled
        private List<ShortcutManager.ShortcutController> historyShortcuts = new List<ShortcutManager.ShortcutController>();
        private List<ShortcutManager.ShortcutController> selectionDependentShortcuts = new List<ShortcutManager.ShortcutController>();

        private List<Beat> clipboard = null;

        public QuickTabsContextMenu(Editor mainForm, TabEditor editor, Fretboard fretboard)
        {
            EditorForm = mainForm;
            Editor = editor;
            Fretboard = fretboard;
            Logo = DrawingIcons.QuickTabsLogo;

            setupFileSection();
            setupViewSection();
            setupPlaybackSection();
            setupHistorySection();
            setupMeasureSection();
            setupSelectionSection();

            updateUI();
        }
        public void UpdateAvailableContent()
        {
            bool changed = false;
            bool invalOnlyChange = false;
            if (SequencePlayer.PlayState == Enums.PlayState.Playing)
            {
                if (!playPause.Selected)
                {
                    playPause.Selected = true;
                    invalOnlyChange = true;
                }
                if (Sections.Contains(historySection))
                {
                    Sections.Remove(historySection);
                    changed = true;
                }
                foreach (ShortcutManager.ShortcutController shortcut in historyShortcuts)
                {
                    shortcut.Enabled = false;
                }
            } else
            {
                if (playPause.Selected)
                {
                    playPause.Selected = false;
                    invalOnlyChange = true;
                }
                if (!Sections.Contains(historySection))
                {
                    Sections.Add(historySection);
                    changed = true;
                }
                foreach (ShortcutManager.ShortcutController shortcut in historyShortcuts)
                {
                    shortcut.Enabled = true;
                }
            }
            if (editor.Selection != null && SequencePlayer.PlayState == Enums.PlayState.NotPlaying)
            {
                if (!Sections.Contains(measureSection))
                {
                    Sections.Add(measureSection);
                    changed = true;
                }
                if (!Sections.Contains(selectionSection))
                {
                    Sections.Add(selectionSection);
                    changed = true;
                }
                foreach (ShortcutManager.ShortcutController shortcut in selectionDependentShortcuts)
                {
                    shortcut.Enabled = true;
                }
                if (editor.Selection.SelectionStart - 1 != 0 && Song.Tab[editor.Selection.SelectionStart - 1].Type == Enums.StepType.SectionHead)
                {
                    if (!removeSection.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    removeSection.Selected = true;
                } else
                {
                    if (removeSection.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    removeSection.Selected = false;
                }
                if (countMeasuresInSection(editor.Selection.SelectionStart) > 1)
                {
                    if (!removeMeasure.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    removeMeasure.Selected = true;
                } else
                {
                    if (removeMeasure.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    removeMeasure.Selected = false;
                }
                MusicalTimespan selectedDivision = ((Beat)Song.Tab[editor.Selection.SelectionStart]).BeatDivision;
                updateDivisionButton(division16, new MusicalTimespan(1, 16), selectedDivision, ref invalOnlyChange);
                updateDivisionButton(division8, new MusicalTimespan(1, 8), selectedDivision, ref invalOnlyChange);
                updateDivisionButton(division4, new MusicalTimespan(1, 4), selectedDivision, ref invalOnlyChange);
                updateDivisionButton(division2, new MusicalTimespan(1, 2), selectedDivision, ref invalOnlyChange);
                updateDivisionButton(division1, new MusicalTimespan(1, 1), selectedDivision, ref invalOnlyChange);
                int nextMeasure = findFirstBeatInMeasure(editor.Selection.SelectionStart, true);
                if (nextMeasure - 1 < Song.Tab.Count)
                {
                    if (Song.Tab[nextMeasure - 1].Type == Enums.StepType.SectionHead)
                    {
                        if (addSection.Selected)
                        {
                            invalOnlyChange = true;
                        }
                        addSection.Selected = false;
                    } else
                    {
                        if (!addSection.Selected)
                        {
                            invalOnlyChange = true;
                        }
                        addSection.Selected = true;
                    }
                }
                if (clipboard != null)
                {
                    if (!paste.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    paste.Selected = true;
                } else
                {
                    if (paste.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    paste.Selected = false;
                }
            } else
            {
                if (Sections.Contains(measureSection))
                {
                    Sections.Remove(measureSection);
                    changed = true;
                }
                if (Sections.Contains(selectionSection))
                {
                    Sections.Remove(selectionSection);
                    changed = true;
                }
                foreach (ShortcutManager.ShortcutController shortcut in selectionDependentShortcuts)
                {
                    shortcut.Enabled = false;
                }
            }
            if (changed)
            {
                updateUI();
            }
            if (invalOnlyChange || changed)
            {
                Invalidate();
            }
        }
        private void updateDivisionButton(ContextItem divButton, MusicalTimespan buttonValue, MusicalTimespan selectionValue, ref bool invalOnlyChange)
        {
            if (selectionValue == buttonValue)
            {
                if (!divButton.Selected)
                {
                    divButton.Selected = true;
                    invalOnlyChange = true;
                }
            }
            else
            {
                if (divButton.Selected)
                {
                    divButton.Selected = false;
                    invalOnlyChange = true;
                }
            }
        }
    }
}
