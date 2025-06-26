using Newtonsoft.Json.Linq;
using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public class Annotation : Step
    {
        public override StepType Type => StepType.Annotation;

        public string Text { get; set; } = string.Empty;
        public bool IsChordAnnotation { get; set; }
        public MusicalTimespan Duration { get; set; }

        public override JObject SaveAsJObject(Song Song)
        {
            throw new NotImplementedException();
        }
    }
}
