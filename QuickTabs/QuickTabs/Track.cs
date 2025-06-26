using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    public class Track
    {
        public string Name { get; set; } = "Untitled track";
        public Tab Tab { get; set; } = new Tab();
        public bool Mute { get; set; } = false;
        public bool Solo { get; set; } = false;
        public float Volume { get; set; } = 1.0F;
        public bool NamedByUser { get; set; } = false;
        public bool IsAnnotationTrack { get; set; } = false;
        public void UpdateAutoName()
        {
            if (NamedByUser)
            {
                return;
            }
            if (IsAnnotationTrack)
            {
                Name = "Annotation track";
                return;
            }
            Tuning tabTuning = Tab.Tuning;
            if (tabTuning.Equals(Tuning.StandardGuitar) || tabTuning.Equals(Tuning.DropD))
            {
                Name = "Guitar track";
            } else if (tabTuning.Equals(Tuning.StandardUke))
            {
                Name = "Ukulele track";
            } else if (tabTuning.Equals(Tuning.StandardBass))
            {
                Name = "Bass guitar track";
            } else
            {
                Name = "Untitled track";
            }
        }
    }
}
