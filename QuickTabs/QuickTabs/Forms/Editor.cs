using QuickTabs.Songwriting;

namespace QuickTabs
{
    public partial class Editor : Form
    {
        private Controls.ContextMenu contextMenu = new Controls.ContextMenu();
        private Controls.TabEditor tabEditor = new Controls.TabEditor();
        private Controls.ToolMenu toolMenu = new Controls.ToolMenu();

        private Song song = new Song();
        public Editor()
        {
            InitializeComponent();
            Controls.Add(contextMenu);
            Controls.Add(tabEditor);
            Controls.Add(toolMenu);
            this.Width = 1500;
            this.Height = 1200;
            /*song.Tab.SetLength(99);
            song.Tab[0] = new SectionHead() { Name = "First one!" };
            song.Tab[49] = new SectionHead() { Name = "Test" };
            song.Tab[82] = new SectionHead() { Name = "Another one" };*/
            song.Tab.SetLength(65);
            ((SectionHead)song.Tab[0]).Name = "Untitled Section";
            int currentString = 0;
            for (int i = 1; i < 9; i += 2)
            {
                Beat beat = (Beat)song.Tab[i];
                beat[new Fret(currentString, i)] = true;
                currentString++;
                beat.NoteLength = 2;
                if (currentString >= 6)
                {
                    currentString = 0;
                }
            }
            ((Beat)song.Tab[3])[new Fret(2, 7)] = true;
            tabEditor.Song = song;
            tabEditor.Refresh();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Console.WriteLine("balls?");
            contextMenu.Location = new Point(0, 0);
            contextMenu.Size = new Size(this.Width, 160);
            tabEditor.Location = new Point(0, 160);
            tabEditor.Size = new Size(this.Width, this.Height - 160 - 270);
            toolMenu.Location = new Point(0, this.Height - 270);
            toolMenu.Size = new Size(this.Width, 270);
        }
    }
}