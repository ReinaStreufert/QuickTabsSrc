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
            return (440F / 32F) * (float)(Math.Pow(2, (MidiNoteNumber - 9F) / 12F));
        }
    }
}
