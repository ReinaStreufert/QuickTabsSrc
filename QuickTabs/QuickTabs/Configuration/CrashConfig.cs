using QuickTabs.Controls;
using QuickTabs.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class CrashConfig : PreferenceCategory
    {
        public override string Title => "Crash handling";
        private string[] reportPaths;

        protected override IEnumerable<Preference> preferences
        {
            get
            {
                reportPaths = CrashManager.GetPastReportPaths().Reverse().ToArray();
                string[] buttonNames = new string[reportPaths.Length];
                int dtFormatSegmentCount = CrashManager.DtFormat.Split('-').Length;
                for (int i = 0; i < reportPaths.Length; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(reportPaths[i]);
                    if (fileName.Length == CrashManager.DtFormat.Length)
                    {
                        string[] segments = fileName.Split('-');
                        if (segments.Length != dtFormatSegmentCount)
                        {
                            buttonNames[i] = fileName;
                        } else
                        {
                            buttonNames[i] = segments[0] + "/" + segments[1] + "/" + segments[2] + " " + segments[3] + ":" + segments[4] + ":" + segments[5];
                        }
                    } else
                    {
                        buttonNames[i] = fileName;
                    }
                }
                yield return new ButtonListPreference("View past crash reports", buttonNames);
                yield return new CheckPreference("Autosave on crash", QTPersistence.Keys.AutosaveOnCrash);
                yield return new CheckPreference("Autorestart on crash", QTPersistence.Keys.AutorestartOnCrash);
                #if DEBUG
                    yield return new ButtonPreference("Test crash", DrawingIcons.Clear);
                #endif
            }
        }

        protected override void refreshLiveApplication(string changedPrefName, Editor editorForm, TabEditor tabEditor, QuickTabsContextMenu ctxMenu, Fretboard fretboard)
        {
            if (changedPrefName == "Autosave on crash" || changedPrefName == "Autorestart on crash")
            {
                CrashManager.NotifyPersistenceUpdate();
            } else if (changedPrefName == "Test crash")
            {
                Thread thread = new Thread(() =>
                {
                    throw new Exception();
                });
                thread.Start(); // crashes outside the main thread are more difficult to handle
            } else
            {
                string[] split = changedPrefName.Split('.');
                if (split.Length == 2 && split[0] == "View past crash reports")
                {
                    int reportIndex = int.Parse(split[1]);
                    CrashManager.CrashReport report = CrashManager.LoadPastReport(reportPaths[reportIndex]);
                    using (CrashRecovery crashRecovery = new CrashRecovery())
                    {
                        crashRecovery.Report = report;
                        crashRecovery.ShowDialog();
                        if (crashRecovery.AttemptRecover)
                        {
                            editorForm.LoadRecoveredDocument(report);
                        }
                    }
                }
            }
        }
    }
}
