using QuickTabs.Controls;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    public static class History
    {
        public static event Action StateChange;
        public static event Action SubstantialChange;

        private static HistoryState currentState = null;
        public static void PushState(Song song, Selection selection, bool substantialChange = true)
        {
            HistoryState newState = new HistoryState(song, selection);
            newState.IsSubstantialChange = substantialChange;
            if (currentState == null)
            {
                currentState = newState;
                StateChange?.Invoke();
                if (substantialChange)
                {
                    SubstantialChange?.Invoke();
                }
                return;
            }
            newState.PreviousState = currentState;
            if (currentState.NextState != null)
            {
                currentState.AlternateNextState = currentState.NextState;
                currentState.NextState = newState;
            } else
            {
                currentState.NextState = newState;
            }
            currentState = newState;
            StateChange?.Invoke();
            if (substantialChange)
            {
                SubstantialChange?.Invoke();
            }
        }
        
        public static bool CanUndo
        {
            get
            {
                if (currentState == null)
                {
                    return false;
                }
                return (currentState.PreviousState != null);
            }
        }
        public static bool CanRedo
        {
            get
            {
                if (currentState == null)
                {
                    return false;
                }
                return (currentState.NextState != null);
            }
        }
        public static bool CanAlternateRedo
        {
            get
            {
                if (currentState == null)
                {
                    return false;
                }
                return (currentState.AlternateNextState != null);
            }
        }

        public static void Undo(Song destination, out Selection selection)
        {
            if (!CanUndo)
            {
                throw new InvalidOperationException();
            }
            bool substantial = currentState.IsSubstantialChange;
            currentState = currentState.PreviousState;
            currentState.CopyTo(destination);
            selection = currentState.Selection;
            StateChange?.Invoke();
            if (substantial)
            {
                SubstantialChange?.Invoke();
            }
        }
        public static void Redo(Song destination, out Selection selection)
        {
            if (!CanRedo)
            {
                throw new InvalidOperationException();
            }
            currentState = currentState.NextState;
            currentState.CopyTo(destination);
            selection = currentState.Selection;
            StateChange?.Invoke();
            if (currentState.IsSubstantialChange)
            {
                SubstantialChange?.Invoke();
            }
        }
        public static void RedoAlternate(Song destination, out Selection selection)
        {
            if (!CanAlternateRedo)
            {
                throw new InvalidOperationException();
            }
            currentState = currentState.AlternateNextState;
            currentState.CopyTo(destination);
            selection = currentState.Selection;
            StateChange?.Invoke();
            if (currentState.IsSubstantialChange)
            {
                SubstantialChange?.Invoke();
            }
        }
        public static bool TryGetSafeState(out Song song)
        {
            if (currentState == null)
            {
                song = null;
                return false;
            }
            song = new Song();
            currentState.CopyTo(song);
            return true;
        }
        public static void ClearHistory()
        {
            currentState = null;
            StateChange?.Invoke();
        }

        private class TrackState
        {
            public string Name { get; private set; }
            public bool NamedByUser { get; private set; }
            public Tuning TrackTuning { get; private set; }
            public Step[] Steps { get; private set; }
            public bool Mute { get; private set; }
            public bool Solo { get; private set; }
            public float Volume { get; private set; }

            public TrackState(Track source)
            {
                TrackTuning = source.Tab.Tuning;
                Name = source.Name;
                NamedByUser = source.NamedByUser;
                Steps = new Step[source.Tab.Count];
                for (int i = 0; i < Steps.Length; i++)
                {
                    Step sourceStep = source.Tab[i];
                    if (sourceStep.Type == Enums.StepType.Beat)
                    {
                        Beat sourceBeat = (Beat)sourceStep;
                        Steps[i] = sourceBeat.Copy();
                    }
                    else if (sourceStep.Type == Enums.StepType.SectionHead)
                    {
                        SectionHead sourceSectionHead = (SectionHead)sourceStep;
                        SectionHead newSectionHead = new SectionHead();
                        newSectionHead.Name = sourceSectionHead.Name;
                        Steps[i] = newSectionHead;
                    }
                }
                Mute = source.Mute;
                Solo = source.Solo;
                Volume = source.Volume;
            }

            public void CopyTo(Track track)
            {
                track.Name = Name;
                track.NamedByUser = NamedByUser;
                track.Tab.Tuning = TrackTuning;
                track.Tab.SetLength(Steps.Length, MusicalTimespan.Zero);
                for (int i = 0; i < Steps.Length; i++)
                {
                    Step sourceStep = Steps[i];
                    if (sourceStep.Type == Enums.StepType.Beat)
                    {
                        Beat sourceBeat = (Beat)sourceStep;
                        track.Tab[i] = sourceBeat.Copy();
                    }
                    else if (sourceStep.Type == Enums.StepType.SectionHead)
                    {
                        SectionHead sourceSectionHead = (SectionHead)sourceStep;
                        SectionHead newSectionHead = new SectionHead();
                        newSectionHead.Name = sourceSectionHead.Name;
                        track.Tab[i] = newSectionHead;
                    }
                }
                track.Mute = Mute;
                track.Solo = Solo;
                track.Volume = Volume;
            }
        }
        private class HistoryState
        {
            public HistoryState NextState { get; set; } = null;
            public HistoryState AlternateNextState { get; set; } = null;
            public HistoryState PreviousState { get; set; } = null;
            public bool IsSubstantialChange { get; set; }

            public string SongName { get; private set; }
            public int SongTempo { get; private set; }
            public TimeSignature SongTimeSignature { get; private set; }
            public TrackState[] Tracks { get; private set; }
            public int FocusedTrack { get; private set; }
            public Selection Selection { get; private set; }

            public HistoryState(Song source, Selection selectionSource)
            {
                SongName = source.Name;
                SongTempo = source.Tempo;
                SongTimeSignature = source.TimeSignature;
                Tracks = new TrackState[source.Tracks.Count];
                int trackI = 0;
                foreach (Track sourceTrack in source.Tracks)
                {
                    Tracks[trackI] = new TrackState(sourceTrack);
                    trackI++;
                }
                FocusedTrack = source.FocusedTrackIndex;
                if (selectionSource == null)
                {
                    Selection = null;
                } else
                {
                    Selection = new Selection(selectionSource.SelectionStart, selectionSource.SelectionLength);
                }
            }
            public void CopyTo(Song destination)
            {
                destination.Name = SongName;
                destination.Tempo = SongTempo;
                destination.TimeSignature = SongTimeSignature;
                destination.Tracks = new List<Track>();
                foreach (TrackState trackState in Tracks)
                {
                    Track track = new Track();
                    trackState.CopyTo(track);
                    destination.Tracks.Add(track);
                }
                destination.FocusedTrackIndex = FocusedTrack;
            }
        }
    }
}
