using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class ContextMenu : Control
    {
        public List<ContextSection> Sections = new List<ContextSection>();
        public ContextMenu()
        {
            this.BackColor = Color.FromArgb(0x0, 0x0, 0x0);
        }
    }
    internal class ContextSection
    {
        public string SectionName { get; set; }
        public ToggleType ToggleType { get; set; }
        public List<ContextItem> Items { get; set; }
    }
    internal class ContextItem
    {
        public delegate void ContextItemClick();

        public MultiColorBitmap Icon { get; set; }
        public bool Selected { get; set; }
        public event ContextItemClick Click;

        public ContextItem(MultiColorBitmap icon)
        {
            Icon = icon;
        }
    }
    enum ToggleType
    {
        NotTogglable,
        Togglable,
        Radio
    }
}
