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
    public class Beat : Step, IEnumerable<KeyValuePair<Fret,MusicalTimespan>>
    {
        private Dictionary<Fret, MusicalTimespan> heldFrets = new Dictionary<Fret, MusicalTimespan>();

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
            }
        }
        public MusicalTimespan this[Fret fret]
        {
            get
            {
                if (heldFrets.ContainsKey(fret))
                {
                    return heldFrets[fret];
                } else
                {
                    return MusicalTimespan.Zero;
                }
            }
            set
            {
                if (value == MusicalTimespan.Zero)
                {
                    heldFrets.Remove(fret);
                } else
                {
                    heldFrets[fret] = value;
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
            foreach (KeyValuePair<Fret, MusicalTimespan> fret in heldFrets)
            {
                copy[fret.Key] = fret.Value;
            }
            copy.BeatDivision = BeatDivision;
            return copy;
        }

        public override JObject SaveAsJObject(Song Song)
        {
            JObject beatJson = new JObject();
            beatJson.Add("type", "b");
            beatJson.Add("div", BeatDivision.SerializeToInt32());
            JArray states = new JArray();
            for (int i = 0; i < Song.FocusedTab.Tuning.Count; i++)
            {
                states.Add(null);
            }
            foreach (KeyValuePair<Fret,MusicalTimespan> fret in heldFrets)
            {
                states[fret.Key.String] = new JArray() { fret.Key.Space, fret.Value.SerializeToInt32() };
            }
            beatJson.Add("states", states);
            return beatJson;
        }

        public IEnumerator<KeyValuePair<Fret,MusicalTimespan>> GetEnumerator()
        {
            return heldFrets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return heldFrets.GetEnumerator();
        }
    }
}
