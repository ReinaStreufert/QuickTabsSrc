using Newtonsoft.Json.Linq;
using QuickTabs.Forms;
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
        private static bool isSaved = true;
        public static bool IsSaved
        {
            get
            {
                return isSaved;
            }
            private set
            {
                bool oldValue = isSaved;
                isSaved = value;
                if (oldValue != value)
                {
                    FileStateChange?.Invoke();
                }
            }
        }
        private static string currentFilePath = "";
        public static string CurrentFilePath
        {
            get
            {
                return currentFilePath;
            }
            private set
            {
                string oldValue = currentFilePath;
                currentFilePath = value;
                if (oldValue != value)
                {
                    FileStateChange?.Invoke();
                }
            }
        }
        public static event Action FileStateChange;

        public static void Initialize()
        {
            History.SubstantialChange += History_SubstantialChange;
        }

        private static void History_SubstantialChange()
        {
            IsSaved = false;
        }

        public static void Save(Song song)
        {
            if (CurrentFilePath == "")
            {
                SaveAs(song); return;
            }
            IsSaved = true;
            if (File.Exists(CurrentFilePath))
            {
                File.Delete(CurrentFilePath);
            }
            File.WriteAllText(CurrentFilePath, song.SaveAsJObject(song).ToString());
        }
        public static void New()
        {
            CurrentFilePath = "";
            IsSaved = true;
        }
        public static void SaveAs(Song song)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "QuickTabs File (*.qtjson)|*.qtjson|JSON File (*.json)|*.json|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "qtjson";
                DialogResult saveResult = saveDialog.ShowDialog();
                if (saveResult == DialogResult.OK)
                {
                    CurrentFilePath = saveDialog.FileName;
                    Save(song);
                }
            }
        }
        public static Song Open(out bool failed)
        {
            string newFilePath;
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "QuickTabs File (*.qtjson)|*.qtjson|JSON File (*.json)|*.json|All Files (*.*)|*.*";
                DialogResult openResult = openDialog.ShowDialog();
                if (openResult != DialogResult.OK)
                {
                    failed = false;
                    return null;
                }
                newFilePath = openDialog.FileName;
            }
            string fileText = File.ReadAllText(newFilePath);
            JObject songJson;
            try
            {
                songJson = JObject.Parse(fileText);
            } catch
            {
                failed = true;
                return null;
            }
            Song song = new Song();
            if (songJson.ContainsKey("name") && songJson["name"].Type == JTokenType.String)
            {
                song.Name = songJson["name"].ToString();
            } else
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
                } else
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
                song.Tab.Tuning = new Tuning(tuning.ToArray());
            } else
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
                                } else
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
                    } else
                    {
                        failed = true;
                        return null;
                    }
                    stepIndex++;
                }
            }
            CurrentFilePath = newFilePath;
            IsSaved = true;
            failed = false;
            return song;
        }
    }
}
