using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public abstract class PreferenceCategory
    {
        public abstract string Title { get; }

        public IEnumerable<Preference> GeneratePage(Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            IEnumerable<Preference> content = preferences.ToArray();
            foreach (Preference preference in content)
            {
                preference.ValueChange += (string extraInfo) => 
                {
                    string changedPrefParam = preference.Name;
                    if (extraInfo != "")
                    {
                        changedPrefParam = changedPrefParam + "." + extraInfo;
                    }
                    refreshLiveApplication(changedPrefParam, editorForm, tabEditor, ctxMenu, fretboard);
                };
            }
            return content;
        }

        protected abstract IEnumerable<Preference> preferences { get; }
        protected abstract void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard);
    }
}
