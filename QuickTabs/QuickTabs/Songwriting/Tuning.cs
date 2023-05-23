using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal struct Tuning
    {
        public static readonly Tuning StandardGuitar = new Tuning('E', 'B', 'G', 'D', 'A', 'E');
        public static readonly Tuning StandardUke = new Tuning('G', 'D', 'A', 'E');

        private char[] tuning;
        public Tuning(params char[] notes)
        {
            tuning = notes;
        }
        public int Count
        {
            get
            {
                return tuning.Length;
            }
        }
        public string Name
        {
            get
            {
                return new string(tuning).ToUpper();
            }
        }
        public string this[int i]
        {
            get
            {
                return tuning[i].ToString();
            }
        }
    }
}
