using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickTabs.Songwriting;

namespace QuickTabs
{
    internal class Song
    {
        public string Name;
        public int Tempo { get; set; } = 120;
        public TimeSignature TimeSignature { get; set; } = new TimeSignature(4, 4);
        public Tab Tab { get; set; } = new Tab();
    }
}
