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
    internal static class History
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
        public static void ClearHistory()
        {
            currentState = null;
            StateChange?.Invoke();
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
            public Tuning SongTuning { get; private set; }
            public Step[] Steps { get; private set; }
            public Selection Selection { get; private set; }

            public HistoryState(Song source, Selection selectionSource)
            {
                SongName = source.Name;
                SongTempo = source.Tempo;
                SongTimeSignature = source.TimeSignature;
                SongTuning = source.Tab.Tuning;
                Steps = new Step[source.Tab.Count];
                for (int i = 0; i < Steps.Length; i++)
                {
                    Step sourceStep = source.Tab[i];
                    if (sourceStep.Type == Enums.StepType.Beat)
                    {
                        Beat sourceBeat = (Beat)sourceStep;
                        Steps[i] = sourceBeat.Copy();
                    } else if (sourceStep.Type == Enums.StepType.SectionHead)
                    {
                        SectionHead sourceSectionHead = (SectionHead)sourceStep;
                        SectionHead newSectionHead = new SectionHead();
                        newSectionHead.Name = sourceSectionHead.Name;
                        Steps[i] = newSectionHead;
                    }
                }
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
                destination.Tab.Tuning = SongTuning;
                destination.Tab.SetLength(Steps.Length);
                for (int i = 0; i < Steps.Length; i++)
                {
                    Step sourceStep = Steps[i];
                    if (sourceStep.Type == Enums.StepType.Beat)
                    {
                        Beat sourceBeat = (Beat)sourceStep;
                        destination.Tab[i] = sourceBeat.Copy();
                    }
                    else if (sourceStep.Type == Enums.StepType.SectionHead)
                    {
                        SectionHead sourceSectionHead = (SectionHead)sourceStep;
                        SectionHead newSectionHead = new SectionHead();
                        newSectionHead.Name = sourceSectionHead.Name;
                        destination.Tab[i] = newSectionHead;
                    }
                }
            }
        }
    }
}
