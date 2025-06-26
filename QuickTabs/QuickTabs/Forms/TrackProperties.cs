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
    public partial class TrackProperties : Form
    {
        public bool ChangesMade { get; private set; } = false;

        private Track track;
        private TuningPicker tuningPicker;

        public TrackProperties(Track track)
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);

            this.track = track;
            tuningPicker = new TuningPicker();
            tuningPicker.Tuning = track.Tab.Tuning;
            tuningPicker.Location = new Point(0, 0);
            tuningPicker.Size = tuningPickerPlaceholder.Size;
            tuningPickerPlaceholder.Controls.Add(tuningPicker);
            if (track.NamedByUser)
            {
                nameInput.Text = track.Name;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            tuningPicker.Refresh();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            ChangesMade = true;
            if (nameInput.Text != "")
            {
                track.Name = nameInput.Text;
                track.NamedByUser = true;
            }
            if (tuningPicker.StringShift != 0 || !tuningPicker.Tuning.Equals(track.Tab.Tuning))
            {
                track.Tab.AlterTuning(tuningPicker.Tuning, tuningPicker.StringShift);
                track.UpdateAutoName();
            }
            this.Close();
        }

        private void tuningPresetsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (TuningPresets tuningPresets = new TuningPresets())
            {
                tuningPresets.TuningPicker = tuningPicker;
                tuningPresets.ShowDialog();
                tuningPicker.Refresh();
            }
        }
    }
}
