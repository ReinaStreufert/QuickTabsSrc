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
        public int EighthNotesPerMeasure
        {
            get
            {
                if (T2 == 1)
                {
                    return T1 * 8;
                }else if (T2 == 2)
                {
                    return T1 * 4;
                } else if (T2 == 4)
                {
                    return T1 * 2;
                } else if (T2 == 8)
                {
                    return T1;
                } else if (T2 == 16)
                {
                    return T1 / 2;
                } else
                {
                    return T1 / 4;
                }
            }
        }
        public static bool operator ==(TimeSignature left, TimeSignature right)
        {
            return (left.T1 == right.T1 && left.T2 == right.T2);
        }
        public static bool operator !=(TimeSignature left, TimeSignature right)
        {
            return (left.T1 != right.T1 || left.T2 != right.T2);
        }
    }
}
