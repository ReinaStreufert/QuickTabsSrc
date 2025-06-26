using QuickTabs.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public class Tab : IEnumerable<Step>
    {
        private List<Step> steps = new List<Step>();
        public Tuning Tuning { get; set; } = Tuning.StandardGuitar;
        public int Count
        {
            get
            {
                return steps.Count;
            }
        }
        public int BeatCount
        {
            get
            {
                return steps.OfType<Beat>().Count();
            }
        }
        public Step this[int i]
        {
            get
            {
                return steps[i];
            }
            set
            {
                steps[i] = value;
                value.IndexWithinTab = i;
            }
        }
        public void InsertBeats(int index, int count, MusicalTimespan? division)
        {
            for (int i = 0; i < count; i++)
            {
                Beat beat = new Beat();
                if (division.HasValue)
                {
                    beat.BeatDivision = division.Value;
                }
                steps.Insert(index, beat);
            }
            for (int i = index; i < steps.Count; i++)
            {
                steps[i].IndexWithinTab = i;
            }
        }
        public void RemoveBeats(int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                steps.RemoveAt(index);
            }
            for (int i = index; i < steps.Count; i++)
            {
                steps[i].IndexWithinTab = i;
            }
        }
        public void SetLength(int length, MusicalTimespan? division)
        {
            if (steps.Count == 0 && length > 0)
            {
                SectionHead head = new SectionHead();
                head.IndexWithinTab = steps.Count;
                steps.Add(head);
            }
            while (steps.Count < length)
            {
                Beat beat = new Beat();
                beat.IndexWithinTab = steps.Count;
                if (division.HasValue)
                {
                    beat.BeatDivision = division.Value;
                }
                steps.Add(beat);
            }
            if (steps.Count > length)
            {
                int remove = steps.Count - length;
                steps.RemoveRange(steps.Count - remove, remove);
            }
        }
        public void AlterTuning(Tuning newTuning, int stringShift = 0)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Type == Enums.StepType.Beat)
                {
                    Beat srcBeat = (Beat)this[i];
                    Beat newBeat = new Beat();
                    newBeat.BeatDivision = srcBeat.BeatDivision;
                    foreach (KeyValuePair<Fret, MusicalTimespan> fret in srcBeat)
                    {
                        int newString = fret.Key.String + stringShift; ;
                        if (newString >= 0 && newString < newTuning.Count)
                        {
                            newBeat[new Fret(newString, fret.Key.Space)] = fret.Value;
                        }
                    }
                    this[i] = newBeat;
                }
            }
            this.Tuning = newTuning;
        }
        public int FindClosestBeatIndexToTime(MusicalTimespan position, out MusicalTimespan resultPosition)
        {
            MusicalTimespan counter = MusicalTimespan.Zero;
            int stepIndex = 0;
            for (; ; stepIndex++)
            {
                if (stepIndex >= this.Count)
                {
                    break;
                }
                if (this[stepIndex].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)this[stepIndex];
                    counter += beat.BeatDivision;
                    if (counter > position)
                    {
                        counter -= beat.BeatDivision;
                        break;
                    }
                }
            }
            resultPosition = counter;
            return stepIndex;
        }
        public MusicalTimespan FindIndexTime(int index)
        {
            MusicalTimespan result = MusicalTimespan.Zero;
            for (int i = 0; i < index; i++)
            {
                if (this[i].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)this[i];
                    result += beat.BeatDivision;
                }
            }
            return result;
        }
        public MusicalTimespan FindBeatTime(Beat beat, int offset = 0) // set offset to keep counting a certain amount of beats after the matching beat
        {
            MusicalTimespan result = MusicalTimespan.Zero;
            bool matched = false;
            foreach (Step step in this)
            {
                if (step.Type == Enums.StepType.Beat)
                {
                    if (matched)
                    {
                        if (offset <= 0)
                        {
                            return result;
                        }
                    }
                    Beat enumBeat = (Beat)step;
                    if (beat == enumBeat)
                    {
                        if (offset > 0)
                        {
                            matched = true;
                            result += enumBeat.BeatDivision;
                            offset--;
                            continue;
                        } else
                        {
                            return result;
                        }
                    } else
                    {
                        result += enumBeat.BeatDivision;
                        if (matched)
                        {
                            offset--;
                        }
                    }
                }
            }
            if (matched && offset <= 0)
            {
                return result;
            }
            throw new IndexOutOfRangeException("The specified beat was not found in the tab");
        }

        public IEnumerator<Step> GetEnumerator()
        {
            return steps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return steps.GetEnumerator();
        }
    }
}
