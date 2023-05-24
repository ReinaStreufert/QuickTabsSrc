using Newtonsoft.Json.Linq;
using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal class SectionHead : Step
    {
        public override StepType Type => StepType.SectionHead;

        public string Name { get; set; } = "";

        public override JObject SaveAsJObject(Song Song)
        {
            JObject sectionHeadJson = new JObject();
            sectionHeadJson.Add("type", "sh");
            sectionHeadJson.Add("name", Name);
            return sectionHeadJson;
        }
    }
}
