using QuickTabs.Controls;
using QuickTabs.Forms;
using QuickTabs.Songwriting;

namespace QuickTabs
{
    public partial class Editor : Form
    {
        private QuickTabsContextMenu contextMenu;
        private LogoPanel tabEditorPanel;
        private Controls.TabEditor tabEditor;
        private Controls.ToolMenu toolMenu;
        private Controls.Tools.Fretboard fretboard;

        internal Song song = new Song();
        private bool ignoreSizeEvent = false;
        private FormWindowState lastWindowState = FormWindowState.Normal;
        public Editor()
        {
            InitializeComponent();
            DrawingIcons.LoadAll();
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
            this.Width = 1800;
            this.Height = 1200;
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
            tabEditor.Refresh();
            tabEditor.SizeChanged += TabEditor_SizeChanged;
            this.KeyPreview = true;
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
            contextMenu.Size = new Size(this.ClientSize.Width, 160);
            tabEditorPanel.Location = new Point(0, 160);
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
            toolMenu.Location = new Point(0, this.Height - 320);
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
        private void updateTabPanelHeight()
        {
            tabEditorPanel.Size = new Size(this.ClientSize.Width, this.Height - 160 - 320);
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