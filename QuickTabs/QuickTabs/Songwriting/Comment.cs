﻿using Newtonsoft.Json.Linq;
using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    public class Comment : Step
    {
        public override StepType Type => StepType.Annotation;

        public override JObject SaveAsJObject(Song Song)
        {
            throw new NotImplementedException();
        }
    }
}
