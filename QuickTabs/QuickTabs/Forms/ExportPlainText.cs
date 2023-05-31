using QuickTabs.Controls;
using QuickTabs.Enums;
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
    internal partial class ExportPlainText : Form
    {
        public Song Song { get; set; }

        private PlainTextTabWriter tabWriter = null;

        public ExportPlainText()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);

            previewBox.ReadOnly = true;
            previewBox.ScrollBars = ScrollBars.Both;
            staffStyle.SelectedIndex = 0;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            tabWriter = new PlainTextTabWriter(Song);
            OnSizeChanged(null);
            updatePreview();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            previewBox.Size = new Size(this.ClientSize.Width - previewBox.Location.X, this.ClientSize.Height - previewBox.Location.Y);
        }

        private void updatePreview()
        {
            if (tabWriter == null)
            {
                return;
            }
            TabMetadataComponents metadataComponents = TabMetadataComponents.None;
            if (includeName.Checked)
            {
                metadataComponents |= TabMetadataComponents.Name;
            }
            if (includeTempo.Checked)
            {
                metadataComponents |= TabMetadataComponents.Tempo;
            }
            if (includeTs.Checked)
            {
                metadataComponents |= TabMetadataComponents.TimeSignature;
            }
            if (includeTuning.Checked)
            {
                metadataComponents |= TabMetadataComponents.ExactTuning;
            }
            tabWriter.IncludedMetadata = metadataComponents;
            if (staffStyle.SelectedIndex == 0) // QuickTabs style
            {
                tabWriter.StaffWriter = new QuickTabsStyleStaffWriter();
            } else if (staffStyle.SelectedIndex == 1) // standard style
            {
                tabWriter.StaffWriter = new StandardStyleStaffWriter();
            }
            if (enableWrap.Checked)
            {
                tabWriter.MeasureWrap = (int)wrapEvery.Value;
            } else
            {
                tabWriter.MeasureWrap = 0;
            }
            previewBox.Text = tabWriter.WriteTab();
        }

        private void enableWrap_CheckedChanged(object sender, EventArgs e)
        {
            if (enableWrap.Checked)
            {
                wrapEveryText.ForeColor = Color.White;
                measuresText.ForeColor = Color.White;
                wrapEvery.Enabled = true;
                wrapEvery.BackColor = SystemColors.ControlDarkDark;
            } else
            {
                wrapEveryText.ForeColor = SystemColors.ControlDarkDark;
                measuresText.ForeColor = SystemColors.ControlDarkDark;
                wrapEvery.Enabled = false;
                wrapEvery.BackColor = Color.Black;
            }
            updatePreview();
        }

        private void includeName_CheckedChanged(object sender, EventArgs e)
        {
            updatePreview();
        }

        private void includeTempo_CheckedChanged(object sender, EventArgs e)
        {
            updatePreview();
        }

        private void includeTs_CheckedChanged(object sender, EventArgs e)
        {
            updatePreview();
        }

        private void includeTuning_CheckedChanged(object sender, EventArgs e)
        {
            updatePreview();
        }

        private void staffStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            updatePreview();
        }

        private void wrapEvery_ValueChanged(object sender, EventArgs e)
        {
            updatePreview();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*";
                saveDialog.DefaultExt = "txt";
                DialogResult saveResult = saveDialog.ShowDialog();
                if (saveResult == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, previewBox.Text);
                }
            }
            this.Close();
        }
    }
}
