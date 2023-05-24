using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuickTabs.Songwriting;

namespace QuickTabs
{
    internal class Song : IJsonSaveable
    {
        public string Name { get; set; } = "Untitled tab";
        public int Tempo { get; set; } = 120;
        public TimeSignature TimeSignature { get; set; } = new TimeSignature(4, 4);
        public Tab Tab { get; set; } = new Tab();

        public JObject SaveAsJObject(Song Song)
        {
            JObject songJson = new JObject();
            songJson.Add("name", Name);
            songJson.Add("tempo", Tempo);
            JArray tsJson = new JArray();
            tsJson.Add(TimeSignature.T1);
            tsJson.Add(TimeSignature.T2);
            songJson.Add("ts", tsJson);
            JArray tuningJson = new JArray();
            for (int i = Tab.Tuning.Count - 1; i >= 0; i--)
            {
                tuningJson.Add(Tab.Tuning[i]);
            }
            songJson.Add("tuning", tuningJson);
            JArray stepsJson = new JArray();
            foreach (Step step in Tab)
            {
                stepsJson.Add(step.SaveAsJObject(this));
            }
            songJson.Add("steps", stepsJson);
            return songJson;
        }
    }
}
