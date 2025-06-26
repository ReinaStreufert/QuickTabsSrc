using QuickTabs.Controls;
using QuickTabs.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class UpdaterConfig : PreferenceCategory
    {
        public override string Title => "Updates";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                yield return new CheckPreference("Enable auto updater", QTPersistence.Keys.EnableAutoUpdate);
                yield return new ButtonPreference("Show release notes");
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            if (changedPrefName == "Show release notes")
            {
                using (ReleaseNotes releaseNotes = new ReleaseNotes())
                {
                    releaseNotes.ShowDialog();
                }
            }
        }
    }
}
