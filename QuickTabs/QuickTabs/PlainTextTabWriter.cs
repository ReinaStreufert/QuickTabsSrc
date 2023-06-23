using QuickTabs.Enums;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal class PlainTextTabWriter
    {
        public StaffWriter StaffWriter { get; set; } = new QuickTabsStyleStaffWriter();
        public TabMetadataComponents IncludedMetadata { get; set; } = TabMetadataComponents.None;
        public int MeasureWrap { get; set; } = 0; // 0 = dont wrap, otherwise value is max measures per line

        private Song source;
        private List<Section> sections = new List<Section>();

        public PlainTextTabWriter(Song source)
        {
            this.source = source;
        }
        public string WriteTab()
        {
            splitSections();
            StringBuilder result = new StringBuilder();
            if (IncludedMetadata != TabMetadataComponents.None)
            {
                if (IncludedMetadata.HasFlag(TabMetadataComponents.Name))
                {
                    result.AppendLine(source.Name);
                }
                if (IncludedMetadata.HasFlag(TabMetadataComponents.Tempo))
                {
                    result.Append("Tempo: ");
                    result.Append(source.Tempo.ToString());
                    result.AppendLine(" BPM");
                }
                if (IncludedMetadata.HasFlag(TabMetadataComponents.TimeSignature))
                {
                    result.Append("Time signature: ");
                    result.Append(source.TimeSignature.T1.ToString());
                    result.Append(" / ");
                    result.AppendLine(source.TimeSignature.T2.ToString());
                }
                if (IncludedMetadata.HasFlag(TabMetadataComponents.ExactTuning))
                {
                    result.Append("Tuning:");
                    for (int i = source.Tab.Tuning.Count - 1; i >= 0; i--)
                    {
                        result.Append(" ");
                        result.Append(source.Tab.Tuning.GetMusicalNote(i).ToString());
                    }
                    result.AppendLine();
                }
                result.AppendLine();
            }
            foreach (Section section in sections)
            {
                if (section.Name != "")
                {
                    result.Append("Section: ");
                    result.AppendLine(section.Name);
                }
                result.Append(StaffWriter.WriteStaff(section.Beats, source.TimeSignature, source.Tab.Tuning));
                result.AppendLine();
            }
            return result.ToString();
        }
        private void splitSections()
        {
            sections.Clear();

            MusicalTimespan measureLength = source.TimeSignature.MeasureLength;
            MusicalTimespan beatCount = MusicalTimespan.Zero;
            int measureCount = 0;

            Section currentSection = new Section();
            Tab sourceTab = source.Tab;
            for (int i = 0; i < sourceTab.Count; i++)
            {
                if (sourceTab[i].Type == Enums.StepType.SectionHead)
                {
                    SectionHead sectionHead = (SectionHead)sourceTab[i];
                    if (currentSection.Beats.Count == 0)
                    {
                        currentSection.Name = sectionHead.Name;
                    } else
                    {
                        sections.Add(currentSection);
                        currentSection = new Section();
                        currentSection.Name = sectionHead.Name;
                        beatCount = MusicalTimespan.Zero;
                        measureCount = 0;
                    }
                } else if (sourceTab[i].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)sourceTab[i];
                    currentSection.Beats.Add(beat);
                    if (MeasureWrap > 0)
                    {
                        beatCount += beat.BeatDivision;
                        if (beatCount >= measureLength)
                        {
                            beatCount = MusicalTimespan.Zero;
                            measureCount++;
                            if (measureCount >= MeasureWrap)
                            {
                                measureCount = 0;
                                sections.Add(currentSection);
                                currentSection = new Section();
                                currentSection.Name = "";
                            }
                        }
                    }
                }
            }
            if (currentSection.Beats.Count > 0)
            {
                sections.Add(currentSection);
            }
        }
        private class Section
        {
            public string Name { get; set; }
            public List<Beat> Beats { get; set; } = new List<Beat>();
        }
    }
    internal abstract class StaffWriter
    {
        public abstract string WriteStaff(List<Beat> beats, TimeSignature timeSignature, Tuning tuning);
    }
    internal class QuickTabsStyleStaffWriter : StaffWriter
    {
        public override string WriteStaff(List<Beat> beats, TimeSignature timeSignature, Tuning tuning)
        {
            List<StringBuilder> strings = new List<StringBuilder>();
            for (int i = 0; i < tuning.Count; i++)
            {
                strings.Add(new StringBuilder());
                if (tuning[i].Length == 1)
                {
                    strings[i].Append(' ');
                }
                strings[i].Append(tuning[i]);
                strings[i].Append('|');
            }
            MusicalTimespan beatCount = MusicalTimespan.Zero;
            MusicalTimespan[] holds = new MusicalTimespan[strings.Count];
            for (int i = 0; i < holds.Length; i++)
            {
                holds[i] = MusicalTimespan.Zero;
            }
            foreach (Beat beat in beats)
            {
                bool[] stringsSet = new bool[strings.Count];
                int spaceSize = 1;
                foreach (Fret heldFret in beat)
                {
                    string fretNumberString = heldFret.Space.ToString();
                    if (fretNumberString.Length > spaceSize)
                    {
                        spaceSize = fretNumberString.Length;
                    }
                    strings[heldFret.String].Append(fretNumberString);
                    if (fretNumberString.Length < spaceSize)
                    {
                        for (int i = 0; i < spaceSize - fretNumberString.Length; i++)
                        {
                            strings[heldFret.String].Append(' ');
                        }
                    }
                    stringsSet[heldFret.String] = true;
                    holds[heldFret.String] = beat.SustainTime;
                }
                for (int i = 0; i < stringsSet.Length; i++)
                {
                    if (!stringsSet[i])
                    {
                        if (holds[i] > MusicalTimespan.Zero)
                        {
                            strings[i].Append('>');
                        }
                        else
                        {
                            strings[i].Append('-');
                        }
                        if (spaceSize > 1)
                        {
                            for (int ii = 0; ii < spaceSize - 1; ii++)
                            {
                                strings[i].Append(' ');
                            }
                        }
                    }
                }
                for (int i = 0; i < holds.Length; i++)
                {
                    if (holds[i] > MusicalTimespan.Zero)
                    {
                        holds[i] -= beat.BeatDivision;
                    }
                }
                beatCount += beat.BeatDivision;
                if (beatCount >= timeSignature.MeasureLength)
                {
                    beatCount = MusicalTimespan.Zero;
                    foreach (StringBuilder stringBuilder in strings)
                    {
                        stringBuilder.Append('|');
                    }
                }
            }
            StringBuilder result = new StringBuilder();
            foreach (StringBuilder stringBuilder in strings)
            {
                result.AppendLine(stringBuilder.ToString());
            }
            return result.ToString();
        }
    }
    internal class StandardStyleStaffWriter : StaffWriter
    {
        public override string WriteStaff(List<Beat> beats, TimeSignature timeSignature, Tuning tuning)
        {
            List<StringBuilder> strings = new List<StringBuilder>();
            for (int i = 0; i < tuning.Count; i++)
            {
                strings.Add(new StringBuilder());
                if (tuning[i].Length == 1)
                {
                    strings[i].Append(' ');
                }
                strings[i].Append(tuning[i]);
                strings[i].Append('|');
            }
            MusicalTimespan beatCount = MusicalTimespan.Zero;
            foreach (Beat beat in beats)
            {
                bool[] stringsSet = new bool[strings.Count];
                foreach (Fret fret in beat)
                {
                    stringsSet[fret.String] = true;
                    strings[fret.String].Append(fret.Space.ToString());
                    if (fret.Space < 10)
                    {
                        strings[fret.String].Append("-");
                    }
                }
                for (int i = 0; i < stringsSet.Length; i++)
                {
                    if (!stringsSet[i])
                    {
                        strings[i].Append("--");
                    }
                }
                beatCount += beat.BeatDivision;
                if (beatCount >= timeSignature.MeasureLength)
                {
                    beatCount = MusicalTimespan.Zero;
                    foreach (StringBuilder stringBuilder in strings)
                    {
                        stringBuilder.Append('|');
                    }
                }
            }
            StringBuilder result = new StringBuilder();
            foreach (StringBuilder stringBuilder in strings)
            {
                result.AppendLine(stringBuilder.ToString());
            }
            return result.ToString();
        }
    }
}
