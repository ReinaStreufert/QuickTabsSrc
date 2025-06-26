using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public struct Margins
    {
        public int Left = 0;
        public int Top = 0;
        public int Right = 0;
        public int Bottom = 0;
        public Margins() { }
        public Margins(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
