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
    public partial class QuickTabsContextMenu : ContextMenu
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
            setupConfigSection();
            setupPlaybackSection();
            setupHistorySection();
            setupTrackSection();
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
                if (Sections.Contains(trackSection))
                {
                    Sections.Remove(trackSection);
                    changed = true;
                }
                foreach (ShortcutManager.ShortcutController shortcut in historyShortcuts)
                {
                    shortcut.Enabled = false;
                }
            } else
            {
                if (AudioEngine.Enabled && playPause.Selected)
                {
                    playPause.Selected = false;
                    invalOnlyChange = true;
                }
                if (!Sections.Contains(historySection))
                {
                    Sections.Add(historySection);
                    changed = true;
                }
                if (!Sections.Contains(trackSection))
                {
                    Sections.Add(trackSection);
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
                if (findSectionHead(editor.Selection.SelectionStart) > 0)
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
                if (Song.Tracks.Count > 1)
                {
                    if (!removeTrack.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    removeTrack.Selected = true;
                } else
                {
                    if (removeTrack.Selected)
                    {
                        invalOnlyChange = true;
                    }
                    removeTrack.Selected = false;
                }
                if (!trackProperties.Selected)
                {
                    invalOnlyChange = true;
                }
                trackProperties.Selected = true;
                if (checkMultipleSections() || countMeasuresInSection(editor.Selection.SelectionStart) > 1)
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

                if (refreshDivisionButtons())
                {
                    invalOnlyChange = true;
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
                if (removeTrack.Selected)
                {
                    removeTrack.Selected = false;
                    invalOnlyChange = true;
                }
                if (trackProperties.Selected)
                {
                    trackProperties.Selected = false;
                    invalOnlyChange = true;
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
