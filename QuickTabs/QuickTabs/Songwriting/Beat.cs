using Newtonsoft.Json.Linq;
using QuickTabs.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal class Beat : Step, IEnumerable<Fret>
    {
        private List<Fret> heldFrets = new List<Fret>();

        public override StepType Type => StepType.Beat;

        public int NoteLength = 1;
        public bool this[Fret fret]
        {
            get
            {
                return (heldFrets.Contains(fret));
            }
            set
            {
                if (value && !heldFrets.Contains(fret))
                {
                    heldFrets.Add(fret);
                }
                if (!value && heldFrets.Contains(fret))
                {
                    heldFrets.Remove(fret);
                }
            }
        }

        public int HeldCount
        {
            get
            {
                return heldFrets.Count;
            }
        }

        public Beat Copy()
        {
            Beat copy = new Beat();
            foreach (Fret fret in heldFrets)
            {
                copy.heldFrets.Add(fret);
            }
            copy.NoteLength = NoteLength;
            return copy;
        }

        public override JObject SaveAsJObject(Song Song)
        {
            JObject beatJson = new JObject();
            beatJson.Add("type", "b");
            beatJson.Add("length", NoteLength);
            JArray states = new JArray();
            for (int i = 0; i < Song.Tab.Tuning.Count; i++)
            {
                states.Add(null);
            }
            foreach (Fret fret in heldFrets)
            {
                states[fret.String] = fret.Space;
            }
            beatJson.Add("states", states);
            return beatJson;
        }

        public IEnumerator<Fret> GetEnumerator()
        {
            return heldFrets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return heldFrets.GetEnumerator();
        }
    }
}
