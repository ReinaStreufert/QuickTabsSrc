using NAudio.Wave;
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
        public MusicalTimespan MeasureLength
        {
            get
            {
                return new MusicalTimespan(T1, T2);
            }
        }
        public MusicalTimespan DefaultDivision
        {
            get
            {
                int division = 8;
                MusicalTimespan measureLength = MeasureLength;
                MusicalTimespan result = new MusicalTimespan(1, division);
                while (!measureLength.IsDivisibleBy(result))
                {
                    division *= 2;
                    result = new MusicalTimespan(1, division);
                }
                return result;
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
