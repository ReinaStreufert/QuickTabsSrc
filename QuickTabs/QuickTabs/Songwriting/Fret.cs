using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public struct Fret
    {
        public int String { get; set; }
        public int Space { get; set; }

        public Fret(int String, int Space)
        {
            this.String = String;
            this.Space = Space;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj.GetType() != typeof(Fret)) return false;
            Fret otherFret = (Fret)obj;
            return (String == otherFret.String && Space == otherFret.Space);
        }

        public static bool operator ==(Fret c1, Fret c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(Fret c1, Fret c2)
        {
            return !c1.Equals(c2);
        }

        public override int GetHashCode()
        {
            return (String + Space) * (String + Space + 1) / 2 + String;
        }
    }
}
