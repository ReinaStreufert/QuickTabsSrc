using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public class Section // used only by Song.GetSectionGraph()
    {
        public string SectionName { get; set; }
        public MusicalTimespan FirstBeatPosition { get; set; }
        public Dictionary<Track, Beat[]> Content { get; set; }
    }
}
