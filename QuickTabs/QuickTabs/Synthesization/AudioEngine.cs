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
        public delegate void AudioEngineTick(DateTime timestamp, float bufferDurationMs);

        public static bool Enabled { get; set; } = false;
        public static event AudioEngineTick Tick;

        private static AsioOut asioOut;
        private static SquareOscillator oscillator;
        private static List<PlayingNote> playingNotes = new List<PlayingNote>();
        private static float bufferDurationMs;
        private static DateTime lastTickTimestamp;

        public static void Initialize()
        {
            if (AsioOut.isSupported())
            {
                asioOut = new AsioOut();
                oscillator = new SquareOscillator();
                asioOut.Init(oscillator);
                oscillator.InitializeBuffer(asioOut.FramesPerBuffer);
                bufferDurationMs = asioOut.FramesPerBuffer * (1000F / oscillator.WaveFormat.SampleRate);
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
            playingNote.StartTime = lastTickTimestamp;
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

        public static void SetMidtick(float timeMS) // to be called only from AudioEngine.Tick. indicates that the AudioEngine should tick again part way through filling the buffer.
        {
            int timeSamples = (int)Math.Ceiling(timeMS * (oscillator.WaveFormat.SampleRate / 1000F));
            oscillator.MidtickSamples = timeSamples;
        }

        private static void Oscillator_BeforeBufferFill()
        {
            DateTime timestamp = oscillator.StartTime.AddMilliseconds(oscillator.SamplesWritten * (1000F / oscillator.WaveFormat.SampleRate));
            lastTickTimestamp = timestamp;
            foreach (PlayingNote playingNote in playingNotes.ToArray())
            {
                TimeSpan elapsed = timestamp - playingNote.StartTime;
                if (elapsed.TotalMilliseconds >= playingNote.DurationMs)
                {
                    playingNotes.Remove(playingNote);
                    oscillator.ClearFrequency(playingNote.Frequency);
                }
            }
            Tick?.Invoke(timestamp, bufferDurationMs);
        }

        private class PlayingNote
        {
            public SquareOscillator.PlayingFrequency Frequency { get; set; }
            public int DurationMs { get; set; }
            public DateTime StartTime { get; set; }
        }
    }
}
