using Newtonsoft.Json.Linq;
using QuickTabs.Synthesization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal static class Updater
    {
        public static bool IsUpdating { get; private set; } = false;
        public static event Action UpdateStarted;   // ]
        public static event Action UpdateFailed;    // ] both of these events may be invoked outside of the main thread.

        public const int SelfReleaseVersion = 0;
        public const string SelfReleaseNotes = "working on update text";
        public const string VersionStatusUrl = "http://reinastreufert.github.io/QuickTabs/updater/status.json";
        public const string DevStatusUrl = "http://192.168.1.146:8080/updater/status.json";
        public const bool DevMode = true;

        public static bool WasJustUpdated { get; private set; } = false;

        private static HttpClient httpClient;
        private static HttpClient downloaderClient;
        private static bool devStatusFailed = false;

        public static void Initialize()
        {
            if (File.Exists("QuickTabsOld.exe"))
            {
                WasJustUpdated = true;
                Task.Delay(100).ContinueWith(completeUpdate); // wait for old QuickTabs to fully exit and release the file
            } else
            {
                httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                string vStatusUrl = VersionStatusUrl;
                if (DevMode)
                {
                    vStatusUrl = DevStatusUrl;
                }
                httpClient.GetStringAsync(vStatusUrl).ContinueWith(statusReceived);
            }
        }

        private static void statusReceived(Task<string> task)
        {
            if (!task.IsCompletedSuccessfully)
            {
                if (DevMode && !devStatusFailed)
                {
                    devStatusFailed = true;
                    httpClient.GetStringAsync(VersionStatusUrl).ContinueWith(statusReceived);
                }
                return;
            }
            JObject statusJson = JObject.Parse(task.Result);
            if (statusJson.ContainsKey("latest-version") && statusJson["latest-version"].Type == JTokenType.Integer)
            {
                int latestReleaseVersion = (int)statusJson["latest-version"];
                if (SelfReleaseVersion < latestReleaseVersion)
                {
                    update(statusJson);
                }
            } else
            {
                return;
            }
        }
        private static void update(JObject statusJson)
        {
            UpdateStarted?.Invoke();
            IsUpdating = true;
            if (!InstallOperations.IsElevated)
            {
                if (!InstallOperations.RestartElevated())
                {
                    IsUpdating = false;
                    UpdateFailed?.Invoke();
                    return;
                }
            }

            downloaderClient = new HttpClient();
            downloaderClient.Timeout = TimeSpan.FromSeconds(60); // binary downloads may take significantly longer

            string latestExeKey;
            if (Environment.Is64BitProcess)
            {
                latestExeKey = "latest-x64";
            } else
            {
                latestExeKey = "latest-x86";
            }
            string latestExecutableUrl;
            if (statusJson.ContainsKey(latestExeKey) && statusJson[latestExeKey].Type == JTokenType.String)
            {
                latestExecutableUrl = statusJson[latestExeKey].ToString();
            } else
            {
                IsUpdating = false;
                UpdateFailed?.Invoke();
                return;
            }

            Task exeTask;
            List<Task> dependencyTasks = new List<Task>();
            if (statusJson.ContainsKey("dependencies") && statusJson["dependencies"].Type == JTokenType.Array)
            {
                foreach (JToken token in statusJson["dependencies"])
                {
                    if (token.Type == JTokenType.Object)
                    {
                        JObject dependency = (JObject)token;
                        if (dependency.ContainsKey("name") && dependency["name"].Type == JTokenType.String && dependency.ContainsKey("url") && dependency["url"].Type == JTokenType.String)
                        {
                            string name = dependency["name"].ToString();
                            string url = dependency["url"].ToString();
                            if (!File.Exists(name))
                            {
                                Task<byte[]> dependencyTask = downloaderClient.GetByteArrayAsync(url);
                                dependencyTasks.Add(dependencyTask.ContinueWith(dependencyReceived, name));
                            }
                        }
                    }
                }
            } else
            {
                IsUpdating = false;
                UpdateFailed?.Invoke();
                return;
            }
            exeTask = downloaderClient.GetByteArrayAsync(latestExecutableUrl).ContinueWith(executableReceived);
            Task[] waitTasks = new Task[dependencyTasks.Count + 1];
            waitTasks[0] = exeTask;
            if (dependencyTasks.Count > 0)
            {
                dependencyTasks.CopyTo(waitTasks, 1);
            }
            try
            {
                Task.WaitAll(waitTasks);
            } catch (AggregateException ex)
            {
                IsUpdating = false;
                rollbackUpdate();
                UpdateFailed?.Invoke();
                return;
            }
            foreach (Task task in waitTasks)
            {
                if (!task.IsCompletedSuccessfully)
                {
                    IsUpdating = false;
                    rollbackUpdate();
                    UpdateFailed?.Invoke();
                    return;
                }
            }
            if (File.Exists("QuicktabsOld.exe") && File.Exists("QuickTabs.exe")) // final just-in-case fail check
            {
                ProcessStartInfo psi = new ProcessStartInfo("QuickTabs.exe");
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                Process.Start(psi);
                AudioEngine.Stop();
                Environment.Exit(0);
            } else
            {
                IsUpdating = false;
                rollbackUpdate();
                UpdateFailed?.Invoke();
                return;
            }
        }
        private static void executableReceived(Task<byte[]> task)
        {
            if (!task.IsCompletedSuccessfully)
            {
                throw new Exception();
            }
            File.Move("QuickTabs.exe", "QuickTabsOld.exe");
            File.WriteAllBytes("QuickTabs.exe", task.Result);
        }
        private static void dependencyReceived(Task<byte[]> task, object name)
        {
            if (!task.IsCompletedSuccessfully)
            {
                throw new Exception();
            }
            File.WriteAllBytes((string)name, task.Result);
        }
        private static void completeUpdate(Task task)
        {
            File.Delete("QuickTabsOld.exe");
        }
        private static void rollbackUpdate()
        {
            if (File.Exists("QuickTabsOld.exe"))
            {
                File.Delete("QuickTabs.exe");
                File.Move("QuickTabsOld.exe", "QuickTabs.exe");
            }
        }
    }
}
