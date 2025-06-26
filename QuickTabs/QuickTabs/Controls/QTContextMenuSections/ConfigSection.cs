using QuickTabs.Configuration;
using QuickTabs.Enums;
using QuickTabs.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection configSection;
        private ContextItem allPreferences;

        private void setupConfigSection()
        {
            configSection = new ContextSection();
            configSection.SectionName = "Config";
            configSection.ToggleType = ToggleType.Togglable;
            addConfigItems();
            allPreferences = new ContextItem(DrawingIcons.Preferences, "All preferences");
            allPreferences.Selected = true;
            allPreferences.ExcludeFromToggle = true;
            allPreferences.Click += allPreferencesClick;
            configSection.AddItem(allPreferences);
            Sections.Add(configSection);
        }

        public void RefreshPinnedConfig()
        {
            configSection.ClearItems();
            addConfigItems();
            configSection.AddItem(allPreferences);
            updateUI();
            this.Invalidate();
        }

        private void addConfigItems()
        {
            // add pinned preferences
            string[] pinnedPrefNames = QTPersistence.Current.ConfigPinnedPrefs;
            foreach (PreferenceCategory prefCat in PreferenceConfiguration.Config)
            {
                foreach (Preference pref in prefCat.GeneratePage(EditorForm, editor, this, Fretboard))
                {
                    if (pinnedPrefNames.Contains(pref.Name))
                    {
                        ContextItem item = pref.GenerateContextItem();
                        configSection.AddItem(item);
                    }
                }
            }
        }
        private void allPreferencesClick(object sender, ContextItem.ContextItemClickEventArgs e)
        {
            using (Preferences allPreferences = new Preferences(EditorForm, editor, Fretboard, this))
            {
                allPreferences.ShowDialog();
            }
        }
    }
}
