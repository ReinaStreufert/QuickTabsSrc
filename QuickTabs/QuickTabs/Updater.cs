using Newtonsoft.Json.Linq;
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
        public static event Action UpdateStarted;   // ]
        public static event Action UpdateFailed;    // ] both of these events may be invoked outside of the main thread.

        public const int SelfReleaseVersion = 0;
        public const string SelfReleaseNotes = "Development version";
        private const string versionStatusUrl = "http://localhost:4040/updater/status.json"; // will be URL of version status json file hosted on github pages. this tells the client what the latest version number is and where to find executables and dependencies.

        public static bool WasJustUpdated { get; private set; } = false;

        private static HttpClient httpClient;

        public static void Initialize()
        {
            if (File.Exists("QuickTabsOld.exe"))
            {
                WasJustUpdated = true;
                Task.Delay(100).ContinueWith(completeUpdate); // wait for old QuickTabs to fully exit and release the file
            } else
            {
                httpClient = new HttpClient();
                httpClient.GetStringAsync(versionStatusUrl).ContinueWith(statusReceived);
            }
        }

        private static void statusReceived(Task<string> task)
        {
            if (task.IsFaulted)
            {
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

            string latestExecutableUrl;
            string latestDllUrl;
            if (statusJson.ContainsKey("latest-exe") && statusJson["latest-exe"].Type == JTokenType.String)
            {
                latestExecutableUrl = statusJson["latest-exe"].ToString();
            } else
            {
                UpdateFailed?.Invoke();
                return;
            }
            if (statusJson.ContainsKey("latest-dll") && statusJson["latest-dll"].Type == JTokenType.String)
            {
                latestDllUrl = statusJson["latest-dll"].ToString();
            }
            else
            {
                UpdateFailed?.Invoke();
                return;
            }

            Task exeTask;
            Task dllTask;
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
                                Task<byte[]> dependencyTask = httpClient.GetByteArrayAsync(url);
                                dependencyTasks.Add(dependencyTask.ContinueWith(dependencyReceived, name));
                            }
                        }
                    }
                }
            } else
            {
                UpdateFailed?.Invoke();
                return;
            }
            exeTask = httpClient.GetByteArrayAsync(latestExecutableUrl).ContinueWith(executableReceived);
            dllTask = httpClient.GetByteArrayAsync(latestDllUrl).ContinueWith(dllReceived);
            Task[] waitTasks = new Task[dependencyTasks.Count + 2];
            waitTasks[0] = exeTask;
            waitTasks[1] = dllTask;
            dependencyTasks.CopyTo(waitTasks, 2);
            Task.WaitAll(waitTasks);
            foreach (Task task in waitTasks)
            {
                if (task.IsFaulted)
                {
                    rollbackUpdate();
                    UpdateFailed?.Invoke();
                    return;
                }
            }
            Process.Start("QuickTabs.exe");
            Environment.Exit(0);
        }
        private static void executableReceived(Task<byte[]> task)
        {
            if (task.IsFaulted)
            {
                return;
            }
            File.Move("QuickTabs.exe", "QuickTabsOld.exe");
            File.WriteAllBytes("QuickTabs.exe", task.Result);
        }
        private static void dllReceived(Task<byte[]> task)
        {
            if (task.IsFaulted)
            {
                return;
            }
            File.Move("QuickTabs.dll", "QuickTabsOld.dll");
            File.WriteAllBytes("QuickTabs.dll", task.Result);
        }
        private static void dependencyReceived(Task<byte[]> task, object name)
        {
            if (task.IsFaulted)
            {
                return;
            }
            File.WriteAllBytes((string)name, task.Result);
        }
        private static void completeUpdate(Task task)
        {
            File.Delete("QuickTabsOld.exe");
            File.Delete("QuickTabsOld.dll");
        }
        private static void rollbackUpdate()
        {
            if (File.Exists("QuickTabsOld.exe"))
            {
                File.Delete("QuickTabs.exe");
                File.Move("QuickTabsOld.exe", "QuickTabs.exe");
            }
            if (File.Exists("QuickTabsOld.dll"))
            {
                File.Delete("QuickTabs.dll");
                File.Move("QuickTabsOld.dll", "QuickTabs.dll");
            }
        }
    }
}
