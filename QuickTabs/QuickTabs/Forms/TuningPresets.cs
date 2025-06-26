using QuickTabs.Controls;
using QuickTabs.Songwriting;
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
    public partial class TuningPresets : Form
    {
        public TuningPicker TuningPicker { get; set; }

        private Tuning[] tuningOptions;

        public TuningPresets()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);

            tuningOptions = new Tuning[] { Tuning.StandardGuitar, Tuning.DropD, Tuning.StandardBass, Tuning.StandardUke };
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (presetSelect.SelectedItems.Count > 0)
            {
                TuningPicker.Tuning = tuningOptions[presetSelect.SelectedIndex];
            }
            this.Close();
        }
    }
}
