using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class DivisionScalingConfig : PreferenceCategory
    {
        public override string Title => "Division scaling";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                yield return new TwoStatePreference("Downscaling mode", QTPersistence.Keys.ScaleMergeSteps, "Delete extra steps", "Merge extra steps", DrawingIcons.Downscaling);
                yield return new CheckPreference("Ask about downscaling loss", QTPersistence.Keys.ScaleAskAboutLoss);
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            
        }
    }
}
