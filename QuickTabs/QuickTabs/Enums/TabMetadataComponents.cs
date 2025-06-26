using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Enums
{
    [Flags]
    public enum TabMetadataComponents : byte
    {
        None = 0,
        Name = 1,
        Tempo = 2,
        TimeSignature = 4,
        ExactTuning = 8
    }
}
