using QuickTabs.Controls;
using QuickTabs.Synthesization;

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
            Task iconLoader = new Task(DrawingIcons.LoadAll);
            iconLoader.Start();
            Application.Run(new Forms.Splash(iconLoader));
        }
    }
}