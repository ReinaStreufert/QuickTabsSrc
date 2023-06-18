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
    internal class TabPlayer
    {
        public Tab Tab { get; private set; }
        private int bpm;
        public int BPM 
        { 
            get
            {
                return bpm;
            }
            set
            {
                if (bpm != value)
                {
                    if (IsPlaying)
                    {
                        playStartBeat = this.Position;
                        playStartTime = DateTime.Now;
                    }
                    bpm = value;
                    eighthNoteLength = calculateEighthNoteDuration();
                }
            }
        }
        public bool Loop { get; set; } = false;
        private bool metronome = false;
        public bool Metronome
        {
            get
            {
                return metronome;
            }
            set
            {
                if (value && !metronome)
                {
                    metronomeDisoriented = true;
                }
                metronome = value;
            }
        }
        public TimeSignature MetronomeTimeSignature { get; set; } = new TimeSignature(4, 4);
        public bool IsPlaying { get; private set; } = false;
        public int Position
        {
            get
            {
                if (IsPlaying)
                {
                    TimeSpan elapsedSinceStart = lastTickTimestamp - playStartTime;
                    int beat = playStartBeat + calculateElapsedEighthNotes(elapsedSinceStart) + nonBeats;
                    if (beat >= Tab.Count)
                    {
                        if (Loop)
                        {
                            beat = 1;
                            playStartBeat = 1;
                            nonBeats = 0;
                            playStartTime = lastTickTimestamp;
                        } else
                        {
                            beat = Tab.Count - 1;
                            IsPlaying = false;
                        }
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
                    orientMetronome();
                } else
                {
                    throw new InvalidOperationException("Position must be a beat");
                }
            }
        }
        private int playStartBeat;
        private DateTime playStartTime;
        private bool playStartTimeUnset;
        private DateTime lastTickTimestamp;
        private int nonBeats = 1;
        private int lastPosition;
        private int metronomeCounter;
        private int metronomeBeatsPerMeasure;
        private bool metronomeDisoriented = false;
        private float eighthNoteLength;
        private readonly Note metronomeHighNote = new Note("G5");
        private readonly Note metronomeLowNote = new Note("A4");

        public TabPlayer(Tab tab)
        {
            Tab = tab;
        }

        public void Start()
        {
            AudioEngine.AudioEngineTick initTick;
            playStartTime = DateTime.Now;
            playStartTimeUnset = true;
            lastPosition = -1;
            IsPlaying = true;
            AudioEngine.Tick += AudioEngine_Tick;
        }

        public void Stop()
        {
            playStartBeat = Position;
            IsPlaying = false;
        }

        private void AudioEngine_Tick(DateTime timestamp, float bufferDurationMS)
        {
            if (!IsPlaying)
            {
                AudioEngine.SilenceAll();
                AudioEngine.Tick -= AudioEngine_Tick;
                return;
            }
            lastTickTimestamp = timestamp;
            if (playStartTimeUnset)
            {
                playStartTime = timestamp;
                playStartTimeUnset = false;
            }
            int position = Position;
            if (position != lastPosition)
            {
                if (metronome)
                {
                    if (metronomeDisoriented)
                    {
                        orientMetronome();
                    }
                    if (metronomeCounter == 0) // = beginning of measure
                    {
                        AudioEngine.PlayKick(metronomeHighNote, 100, 0.5F);
                    }
                    else if (metronomeCounter % 2 == 0) // = on a quarter note
                    {
                        AudioEngine.PlayKick(metronomeLowNote, 100, 0.5F);
                    }
                    metronomeCounter++;
                    if (metronomeCounter >= metronomeBeatsPerMeasure)
                    {
                        metronomeCounter = 0;
                    }
                }
                Beat beat = (Beat)Tab[position];
                foreach (Fret fret in beat)
                {
                    Songwriting.Note note = Songwriting.Note.FromSemitones(Tab.Tuning.GetMusicalNote(fret.String), fret.Space);
                    AudioEngine.PlayNote(note, (int)(eighthNoteLength * beat.NoteLength), 0.25F);
                }
                lastPosition = position;
            } else
            {
                int timeUntilNextBeat = calculateTimeUntilNextEighthNote();
                if (timeUntilNextBeat < bufferDurationMS)
                {
                    AudioEngine.SetMidtick(timeUntilNextBeat);
                }
            }
        }

        private void orientMetronome()
        {
            int position = Position;
            metronomeBeatsPerMeasure = MetronomeTimeSignature.EighthNotesPerMeasure;
            metronomeCounter = 0;
            for (int i = 0; i < position; i++)
            {
                if (Tab[i].Type == Enums.StepType.Beat)
                {
                    metronomeCounter++;
                    if (metronomeCounter >= metronomeBeatsPerMeasure)
                    {
                        metronomeCounter = 0;
                    }
                }
            }
            metronomeDisoriented = false;
        }

        private int calculateElapsedEighthNotes(TimeSpan elapsedTime)
        {
            return (int)Math.Floor(elapsedTime.TotalMilliseconds / eighthNoteLength);
        }
        private int calculateTimeUntilNextEighthNote()
        {
            TimeSpan elapsedTime = lastTickTimestamp - playStartTime;
            float exactElapsedSteps = (float)elapsedTime.TotalMilliseconds / eighthNoteLength;
            int floorElapsedSteps = (int)Math.Floor(exactElapsedSteps);
            float durationThroughStep = exactElapsedSteps - floorElapsedSteps;
            return (int)Math.Ceiling((1 - durationThroughStep) * eighthNoteLength);
        }
        private float calculateEighthNoteDuration()
        {
            float beatsPerSecond = BPM / 60F;
            float beatDurationMs = 1000 / beatsPerSecond;
            float eighthNoteDurationMs = beatDurationMs / 2;
            return eighthNoteDurationMs;
        }
    }
}
