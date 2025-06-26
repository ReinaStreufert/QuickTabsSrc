using QuickTabs.Configuration;
using QuickTabs.Controls;
using QuickTabs.Synthesization;
using System.Windows;

namespace QuickTabs
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CrashManager.Initialize();
            QTPersistence.Current.Initialize();
            CrashManager.NotifyPersistenceUpdate();
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);

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
                if (QTPersistence.Current.EnableAutoUpdate)
                {
                    Updater.Initialize();
                }
                Application.Run(splash);
            }
        }
    }
}