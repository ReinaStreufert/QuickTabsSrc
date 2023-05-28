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
        private Tab sourceTab;
        private List<Section> sections = new List<Section>();

        public PlainTextTabWriter(Tab source)
        {
            sourceTab = source;
            splitSections();
        }
        public string WriteTab(TimeSignature timeSignature)
        {
            StringBuilder result = new StringBuilder();
            foreach (Section section in sections)
            {
                result.AppendLine(section.Name);
                result.AppendLine();
                result.Append(writeStaff(section.Beats, timeSignature));
                result.AppendLine();
            }
            return result.ToString();
        }
        private string writeStaff(List<Beat> beats, TimeSignature timeSignature)
        {
            int beatsPerMeasure = timeSignature.EighthNotesPerMeasure;
            List<StringBuilder> strings = new List<StringBuilder>();
            for (int i = 0; i < sourceTab.Tuning.Count; i++)
            {
                strings.Add(new StringBuilder());
                strings[i].Append(sourceTab.Tuning[i]);
                strings[i].Append('|');
            }
            int beatCount = 0;
            int[] holds = new int[strings.Count];
            for (int i = 0; i < holds.Length; i++)
            {
                holds[i] = 0;
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
                    holds[heldFret.String] = beat.NoteLength;
                }
                for (int i = 0; i < stringsSet.Length; i++)
                {
                    if (!stringsSet[i])
                    {
                        if (holds[i] > 0)
                        {
                            strings[i].Append('>');
                        } else
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
                    if (holds[i] > 0)
                    {
                        holds[i]--;
                    }
                }
                beatCount++;
                if (beatCount >= beatsPerMeasure)
                {
                    beatCount = 0;
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
        private void splitSections()
        {
            Section currentSection = new Section();
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
                    }
                } else if (sourceTab[i].Type == Enums.StepType.Beat)
                {
                    currentSection.Beats.Add((Beat)sourceTab[i]);
                }
            }
            sections.Add(currentSection);
        }
        private class Section
        {
            public string Name { get; set; }
            public List<Beat> Beats { get; set; } = new List<Beat>();
        }
    }
}
