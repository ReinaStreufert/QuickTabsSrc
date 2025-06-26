using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    public class ConstantVolume : IVolumeProvider
    {
        private float volume;
        public float Volume => volume;

        public ConstantVolume(float volume)
        {
            this.volume = volume;
        }
    }
}
