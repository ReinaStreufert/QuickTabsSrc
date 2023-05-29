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
        private ContextSection historySection;
        private ContextSection playbackSection;

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

        private List<ShortcutManager.ShortcutController> measureShortcuts = new List<ShortcutManager.ShortcutController>();
        private List<ShortcutManager.ShortcutController> selectionShortcuts = new List<ShortcutManager.ShortcutController>();

        private List<Beat> clipboard = null;
        private Synthesization.TabPlayer tabPlayer = null;
        public QuickTabsContextMenu()
        {
            Logo = DrawingIcons.QuickTabsLogo;

            History.StateChange += historyStateChanged;

            fileSection = new ContextSection();
            fileSection.SectionName = "File";
            fileSection.ToggleType = ToggleType.NotTogglable;
            ContextItem newFile = new ContextItem(DrawingIcons.Reload, "New");
            newFile.Selected = true;
            newFile.Click += newClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.N, newClick);
            fileSection.AddItem(newFile);
            ContextItem open = new ContextItem(DrawingIcons.OpenFile, "Open...");
            open.Selected = true;
            open.Click += openClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.O, openClick);
            fileSection.AddItem(open);
            ContextItem save = new ContextItem(DrawingIcons.SaveFile, "Save");
            save.Selected = true;
            save.Click += saveClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.S, saveClick);
            fileSection.AddItem(save);
            ContextItem saveAs = new ContextItem(DrawingIcons.SaveFileAs, "Save as...");
            saveAs.Selected = true;
            saveAs.Click += saveAsClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.S, saveAsClick);
            fileSection.AddItem(saveAs);
            ContextItem export = new ContextItem(DrawingIcons.Export, "Export plain text...");
            export.Selected = true;
            export.Click += exportClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.E, exportClick);
            fileSection.AddItem(export);
            ContextItem documentProperties = new ContextItem(DrawingIcons.EditDocumentProperties, "Document properties...");
            documentProperties.Selected = true;
            documentProperties.Click += documentPropertiesClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.D, documentPropertiesClick);
            fileSection.AddItem(documentProperties);
            Sections.Add(fileSection);

            viewSection = new ContextSection();
            viewSection.SectionName = "View";
            viewSection.ToggleType = ToggleType.Togglable;
            ContextItem fretCounts = new ContextItem(DrawingIcons.Counter, "Fret counter");
            fretCounts.Selected = true;
            fretCounts.Click += viewFretCountClick;
            viewSection.AddItem(fretCounts);
            ContextItem dots = new ContextItem(DrawingIcons.Dots, "Fret navigation dots");
            dots.Selected = true;
            dots.Click += viewDotsClick;
            viewSection.AddItem(dots);
            Sections.Add(viewSection);

            historySection = new ContextSection();
            historySection.SectionName = "History";
            historySection.ToggleType = ToggleType.NotTogglable;
            undo = new ContextItem(DrawingIcons.Undo, "Undo");
            undo.Selected = false;
            undo.Click += undoClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.Z, undoClick);
            historySection.AddItem(undo);
            redo = new ContextItem(DrawingIcons.Redo, "Redo");
            redo.Selected = false;
            redo.Click += redoClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.Y, redoClick);
            historySection.AddItem(redo);
            redoAlternate = new ContextItem(DrawingIcons.RedoAlternate, "Alternate redo");
            redoAlternate.Selected = false;
            redoAlternate.Click += redoAlternateClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.Y, redoAlternateClick);
            historySection.AddItem(redoAlternate);
            Sections.Add(historySection);

            playbackSection = new ContextSection();
            playbackSection.SectionName = "Player";
            if (AudioEngine.Enabled)
            {
                playbackSection.ToggleType = ToggleType.Togglable;
                playPause = new ContextItem(DrawingIcons.PlayPause, "Play/pause");
                playPause.Selected = false;
                playPause.Click += () => { playPauseClick(false); };
                ShortcutManager.AddShortcut(Keys.None, Keys.Space, () => { playPauseClick(true); });
                playbackSection.AddItem(playPause);
                repeat = new ContextItem(DrawingIcons.Repeat, "Repeat");
                repeat.Selected = false;
                repeat.Click += repeatClick;
                playbackSection.AddItem(repeat);
                metronome = new ContextItem(DrawingIcons.Metronome, "Metronome");
                metronome.Selected = false;
                metronome.Click += metronomeClick;
                playbackSection.AddItem(metronome);
            } else
            {
                playbackSection.ToggleType = ToggleType.NotTogglable;
                ContextItem downloadAsio = new ContextItem(DrawingIcons.Download, "Install ASIO driver...");
                downloadAsio.Selected = true;
                playbackSection.AddItem(downloadAsio);
                ContextItem recheckAsio = new ContextItem(DrawingIcons.Reload, "Check for driver again...");
                recheckAsio.Selected = true;
                playbackSection.AddItem(recheckAsio);
            }
            Sections.Add(playbackSection);

            measureSection = new ContextSection();
            measureSection.SectionName = "Measure";
            measureSection.ToggleType = ToggleType.NotTogglable;
            ContextItem addMeasure = new ContextItem(DrawingIcons.AddMeasure, "Add measure");
            addMeasure.Selected = true;
            addMeasure.Click += addMeasureClick;
            measureSection.AddItem(addMeasure);
            removeMeasure = new ContextItem(DrawingIcons.RemoveMeasure, "Remove measure");
            removeMeasure.Selected = true;
            removeMeasure.Click += removeMeasureClick;
            measureSection.AddItem(removeMeasure);
            addSection = new ContextItem(DrawingIcons.AddSection, "Add or split section");
            addSection.Selected = true;
            addSection.Click += addSectionClick;
            measureSection.AddItem(addSection);
            removeSection = new ContextItem(DrawingIcons.RemoveSection, "Collate section");
            removeSection.Selected = false;
            removeSection.Click += removeSectionClick;
            measureSection.AddItem(removeSection);
            ContextItem renameSection = new ContextItem(DrawingIcons.Rename, "Rename section...");
            renameSection.Selected = true;
            renameSection.Click += renameSectionClick;
            measureSection.AddItem(renameSection);

            selectionSection = new ContextSection();
            selectionSection.SectionName = "Selection";
            selectionSection.ToggleType = ToggleType.NotTogglable;
            ContextItem copy = new ContextItem(DrawingIcons.Copy, "Copy");
            copy.Selected = true;
            copy.Click += copyClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.C, copyClick));
            selectionSection.AddItem(copy);
            paste = new ContextItem(DrawingIcons.Paste, "Paste");
            paste.Selected = false;
            paste.Click += pasteClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.V, pasteClick));
            selectionSection.AddItem(paste);
            ContextItem shiftLeft = new ContextItem(DrawingIcons.ShiftLeft, "Shift beats left");
            shiftLeft.Selected = true;
            shiftLeft.DontCloseDropdown = true;
            shiftLeft.Click += shiftLeftClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.A, shiftLeftClick));
            selectionSection.AddItem(shiftLeft);
            ContextItem shiftRight = new ContextItem(DrawingIcons.ShiftRight, "Shift beats right");
            shiftRight.Selected = true;
            shiftRight.DontCloseDropdown = true;
            shiftRight.Click += shiftRightClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.D, shiftRightClick));
            selectionSection.AddItem(shiftRight);
            ContextItem shiftUp = new ContextItem(DrawingIcons.ShiftUp, "Shift strings up");
            shiftUp.Selected = true;
            shiftUp.DontCloseDropdown = true;
            shiftUp.Click += () => { shiftStrings(-1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.W, () => { shiftStrings(-1); }));
            selectionSection.AddItem(shiftUp);
            ContextItem shiftDown = new ContextItem(DrawingIcons.ShiftDown, "Shift strings down");
            shiftDown.Selected = true;
            shiftDown.DontCloseDropdown = true;
            shiftDown.Click += () => { shiftStrings(1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.S, () => { shiftStrings(1); }));
            selectionSection.AddItem(shiftDown);
            ContextItem fretUp = new ContextItem(DrawingIcons.Plus, "Shift frets up");
            fretUp.Selected = true;
            fretUp.DontCloseDropdown = true;
            fretUp.Click += () => { shiftFrets(1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.Oemplus, () => { shiftFrets(1); }));
            selectionSection.AddItem(fretUp);
            ContextItem fretDown = new ContextItem(DrawingIcons.Minus, "Shift frets down");
            fretDown.Selected = true;
            fretDown.DontCloseDropdown = true;
            fretDown.Click += () => { shiftFrets(-1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, (Keys)189, () => { shiftFrets(-1); }));
            selectionSection.AddItem(fretDown);
            ContextItem clear = new ContextItem(DrawingIcons.Clear, "Clear beats");
            clear.Selected = true;
            clear.Click += clearClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.None, Keys.Delete, clearClick));
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.X, () => { copyClick(); clearClick(); }));
            selectionSection.AddItem(clear);

            ShortcutManager.AddShortcut(Keys.None, Keys.X, silencePressed);
            ShortcutManager.AddShortcut(Keys.None, Keys.A, () => { setRelativeSelection(-1); });
            ShortcutManager.AddShortcut(Keys.None, Keys.D, () => { setRelativeSelection(1); });
            ShortcutManager.AddShortcut(Keys.Shift, Keys.A, () => { lengthenSelection(-1); });
            ShortcutManager.AddShortcut(Keys.Shift, Keys.D, () => { lengthenSelection(1); });

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
                int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
                if (countBeatsInSection(editor.Selection.SelectionStart) > beatsPerMeasure)
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
        private void historyStateChanged()
        {
            bool changed = false;
            if (History.CanUndo != undo.Selected)
            {
                changed = true;
                undo.Selected = History.CanUndo;
            }
            if (History.CanRedo != redo.Selected)
            {
                changed = true;
                redo.Selected = History.CanRedo;
            }
            if (History.CanAlternateRedo != redoAlternate.Selected)
            {
                changed = true;
                redoAlternate.Selected = History.CanAlternateRedo;
            }
            if (changed)
            {
                this.Invalidate();
            }
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
            if (!FileManager.IsSaved)
            {
                using (UnsavedChanges unsavedChanges = new UnsavedChanges())
                {
                    unsavedChanges.Verb = "open a new file";
                    unsavedChanges.ShowDialog();
                    if (!unsavedChanges.Continue)
                    {
                        return;
                    }
                }
            }
            bool failed;
            Song openedSong = FileManager.Open(out failed);
            if (openedSong == null)
            {
                if (failed)
                {
                    using (GenericMessage errMessage = new GenericMessage())
                    {
                        errMessage.Text = "Could not open file";
                        errMessage.Message = "File format was not valid qtjson";
                        errMessage.ShowDialog(MainForm);
                    }
                }
                return;
            }
            History.ClearHistory();
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Stop();
            }
            Song = openedSong;
            editor.QuietlySelect(new Selection(1, 1));
            MainForm.song = Song;
            editor.Song = Song;
            Fretboard.Song = Song;
            Fretboard.Refresh();
            editor.Refresh();
            editor.Selection = new Selection(1, 1);
            History.PushState(Song, editor.Selection, false);
        }
        private void newClick()
        {
            if (!FileManager.IsSaved)
            {
                using (UnsavedChanges unsavedChanges = new UnsavedChanges())
                {
                    unsavedChanges.Verb = "start a new tab";
                    unsavedChanges.ShowDialog();
                    if (!unsavedChanges.Continue)
                    {
                        return;
                    }
                }
            }
            FileManager.New();
            History.ClearHistory();
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Stop();
            }
            Song.Name = "Untitled tab";
            Song.Tempo = 120;
            Song.TimeSignature = new TimeSignature(4, 4);
            Song.Tab.Tuning = Tuning.StandardGuitar;
            Song.Tab.SetLength(17);
            ((SectionHead)Song.Tab[0]).Name = "Untitled Section";
            for (int i = 1; i < 17; i++)
            {
                Song.Tab[i] = new Beat();
            }
            editor.Selection = new Selection(1, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection, false);
        }
        private void exportClick()
        {
            /*PlainTextTabWriter exporter = new PlainTextTabWriter(Song.Tab);
            string exportText = exporter.WriteTab(Song.TimeSignature);
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "txt";
                DialogResult saveResult = saveDialog.ShowDialog();
                if (saveResult == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, exportText);
                }
            }*/
            using (ExportPlainText exportPlainText = new ExportPlainText())
            {
                exportPlainText.Song = Song;
                exportPlainText.ShowDialog();
            }
        }
        private void documentPropertiesClick()
        {
            bool changed;
            using (DocumentProperties dp = new DocumentProperties())
            {
                dp.Song = Song;
                dp.ShowDialog();
                changed = dp.ChangesSaved;
            }
            if (changed)
            {
                if (editor.Selection.SelectionStart + editor.Selection.SelectionLength >= Song.Tab.Count)
                {
                    editor.Selection = new Selection(1, 1);
                }
                editor.Refresh();
                Fretboard.Refresh();
                History.PushState(Song, editor.Selection);
            }
        }
        private void undoClick()
        {
            if (!History.CanUndo)
            {
                return;
            }
            Selection newSelection;
            History.Undo(Song, out newSelection);
            editor.QuietlySelect(newSelection);
            Fretboard.Refresh();
            editor.Refresh();
            editor.Selection = newSelection;
        }
        private void redoClick()
        {
            if (!History.CanRedo)
            {
                return;
            }
            Selection newSelection;
            History.Redo(Song, out newSelection);
            editor.QuietlySelect(newSelection);
            Fretboard.Refresh();
            editor.Refresh();
            editor.Selection = newSelection;
        }
        private void redoAlternateClick()
        {
            if (!History.CanAlternateRedo)
            {
                return;
            }
            Selection newSelection;
            History.RedoAlternate(Song, out newSelection);
            editor.QuietlySelect(newSelection);
            Fretboard.Refresh();
            editor.Refresh();
            editor.Selection = newSelection;
        }
        private void playPauseClick(bool fromShortcut) // this whole fromShortcut bullshit is because if you click the button directly, ContextMenu will already toggle it after this gets called. but if this is called from the shortcut, it will not.
        {
            if (tabPlayer == null || !tabPlayer.IsPlaying)
            {
                tabPlayer = new Synthesization.TabPlayer(Song.Tab);
                tabPlayer.BPM = Song.Tempo;
                tabPlayer.Loop = repeat.Selected;
                tabPlayer.MetronomeTimeSignature = Song.TimeSignature;
                tabPlayer.Metronome = metronome.Selected;
                if (editor.Selection == null)
                {
                    tabPlayer.Position = 1;
                } else
                {
                    tabPlayer.Position = editor.Selection.SelectionStart;
                }
                editor.PlayMode = true;
                tabPlayer.Start();
                Timer t = new Timer();
                t.Interval = 50;
                t.Tick += (object sender, EventArgs e) =>
                {
                    if (tabPlayer.IsPlaying)
                    {
                        int playerPosition = tabPlayer.Position;
                        if (editor.Selection == null || editor.Selection.SelectionStart != playerPosition)
                        {
                            editor.Selection = new Selection(tabPlayer.Position, 1);
                            editor.Refresh();
                        }
                    }
                    else
                    {
                        if (playPause.Selected)
                        {
                            playPause.Selected = false;
                            this.Invalidate();
                        }
                        if (editor.PlayMode)
                        {
                            editor.PlayMode = false;
                            editor.Invalidate();
                        }
                        t.Stop();
                        t.Dispose();
                    }
                };
                t.Start();
                if (fromShortcut && !playPause.Selected)
                {
                    playPause.Selected = true;
                    this.Invalidate();
                }
            } else
            {
                tabPlayer.Stop();
                if (fromShortcut && playPause.Selected)
                {
                    playPause.Selected = false;
                    this.Invalidate();
                }
            }
        }
        private void repeatClick()
        {
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Loop = !tabPlayer.Loop;
            }
        }
        private void metronomeClick()
        {
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Metronome = !tabPlayer.Metronome;
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
            int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
            int nextMeasure = findNextMeasureAlignedStepIndex(editor.Selection.SelectionStart, true);
            if (nextMeasure >= Song.Tab.Count)
            {
                Song.Tab.SetLength(Song.Tab.Count + beatsPerMeasure);
            } else
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
                } else
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
                head.Name = sectionNameForm.Name;
                editor.Refresh();
            }
            History.PushState(Song, editor.Selection);
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
            History.PushState(Song, editor.Selection);
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
            History.PushState(Song, editor.Selection);
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
            History.PushState(Song, editor.Selection);
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
            History.PushState(Song, editor.Selection);
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
            History.PushState(Song, editor.Selection);
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
            History.PushState(Song, editor.Selection);
        }
        private void silencePressed()
        {
            if (tabPlayer == null || !tabPlayer.IsPlaying)
            {
                Action eventHandler = null;
                eventHandler = () =>
                {
                    AudioEngine.SilenceAll();
                    AudioEngine.Tick -= eventHandler;
                };
                AudioEngine.Tick += eventHandler;
                // because threads
            }
        }
        private void setRelativeSelection(int direction)
        {
            if (editor.Selection != null)
            {
                Selection selection = editor.Selection;
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
                editor.Selection = new Selection(newStart, 1);
                editor.Refresh();
            }
        }
        private void lengthenSelection(int direction)
        {
            if (editor.Selection != null)
            {
                Selection selection = editor.Selection;
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
                } else if (direction > 0)
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
                } else
                {
                    return;
                }
                editor.Selection = new Selection(newStart, newLength);
                editor.Refresh();
            }
        }
    }
}
