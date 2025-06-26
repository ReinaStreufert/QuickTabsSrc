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
    public partial class DocumentProperties : Form
    {
        public Song Song { get; set; }
        public bool ChangesSaved { get; set; } = false;

        private bool tsChanged = false;
        private int oldts2Value = 4;
        private bool ignoreTs2Changes = false;
        private TapTempo tapTempo = new TapTempo();
        public DocumentProperties()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            DrawingConstants.ApplyThemeToUIForm(this);

            tapTempo.OnSetTempo += TapTempo_OnSetTempo;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            nameInput.Text = Song.Name;
            tempoInput.Value = Song.Tempo;
            bool songEmpty = true;
            foreach (Track track in Song.Tracks)
            {
                if (!isTabEmpty(track.Tab))
                {
                    songEmpty = false;
                    break;
                }
            }
            if (songEmpty)
            {
                ts1DisabledLabel.Visible = false;
                ts2DisabledLabel.Visible = false;
                oldts2Value = Song.TimeSignature.T2;
                ts1Input.Value = Song.TimeSignature.T1;
                ts2Input.Value = Song.TimeSignature.T2;
                tsChanged = false;
            } else
            {
                ts1Input.Visible = false;
                ts2Input.Visible = false;
                ts1DisabledLabel.Text = Song.TimeSignature.T1.ToString();
                ts2DisabledLabel.Text = Song.TimeSignature.T2.ToString();
            }
            tapTempo.TimeSignature = Song.TimeSignature;
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
                int beatsPerMeasure = Song.TimeSignature.MeasureLength / Song.TimeSignature.DefaultDivision;
                foreach (Track track in Song.Tracks)
                {
                    Tab tab = track.Tab;
                    tab.SetLength(1, MusicalTimespan.Zero);
                    tab.SetLength(beatsPerMeasure * 2 + 1, Song.TimeSignature.DefaultDivision);
                }
            }
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ts1Input_ValueChanged(object sender, EventArgs e)
        {
            tsChanged = true;
            tapTempo.TimeSignature = new TimeSignature((int)ts1Input.Value, (int)ts2Input.Value);
        }

        private void ts2Input_ValueChanged(object sender, EventArgs e)
        {
            if (ignoreTs2Changes)
            {
                return;
            }
            tsChanged = true;
            tapTempo.TimeSignature = new TimeSignature((int)ts1Input.Value, (int)ts2Input.Value);
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

        private void TapTempo_OnSetTempo(int bpm)
        {
            if (bpm > tempoInput.Maximum)
            {
                tempoInput.Value = tempoInput.Maximum;
            } else
            {
                tempoInput.Value = bpm;
            }
        }

        private void tapTempoLink_LinkClicked(object sender, MouseEventArgs e)
        {
            tapTempo.Tap();
        }
    }
}
