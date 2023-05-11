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

        public void SetLength(int length)
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
                steps.Add(beat);
            }
            if (steps.Count > length)
            {
                steps.RemoveRange(length - 1, length - steps.Count);
            }
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
