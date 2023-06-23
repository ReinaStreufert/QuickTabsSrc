using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal class Tab : IEnumerable<Step>
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
        public int FindClosestBeatIndexToTime(MusicalTimespan position, out MusicalTimespan resultPosition)
        {
            MusicalTimespan counter = MusicalTimespan.Zero;
            int stepIndex = 0;
            for (; ; stepIndex++)
            {
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
