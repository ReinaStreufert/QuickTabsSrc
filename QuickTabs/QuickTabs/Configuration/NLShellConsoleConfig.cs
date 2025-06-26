using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class NLShellConsoleConfig : PreferenceCategory
    {
        public override string Title => "NLShell debug console";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                yield return new ButtonPreference("Open debug console", DrawingIcons.Dots);
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            if (!NLShellDebugConsole.IsConsoleEnabled)
            {
                NLShellDebugConsole.Enable();
            } else
            {
                NLShellDebugConsole.Disable();
            }
        }
    }
}
