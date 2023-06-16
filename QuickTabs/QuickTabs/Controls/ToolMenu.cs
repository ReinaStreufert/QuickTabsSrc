using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class ToolMenu : Control
    {
        public ControlCollection Tools { get; set; }
        public List<MultiColorBitmap> Icons { get; set; }

        public ToolMenu()
        {
            this.BackColor = Color.FromArgb(0xFF, 0x1A, 0x1A, 0x1A);
            
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Console.WriteLine(this.Size);
        }
    }
}
