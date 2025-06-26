using Newtonsoft.Json.Linq;
using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public abstract class Step : IJsonSaveable
    {
        public int IndexWithinTab;
        public abstract StepType Type { get; }

        public abstract JObject SaveAsJObject(Song Song);
    }
}
