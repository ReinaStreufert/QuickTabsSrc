using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class FretboardConfig : PreferenceCategory
    {
        public override string Title => "Fretboard";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                yield return new CheckPreference("Larger fretboard", QTPersistence.Keys.ViewLargeFretboard, DrawingIcons.LargeFretboard);
                yield return new CheckPreference("Show fret counter", QTPersistence.Keys.ViewFretCounter, DrawingIcons.Counter);
                yield return new CheckPreference("Show fret navigation dots", QTPersistence.Keys.ViewNavDots, DrawingIcons.Dots);
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            if (changedPrefName == "Larger fretboard")
            {
                editorForm.RefreshLayout();
            } else
            {
                fretboard.Refresh();
            }
        }
    }
}
