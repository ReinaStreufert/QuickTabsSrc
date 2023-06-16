using IWshRuntimeLibrary;
using Newtonsoft.Json.Linq;
using QuickTabs.Forms;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace QuickTabs
{
    internal static class InstallOperations
    {
        public static event Action InstallStarted;          //  ]
        public static event Action<string> InstallComplete; //  ]
        public static event Action<string> InstallFailed; //    ] may be called outside of the main thread

        public static bool IsElevated
        {
            get
            {
                return isAdministrator();
            }
        }
        public static string DefaultInstallDir
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\QuickTabs";
            }
        }
        public static string SelfExe
        {
            get
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            }
        }

        private static string originalExePath = "";
        private static string startShortcutPath = "";
        private static string resolvedInstallDir = "";
        private static HttpClient httpClient = null;

        public static void RestartElevated()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(SelfExe);
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";
            System.Diagnostics.Process.Start(startInfo);
            AudioEngine.Stop();
            Environment.Exit(0);
        }
        public static void StartInstall(string installDir, bool createStartShortcut)
        {
            resolvedInstallDir = installDir.Replace('/', '\\').TrimEnd('\\');
            Directory.CreateDirectory(resolvedInstallDir);
            try
            {
                clearDirectory(resolvedInstallDir);
            } catch
            {
                onFail("Check to make sure you have file permissions at install location.");
                return;
            }
            string newExePath = installDir + "\\QuickTabs.exe";
            originalExePath = SelfExe;
            try
            {
                File.Move(originalExePath, newExePath);
            } catch
            {
                onFail("Check to make sure you have file permissions at install location.");
                return;
            }
            if (createStartShortcut)
            {
                startShortcutPath = addShortcut(newExePath, "QuickTabs", "Start QuickTabs");
            } else
            {
                startShortcutPath = "";
            }
            InstallStarted?.Invoke();
            if (httpClient == null)
            {
                httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMilliseconds(4000);
            }
            httpClient.GetStringAsync(Updater.VersionStatusUrl).ContinueWith(statusReceived);
        }

        private static bool isAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private static void clearDirectory(string dirPath)
        {
            foreach (string file in Directory.GetFiles(dirPath))
            {
                File.Delete(file);
            }
            foreach (string dir in Directory.GetDirectories(dirPath))
            {
                Directory.Delete(dir, true);
            }
        }
        private static string addShortcut(string pathToExe, string name, string description) // returns created shortcut folder
        {
            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", name);

            if (!Directory.Exists(appStartMenuPath))
                Directory.CreateDirectory(appStartMenuPath);

            string shortcutLocation = Path.Combine(appStartMenuPath, name + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = description;
            shortcut.TargetPath = pathToExe;
            shortcut.Save();

            return appStartMenuPath;
        }
        private static void statusReceived(Task<string> task)
        {
            if (task.IsFaulted)
            {
                onFail("Could not download external dependencies. Check internet connection.");
                return;
            }
            JObject statusJson;
            try
            {
                statusJson = JObject.Parse(task.Result);
            } catch
            {
                onFail("There was an error while attempting to download external dependencies.");
                return;
            }
            if (statusJson.ContainsKey("install-image") && statusJson["install-image"].Type == JTokenType.String)
            {
                httpClient.GetByteArrayAsync(statusJson["install-image"].ToString()).ContinueWith(installImageReceived);
            } else
            {
                onFail("There was an error while attempting to download external dependencies.");
                return;
            }
        }
        private static void installImageReceived(Task<byte[]> task)
        {
            if (task.IsFaulted)
            {
                onFail("There was an error while attempting to download external dependencies.");
                return;
            }
            string zipPath = resolvedInstallDir + "\\install.zip";
            File.WriteAllBytes(zipPath, task.Result);
            try
            {
                ZipFile.ExtractToDirectory(zipPath, resolvedInstallDir);
            } catch
            {
                onFail("Failed to extract dependencies.");
                return;
            }
            File.Delete(zipPath);
            InstallComplete?.Invoke(resolvedInstallDir + "\\QuickTabs.exe");
        }
        private static void onFail(string errMessage)
        {
            // attempt to cleanup install
            if (startShortcutPath != "")
            {
                try
                {
                    Directory.Delete(startShortcutPath, true);
                } catch { }
            }
            if (resolvedInstallDir != "")
            {
                string expectedInstalledExePath = resolvedInstallDir + "\\QuickTabs.exe";
                if (File.Exists(expectedInstalledExePath))
                {
                    try
                    {
                        File.Move(expectedInstalledExePath, originalExePath);
                    } catch { }
                }
                try
                {
                    Directory.Delete(resolvedInstallDir, true);
                }
                catch { }
            }

            InstallFailed?.Invoke(errMessage);
        }
    }
}
