using NAudio.Wave;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    internal class BeatPlayer
    {
        public Beat Beat { get; private set; }
        public int BPM { get; set; }
        public Tuning Tuning { get; set; }

        private DateTime playStartTime;
        private int lastElapsed = 0;

        public BeatPlayer(Beat beat)
        {
            this.Beat = beat;
        }

        public void Start()
        {
            AudioEngine.Tick += AudioEngine_Tick;
        }

        private void AudioEngine_Tick(DateTime timestamp, float bufferDurationMS)
        {
            foreach (Fret fret in Beat)
            {
                Songwriting.Note note = Songwriting.Note.FromSemitones(Tuning.GetMusicalNote(fret.String), fret.Space);
                AudioEngine.PlayNote(note, (int)Beat.SustainTime.ToTimespan(BPM).TotalMilliseconds, 0.25F);
            }
            AudioEngine.Tick -= AudioEngine_Tick;
        }
    }
}
