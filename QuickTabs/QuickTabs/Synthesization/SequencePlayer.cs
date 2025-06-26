using QuickTabs.Enums;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    // this is the new version of TabPlayer which I decided to completely rewrite in order to most cleanly
    // support varying beat divisions as well as parallel track support for the future when that happens.
    public class SequencePlayer
    {
        public PlayState PlayState { get; private set; } = PlayState.NotPlaying;
        private List<Track> source;
        public List<Track> Source // SequencePlayer expects all source tabs to be the same MusicalTimespan length
        {
            get
            {
                return source;
            }
            set
            {
                if (PlayState == PlayState.NotPlaying)
                {
                    source = value;
                } else
                {
                    throw new InvalidOperationException("Pause then set then play");
                }
            }
        }
        private int tempo = 120;
        public int Tempo
        {
            get
            {
                return tempo;
            }
            set
            {
                tempo = value;
            }
        }
        private bool metronomeEnabled = false;
        public bool MetronomeEnabled
        {
            get
            {
                return metronomeEnabled;
            }
            set
            {
                if (value && !metronomeEnabled)
                {
                    metTickCounterUnset = true;
                }
                metronomeEnabled = value;
            }
        }
        private TimeSignature metronomeTimeSignature = new TimeSignature(4, 4);
        public TimeSignature MetronomeTimeSignature
        {
            get
            {
                return metronomeTimeSignature;
            }
            set
            {
                if (PlayState == PlayState.NotPlaying)
                {
                    metronomeTimeSignature = value;
                }
                else
                {
                    throw new InvalidOperationException("Pause then set then play");
                }
            }
        }
        private MusicalTimespan metronomeInterval = new MusicalTimespan(1, 4);
        public MusicalTimespan MetronomeInterval
        {
            get
            {
                return metronomeInterval;
            }
            set
            {
                if (PlayState == PlayState.NotPlaying)
                {
                    metronomeInterval = value;
                }
                else
                {
                    throw new InvalidOperationException("Pause then set then play");
                }
            }
        }
        public bool Loop { get; set; } = false;
        public MusicalTimespan Position { get; private set; } = MusicalTimespan.Zero;
        public event Action PositionUpdate;
        public event Action PlaybackStopped;

        private EventWaitHandle queueHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private TrackPlayerInfo[] trackInfo;
        private MusicalTimespan trackUpdateWaitTime;
        private DateTime nextTrackUpdateTime;
        private Editor editor;
        private bool metTickCounterUnset = true;
        private MusicalTimespan metronomeTickCounter = MusicalTimespan.Zero;
        private bool soloMode = false;

        private readonly Note metronomeHighNote = new Note("G5");
        private readonly Note metronomeLowNote = new Note("A4");

        public SequencePlayer(Editor editor, List<Track> source)
        {
            Source = source;
            this.editor = editor;
        }

        public void PlayFrom(MusicalTimespan time)
        {
            if (PlayState == PlayState.NotPlaying)
            {
                Position = time;
                metTickCounterUnset = true;
                PlayState = PlayState.WaitingToPlay;
                soloMode = false;
                foreach (Track track in source)
                {
                    if (track.Solo)
                    {
                        soloMode = true;
                    }
                }
                AudioEngine.Tick += AudioEngine_Tick;
                queueHandle.WaitOne(1000); // if an exception happens to occur on the audio thread before it sets the wait handle,
                                           // the main thread will block forever withot a timeout causing crash handling (which cannot be
                                           // done on the audio thread) to not occur. The timeout breaks this.
            } else
            {
                throw new InvalidOperationException("Cannot play from this state");
            }
        }
        public void Stop()
        {
            if (PlayState == PlayState.Playing)
            {
                PlayState = PlayState.WaitingToStop;
                queueHandle.WaitOne(1000);
            } else
            {
                throw new InvalidOperationException("Cannot stop from this state");
            }
        }
        public int GetTabPositionForTrack(int trackIndex)
        {
            if (PlayState == PlayState.Playing)
            {
                return trackInfo[trackIndex].CurrentStepPosition;
            } else
            {
                throw new InvalidOperationException("Must be playing to get position in track");
            }
        }

        private void AudioEngine_Tick(DateTime timestamp, float bufferDurationMs)
        {
            bool firstTick = false;
            if (PlayState == PlayState.WaitingToPlay)
            {
                firstTick = true;
                initializePlayback(timestamp);
                PlayState = PlayState.Playing;
                queueHandle.Set();
                editor.BeginInvoke(() => { PositionUpdate?.Invoke(); });
            } else if (PlayState == PlayState.WaitingToStop)
            {
                AudioEngine.Tick -= AudioEngine_Tick;
                AudioEngine.SilenceAll();
                PlayState = PlayState.NotPlaying;
                queueHandle.Set();
                editor.BeginInvoke(() => { PlaybackStopped?.Invoke(); });
                return;
            }
            if (metronomeEnabled && metTickCounterUnset)
            {
                MusicalTimespan measureOverflow;
                Position.DivideBy(metronomeTimeSignature.MeasureLength, out measureOverflow);
                measureOverflow.DivideBy(metronomeInterval, out metronomeTickCounter);
                metTickCounterUnset = false;
                if (firstTick && metronomeTickCounter == MusicalTimespan.Zero)
                {
                    playMetronome();
                }
            }
            if (timestamp >= nextTrackUpdateTime)
            {
                advanceTracks(timestamp);
                if (PlayState == PlayState.NotPlaying)
                {
                    AudioEngine.Tick -= AudioEngine_Tick;
                    AudioEngine.SilenceAll();
                    editor.BeginInvoke(() => { PlaybackStopped?.Invoke(); });
                    return;
                }
                if (metronomeEnabled)
                {
                    if (metronomeTickCounter >= MetronomeInterval || Position.IsDivisibleBy(metronomeTimeSignature.MeasureLength))
                    {
                        playMetronome();
                    }
                }
                editor.BeginInvoke(() => { PositionUpdate?.Invoke(); });
            } else
            {
                TimeSpan untilNextUpdate = nextTrackUpdateTime - timestamp;
                if (untilNextUpdate.TotalMilliseconds < bufferDurationMs)
                {
                    AudioEngine.SetMidtick((float)untilNextUpdate.TotalMilliseconds);
                }
            }
        }
        private void initializePlayback(DateTime timestamp)
        {
            trackInfo = new TrackPlayerInfo[source.Count];
            MusicalTimespan smallestNextStep = MusicalTimespan.Zero;
            for (int i = 0; i < trackInfo.Length; i++)
            {
                TrackPlayerInfo track = new TrackPlayerInfo();
                track.TrackRef = Source[i];
                track.VolumeProvider = new TrackVolumeProvider(track.TrackRef);
                track.Steps = track.TrackRef.Tab;
                MusicalTimespan closestStepPosition;
                track.CurrentStepPosition = track.Steps.FindClosestBeatIndexToTime(Position, out closestStepPosition);
                MusicalTimespan overflow = Position - closestStepPosition;
                track.UntilNextStep = ((Beat)track.Steps[track.CurrentStepPosition]).BeatDivision - overflow;
                if (i == 0 || track.UntilNextStep < smallestNextStep)
                {
                    smallestNextStep = track.UntilNextStep;
                }
                if (!track.TrackRef.Mute)
                {
                    if (soloMode)
                    {
                        if (track.TrackRef.Solo)
                        {
                            playTrack(track, overflow);
                        }
                    } else
                    {
                        playTrack(track, overflow);
                    }
                }
                trackInfo[i] = track;
            }
            if (metronomeEnabled && metronomeInterval < smallestNextStep)
            {
                smallestNextStep = metronomeInterval;
            }
            trackUpdateWaitTime = smallestNextStep;
            nextTrackUpdateTime = timestamp + smallestNextStep.ToTimespan(Tempo);
        }
        private void advanceTracks(DateTime timestamp)
        {
            MusicalTimespan smallestNextStep = MusicalTimespan.Zero;
            bool reset = false;
            bool newSoloVal = false;
            for (int i = 0; i < trackInfo.Length; i++)
            {
                TrackPlayerInfo track = trackInfo[i];
                if (track.TrackRef.Solo)
                {
                    newSoloVal = true;
                }
                if (trackUpdateWaitTime >= track.UntilNextStep)
                {
                    track.CurrentStepPosition++;
                    if (track.CurrentStepPosition >= track.Steps.Count)
                    {
                        if (Loop)
                        {
                            reset = true;
                            track.CurrentStepPosition = 1;
                        } else
                        {
                            PlayState = PlayState.NotPlaying;
                            return;
                        }
                    }
                    if (track.Steps[track.CurrentStepPosition].Type == StepType.SectionHead)
                    {
                        track.CurrentStepPosition++;
                    }
                    track.UntilNextStep = ((Beat)track.Steps[track.CurrentStepPosition]).BeatDivision;
                    if (i == 0 || track.UntilNextStep < smallestNextStep)
                    {
                        smallestNextStep = track.UntilNextStep;
                    }
                    if (!track.TrackRef.Mute)
                    {
                        if (soloMode)
                        {
                            if (track.TrackRef.Solo)
                            {
                                playTrack(track);
                            }
                        }
                        else
                        {
                            playTrack(track);
                        }
                    }
                } else
                {
                    track.UntilNextStep -= trackUpdateWaitTime;
                    if (i == 0 || track.UntilNextStep < smallestNextStep)
                    {
                        smallestNextStep = track.UntilNextStep;
                    }
                }
            }
            soloMode = newSoloVal;
            if (reset)
            {
                Position = MusicalTimespan.Zero;
                if (metronomeEnabled)
                {
                    metronomeTickCounter = MetronomeInterval; // MetronomeInterval and not zero so it ticks properly on the first beat when it loops instead of uhhh not
                }
            } else
            {
                Position += trackUpdateWaitTime;
                if (metronomeEnabled)
                {
                    metronomeTickCounter += trackUpdateWaitTime;
                }
            }
            if (metronomeEnabled)
            {
                MusicalTimespan timeUntilNextTick = (metronomeInterval - metronomeTickCounter);
                if (timeUntilNextTick == MusicalTimespan.Zero)
                {
                    timeUntilNextTick = metronomeInterval;
                }
                if (timeUntilNextTick < smallestNextStep)
                {
                    smallestNextStep = timeUntilNextTick;
                }
                MusicalTimespan timeUntilNextMeasure = nextMeasurePosition() - Position;
                if (timeUntilNextMeasure < smallestNextStep)
                {
                    smallestNextStep = timeUntilNextMeasure;
                }
            }
            trackUpdateWaitTime = smallestNextStep;
            nextTrackUpdateTime = nextTrackUpdateTime + smallestNextStep.ToTimespan(Tempo);
        }
        private void playTrack(TrackPlayerInfo track)
        {
            playTrack(track, MusicalTimespan.Zero);
        }
        private void playTrack(TrackPlayerInfo track, MusicalTimespan startPointInSustain)
        {
            Beat beat = (Beat)track.Steps[track.CurrentStepPosition];
            foreach (KeyValuePair<Fret,MusicalTimespan> fret in beat)
            {
                Songwriting.Note note = Songwriting.Note.FromSemitones(track.Steps.Tuning.GetMusicalNote(fret.Key.String), fret.Key.Space);
                AudioEngine.PlayNote(note, (int)((fret.Value - startPointInSustain).ToTimespan(tempo).TotalMilliseconds), track.VolumeProvider);
            }
        }
        private void playMetronome()
        {
            metronomeTickCounter = MusicalTimespan.Zero;
            bool firstTickOfMeasure = Position.IsDivisibleBy(metronomeTimeSignature.MeasureLength);
            if (firstTickOfMeasure)
            {
                AudioEngine.PlayKick(metronomeHighNote, 100, 0.5F);
            } else
            {
                AudioEngine.PlayKick(metronomeLowNote, 100, 0.5F);
            }
        }
        private MusicalTimespan nextMeasurePosition()
        {
            int currentMeasure = Position / metronomeTimeSignature.MeasureLength;
            return metronomeTimeSignature.MeasureLength * (currentMeasure + 1);
        }

        private class TrackPlayerInfo
        {
            public Track TrackRef;
            public TrackVolumeProvider VolumeProvider;
            public Tab Steps;
            public int CurrentStepPosition;
            public MusicalTimespan UntilNextStep;
        }
    }
}
