using QuickTabs.Controls;
using QuickTabs.Songwriting;

namespace QuickTabs
{
    public partial class Editor : Form
    {
        private Controls.ContextMenu contextMenu = new Controls.ContextMenu();
        private Controls.TabEditor tabEditor = new Controls.TabEditor();
        private Controls.ToolMenu toolMenu = new Controls.ToolMenu();
        private Controls.Tools.Fretboard fretboard = new Controls.Tools.Fretboard();

        private Song song = new Song();
        public Editor()
        {
            InitializeComponent();
            DrawingIcons.LoadAll();
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
            contextMenu.Location = new Point(0, 0);
            contextMenu.Size = new Size(this.Width, 160);
            tabEditor.Location = new Point(0, 160);
            tabEditor.Size = new Size(this.Width, this.Height - 160 - 270);
            toolMenu.Location = new Point(0, this.Height - 270);
            toolMenu.Size = new Size(this.Width, 270);
            fretboard.Location = toolMenu.Location;
            fretboard.Size = toolMenu.Size;
        }
    }
}