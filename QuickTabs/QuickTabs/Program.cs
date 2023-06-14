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
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Task iconLoader = new Task(DrawingIcons.LoadAll);
            iconLoader.Start();
            Updater.Initialize();
            Application.Run(new Forms.Splash(iconLoader));
        }
    }
}