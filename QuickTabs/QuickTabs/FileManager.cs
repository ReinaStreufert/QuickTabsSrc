using Newtonsoft.Json.Linq;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal static class FileManager
    {
        public static string CurrentFilePath { get; private set; } = "";
        public static void Save(Song song)
        {
            if (CurrentFilePath == "")
            {
                SaveAs(song); return;
            }
            if (File.Exists(CurrentFilePath))
            {
                File.Delete(CurrentFilePath);
            }
            File.WriteAllText(CurrentFilePath, song.SaveAsJObject(song).ToString());
        }
        public static void New()
        {
            CurrentFilePath = "";
        }
        public static void SaveAs(Song song)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "QuickTabs File (*.qtjson)|*.qtjson|JSON File (*.json)|*.json|All Files (*.*)|*.*";
            saveDialog.DefaultExt = "qtjson";
            saveDialog.ShowDialog();
            CurrentFilePath = saveDialog.FileName;
            Save(song);
        }
        public static Song Open()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "QuickTabs File (*.qtjson)|*.qtjson|JSON File (*.json)|*.json|All Files (*.*)|*.*";
            openDialog.ShowDialog();
            CurrentFilePath = openDialog.FileName;
            string fileText = File.ReadAllText(CurrentFilePath);
            JObject songJson;
            try
            {
                songJson = JObject.Parse(fileText);
            } catch
            {
                return null;
            }
            Song song = new Song();
            if (songJson.ContainsKey("name") && songJson["name"].Type == JTokenType.String)
            {
                song.Name = songJson["name"].ToString();
            } else
            {
                return null;
            }
            if (songJson.ContainsKey("tempo") && songJson["tempo"].Type == JTokenType.Integer)
            {
                song.Tempo = (int)(songJson["tempo"]);
            }
            else
            {
                return null;
            }
            if (songJson.ContainsKey("ts") && songJson["ts"].Type == JTokenType.Array)
            {
                JArray tsJson = (JArray)(songJson["ts"]);
                if (tsJson.Count == 2 && tsJson[0].Type == JTokenType.Integer && tsJson[1].Type == JTokenType.Integer)
                {
                    song.TimeSignature = new Songwriting.TimeSignature((int)tsJson[0], (int)tsJson[1]);
                } else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            if (songJson.ContainsKey("tuning") && songJson["tuning"].Type == JTokenType.Array)
            {
                List<char> tuning = new List<char>();
                foreach (JToken token in songJson["tuning"])
                {
                    if (token.Type != JTokenType.String)
                    {
                        return null;
                    }
                    tuning.Add(token.ToString()[0]);
                }
                tuning.Reverse();
                song.Tab.Tuning = new Tuning(tuning.ToArray());
            } else
            {
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
                                } else
                                {
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
                                    return null;
                                }
                                if (stepJson.ContainsKey("states") && stepJson["states"].Type == JTokenType.Array)
                                {
                                    JArray statesJson = (JArray)stepJson["states"];
                                    if (statesJson.Count != song.Tab.Tuning.Count)
                                    {
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
                                    return null;
                                }
                                song.Tab[stepIndex] = beat;
                                break;
                            default:
                                return null;
                        }
                    } else
                    {
                        return null;
                    }
                    stepIndex++;
                }
            }
            return song;
        }
    }
}
