using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal struct Tuning
    {
        public static readonly Tuning StandardGuitar = new Tuning("E4", "B3", "G3", "D3", "A2", "E2");
        public static readonly Tuning StandardUke = new Tuning("A4", "E4", "C4", "G4");
        public static readonly Tuning StandardBass = new Tuning("G2", "D2", "A1", "E1");

        private Note[] tuning;
        public Tuning(params string[] notes)
        {
            tuning = new Note[notes.Length];
            for (int i = 0; i < tuning.Length; i++)
            {
                tuning[i] = new Note(notes[i]);
            }
        }
        public Tuning(params Note[] notes)
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
        public string this[int i]
        {
            get
            {
                return tuning[i].ToString(false);
            }
        }
        public Note GetMusicalNote(int stringIndex)
        {
            return tuning[stringIndex];
        }
    }
}
