using NAudio.Wave;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    internal class TabPlayer
    {
        public Tab Tab { get; private set; }
        public int BPM { get; set; }
        public bool IsPlaying { get; private set; } = false;
        public int Position
        {
            get
            {
                if (IsPlaying)
                {
                    TimeSpan elapsedSinceStart = DateTime.Now - playStartTime;
                    int beat = playStartBeat + calculateElapsedEighthNotes(elapsedSinceStart) + nonBeats;
                    if (beat >= Tab.Count)
                    {
                        beat = Tab.Count - 1;
                        IsPlaying = false;
                    }
                    while (Tab[beat].Type != Enums.StepType.Beat)
                    {
                        beat++;
                        nonBeats++;
                    }
                    return beat;
                } else
                {
                    return playStartBeat;
                }
            }
            set
            {
                if (IsPlaying)
                {
                    throw new InvalidOperationException("Cannot change position while playing");
                } else if (Tab[value].Type == Enums.StepType.Beat)
                {
                    playStartBeat = value;
                    nonBeats = 0;
                } else
                {
                    throw new InvalidOperationException("Position must be a beat");
                }
            }
        }
        private int playStartBeat;
        private DateTime playStartTime;
        private int nonBeats = 1;
        private int lastPosition;

        public TabPlayer(Tab tab)
        {
            Tab = tab;
        }

        public void Start()
        {
            playStartTime = DateTime.Now;
            lastPosition = 0;
            IsPlaying = true;
            AudioEngine.Tick += AudioEngine_Tick;
        }

        public void Stop()
        {
            playStartBeat = Position;
            IsPlaying = false;
        }

        private void AudioEngine_Tick()
        {
            int position = Position;
            if (!IsPlaying)
            {
                AudioEngine.SilenceAll();
                AudioEngine.Tick -= AudioEngine_Tick;
                return;
            }
            if (position != lastPosition)
            {
                Beat beat = (Beat)Tab[position];
                foreach (Fret fret in beat)
                {
                    Songwriting.Note note = Songwriting.Note.FromSemitones(Tab.Tuning.GetMusicalNote(fret.String), fret.Space);
                    AudioEngine.PlayNote(note, calculateEighthNoteDuration() * beat.NoteLength, 0.25F);
                }
                lastPosition = position;
            }
        }

        private int calculateElapsedEighthNotes(TimeSpan elapsedTime)
        {
            float beatsPerSecond = BPM / 60F;
            float beatDurationMs = 1000 / beatsPerSecond;
            float eightNoteDurationMs = beatDurationMs / 2;
            return (int)Math.Floor(elapsedTime.TotalMilliseconds / eightNoteDurationMs);
        }
        private int calculateEighthNoteDuration()
        {
            float beatsPerSecond = BPM / 60F;
            float beatDurationMs = 1000 / beatsPerSecond;
            float eighthNoteDurationMs = beatDurationMs / 2;
            return (int)eighthNoteDurationMs;
        }
    }
}
