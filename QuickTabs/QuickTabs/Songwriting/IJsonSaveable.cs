using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public interface IJsonSaveable
    {
        public JObject SaveAsJObject(Song Song);
    }
}
