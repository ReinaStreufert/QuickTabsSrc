using Newtonsoft.Json.Linq;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Data
{
    // rewritten cuz trash code and then the new code still ended up being trashy. smh my head.
    public class QtJsonFormat : FileFormat
    {
        public override string Extension => ".qtjson";
        public override string Name => "QuickTabs Json File (*.qtjson)";

        public override Song Open(string fileName, out bool failed)
        {
            string fileText = File.ReadAllText(fileName);
            JObject songJson;
            try
            {
                songJson = JObject.Parse(fileText);
            }
            catch
            {
                failed = true;
                return null;
            }
            return Open(songJson, out failed);
        }
        public Song Open(JObject songJson, out bool failed)
        {
            Song song = new Song();
            song.Tracks.Clear();
            string? songName = enforceJString(songJson, "name");
            if (songName == null)
            {
                failed = true;
                return null;
            }
            song.Name = songName;
            int? tempo = enforceJInteger(songJson, "tempo");
            if (!tempo.HasValue)
            {
                failed = true;
                return null;
            }
            song.Tempo = tempo.Value;
            JArray? tsArray = enforceJArray(songJson, "ts");
            if (tsArray == null || tsArray.Count != 2 || tsArray[0].Type != JTokenType.Integer || tsArray[1].Type != JTokenType.Integer)
            {
                failed = true;
                return null;
            }
            song.TimeSignature = new Songwriting.TimeSignature((int)tsArray[0], (int)tsArray[1]);
            JArray? tracksJson = enforceJArray(songJson, "tracks");
            if (tracksJson == null)
            {
                // try older format version
                JArray tuningJson = enforceJArray(songJson, "tuning");
                if (tuningJson == null)
                {
                    failed = true;
                    return null;
                }
                Tuning? tuning = tuningFromJson(tuningJson);
                if (!tuning.HasValue)
                {
                    failed = true;
                    return null;
                }
                JArray? stepsJson = enforceJArray(songJson, "steps");
                if (stepsJson == null)
                {
                    failed = true;
                    return null;
                }
                Tab? tab = tabFromStepsJson(stepsJson, tuning.Value);
                if (tab == null)
                {
                    failed = true;
                    return null;
                }
                Track track = new Track();
                track.Tab = tab;
                track.UpdateAutoName();
                song.Tracks.Add(track);
                failed = false;
                return song;
            } else
            {
                foreach (JToken trackJson in tracksJson)
                {
                    Track? track = trackFromTrackJson(trackJson);
                    if (track == null)
                    {
                        failed = true;
                        return null;
                    }
                    song.Tracks.Add(track);
                }
                failed = false;
                return song;
            }
        }
        public override void Save(Song song, string fileName)
        {
            File.WriteAllText(fileName, song.SaveAsJObject(song).ToString());
        }

        private static Track? trackFromTrackJson(JToken trackJson)
        {
            if (trackJson.Type != JTokenType.Object)
            {
                return null;
            }
            JObject trackJsonObj = (JObject)trackJson;
            Track track = new Track();
            string? name = enforceJString(trackJsonObj, "name");
            if (name == null)
            {
                return null;
            }
            track.Name = name;
            bool? mute = enforceJBool(trackJsonObj, "mute");
            if (!mute.HasValue)
            {
                return null;
            }
            track.Mute = mute.Value;
            bool? solo = enforceJBool(trackJsonObj, "solo");
            if (!solo.HasValue)
            {
                return null;
            }
            track.Solo = solo.Value;
            float? volume = enforceJFloatOrInteger(trackJsonObj, "volume");
            if (!volume.HasValue)
            {
                return null;
            }
            track.Volume = volume.Value;
            bool? namedByUser = enforceJBool(trackJsonObj, "namedbyuser");
            if (!namedByUser.HasValue)
            {
                return null;
            }
            track.NamedByUser = namedByUser.Value;
            JArray? tuningJson = enforceJArray(trackJsonObj, "tuning");
            if (tuningJson == null)
            {
                return null;
            }
            Tuning? tuning = tuningFromJson(tuningJson);
            if (!tuning.HasValue)
            {
                return null;
            }
            JArray? stepsJson = enforceJArray(trackJsonObj, "steps");
            if (stepsJson == null)
            {
                return null;
            }
            Tab? tab = tabFromStepsJson(stepsJson, tuning.Value);
            if (tab == null)
            {
                return null;
            }
            track.Tab = tab;
            return track;
        }

        private static SectionHead? sectionHeadFromStepJson(JObject stepJson)
        {
            SectionHead sectionHead = new SectionHead();
            string? name = enforceJString(stepJson, "name");
            if (name == null)
            {
                return null;
            }
            sectionHead.Name = name;
            return sectionHead;
        }

        private static bool oldReadBeatStates(Beat dest, JArray states, MusicalTimespan length)
        {
            for (int stringIndex = 0; stringIndex < states.Count; stringIndex++)
            {
                if (states[stringIndex].Type == JTokenType.Null)
                {
                    continue;
                }
                if (states[stringIndex].Type != JTokenType.Integer)
                {
                    return false;
                }
                int space = (int)states[stringIndex];
                dest[new Fret(stringIndex, space)] = length;
            }
            return true;
        }

        private static bool currentReadBeatStates(Beat dest, JArray states)
        {
            for (int stringIndex = 0; stringIndex < states.Count; stringIndex++)
            {
                if (states[stringIndex].Type == JTokenType.Null)
                {
                    continue;
                }
                if (states[stringIndex].Type != JTokenType.Array)
                {
                    return false;
                }
                JArray stringStates = (JArray)states[stringIndex];
                if (stringStates.Count == 2 && stringStates[0].Type == JTokenType.Integer && stringStates[1].Type == JTokenType.Integer)
                {
                    int space = (int)stringStates[0];
                    MusicalTimespan sustain = MusicalTimespan.DeserializeInt32((int)stringStates[1]);
                    dest[new Fret(stringIndex, space)] = sustain;
                } else
                {
                    return false;
                }
            }
            return true;
        }

        private static Beat? beatFromStepJson(JObject stepJson)
        {
            Beat beat = new Beat();
            int? div = enforceJInteger(stepJson, "div");
            if (div.HasValue)
            {
                beat.BeatDivision = MusicalTimespan.DeserializeInt32(div.Value);
            } else
            {
                beat.BeatDivision = new MusicalTimespan(1, 8); // backwards compatibility
            }
            float? length = enforceJFloatOrInteger(stepJson, "length"); // check for old version "length" field on beat
            JArray? states = enforceJArray(stepJson, "states");
            if (length.HasValue)
            {
                if (oldReadBeatStates(beat, states, new MusicalTimespan(length.Value, 8)))
                {
                    return beat;
                } else
                {
                    return null;
                }
            } else
            {
                if (currentReadBeatStates(beat, states))
                {
                    return beat;
                } else
                {
                    return null;
                }
            }
        }

        private static Step? stepFromStepJson(JToken stepJson)
        {
            if (stepJson.Type != JTokenType.Object)
            {
                return null;
            }
            JObject stepJsonObj = (JObject)stepJson;
            string? type = enforceJString(stepJsonObj, "type");
            if (type == null)
            {
                return null;
            }
            if (type == "b")
            {
                return beatFromStepJson(stepJsonObj);
            } else if (type == "sh")
            {
                return sectionHeadFromStepJson(stepJsonObj);
            } else
            {
                return null;
            }
        }

        private static Tab? tabFromStepsJson(JArray stepsJson, Tuning tuning)
        {
            Tab result = new Tab();
            result.Tuning = tuning;
            result.SetLength(stepsJson.Count, MusicalTimespan.Zero);
            int i = 0;
            foreach (JToken stepJson in stepsJson)
            {
                Step? step = stepFromStepJson(stepJson);
                if (step == null)
                {
                    return null;
                }
                result[i] = step;
                i++;
            }
            return result;
        }

        private static Tuning? tuningFromJson(JArray tuningJson)
        {
            List<string> tuning = new List<string>();
            foreach (JToken token in tuningJson)
            {
                if (token.Type != JTokenType.String)
                {
                    return null;
                }
                tuning.Add(token.ToString());
            }
            tuning.Reverse();
            try
            {
                Tuning result = new Tuning(tuning.ToArray());
                return result;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static string? enforceJString(JObject parent, string name)
        {
            if (parent.ContainsKey(name) && parent[name].Type == JTokenType.String)
            {
                return (string)parent[name];
            } else
            {
                return null;
            }
        }
        private static int? enforceJInteger(JObject parent, string name)
        {
            if (parent.ContainsKey(name) && parent[name].Type == JTokenType.Integer)
            {
                return (int)parent[name];
            } else
            {
                return null;
            }
        }
        private static float? enforceJFloatOrInteger(JObject parent, string name)
        {
            if (parent.ContainsKey(name) && (parent[name].Type == JTokenType.Integer || parent[name].Type == JTokenType.Float))
            {
                return parent[name].Value<float>();
            } else
            {
                return null;
            }
        }
        private static JObject enforceJObject(JObject parent, string name)
        {
            if (parent.ContainsKey(name) && parent[name].Type == JTokenType.Object)
            {
                return (JObject)parent[name];
            } else
            {
                return null;
            }
        }
        private static JArray enforceJArray(JObject parent, string name)
        {
            if (parent.ContainsKey(name) && parent[name].Type == JTokenType.Array)
            {
                return (JArray)parent[name];
            }
            else
            {
                //Debug.WriteLine(name);
                return null;
            }
        }
        private static bool? enforceJBool(JObject parent, string name)
        {
            if (parent.ContainsKey(name) && parent[name].Type == JTokenType.Boolean)
            {
                return (bool)parent[name];
            } else
            {
                return null;
            }
        }
    }
}
