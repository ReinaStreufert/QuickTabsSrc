using NAudio.Wave;
using QuickTabs.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    public static class AudioEngine
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
            if (Enabled)
            {
                throw new InvalidOperationException();
            }
            if (AsioOut.isSupported())
            {
                long drivIndex = QTPersistence.Current.AsioOutputDevice;
                if (drivIndex == (long)0)
                {
                    asioOut = new AsioOut("ASIO4ALL v2");
                } else
                {
                    asioOut = new AsioOut((int)(drivIndex - 1));
                }
                oscillator = new SquareOscillator();
                asioOut.Init(oscillator);
                oscillator.InitializeBuffer(asioOut.FramesPerBuffer);
                bufferDurationMs = asioOut.FramesPerBuffer * (1000F / oscillator.WaveFormat.SampleRate);
                oscillator.BeforeBufferFill += Oscillator_BeforeBufferFill;
                asioOut.Play();
                Enabled = true;
            }
        }

        public static void OpenControlPanel()
        {
            asioOut.ShowControlPanel();
        }

        public static void Reinitialize() // this is likely a terrible idea
        {
            Stop();
            Initialize();
        }

        public static void Stop()
        {
            if (!Enabled)
            {
                return;
            }
            if (asioOut == null)
            {
                Enabled = false;
                return;
            }
            asioOut.Stop();
            asioOut.Dispose();
            Enabled = false;
        }

        public static void PlayNote(Songwriting.Note note, int duration, IVolumeProvider volume, bool simulatePluck = true)
        {
            const float noteLevel = 0.25F;
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
                playingNote.Frequency = oscillator.AddFrequency(startFrequency, endFrequency, 25F, noteLevel, noteLevel / 4F, duration, volume);
            } else
            {
                playingNote.Frequency = oscillator.AddFrequency(NoteUtils.GetMidiNoteFrequency(note.MidiNumber), noteLevel, volume);
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
            playingNote.Frequency = oscillator.AddFrequency(startFrequency, endFrequency, 25F, volume, 0F, 50F, new ConstantVolume(1.0F));
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
            try
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
            } catch (Exception ex)
            {
                // unhandled exceptions on the audio rendering thread do not get handled by the
                // crash manager as they should; hence this.
                oscillator.BeforeBufferFill -= Oscillator_BeforeBufferFill;
                try
                {
                    if (asioOut.PlaybackState == PlaybackState.Playing || asioOut.PlaybackState == PlaybackState.Paused)
                    {
                        if (Application.OpenForms.Count > 0)
                        {
                            Application.OpenForms[0].BeginInvoke(new MethodInvoker(() =>
                            {
                                if (asioOut != null && asioOut.PlaybackState == PlaybackState.Playing || asioOut.PlaybackState == PlaybackState.Paused)
                                {
                                    asioOut.Stop(); // AsioOut.Stop does NOT like being called from its own callback.
                                    asioOut.Dispose();
                                    asioOut = null;
                                }
                                Enabled = false;
                                CrashManager.FailHard(ex);
                            }));
                        }
                    }
                } catch { }
            }
        }

        private class PlayingNote
        {
            public SquareOscillator.PlayingFrequency Frequency { get; set; }
            public int DurationMs { get; set; }
            public DateTime StartTime { get; set; }
        }
    }
}
