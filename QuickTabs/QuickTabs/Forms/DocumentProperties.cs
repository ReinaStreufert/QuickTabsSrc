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
    internal partial class DocumentProperties : Form
    {
        public Song Song { get; set; }
        public bool ChangesSaved { get; set; } = false;

        private TuningPicker tuningPicker;
        private bool tsChanged = false;
        private int oldts2Value = 4;
        private bool ignoreTs2Changes = false;
        public DocumentProperties()
        {
            InitializeComponent();
            tuningPicker = new TuningPicker();
            tuningPicker.Tuning = Songwriting.Tuning.StandardGuitar;
            tuningPicker.Location = new Point(0, 0);
            tuningPicker.Size = tuningPickerContainer.Size;
            tuningPickerContainer.Controls.Add(tuningPicker);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            nameInput.Text = Song.Name;
            tempoInput.Value = Song.Tempo;
            if (isTabEmpty(Song.Tab))
            {
                ts1DisabledLabel.Visible = false;
                ts2DisabledLabel.Visible = false;
                ts1Input.Value = Song.TimeSignature.T1;
                ts2Input.Value = Song.TimeSignature.T2;
                tsChanged = false;
                oldts2Value = Song.TimeSignature.T2;
            } else
            {
                ts1Input.Visible = false;
                ts2Input.Visible = false;
                ts1DisabledLabel.Text = Song.TimeSignature.T1.ToString();
                ts2DisabledLabel.Text = Song.TimeSignature.T2.ToString();
            }
            tuningPicker.Tuning = Song.Tab.Tuning;
            tuningPicker.Refresh();
        }

        private bool isTabEmpty(Tab tab)
        {
            bool firstSectionHead = true;
            foreach (Step step in tab)
            {
                if (step.Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)step;
                    if (beat.Count() > 0)
                    {
                        return false;
                    }
                } else if (step.Type == Enums.StepType.SectionHead)
                {
                    if (firstSectionHead)
                    {
                        firstSectionHead = false;
                    } else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            ChangesSaved = true;
            Song.Name = nameInput.Text;
            Song.Tempo = (int)tempoInput.Value;
            if (tsChanged)
            {
                Song.TimeSignature = new Songwriting.TimeSignature((int)ts1Input.Value, (int)ts2Input.Value);
                int beatsPerMeasure = Song.TimeSignature.EighthNotesPerMeasure;
                Song.Tab.SetLength(beatsPerMeasure * 2 + 1);
            }
            if (Song.Tab.Tuning.Count != tuningPicker.Tuning.Count)
            {
                for (int i = 0; i < Song.Tab.Count; i++)
                {
                    if (Song.Tab[i].Type == Enums.StepType.Beat)
                    {
                        Beat srcBeat = (Beat)Song.Tab[i];
                        Beat newBeat = new Beat();
                        newBeat.NoteLength = srcBeat.NoteLength;
                        foreach (Fret fret in srcBeat)
                        {
                            int newString = fret.String + tuningPicker.StringShift;
                            if (newString >= 0 && newString < tuningPicker.Tuning.Count)
                            {
                                newBeat[new Fret(newString, fret.Space)] = true;
                            }
                        }
                        Song.Tab[i] = newBeat;
                    }
                }
            }
            Song.Tab.Tuning = tuningPicker.Tuning;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ts1Input_ValueChanged(object sender, EventArgs e)
        {
            tsChanged = true;
        }

        private void ts2Input_ValueChanged(object sender, EventArgs e)
        {
            if (ignoreTs2Changes)
            {
                return;
            }
            tsChanged = true;
            if (ts2Input.Value > oldts2Value)
            {
                ignoreTs2Changes = true;
                ts2Input.Value = oldts2Value * 2;
                ignoreTs2Changes = false;
            } else if (ts2Input.Value < oldts2Value)
            {
                ignoreTs2Changes = true;
                if (oldts2Value == 1)
                {
                    ts2Input.Value = oldts2Value;
                } else
                {
                    ts2Input.Value = oldts2Value / 2;
                }
                ignoreTs2Changes = false;
            }
            oldts2Value = (int)ts2Input.Value;
        }
    }
}
