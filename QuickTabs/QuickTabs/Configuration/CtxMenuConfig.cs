using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class CtxMenuConfig : PreferenceCategory
    {
        public override string Title => "Context menu";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                yield return new CheckPreference("Compact context menu", QTPersistence.Keys.ViewCompactCtxMenu, DrawingIcons.CompactContextMenu);
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            if (changedPrefName == "Compact context menu")
            {
                if (QTPersistence.Current.ViewCompactCtxMenu)
                {
                    ctxMenu.Style = Enums.ContextMenuStyle.Collapsed;
                } else
                {
                    ctxMenu.Style = Enums.ContextMenuStyle.Responsive;
                }
                editorForm.RefreshLayout();
            }
        }
    }
}
