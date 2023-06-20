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
        public TabEditor Editor
        {
            get
            {
                return editor;
            }
            set
            {
                editor = value;
                value.SelectionChanged += updateSections;
            }
        }
        public Fretboard Fretboard { get; set; }

        // these buttons need their own members because their Selected value gets updated
        private ContextItem removeMeasure;
        private ContextItem removeSection;
        private ContextItem addSection;
        private ContextItem paste;
        private ContextItem undo;
        private ContextItem redo;
        private ContextItem redoAlternate;
        private ContextItem playPause;
        private ContextItem repeat;
        private ContextItem metronome;

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
        private void updateSections()
        {
            bool changed = false;
            bool invalOnlyChange = false;
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
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
            if (editor.Selection != null && (tabPlayer == null || !tabPlayer.IsPlaying))
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
                int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
                if (countBeatsInSection(editor.Selection.SelectionStart) > beatsPerMeasure)
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
                int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart, false);
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
    }
}
