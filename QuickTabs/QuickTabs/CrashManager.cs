using Newtonsoft.Json.Linq;
using QuickTabs.Configuration;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    public static class CrashManager
    {
        public static bool ReportAvailable { get; private set; } = false;
        public static CrashReport LastCrashReport { get; private set; }

        private static string lastReportPath;
        private static bool failStarted = false;
        private static bool persistenceAutosave = true;
        private static bool persistenceAutorestart = true;

        public const string DtFormat = "MM-dd-yy-HH-mm-ss";

        public static void Initialize()
        {
            #if DEBUG
              return; // do not use crash manager in debug mode
            #endif

            // put just in case hooks in place. of course, the ultimate goal is to handle all possible exceptions at their source.
            // however this is in beta and the chances that im catching everything like i wanna be is pretty much zero. so i am putting this fail save so hopefully no data loss occurs.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (object sender, ThreadExceptionEventArgs e) => FailHard(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => FailHard((Exception)e.ExceptionObject);
            // now check for past crash reports. 
            string qtData = Persistence.QuickTabsDataDir; // getter for this already make sure the dir exists.
            string crashData = Path.Combine(qtData, "crash-reports");
            if (Directory.Exists(crashData))
            {
                string[] crashFiles = Directory.GetFiles(crashData);
                if (crashFiles.Length < 1)
                {
                    ReportAvailable = false;
                    LastCrashReport = null;
                    return;
                }
                // there should never be more than one report in crash-reports, but if there are, the oldest one is used
                // and all subsequent are logged (supposed to be the behavior of FailHard)
                DateTime earliestReportTime = DateTime.Now;
                string earliestReportPath = "";
                foreach (string fileName in crashFiles)
                {
                    if (Path.GetExtension(fileName) != ".json")
                    {
                        File.Delete(fileName);
                        continue;
                    }
                    DateTime creationTime = File.GetCreationTime(fileName);
                    if (creationTime < earliestReportTime)
                    {
                        // log what we thought was the earliest report, set new earliest report
                        if (earliestReportPath != "")
                        {
                            string loggedPath = Path.Combine(crashData, "logged");
                            if (!Directory.Exists(loggedPath))
                            {
                                Directory.CreateDirectory(loggedPath);
                            }
                            File.Move(earliestReportPath, Path.Combine(loggedPath, Path.GetFileName(earliestReportPath)));
                        }
                        earliestReportTime = creationTime;
                        earliestReportPath = fileName;
                    } else
                    {
                        string loggedPath = Path.Combine(crashData, "logged");
                        if (!Directory.Exists(loggedPath))
                        {
                            Directory.CreateDirectory(loggedPath);
                        }
                        File.Move(fileName, Path.Combine(loggedPath, Path.GetFileName(fileName)));
                    }
                }
                if (File.Exists(earliestReportPath))
                {
                    string reportText = File.ReadAllText(earliestReportPath);
                    JObject reportJson;
                    try
                    {
                        reportJson = JObject.Parse(reportText);
                    } catch
                    {
                        File.Delete(earliestReportPath);
                        ReportAvailable = false;
                        LastCrashReport = null;
                        return;
                    }
                    CrashReport report;
                    if (tryConstructReport(reportJson, out report))
                    {
                        ReportAvailable = true;
                        LastCrashReport = report;
                        lastReportPath = earliestReportPath;
                    } else
                    {
                        File.Delete(earliestReportPath);
                        ReportAvailable = false;
                        LastCrashReport = null;
                    }
                } else
                {
                    ReportAvailable = false;
                    LastCrashReport = null;
                }
            } else
            {
                // if crash-reports does not exist there are no crash reports
                ReportAvailable = false;
                LastCrashReport = null;
            }
        }
        public static void NotifyPersistenceUpdate() // crash manager keeps track of relevent persistence settings separately as to not
        {                                            // access the highly cast-y and other risky behavior QTPersistence class while the app is in unsafe states.
            persistenceAutosave = QTPersistence.Current.AutosaveOnCrash;
            persistenceAutorestart = QTPersistence.Current.AutorestartOnCrash;
        }
        public static void FlushReport()
        {
            if (ReportAvailable && File.Exists(lastReportPath))
            {
                string qtData = Persistence.QuickTabsDataDir;
                string loggedDir = Path.Combine(qtData, "crash-reports", "logged");
                if (!Directory.Exists(loggedDir))
                {
                    Directory.CreateDirectory(loggedDir);
                }
                File.Move(lastReportPath, Path.Combine(loggedDir, Path.GetFileName(lastReportPath)));
                ReportAvailable = false;
                LastCrashReport = null;
            }
        }
        public static string[] GetPastReportPaths()
        {
            string qtData = Persistence.QuickTabsDataDir;
            string loggedDir = Path.Combine(qtData, "crash-reports", "logged");
            if (!Directory.Exists(loggedDir))
            {
                return new string[0];
            }
            return Directory.GetFiles(loggedDir);
        }
        public static CrashReport LoadPastReport(string path)
        {
            const string failedMessage = "Could not open crash report";
            if (!File.Exists(path))
            {
                return new CrashReport(failedMessage, false, null);
            }
            string fileText = File.ReadAllText(path);
            JObject reportJson;
            try
            {
                reportJson = JObject.Parse(fileText);
            } catch
            {
                return new CrashReport(failedMessage, false, null);
            }
            CrashReport report;
            if (!tryConstructReport(reportJson, out report))
            {
                return new CrashReport(failedMessage, false, null);
            }
            return report;
        }
        public static void FailHard(Exception ex)
        {
            // the application could be in any state when this is called so weird unexpected things can throw further exceptions.
            // this code has to be very careful
            try
            {
                if (failStarted)
                {
                    return;
                }
                failStarted = true;
                string exceptionInfo;
                try
                {
                    exceptionInfo = ex.ToString();
                } catch
                {
                    exceptionInfo = "There was another error while attempting to collect exception info for the report.";
                }
                bool unsavedSongAvailable;
                JObject recoveredSong;
                if (persistenceAutosave)
                {
                    // this is where things get risky. we will attempt to check if there is unsaved work and if so include the qtjson data
                    // in the crash report for recovery. this involves going through the QuickTabs objects that may have caused the exception in the first place
                    // so it might not work. aka another try catch is needed. luckily the success rate should be high using History.GetSafeState which gets the last cloned state generated by History.PushState.
                    try
                    {
                        if (FileManager.IsSaved)
                        {
                            recoveredSong = null;
                            unsavedSongAvailable = false;
                        }
                        else
                        {
                            Song song;
                            if (History.TryGetSafeState(out song))
                            {
                                recoveredSong = song.SaveAsJObject(song);
                                unsavedSongAvailable = true;
                            }
                            else
                            {
                                recoveredSong = null;
                                unsavedSongAvailable = false;
                            }
                        }
                    }
                    catch
                    {
                        unsavedSongAvailable = false;
                        recoveredSong = null;
                    }
                } else
                {
                    unsavedSongAvailable = false;
                    recoveredSong = null;
                }
                CrashReport report = new CrashReport(exceptionInfo, unsavedSongAvailable, recoveredSong);
                JObject reportJson = generateReportJson(report);
                DateTime reportTime = DateTime.Now;

                bool attemptRestart = persistenceAutorestart;
                string qtData = Persistence.QuickTabsDataDir;
                string crashData = Path.Combine(qtData, "crash-reports");
                if (!Directory.Exists(crashData))
                {
                    Directory.CreateDirectory(crashData);
                }
                string reportDir = crashData;
                if (Directory.GetFiles(reportDir).Length > 0)
                {
                    // if there is already a report that has not been seen by the user, the old one should be shown
                    // any error that happens before a previous report gets to be moved to "logged" will not contain
                    // unsaved data and also will likely be less significant, so it should go straight to logged.
                    reportDir = Path.Combine(crashData, "logged");
                    if (!Directory.Exists(reportDir))
                    {
                        Directory.CreateDirectory(reportDir);
                    }
                    attemptRestart = false; // already existing unseen report typically indicates a restart loop. this will break the loop.
                }
                string reportPath = Path.Combine(reportDir, reportTime.ToString(DtFormat) + ".json");
                File.WriteAllText(reportPath, reportJson.ToString());
                try
                {
                    if (AudioEngine.Enabled)
                        AudioEngine.Stop();
                }
                catch { }
                if (attemptRestart)
                {
                    // try to restart the application if possible before exiting
                    try
                    {
                        Process.Start(InstallOperations.SelfExe);
                    }
                    catch { }
                }
                Environment.Exit(-1);
            } catch
            {
                // this will probably only happen (if ever) if Newtonsoft.Json gets into an invalid state.
                // mega super fail. could not even successfully log the crash even without save recovery. abort abort abort.
                try
                {
                    if (AudioEngine.Enabled)
                        AudioEngine.Stop();
                }
                catch { }
                Environment.Exit(-1);
            }
        }
        private static bool tryConstructReport(JObject reportJson, out CrashReport report)
        {
            string exceptionInfo;
            if (reportJson.ContainsKey("exception") && reportJson["exception"].Type == JTokenType.String)
            {
                exceptionInfo = reportJson["exception"].ToString();
            } else
            {
                report = null;
                return false;
            }
            bool unsavedSongAvailable;
            JObject recoveredSong;
            if (reportJson.ContainsKey("unsaved-song") && reportJson["unsaved-song"].Type == JTokenType.Object)
            {
                unsavedSongAvailable = true;
                recoveredSong = (JObject)reportJson["unsaved-song"];
            } else
            {
                unsavedSongAvailable = false;
                recoveredSong = null;
            }
            report = new CrashReport(exceptionInfo, unsavedSongAvailable, recoveredSong);
            return true;
        }
        private static JObject generateReportJson(CrashReport report)
        {
            JObject reportJson = new JObject();
            reportJson.Add("exception", report.ExceptionInfo);
            if (report.UnsavedSongAvailable)
            {
                reportJson.Add("unsaved-song", report.RecoveredSong);
            }
            return reportJson;
        }
        public class CrashReport
        {
            public string ExceptionInfo { get; private set; }
            public bool UnsavedSongAvailable { get; private set; }
            public JObject RecoveredSong { get; private set; }
            public CrashReport(string exceptionInfo, bool unsavedSongAvilable, JObject recoveredSong)
            {
                ExceptionInfo = exceptionInfo;
                UnsavedSongAvailable = unsavedSongAvilable;
                RecoveredSong = recoveredSong;
            }
        }
    }
}
