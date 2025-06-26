using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    public class TrackVolumeProvider : IVolumeProvider
    {
        private Track track;

        public float Volume
        {
            get
            {
                return track.Volume;
            }
        }

        public TrackVolumeProvider(Track track)
        {
            this.track = track;
        }
    }
}
