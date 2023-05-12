using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal struct TimeSignature
    {
        public int T1 { get; set; }
        public int T2 { get; set; }
        public TimeSignature(int t1, int t2)
        {
            T1 = t1;
            T2 = t2;
        }
    }
}
