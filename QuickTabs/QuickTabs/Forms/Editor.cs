using QuickTabs.Controls;
using QuickTabs.Data;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using QuickTabs.Synthesization;
using QuickTabs.Configuration;
using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs
{
    public partial class Editor : Form
    {
        public override Color BackColor { get => DrawingConstants.EmptySpaceBackColor; set => base.BackColor = value; }

        public int ContextMenuHeight
        {
            get
            {
                if (QTPersistence.Current.ViewCompactCtxMenu)
                {
                    return DrawingConstants.SmallContextMenuHeight;
                } else
                {
                    return DrawingConstants.LargeContextMenuHeight;
                }
            }
        }
        public int FretboardHeight
        {
            get
            {
                if (QTPersistence.Current.ViewLargeFretboard)
                {
                    return DrawingConstants.LargeFretboardHeight;
                } else
                {
                    return DrawingConstants.SmallFretboardHeight;
                }
            }
        }

        private QuickTabsContextMenu contextMenu;
        private LogoPanel tabEditorPanel;
        private Controls.TabEditor tabEditor;
        private Controls.ToolMenu toolMenu;
        private Fretboard fretboard;
        private Synthesization.SequencePlayer sequencePlayer;
        private Song song = new Song();

        private bool ignoreSizeEvent = false;
        private FormWindowState lastWindowState = FormWindowState.Normal;
        private float scale;

        public Editor()
        {
            InitializeComponent();
            //DrawingIcons.LoadAll();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            scale = this.DeviceDpi / 192.0F; // scale from 192 not 96 because i designed quicktabs on a 200% scale laptop like some kind of idiot.
            DrawingConstants.Scale(scale);
            DrawingConstants.InitializeTheme();

            FileManager.Initialize();
            tabEditor = new TabEditor();
            toolMenu = new ToolMenu();
            fretboard = new Fretboard();
            tabEditorPanel = new LogoPanel();
            contextMenu = new QuickTabsContextMenu(this, tabEditor, fretboard);
            Controls.Add(contextMenu);
            tabEditorPanel.Controls.Add(tabEditor);
            tabEditor.Location = new Point(0, 0);
            Controls.Add(fretboard);
            Controls.Add(tabEditorPanel);
            Controls.Add(toolMenu);
            this.Width = (int)(1800 * scale);
            this.Height = (int)(1200 * scale);
            song.FocusedTrack.UpdateAutoName();
            song.FocusedTab.SetLength(17, new MusicalTimespan(1, 8));
            song.TimeSignature = new TimeSignature(4, 4);
            ((SectionHead)song.FocusedTab[0]).Name = "Untitled Section";
            sequencePlayer = new Synthesization.SequencePlayer(this, song.Tracks);
            tabEditor.Song = song;
            fretboard.Song = song;
            fretboard.Editor = tabEditor;
            fretboard.Refresh();
            contextMenu.Song = song;
            contextMenu.Editor = tabEditor;
            contextMenu.Fretboard = fretboard;
            contextMenu.EditorForm = this;
            contextMenu.SequencePlayer = sequencePlayer;
            if (QTPersistence.Current.ViewCompactCtxMenu)
            {
                contextMenu.Style = Enums.ContextMenuStyle.Collapsed;
            } else
            {
                contextMenu.Style = Enums.ContextMenuStyle.Responsive;
            }
            tabEditor.Selection = new Selection(1, 1);
            History.PushState(song, tabEditor.Selection, false);
            tabEditor.Refresh();
            FileManager.FileStateChange += FileManager_FileStateChange;
            this.KeyPreview = true;
            sequencePlayer.PositionUpdate += SequencePlayer_PositionUpdate;
            sequencePlayer.PlaybackStopped += SequencePlayer_PlaybackStopped;
        }

        public void LoadDocument(Song openedSong, bool loadUnsaved = false)
        {
            if (sequencePlayer.PlayState == Enums.PlayState.Playing)
            {
                sequencePlayer.Stop();
                tabEditor.PlayMode = false;
            }
            contextMenu.Song = openedSong;
            tabEditor.QuietlySelect(new Selection(1, 1));
            this.song = openedSong;
            tabEditor.Song = song;
            fretboard.Song = song;
            sequencePlayer.Tempo = song.Tempo;
            sequencePlayer.MetronomeTimeSignature = song.TimeSignature;
            fretboard.Refresh();
            tabEditor.Refresh();
            tabEditor.RefreshTrackView();
            tabEditor.Selection = new Selection(1, 1);
            History.PushState(song, tabEditor.Selection, loadUnsaved);
        }
        public void LoadRecoveredDocument(CrashManager.CrashReport report)
        {
            QtJsonFormat qtjsonParser = new QtJsonFormat();
            bool failed;
            Song openedSong = qtjsonParser.Open(report.RecoveredSong, out failed);
            if (failed)
            {
                using (GenericMessage message = new GenericMessage())
                {
                    message.Text = "Unsaved data recovery";
                    message.Message = "Failed to recover. Song was saved in an invalid format in the crash log.";
                    message.ShowDialog();
                }
            }
            else
            {
                LoadDocument(openedSong, true);
            }
        }
        public void RefreshLayout()
        {
            OnSizeChanged(null);
            tabEditor.Refresh();
        }
        private void FileManager_FileStateChange()
        {
            string endChar = "";
            string joinString = "";
            string fileString = "";
            if (!FileManager.IsSaved)
            {
                endChar = "*";
            }
            if (FileManager.CurrentFilePath != "")
            {
                joinString = " - ";
                fileString = Path.GetFileNameWithoutExtension(FileManager.CurrentFilePath);
            }
            this.Text = "QuickTabs" + joinString + fileString + endChar;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (contextMenu == null)
            {
                return;
            }
            contextMenu.Location = new Point(0, 0);
            contextMenu.Size = new Size(this.ClientSize.Width, ContextMenuHeight);
            tabEditorPanel.Location = new Point(0, ContextMenuHeight);
            tabEditorPanel.Size = new Size(this.ClientSize.Width, this.Height - ContextMenuHeight - FretboardHeight);
            tabEditor.MaxHeight = tabEditorPanel.Height;
            if (tabEditor.Width == this.ClientSize.Width)
            {
                tabEditor.Refresh();
            } else
            {
                tabEditor.Size = new Size(this.ClientSize.Width, tabEditor.Height);
            }
            toolMenu.Location = new Point(0, this.Height - FretboardHeight);
            toolMenu.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - toolMenu.Location.Y);
            fretboard.Location = toolMenu.Location;
            fretboard.Size = toolMenu.Size;
        }
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
            tabEditorPanel.SuspendLogoDraw = true;
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            tabEditorPanel.SuspendLogoDraw = false;
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            /*DebugOutputForm debugOutput = new DebugOutputForm();
            debugOutput.Output = e.Modifiers + " " + e.KeyCode;
            debugOutput.Show();*/
            ShortcutManager.ProcessShortcut(e.Modifiers, e.KeyCode);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!FileManager.IsSaved)
            {
                using (UnsavedChanges message = new UnsavedChanges())
                {
                    message.Verb = "exit QuickTabs";
                    message.ShowDialog();
                    e.Cancel = !message.Continue;
                }
            }
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (CrashManager.ReportAvailable || Updater.WasJustUpdated)
            {
                Timer t = new Timer();
                t.Interval = 200;
                t.Tick += (object sender, EventArgs e) =>
                {
                    t.Stop();
                    if (CrashManager.ReportAvailable)
                    {
                        using (CrashRecovery crashRecovery = new CrashRecovery())
                        {
                            crashRecovery.ShowDialog();
                            if (crashRecovery.AttemptRecover)
                            {
                                LoadRecoveredDocument(CrashManager.LastCrashReport);
                            }
                        }
                        CrashManager.FlushReport();
                    }
                    if (Updater.WasJustUpdated)
                    {
                        using (ReleaseNotes releaseNotes = new ReleaseNotes())
                        {
                            releaseNotes.ShowDialog();
                        }
                    }
                    t.Dispose();
                };
                t.Start();
            }
            this.Activate();
        }

        private void SequencePlayer_PositionUpdate()
        {
            if (sequencePlayer.PlayState == Enums.PlayState.Playing)
            {
                tabEditor.PlayCursor = sequencePlayer.Position;
                tabEditor.Refresh();
            }
        }
        private void SequencePlayer_PlaybackStopped()
        {
            contextMenu.UpdateAvailableContent();
            if (tabEditor.PlayMode)
            {
                tabEditor.PlayMode = false;
                tabEditor.Invalidate();
            }
            History.PushState(song, tabEditor.Selection, false);
        }
    }
}