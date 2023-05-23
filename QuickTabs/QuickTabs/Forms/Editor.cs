using QuickTabs.Controls;
using QuickTabs.Songwriting;

namespace QuickTabs
{
    public partial class Editor : Form
    {
        private Controls.ContextMenu contextMenu;
        private Controls.TabEditor tabEditor;
        private Controls.ToolMenu toolMenu;
        private Controls.Tools.Fretboard fretboard;

        private Song song = new Song();
        public Editor()
        {
            InitializeComponent();
            DrawingIcons.LoadAll();
            contextMenu = new QuickTabsContextMenu();
            tabEditor = new TabEditor();
            toolMenu = new ToolMenu();
            fretboard = new Controls.Tools.Fretboard();
            Controls.Add(contextMenu);
            Controls.Add(tabEditor);
            Controls.Add(fretboard);
            Controls.Add(toolMenu);
            this.Width = 1500;
            this.Height = 1200;
            song.Tab.SetLength(17);
            song.TimeSignature = new TimeSignature(4, 4);
            ((SectionHead)song.Tab[0]).Name = "Untitled Section";
            tabEditor.Song = song;
            tabEditor.Selection = new Selection(1, 1);
            tabEditor.Refresh();
            fretboard.Song = song;
            fretboard.Editor = tabEditor;
            fretboard.Refresh();
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
            tabEditor.Location = new Point(0, 160);
            tabEditor.Size = new Size(this.ClientSize.Width, this.Height - 160 - 320);
            toolMenu.Location = new Point(0, this.Height - 320);
            toolMenu.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - toolMenu.Location.Y);
            fretboard.Location = toolMenu.Location;
            fretboard.Size = toolMenu.Size;
        }
    }
}