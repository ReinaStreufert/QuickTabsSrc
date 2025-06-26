using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    static class NoteUtils
    {
        public const float StandardTuning = 440F;
        public static float GetMidiNoteFrequency(int MidiNoteNumber, float Tuning = NoteUtils.StandardTuning)
        {
            return (float)((440F / 32D) * (Math.Pow(2, (MidiNoteNumber - 9D) / 12D)));
        }
    }
}
