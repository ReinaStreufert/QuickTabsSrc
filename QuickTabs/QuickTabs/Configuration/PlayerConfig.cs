using QuickTabs.Controls;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class PlayerConfig : PreferenceCategory
    {
        public override string Title => "Player";

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                string[] outDevs = NAudio.Wave.AsioOut.GetDriverNames();
                string[] options = new string[outDevs.Length + 1];
                options[0] = "Default device";
                outDevs.CopyTo(options, 1);
                yield return new CheckPreference("Enable preview play", QTPersistence.Keys.EnablePreviewPlay, DrawingIcons.PreviewPlay);
                yield return new ComboPreference("ASIO output device", QTPersistence.Keys.AsioOutputDevice, options, DrawingIcons.OutputDevice);
                yield return new ButtonPreference("Open ASIO control panel", DrawingIcons.AsioControlPanel);
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            if (changedPrefName == "ASIO output device")
            {
                AudioEngine.Reinitialize();
            } else if (changedPrefName == "Open ASIO control panel")
            {
                AudioEngine.OpenControlPanel();
            }
        }
    }
}
