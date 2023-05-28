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
        public static event Action Tick;

        private static AsioOut asioOut;
        private static SquareOscillator oscillator;
        private static List<PlayingNote> playingNotes = new List<PlayingNote>();

        public static void Initialize()
        {
            asioOut = new AsioOut();
            oscillator = new SquareOscillator();
            asioOut.Init(oscillator);
            oscillator.InitializeBuffer(asioOut.FramesPerBuffer);
            oscillator.BeforeBufferFill += Oscillator_BeforeBufferFill;
            asioOut.Play();
        }

        public static void Stop()
        {
            asioOut.Stop();
            asioOut.Dispose();
        }

        public static void PlayNote(Songwriting.Note note, int duration, float volume)
        {
            PlayingNote playingNote = new PlayingNote();
            playingNote.DurationMs = duration;
            playingNote.StartTime = DateTime.Now;
            playingNote.Frequency = oscillator.AddFrequency(NoteUtils.GetMidiNoteFrequency(note.MidiNumber), volume);
            playingNotes.Add(playingNote);
        }

        public static void SilenceAll()
        {
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
