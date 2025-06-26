using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class EditorConfig : PreferenceCategory
    {
        public override string Title => "Editor";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                yield return new CheckPreference("Show mid lines", QTPersistence.Keys.ViewMidLines, DrawingIcons.MidLines);
                yield return new MusicalTimespanPreference("Mid line interval", QTPersistence.Keys.MidlineInterval, DrawingIcons.QuarterNote);
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            tabEditor.Invalidate();
        }
    }
}
