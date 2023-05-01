using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    internal class Tab
    {
        private List<Step> steps = new List<Step>();
        public Tuning Tuning { get; set; } = Tuning.StandardGuitar;
    }
}
