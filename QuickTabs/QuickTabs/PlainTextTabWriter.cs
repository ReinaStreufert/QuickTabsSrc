using QuickTabs.Enums;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    public class PlainTextTabWriter
    {
        public StaffWriter StaffWriter { get; set; } = new QuickTabsStyleStaffWriter();
        public TabMetadataComponents IncludedMetadata { get; set; } = TabMetadataComponents.None;
        public int MeasureWrap { get; set; } = 0; // 0 = dont wrap, otherwise value is max measures per line
        public bool FocusedTrackOnly { get; set; } = false;

        private Song source;
        private List<StaffRow> rows = new List<StaffRow>();

        public PlainTextTabWriter(Song source)
        {
            this.source = source;
        }
        public string WriteTab()
        {
            if (FocusedTrackOnly)
            {
                splitSectionsFocusedTrack();
            } else
            {
                splitSectionsAllTracks();
            }
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
                    for (int i = source.FocusedTab.Tuning.Count - 1; i >= 0; i--)
                    {
                        result.Append(" ");
                        result.Append(source.FocusedTab.Tuning.GetMusicalNote(i).ToString());
                    }
                    result.AppendLine();
                }
                result.AppendLine();
            }
            foreach (StaffRow row in rows)
            {
                if (row.Name != "")
                {
                    result.AppendLine(row.Name);
                }
                result.Append(StaffWriter.WriteStaff(row.Beats, source.TimeSignature, source.FocusedTab.Tuning));
                result.AppendLine();
            }
            return result.ToString();
        }
        private void splitSectionsAllTracks()
        {
            if (source.Tracks.Count <= 1)
            {
                splitSectionsFocusedTrack();
                return;
            }
            rows.Clear();

            MusicalTimespan measureLength = source.TimeSignature.MeasureLength;

            Section[] sectionGraph = source.GetSectionGraph();
            foreach (Section section in sectionGraph)
            {
                Dictionary<Track, Beat[]> content = section.Content;
                foreach (KeyValuePair<Track, Beat[]> sectionTrackBeats in content)
                {
                    Track track = sectionTrackBeats.Key;
                    Beat[] trackBeats = sectionTrackBeats.Value;
                    StaffRow currentRow = new StaffRow();
                    currentRow.Name = "Section: " + section.SectionName + " | Track: " + track.Name;
                    currentRow.Tuning = track.Tab.Tuning;
                    if (MeasureWrap > 0)
                    {
                        MusicalTimespan beatCounter = new MusicalTimespan();
                        int measureCounter = 0;
                        foreach (Beat beat in trackBeats)
                        {
                            currentRow.Beats.Add(beat);
                            beatCounter += beat.BeatDivision;
                            if (beatCounter >= measureLength)
                            {
                                beatCounter = MusicalTimespan.Zero;
                                measureCounter++;
                                if (measureCounter >= MeasureWrap)
                                {
                                    measureCounter = 0;
                                    rows.Add(currentRow);
                                    currentRow = new StaffRow();
                                    currentRow.Tuning = track.Tab.Tuning;
                                    currentRow.Name = "";
                                }
                            }
                        }
                    } else
                    {
                        currentRow.Beats = trackBeats.ToList(); // i cant think of any reason why this would be wrong
                    }
                    if (currentRow.Beats.Count > 0)
                    {
                        rows.Add(currentRow);
                    }
                }
            }
        }
        private void splitSectionsFocusedTrack()
        {
            rows.Clear();

            MusicalTimespan measureLength = source.TimeSignature.MeasureLength;
            MusicalTimespan beatCount = MusicalTimespan.Zero;
            int measureCount = 0;

            StaffRow currentRow = new StaffRow();
            Tab sourceTab = source.FocusedTab;
            currentRow.Tuning = sourceTab.Tuning;
            for (int i = 0; i < sourceTab.Count; i++)
            {
                if (sourceTab[i].Type == Enums.StepType.SectionHead)
                {
                    SectionHead sectionHead = (SectionHead)sourceTab[i];
                    if (currentRow.Beats.Count == 0)
                    {
                        currentRow.Name = sectionHead.Name;
                    } else
                    {
                        rows.Add(currentRow);
                        currentRow = new StaffRow();
                        currentRow.Name = "Section: " + sectionHead.Name;
                        currentRow.Tuning = sourceTab.Tuning;
                        beatCount = MusicalTimespan.Zero;
                        measureCount = 0;
                    }
                } else if (sourceTab[i].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)sourceTab[i];
                    currentRow.Beats.Add(beat);
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
                                rows.Add(currentRow);
                                currentRow = new StaffRow();
                                currentRow.Tuning = sourceTab.Tuning;
                                currentRow.Name = "";
                            }
                        }
                    }
                }
            }
            if (currentRow.Beats.Count > 0)
            {
                rows.Add(currentRow);
            }
        }
        private class StaffRow
        {
            public string Name { get; set; }
            public List<Beat> Beats { get; set; } = new List<Beat>();
            public Tuning Tuning { get; set; }
        }
    }
    public abstract class StaffWriter
    {
        public abstract string WriteStaff(List<Beat> beats, TimeSignature timeSignature, Tuning tuning);
    }
    public class QuickTabsStyleStaffWriter : StaffWriter
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
                foreach (KeyValuePair<Fret,MusicalTimespan> heldFret in beat)
                {
                    string fretNumberString = heldFret.Key.Space.ToString();
                    if (fretNumberString.Length > spaceSize)
                    {
                        spaceSize = fretNumberString.Length;
                    }
                    strings[heldFret.Key.String].Append(fretNumberString);
                    if (fretNumberString.Length < spaceSize)
                    {
                        for (int i = 0; i < spaceSize - fretNumberString.Length; i++)
                        {
                            strings[heldFret.Key.String].Append(' ');
                        }
                    }
                    stringsSet[heldFret.Key.String] = true;
                    holds[heldFret.Key.String] = heldFret.Value;
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
    public class StandardStyleStaffWriter : StaffWriter
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
                foreach (KeyValuePair<Fret,MusicalTimespan> fret in beat)
                {
                    stringsSet[fret.Key.String] = true;
                    strings[fret.Key.String].Append(fret.Key.Space.ToString());
                    if (fret.Key.Space < 10)
                    {
                        strings[fret.Key.String].Append("-");
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
