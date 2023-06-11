using Newtonsoft.Json.Linq;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal class QtJsonFormat : FileFormat
    {
        public override string Extension => ".qtjson";
        public override string Name => "QuickTabs File (*.qtjson)";

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
            Song song = new Song();
            if (songJson.ContainsKey("name") && songJson["name"].Type == JTokenType.String)
            {
                song.Name = songJson["name"].ToString();
            }
            else
            {
                failed = true;
                return null;
            }
            if (songJson.ContainsKey("tempo") && songJson["tempo"].Type == JTokenType.Integer)
            {
                song.Tempo = (int)(songJson["tempo"]);
            }
            else
            {
                failed = true;
                return null;
            }
            if (songJson.ContainsKey("ts") && songJson["ts"].Type == JTokenType.Array)
            {
                JArray tsJson = (JArray)(songJson["ts"]);
                if (tsJson.Count == 2 && tsJson[0].Type == JTokenType.Integer && tsJson[1].Type == JTokenType.Integer)
                {
                    song.TimeSignature = new Songwriting.TimeSignature((int)tsJson[0], (int)tsJson[1]);
                }
                else
                {
                    failed = true;
                    return null;
                }
            }
            else
            {
                failed = true;
                return null;
            }
            if (songJson.ContainsKey("tuning") && songJson["tuning"].Type == JTokenType.Array)
            {
                List<string> tuning = new List<string>();
                foreach (JToken token in songJson["tuning"])
                {
                    if (token.Type != JTokenType.String)
                    {
                        failed = true;
                        return null;
                    }
                    tuning.Add(token.ToString());
                }
                tuning.Reverse();
                try
                {
                    song.Tab.Tuning = new Tuning(tuning.ToArray());
                } catch (FormatException)
                {
                    failed = true;
                    return null;
                }
            }
            else
            {
                failed = true;
                return null;
            }
            if (songJson.ContainsKey("steps") && songJson["steps"].Type == JTokenType.Array)
            {
                song.Tab.SetLength(((JArray)songJson["steps"]).Count);
                int stepIndex = 0;
                foreach (JToken token in songJson["steps"])
                {
                    if (token.Type != JTokenType.Object)
                    {
                        failed = true;
                        return null;
                    }
                    JObject stepJson = (JObject)token;
                    if (stepJson.ContainsKey("type") && stepJson["type"].Type == JTokenType.String)
                    {
                        string typeCode = stepJson["type"].ToString();
                        switch (typeCode)
                        {
                            case "sh":
                                SectionHead sectionHead = new SectionHead();
                                if (stepJson.ContainsKey("name") && stepJson["name"].Type == JTokenType.String)
                                {
                                    sectionHead.Name = stepJson["name"].ToString();
                                }
                                else
                                {
                                    failed = true;
                                    return null;
                                }
                                song.Tab[stepIndex] = sectionHead;
                                break;
                            case "b":
                                Beat beat = new Beat();
                                if (stepJson.ContainsKey("length") && stepJson["length"].Type == JTokenType.Integer)
                                {
                                    beat.NoteLength = (int)stepJson["length"];
                                }
                                else
                                {
                                    failed = true;
                                    return null;
                                }
                                if (stepJson.ContainsKey("states") && stepJson["states"].Type == JTokenType.Array)
                                {
                                    JArray statesJson = (JArray)stepJson["states"];
                                    if (statesJson.Count != song.Tab.Tuning.Count)
                                    {
                                        failed = true;
                                        return null;
                                    }
                                    for (int stringIndex = 0; stringIndex < statesJson.Count; stringIndex++)
                                    {
                                        if (statesJson[stringIndex].Type == JTokenType.Integer)
                                        {
                                            int space = (int)statesJson[stringIndex];
                                            beat[new Fret(stringIndex, space)] = true;
                                        }
                                    }
                                }
                                else
                                {
                                    failed = true;
                                    return null;
                                }
                                song.Tab[stepIndex] = beat;
                                break;
                            default:
                                failed = true;
                                return null;
                        }
                    }
                    else
                    {
                        failed = true;
                        return null;
                    }
                    stepIndex++;
                }
            }
            failed = false;
            return song;
        }

        public override void Save(Song song, string fileName)
        {
            File.WriteAllText(fileName, song.SaveAsJObject(song).ToString());
        }
    }
}
