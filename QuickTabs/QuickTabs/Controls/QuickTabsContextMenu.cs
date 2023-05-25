using QuickTabs.Controls.Tools;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs.Controls
{
    internal class QuickTabsContextMenu : ContextMenu
    {
        public Song Song { get; set; }
        private TabEditor editor;
        public Editor MainForm { get; set; }
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
        private ContextSection selectionSection;

        private ContextItem removeMeasure;
        private ContextItem removeSection;
        private ContextItem addSection;
        private ContextItem paste;

        private List<ShortcutManager.ShortcutController> measureShortcuts = new List<ShortcutManager.ShortcutController>();
        private List<ShortcutManager.ShortcutController> selectionShortcuts = new List<ShortcutManager.ShortcutController>();

        private List<Beat> clipboard = null;
        public QuickTabsContextMenu()
        {
            Logo = DrawingIcons.QuickTabsLogo;

            fileSection = new ContextSection();
            fileSection.SectionName = "File";
            fileSection.ToggleType = ToggleType.NotTogglable;
            ContextItem open = new ContextItem(DrawingIcons.OpenFile);
            open.Selected = true;
            open.Click += openClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.O, openClick);
            fileSection.AddItem(open);
            ContextItem save = new ContextItem(DrawingIcons.SaveFile);
            save.Selected = true;
            save.Click += saveClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.S, saveClick);
            fileSection.AddItem(save);
            ContextItem saveAs = new ContextItem(DrawingIcons.SaveFileAs);
            saveAs.Selected = true;
            saveAs.Click += saveAsClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.S, saveAsClick);
            fileSection.AddItem(saveAs);
            ContextItem export = new ContextItem(DrawingIcons.Export);
            export.Selected = true;
            export.Click += exportClick;
            fileSection.AddItem(export);
            ContextItem newFile = new ContextItem(DrawingIcons.NewFile);
            newFile.Selected = true;
            newFile.Click += newClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.N, newClick);
            fileSection.AddItem(newFile);
            ContextItem documentProperties = new ContextItem(DrawingIcons.EditDocumentProperties);
            documentProperties.Selected = true;
            fileSection.AddItem(documentProperties);
            Sections.Add(fileSection);

            viewSection = new ContextSection();
            viewSection.SectionName = "View";
            viewSection.ToggleType = ToggleType.Togglable;
            ContextItem fretCounts = new ContextItem(DrawingIcons.Counter);
            fretCounts.Selected = true;
            fretCounts.Click += viewFretCountClick;
            viewSection.AddItem(fretCounts);
            ContextItem dots = new ContextItem(DrawingIcons.Dots);
            dots.Selected = true;
            dots.Click += viewDotsClick;
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

            selectionSection = new ContextSection();
            selectionSection.SectionName = "Selection";
            selectionSection.ToggleType = ToggleType.NotTogglable;
            ContextItem copy = new ContextItem(DrawingIcons.Copy);
            copy.Selected = true;
            copy.Click += copyClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.C, copyClick));
            selectionSection.AddItem(copy);
            paste = new ContextItem(DrawingIcons.Paste);
            paste.Selected = false;
            paste.Click += pasteClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.V, pasteClick));
            selectionSection.AddItem(paste);
            ContextItem shiftLeft = new ContextItem(DrawingIcons.ShiftLeft);
            shiftLeft.Selected = true;
            shiftLeft.Click += shiftLeftClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, Keys.A, shiftLeftClick));
            selectionSection.AddItem(shiftLeft);
            ContextItem shiftRight = new ContextItem(DrawingIcons.ShiftRight);
            shiftRight.Selected = true;
            shiftRight.Click += shiftRightClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, Keys.D, shiftRightClick));
            selectionSection.AddItem(shiftRight);
            ContextItem shiftUp = new ContextItem(DrawingIcons.ShiftUp);
            shiftUp.Selected = true;
            shiftUp.Click += () => { shiftStrings(-1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, Keys.W, () => { shiftStrings(-1); }));
            selectionSection.AddItem(shiftUp);
            ContextItem shiftDown = new ContextItem(DrawingIcons.ShiftDown);
            shiftDown.Selected = true;
            shiftDown.Click += () => { shiftStrings(1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, Keys.S, () => { shiftStrings(1); }));
            selectionSection.AddItem(shiftDown);
            ContextItem fretUp = new ContextItem(DrawingIcons.FretUp);
            fretUp.Selected = true;
            fretUp.Click += () => { shiftFrets(1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, Keys.Oemplus, () => { shiftFrets(1); }));
            selectionSection.AddItem(fretUp);
            ContextItem fretDown = new ContextItem(DrawingIcons.FretDown);
            fretDown.Selected = true;
            fretDown.Click += () => { shiftFrets(-1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Shift, (Keys)189, () => { shiftFrets(-1); }));
            selectionSection.AddItem(fretDown);
            ContextItem clear = new ContextItem(DrawingIcons.Clear);
            clear.Selected = true;
            clear.Click += clearClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.Space, clearClick));
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.X, () => { copyClick(); clearClick(); }));
            selectionSection.AddItem(clear);

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
                if (!Sections.Contains(selectionSection))
                {
                    Sections.Add(selectionSection);
                }
                foreach (ShortcutManager.ShortcutController shortcut in selectionShortcuts)
                {
                    shortcut.Enabled = true;
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
                if (clipboard != null)
                {
                    paste.Selected = true;
                } else
                {
                    paste.Selected = false;
                }
            } else
            {
                if (Sections.Contains(measureSection))
                {
                    Sections.Remove(measureSection);
                }
                if (Sections.Contains(selectionSection))
                {
                    Sections.Remove(selectionSection);
                }
                foreach (ShortcutManager.ShortcutController shortcut in selectionShortcuts)
                {
                    shortcut.Enabled = false;
                }
            }
            updateUI();
            Invalidate();
        }
        private void saveClick()
        {
            FileManager.Save(Song);
            MainForm.Cursor = Cursors.WaitCursor;
            Timer t = new Timer();
            t.Interval = 150;
            t.Tick += (object sender, EventArgs e) =>
            {
                MainForm.Cursor = Cursors.Default;
                t.Stop();
                t.Dispose();
            };
            t.Start();
        }
        private void saveAsClick()
        {
            FileManager.SaveAs(Song);
        }
        private void openClick()
        {
            Song openedSong = FileManager.Open();
            if (openedSong == null)
            {
                return;
            }
            Song = openedSong;
            MainForm.song = Song;
            editor.Song = Song;
            Fretboard.Song = Song;
            editor.Refresh();
            editor.Selection = new Selection(1, 1);
            editor.Refresh();
            Fretboard.Refresh();
        }
        private void newClick()
        {
            FileManager.New();
            Song.Tab.SetLength(17);
            ((SectionHead)Song.Tab[0]).Name = "Untitled Section";
            for (int i = 1; i < 17; i++)
            {
                Song.Tab[i] = new Beat();
            }
            editor.Selection = new Selection(1, 1);
            editor.Refresh();
            Fretboard.Refresh();
        }
        private void exportClick()
        {
            PlainTextTabWriter exporter = new PlainTextTabWriter(Song.Tab);
            string exportText = exporter.WriteTab();
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "txt";
                DialogResult saveResult = saveDialog.ShowDialog();
                if (saveResult == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, exportText);
                }
            }
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
        private void viewDotsClick()
        {
            Fretboard.ViewDots = !Fretboard.ViewDots;
            Fretboard.Refresh();
        }
        private void viewFretCountClick()
        {
            Fretboard.ViewFretCounter = !Fretboard.ViewFretCounter;
            Fretboard.Refresh();
        }
        private void copyClick()
        {
            clipboard = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    clipboard.Add(((Beat)Song.Tab[i]).Copy());
                }
            }
            if (!paste.Selected)
            {
                paste.Selected = true;
                updateUI();
                Invalidate();
            }
        }
        private int copyBeats(List<Beat> source, Selection destination) // return value: end index 
        {
            int indexInSource = 0;
            int copied = 0;
            int i;
            for (i = destination.SelectionStart; copied < destination.SelectionLength; i++)
            {
                if (i < 1)
                {
                    indexInSource++;
                    copied++;
                    if (indexInSource >= source.Count)
                    {
                        indexInSource = 0;
                    }
                    continue;
                }
                if (i >= Song.Tab.Count)
                {
                    break;
                }
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Song.Tab[i] = source[indexInSource].Copy();
                    indexInSource++;
                    copied++;
                    if (indexInSource >= source.Count)
                    {
                        indexInSource = 0;
                    }
                }
            }
            return i;
        }
        private int countBeatsInSelection(Selection selection)
        {
            int count = 0;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    count++;
                }
            }
            return count;
        }
        private void pasteClick()
        {
            if (clipboard == null)
            {
                return;
            }
            Selection selection = editor.Selection;
            if (selection.SelectionLength == 1 && clipboard.Count > 1)
            {
                selection.SelectionLength = clipboard.Count;
            } else
            {
                selection.SelectionLength = countBeatsInSelection(selection);
            }
            int endIndex = copyBeats(clipboard, selection);
            editor.Selection = new Selection(selection.SelectionStart, endIndex - selection.SelectionStart);
            editor.Refresh();
            Fretboard.Refresh();
        }
        private void clearClick()
        {
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Song.Tab[i] = new Beat();
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
        }
        private void shiftLeftClick()
        {
            List<Beat> selectionBeats = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    selectionBeats.Add((Beat)(Song.Tab[i]));
                }
            }
            int additive = 1;
            if (Song.Tab[selection.SelectionStart - additive].Type == Enums.StepType.SectionHead && selection.SelectionStart > 1)
            {
                additive = 2;
            }
            int endIndex = copyBeats(selectionBeats, new Selection(selection.SelectionStart - additive, selectionBeats.Count));
            if (Song.Tab[endIndex].Type == Enums.StepType.SectionHead)
            {
                endIndex++;
            }
            Song.Tab[endIndex] = new Beat();
            int newSelectionStart = selection.SelectionStart - additive;
            if (newSelectionStart < 1)
            {
                newSelectionStart = 1;
            }
            if (endIndex - newSelectionStart > 0)
            {
                editor.Selection = new Selection(newSelectionStart, endIndex - newSelectionStart);
            } else
            {
                editor.Selection = null;
            }
            editor.Refresh();
            Fretboard.Refresh();
        }
        private void shiftRightClick()
        {
            List<Beat> selectionBeats = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    selectionBeats.Add((Beat)(Song.Tab[i]));
                }
            }
            int additive = 1;
            if (selection.SelectionStart + additive < Song.Tab.Count && Song.Tab[selection.SelectionStart + additive].Type == Enums.StepType.SectionHead)
            {
                additive = 2;
            }
            int endIndex = copyBeats(selectionBeats, new Selection(selection.SelectionStart + additive, selectionBeats.Count));
            Song.Tab[selection.SelectionStart] = new Beat();
            int newSelectionStart = selection.SelectionStart + additive;
            if (newSelectionStart >= Song.Tab.Count)
            {
                editor.Selection = null;
            } else
            {
                editor.Selection = new Selection(newSelectionStart, endIndex - newSelectionStart);
            }
            editor.Refresh();
            Fretboard.Refresh();
        }
        private void shiftStrings(int additive)
        {
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Beat srcBeat = (Beat)(Song.Tab[i]);
                    Beat newBeat = new Beat();
                    newBeat.NoteLength = srcBeat.NoteLength;
                    foreach (Fret fret in srcBeat)
                    {
                        int newString = fret.String + additive;
                        if (newString >= 0 && newString < Song.Tab.Tuning.Count)
                        {
                            newBeat[new Fret(newString, fret.Space)] = true;
                        }
                    }
                    Song.Tab[i] = newBeat;
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
        }
        public void shiftFrets(int additive)
        {
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Beat srcBeat = (Beat)(Song.Tab[i]);
                    Beat newBeat = new Beat();
                    newBeat.NoteLength = srcBeat.NoteLength;
                    foreach (Fret fret in srcBeat)
                    {
                        int newSpace = fret.Space + additive;
                        if (newSpace >= 0)
                        {
                            newBeat[new Fret(fret.String, newSpace)] = true;
                        }
                    }
                    Song.Tab[i] = newBeat;
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
        }
    }
}
