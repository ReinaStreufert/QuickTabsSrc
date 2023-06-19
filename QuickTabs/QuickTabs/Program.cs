using QuickTabs.Controls;
using QuickTabs.Synthesization;
using System.Windows;

namespace QuickTabs
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CrashManager.Initialize();
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            QTSettings.Current.Initialize();

            Environment.CurrentDirectory = Path.GetDirectoryName(InstallOperations.SelfExe);
            if (!Directory.Exists("icons") || !Directory.Exists("fonts"))
            {
                // launch installer
                if (!InstallOperations.IsElevated)
                {
                    if (!InstallOperations.RestartElevated())
                    {
                        return; // exit
                    }
                }
                Application.Run(new Forms.Installer());
            } else
            {
                // startup normally
                Task iconLoader = new Task(DrawingIcons.LoadAll);
                iconLoader.Start();
                Forms.Splash splash = new Forms.Splash(iconLoader);
                Updater.Initialize();
                Application.Run(splash);
            }
        }
    }
}