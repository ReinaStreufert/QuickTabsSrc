using nlshell;
using QuickTabs.Controls;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    public static class NLShellDebugConsole
    {
        public static NLShellContext NLShellContext { get; private set; }
        public static bool IsConsoleEnabled { get; private set; } = false;

        public static Editor MainForm { get; set; }
        public static Fretboard Fretboard { get; set; }
        public static QuickTabsContextMenu ContextMenu { get; set; }
        public static TabEditor TabEditor { get; set; }
        public static SequencePlayer Player { get; set; }

        private static CancellationTokenSource? consoleCancellationTokenSource;

        public static void Enable()
        {
            AllocConsole();
            //Process.Start("wt.exe");
            consoleCancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                NLShellContext = new NLShellContext();
                NLShellContext.Start("shellsources\\wake.nlisp");
            }, consoleCancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            IsConsoleEnabled = true;
        }

        public static void Disable()
        {
            consoleCancellationTokenSource.Cancel();
            FreeConsole();
            IsConsoleEnabled = false;
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
    }
}
