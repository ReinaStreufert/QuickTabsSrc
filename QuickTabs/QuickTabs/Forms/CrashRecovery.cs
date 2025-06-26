using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickTabs.Forms
{
    public partial class CrashRecovery : Form
    {
        public bool AttemptRecover { get; private set; }
        public CrashManager.CrashReport Report { get; set; } = null;

        public CrashRecovery()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);

            exceptionBox.Font = new Font("Consolas", 9, GraphicsUnit.Point);
            exceptionBox.ReadOnly = true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Report == null)
            {
                Report = CrashManager.LastCrashReport;
            }
            exceptionBox.Text = Report.ExceptionInfo;
            if (!Report.UnsavedSongAvailable)
            {
                recoveryAvailableLabel.Text = "Data recovery: unavailable";
                discardCloseButton.Visible = false;
                recoverCloseButton.Text = "Close";
            }
        }

        private void recoverCloseButton_Click(object sender, EventArgs e)
        {
            if (Report.UnsavedSongAvailable)
            {
                AttemptRecover = true;
            } else
            {
                AttemptRecover = false;
            }
            this.Close();
        }

        private void discardCloseButton_Click(object sender, EventArgs e)
        {
            using (UnsavedChanges unsavedChanges = new UnsavedChanges())
            {
                unsavedChanges.CustomMessage = "Are you sure you would not like to open the recovered song? You can always find it in the crashes section in Config->Preferences.";
                unsavedChanges.ShowDialog();
                if (unsavedChanges.Continue)
                {
                    AttemptRecover = false;
                    this.Close();
                }
            }
        }
    }
}
