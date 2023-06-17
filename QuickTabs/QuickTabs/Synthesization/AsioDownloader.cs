using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    internal static class AsioDownloader
    {
        public static event Action DownloadFailed;  // this event may be invoked outside of the main thread

        private const string asioInstallerUrl = "https://asio4all.org/downloads/ASIO4ALL_2_15_English.exe";

        private static HttpClient httpClient = null;
        private static string exePath;

        public static void DownloadAndInstall()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }
            httpClient.GetByteArrayAsync(asioInstallerUrl).ContinueWith(asioInstallerReceived);
        }

        private static void asioInstallerReceived(Task<byte[]> task)
        {
            if (task.IsFaulted)
            {
                DownloadFailed?.Invoke();
                return;
            }
            exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Guid.NewGuid().ToString() + ".exe");
            File.WriteAllBytes(exePath, task.Result);
            ProcessStartInfo psi = new ProcessStartInfo(exePath);
            psi.UseShellExecute = true; // show UAC message to user instead of crashing because installer requires elevation
            Process installerProcess = Process.Start(psi);
            installerProcess.EnableRaisingEvents = true;
            installerProcess.Exited += installerProcessExited;
        }
        private static void installerProcessExited(object? sender, EventArgs e)
        {
            File.Delete(exePath);
        }
    }
}
