using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal static class ShortcutManager
    {
        private static List<Shortcut> shortcuts = new List<Shortcut>();
        public static ShortcutController AddShortcut(Keys Modifiers, Keys Key, Action Handler)
        {
            Shortcut shortcut = new Shortcut();
            shortcut.Modifiers = Modifiers;
            shortcut.Key = Key;
            shortcut.Handler = Handler;
            shortcuts.Add(shortcut);
            return shortcut;
        }
        public static void ProcessShortcut(Keys Modifiers, Keys Key)
        {
            foreach (Shortcut shortcut in shortcuts)
            {
                if (shortcut.Enabled && shortcut.Modifiers == Modifiers && shortcut.Key == Key)
                {
                    shortcut.Handler();
                }
            }
        }
        public class ShortcutController
        {
            public bool Enabled { get; set; } = true;
        }
        private class Shortcut : ShortcutController
        {
            public Keys Modifiers { get; set; }
            public Keys Key { get; set; }
            public Action Handler { get; set; }
        }
    }
}
