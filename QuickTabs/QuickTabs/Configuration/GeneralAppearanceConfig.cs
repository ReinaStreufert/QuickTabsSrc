using QuickTabs.Controls;
using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class GeneralAppearanceConfig : PreferenceCategory
    {
        public override string Title => "General appearance";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                yield return new CheckPreference("Dark mode", QTPersistence.Keys.ViewDarkMode, DrawingIcons.DarkMode);
                // TODO: possibly UI scale?
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            if (changedPrefName == "Dark mode")
            {
                if (QTPersistence.Current.ViewDarkMode)
                {
                    DrawingConstants.SetTheme(Theme.DarkMode);
                }
                else
                {
                    DrawingConstants.SetTheme(Theme.LightMode);
                }
                deepInvalidate(editorForm); // invalidate EVERYTHING
            }
        }
        private void deepInvalidate(Control control)
        {
            control.Invalidate();
            foreach (Control child in control.Controls)
            {
                deepInvalidate(child); // recursion moment
            }
        }
    }
}
