using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    internal static class AudioEngine
    {
        public static bool Enabled { get; set; } = false;
        public static event Action Tick;

        private static AsioOut asioOut;
        private static SquareOscillator oscillator;
        private static List<PlayingNote> playingNotes = new List<PlayingNote>();

        public static void Initialize()
        {
            if (AsioOut.isSupported())
            {
                asioOut = new AsioOut();
                oscillator = new SquareOscillator();
                asioOut.Init(oscillator);
                oscillator.InitializeBuffer(asioOut.FramesPerBuffer);
                oscillator.BeforeBufferFill += Oscillator_BeforeBufferFill;
                asioOut.Play();
                Enabled = true;
            }
        }

        public static void Stop()
        {
            if (!Enabled)
            {
                return;
            }
            asioOut.Stop();
            asioOut.Dispose();
        }

        public static void PlayNote(Songwriting.Note note, int duration, float volume, bool simulatePluck = true)
        {
            if (!Enabled)
            {
                return;
            }
            PlayingNote playingNote = new PlayingNote();
            playingNote.DurationMs = duration;
            playingNote.StartTime = DateTime.Now;
            if (simulatePluck)
            {
                float startFrequency = NoteUtils.GetMidiNoteFrequency(note.MidiNumber + 4);
                float endFrequency = NoteUtils.GetMidiNoteFrequency(note.MidiNumber);
                playingNote.Frequency = oscillator.AddFrequency(startFrequency, endFrequency, 25F, volume, volume / 4F, duration);
            } else
            {
                playingNote.Frequency = oscillator.AddFrequency(NoteUtils.GetMidiNoteFrequency(note.MidiNumber), volume);
            }
            
            playingNotes.Add(playingNote);
        }

        public static void PlayKick(Songwriting.Note note, int duration, float volume)
        {
            if (!Enabled)
            {
                return;
            }
            PlayingNote playingNote = new PlayingNote();
            playingNote.DurationMs = duration;
            playingNote.StartTime = DateTime.Now;
            float startFrequency = NoteUtils.GetMidiNoteFrequency(note.MidiNumber + 12);
            float endFrequency = NoteUtils.GetMidiNoteFrequency(note.MidiNumber);
            playingNote.Frequency = oscillator.AddFrequency(startFrequency, endFrequency, 25F, volume, 0F, 50F);
            playingNotes.Add(playingNote);
        }

        public static void SilenceAll()
        {
            if (!Enabled)
            {
                return;
            }
            playingNotes.Clear();
            oscillator.ClearAllFrequencies();
        }

        private static void Oscillator_BeforeBufferFill()
        {
            foreach (PlayingNote playingNote in playingNotes.ToArray())
            {
                TimeSpan elapsed = DateTime.Now - playingNote.StartTime;
                if (elapsed.TotalMilliseconds >= playingNote.DurationMs)
                {
                    playingNotes.Remove(playingNote);
                    oscillator.ClearFrequency(playingNote.Frequency);
                }
            }
            Tick?.Invoke();
        }

        private class PlayingNote
        {
            public SquareOscillator.PlayingFrequency Frequency { get; set; }
            public int DurationMs { get; set; }
            public DateTime StartTime { get; set; }
        }
    }
}
