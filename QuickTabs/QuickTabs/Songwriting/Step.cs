﻿using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal abstract class Step
    {
        public int IndexWithinTab;
        public abstract StepType Type { get; }
    }
}
