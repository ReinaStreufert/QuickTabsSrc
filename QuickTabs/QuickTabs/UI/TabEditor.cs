using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.UI
{
    internal class TabEditor : Control
    {
        private MouseTracker mouseTracker;
        public TabEditor()
        {
            mouseTracker = new MouseTracker(this);
            this.BackColor = Color.FromArgb(0x22, 0x22, 0x22);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

        }
    }
}
