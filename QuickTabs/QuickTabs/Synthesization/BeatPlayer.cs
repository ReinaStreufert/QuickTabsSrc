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
    public class BeatPlayer
    {
        public Beat Beat { get; private set; }
        public int BPM { get; set; }
        public Tuning Tuning { get; set; }

        private DateTime playStartTime;
        private int lastElapsed = 0;
        private IVolumeProvider volume;

        public BeatPlayer(Beat beat, Track track)
        {
            this.Beat = beat;
            volume = new TrackVolumeProvider(track);
        }

        public void Start()
        {
            AudioEngine.Tick += AudioEngine_Tick;
        }

        private void AudioEngine_Tick(DateTime timestamp, float bufferDurationMS)
        {
            foreach (KeyValuePair<Fret,MusicalTimespan> fret in Beat)
            {
                Songwriting.Note note = Songwriting.Note.FromSemitones(Tuning.GetMusicalNote(fret.Key.String), fret.Key.Space);
                AudioEngine.PlayNote(note, (int)fret.Value.ToTimespan(BPM).TotalMilliseconds, volume);
            }
            AudioEngine.Tick -= AudioEngine_Tick;
        }
    }
}
