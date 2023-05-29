using QuickTabs.Controls;
using QuickTabs.Forms;
using QuickTabs.Songwriting;

namespace QuickTabs
{
    public partial class Editor : Form
    {
        public int ContextMenuHeight { get; set; } = 160;
        public int FretboardHeight { get; set; } = 350;

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
            InitializeComponent();
            //DrawingIcons.LoadAll();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            scale = this.DeviceDpi / 192.0F; // scale from 192 not 96 because i designed quicktabs on a 200% scale laptop like some kind of idiot.
            DrawingConstants.Scale(scale);

            FileManager.Initialize();
            contextMenu = new QuickTabsContextMenu();
            tabEditor = new TabEditor();
            toolMenu = new ToolMenu();
            fretboard = new Controls.Tools.Fretboard();
            tabEditorPanel = new LogoPanel();
            Controls.Add(contextMenu);
            tabEditorPanel.Controls.Add(tabEditor);
            tabEditorPanel.AutoScroll = true;
            tabEditorPanel.BackColor = Color.FromArgb(0x33, 0x33, 0x33);
            this.BackColor = Color.FromArgb(0x33, 0x33, 0x33);
            tabEditor.Location = new Point(0, 0);
            Controls.Add(fretboard);
            Controls.Add(tabEditorPanel);
            Controls.Add(toolMenu);
            this.Width = (int)(1800 * scale);
            this.Height = (int)(1200 * scale);
            updateTabPanelHeight();
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
            tabEditor.SizeChanged += TabEditor_SizeChanged;
            FileManager.FileStateChange += FileManager_FileStateChange;
            this.KeyPreview = true;
        }
        public void RefreshLayout()
        {
            OnSizeChanged(null);
            updateTabPanelHeight();
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

        private void TabEditor_SizeChanged(object? sender, EventArgs e)
        {
            if (ignoreSizeEvent)
            {
                return;
            }
            ignoreSizeEvent = true;
            updateTabPanelHeight();
            ignoreSizeEvent = false;
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
            tabEditorPanel.Width = this.ClientSize.Width;
            if (this.WindowState != lastWindowState)
            {
                updateTabPanelHeight();
                lastWindowState = this.WindowState;
            } else
            {
                tabEditorPanel.Height = tabEditor.Height;
            }
            updateTabEditorWidth();
            toolMenu.Location = new Point(0, this.Height - (int)(FretboardHeight * scale));
            toolMenu.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - toolMenu.Location.Y);
            fretboard.Location = toolMenu.Location;
            fretboard.Size = toolMenu.Size;
        }
        protected override void OnResizeBegin(EventArgs e)
        {
            base.OnResizeBegin(e);
            tabEditorPanel.Height = tabEditor.Height;
            tabEditorPanel.SuspendPaint = true;
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            updateTabPanelHeight();
            tabEditorPanel.SuspendPaint = false;
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
        private void updateTabPanelHeight()
        {
            tabEditorPanel.Size = new Size(this.ClientSize.Width, this.Height - (int)(ContextMenuHeight*scale) - (int)(FretboardHeight*scale));
            updateTabEditorWidth();
        }
        private void updateTabEditorWidth()
        {
            ignoreSizeEvent = true;
            if (tabEditor.Height > tabEditorPanel.Height)
            {
                tabEditor.Size = new Size(this.ClientSize.Width - SystemInformation.VerticalScrollBarWidth, tabEditor.Height);
            }
            else
            {
                tabEditor.Size = new Size(this.ClientSize.Width, tabEditor.Height);
            }
            ignoreSizeEvent = false;
        }
    }
}