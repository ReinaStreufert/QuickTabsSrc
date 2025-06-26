using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public static class PreferenceConfiguration
    {
        static PreferenceCategory[] config = null;
        public static PreferenceCategory[] Config
        {
            get
            {
                if (config == null)
                {
                    config = new PreferenceCategory[]
                    { 
                        new GeneralAppearanceConfig(),
                        new FretboardConfig(),
                        new CtxMenuConfig(),
                        new EditorConfig(),
                        new DivisionScalingConfig(),
                        new PlayerConfig(),
                        new UpdaterConfig(),
                        new CrashConfig(),
                        #if DEBUG
                        new NLShellConsoleConfig()
                        #endif
                    };
                }
                return config;
            }
        }
    }
}
