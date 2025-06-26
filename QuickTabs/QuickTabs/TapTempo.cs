using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    public class TapTempo
    {
        public delegate void SetTempo(int bpm);

        public TimeSignature TimeSignature
        {
            set
            {
                maxTap = value.MeasureLength / new MusicalTimespan(1, 4);
            }
        }
        public event SetTempo OnSetTempo;

        private int taps = 0;
        private int maxTap;
        private DateTime firstTap;
        private DateTime lastTap;

        public TapTempo()
        {
            TimeSignature = new TimeSignature(4, 4);
        }

        public void Tap()
        {
            DateTime tapTime = DateTime.Now;
            if (taps == 0)
            {
                firstTap = tapTime;
            } else if ((tapTime - lastTap).TotalSeconds > 2)
            {
                firstTap = tapTime;
                taps = 0;
            }
            taps++;
            if (taps >= maxTap)
            {
                taps = 0;
                TimeSpan elapsed = tapTime - firstTap;
                double measureLength = elapsed.TotalSeconds / ((maxTap - 1D) / maxTap); // i think this might be right?
                double bpm = (maxTap / measureLength) * 60D;
                OnSetTempo?.Invoke((int)Math.Round(bpm));
            }
            lastTap = tapTime;
        }
    }
}
