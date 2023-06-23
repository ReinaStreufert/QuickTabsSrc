using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal struct MusicalTimespan
    {
        // changes the resolution at which MusicalTimespan internally stores time values
        // designed to be super easy to change the smallest division supported in the future if desired
        private const int smallestDivision = 16;

        private int absoluteValue = 0;

        public MusicalTimespan() { }
        public MusicalTimespan(int value, int beatDivision) // ex: new MusicalTimespan(3, 8) = 3 eighth notes
        {
            absoluteValue = value * (smallestDivision / beatDivision);
        }
        public MusicalTimespan(float value, int beatDivision)
        {
            absoluteValue = (int)Math.Round(value * (smallestDivision / beatDivision));
        }

        public int GetValueForBeatDivision(int beatDivision)
        {
            return absoluteValue / (smallestDivision / beatDivision);
        }
        public float GetValueForBeatDivisionF(int beatDivision)
        {
            return (float)absoluteValue / (smallestDivision / beatDivision);
        }
        public int DivideBy(MusicalTimespan timespan, out MusicalTimespan remainder)
        {
            (int result, int remainderAbsoluteValue) = Math.DivRem(absoluteValue, timespan.absoluteValue);
            remainder = new MusicalTimespan(remainderAbsoluteValue, smallestDivision);
            return result;
        }
        public bool IsDivisibleBy(MusicalTimespan timespan)
        {
            return (absoluteValue % timespan.absoluteValue == 0);
        }
        public float DivideByF(MusicalTimespan timespan)
        {
            return (float)absoluteValue / timespan.absoluteValue;
        }
        public bool IsExpressableInDivision(int beatDivision)
        {
            return (absoluteValue % (smallestDivision / beatDivision) == 0);
        }
        public TimeSpan ToTimespan(int BPM)
        {
            TimeSpan qNoteDuration = calculateQuarterNoteDuration(BPM);
            float quarterNotes = DivideByF(new MusicalTimespan(1, 4));
            return qNoteDuration * quarterNotes;
        }
        private TimeSpan calculateQuarterNoteDuration(int BPM)
        {
            float beatsPerSecond = BPM / 60F;
            float beatDurationMs = 1000 / beatsPerSecond;
            return TimeSpan.FromMilliseconds(beatDurationMs);
        }

        public override int GetHashCode()
        {
            return absoluteValue;
        }

        public static MusicalTimespan Zero
        {
            get
            {
                return new MusicalTimespan { absoluteValue = 0 };
            }
        }
        public static MusicalTimespan operator +(MusicalTimespan a, MusicalTimespan b)
        {
            MusicalTimespan result = new MusicalTimespan();
            result.absoluteValue = a.absoluteValue + b.absoluteValue;
            return result;
        }
        public static MusicalTimespan operator -(MusicalTimespan a, MusicalTimespan b)
        {
            MusicalTimespan result = new MusicalTimespan();
            result.absoluteValue = a.absoluteValue - b.absoluteValue;
            return result;
        }
        public static MusicalTimespan operator *(MusicalTimespan a, int b)
        {
            MusicalTimespan result = new MusicalTimespan();
            result.absoluteValue = a.absoluteValue * b;
            return result;
        }
        public static int operator /(MusicalTimespan a, MusicalTimespan b)
        {
            MusicalTimespan rem;
            return a.DivideBy(b, out rem);
        }
        public static bool operator >(MusicalTimespan a, MusicalTimespan b)
        {
            return (a.absoluteValue > b.absoluteValue);
        }
        public static bool operator <(MusicalTimespan a, MusicalTimespan b)
        {
            return (a.absoluteValue < b.absoluteValue);
        }
        public static bool operator >=(MusicalTimespan a, MusicalTimespan b)
        {
            return (a.absoluteValue >= b.absoluteValue);
        }
        public static bool operator <=(MusicalTimespan a, MusicalTimespan b)
        {
            return (a.absoluteValue <= b.absoluteValue);
        }
        public static bool operator ==(MusicalTimespan a, MusicalTimespan b)
        {
            return (a.absoluteValue == b.absoluteValue);
        }
        public static bool operator !=(MusicalTimespan a, MusicalTimespan b)
        {
            return (a.absoluteValue != b.absoluteValue);
        }
    }
}
