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
            exceptionBox.Text = CrashManager.LastCrashReport.ExceptionInfo;
            if (!CrashManager.LastCrashReport.UnsavedSongAvailable)
            {
                recoveryAvailableLabel.Text = "Data recovery: unavailable";
                discardCloseButton.Visible = false;
                recoverCloseButton.Text = "Close";
            }
        }

        private void recoverCloseButton_Click(object sender, EventArgs e)
        {
            if (CrashManager.LastCrashReport.UnsavedSongAvailable)
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
                unsavedChanges.CustomMessage = "QuickTabs recovered unsaved data. Are you sure you would like to discard the recovered song?";
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
