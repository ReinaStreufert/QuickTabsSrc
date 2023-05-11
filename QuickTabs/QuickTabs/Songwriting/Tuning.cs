using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal struct Tuning
    {
        public static readonly Tuning StandardGuitar = new Tuning('E', 'A', 'D', 'G', 'B', 'E');
        public static readonly Tuning StandardUke = new Tuning('E', 'A', 'D', 'G');

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
