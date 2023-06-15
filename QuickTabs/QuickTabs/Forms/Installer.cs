using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickTabs.Forms
{
    public partial class Installer : Form
    {
        public Installer()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            installPathInput.Text = InstallOperations.DefaultInstallDir;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            chooseButton.Height = installPathInput.Height; // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        }

        private void chooseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = installPathInput.Text;
                dialog.ShowDialog();
                installPathInput.Text = dialog.SelectedPath;
            }
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            InstallOperations.InstallFailed += InstallOperations_InstallFailed;
            InstallOperations.InstallComplete += InstallOperations_InstallComplete;
            InstallOperations.StartInstall(installPathInput.Text, startShortcutCheck.Checked);
            this.Cursor = Cursors.WaitCursor;
        }

        private void InstallOperations_InstallComplete(string exePath)
        {
            Process.Start(exePath);
            Environment.Exit(0);
        }

        private void InstallOperations_InstallFailed(string errMessage)
        {
            this.Cursor = Cursors.Default;
            using (InstallerMessage message = new InstallerMessage())
            {
                message.Message = errMessage;
                message.ShowDialog();
            }
        }

        private void Splash_FormClosed(object? sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
