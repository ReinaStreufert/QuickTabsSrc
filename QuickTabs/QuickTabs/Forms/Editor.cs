using QuickTabs.Controls;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs
{
    public partial class Editor : Form
    {
        public override Color BackColor { get => DrawingConstants.EmptySpaceBackColor; set => base.BackColor = value; }

        public int ContextMenuHeight { get; set; } = 160;
        public int FretboardHeight { get; set; } = 440;

        private QuickTabsContextMenu contextMenu;
        private LogoPanel tabEditorPanel;
        private Controls.TabEditor tabEditor;
        private Controls.ToolMenu toolMenu;
        private Controls.Tools.Fretboard fretboard;

        internal Song song = new Song();
        private bool ignoreSizeEvent = false;
        private FormWindowState lastWindowState = FormWindowState.Normal;
        private float scale;

        public Editor()
        {
            Debug.WriteLine("test");

            InitializeComponent();
            //DrawingIcons.LoadAll();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            scale = this.DeviceDpi / 192.0F; // scale from 192 not 96 because i designed quicktabs on a 200% scale laptop like some kind of idiot.
            DrawingConstants.Scale(scale);

            FileManager.Initialize();
            tabEditor = new TabEditor();
            toolMenu = new ToolMenu();
            fretboard = new Controls.Tools.Fretboard();
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
            song.Tab.SetLength(17);
            song.TimeSignature = new TimeSignature(4, 4);
            ((SectionHead)song.Tab[0]).Name = "Untitled Section";
            tabEditor.Song = song;
            fretboard.Song = song;
            fretboard.Editor = tabEditor;
            fretboard.Refresh();
            contextMenu.Song = song;
            contextMenu.Editor = tabEditor;
            contextMenu.Fretboard = fretboard;
            contextMenu.MainForm = this;
            tabEditor.Selection = new Selection(1, 1);
            History.PushState(song, tabEditor.Selection, false);
            tabEditor.Refresh();
            FileManager.FileStateChange += FileManager_FileStateChange;
            this.KeyPreview = true;
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
            contextMenu.Size = new Size(this.ClientSize.Width, (int)(ContextMenuHeight * scale));
            tabEditorPanel.Location = new Point(0, (int)(ContextMenuHeight * scale));
            tabEditorPanel.Size = new Size(this.ClientSize.Width, this.Height - (int)(ContextMenuHeight * scale) - (int)(FretboardHeight * scale));
            tabEditor.MaxHeight = tabEditorPanel.Height;
            if (tabEditor.Width == this.ClientSize.Width)
            {
                tabEditor.Refresh();
            } else
            {
                tabEditor.Size = new Size(this.ClientSize.Width, tabEditor.Height);
            }
            toolMenu.Location = new Point(0, this.Height - (int)(FretboardHeight * scale));
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
            if (Updater.WasJustUpdated)
            {
                Timer t = new Timer();
                t.Interval = 200;
                t.Tick += (object sender, EventArgs e) =>
                {
                    t.Stop();
                    ReleaseNotes releaseNotes = new ReleaseNotes();
                    releaseNotes.ShowDialog();
                    t.Dispose();
                };
                t.Start();
            }
            this.Activate();
        }
    }
}