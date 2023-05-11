using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal struct Selection
    {
        public Selection(int Beat, int SelectionLength)
        {
            this.SelectionStart = Beat;
            this.SelectionLength = SelectionLength;
        }
        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }
        
        public bool Contains(int i)
        {
            return (i >= SelectionStart & i < SelectionStart + SelectionLength);
        }
    }
}
