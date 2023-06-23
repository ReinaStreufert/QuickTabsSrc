using Newtonsoft.Json.Linq;
using QuickTabs.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal class Beat : Step, IEnumerable<Fret>
    {
        private List<Fret> heldFrets = new List<Fret>();

        public override StepType Type => StepType.Beat;

        private MusicalTimespan beatDivision = new MusicalTimespan(1, 8);
        public MusicalTimespan BeatDivision
        {
            get
            {
                return beatDivision;
            }
            set
            {
                beatDivision = value;
                if (sustainTime < beatDivision)
                {
                    sustainTime = beatDivision;
                }
            }
        }
        private MusicalTimespan sustainTime = new MusicalTimespan(1, 8);
        public MusicalTimespan SustainTime
        {
            get
            {
                return sustainTime;
            }
            set
            {
                if (value < beatDivision)
                {
                    throw new InvalidOperationException("sustain time must be >= beat division");
                } else
                {
                    sustainTime = value;
                }
            }
        }
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
            copy.BeatDivision = BeatDivision;
            copy.SustainTime = SustainTime;
            return copy;
        }

        public override JObject SaveAsJObject(Song Song)
        {
            JObject beatJson = new JObject();
            beatJson.Add("type", "b");
            beatJson.Add("length", SustainTime.GetValueForBeatDivisionF(8)); // stored this way for backwards compatible opening reasons, and flexibility for future further divisions
            beatJson.Add("div", (new MusicalTimespan(1, 1) / BeatDivision));
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
