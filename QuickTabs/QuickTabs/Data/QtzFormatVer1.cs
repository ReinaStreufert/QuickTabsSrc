using QuickTabs.Songwriting;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Data
{
    public class QtzFormatVer1 : QtzVersion
    {
        public override Song Open(FileStream fs, out bool failed)
        {
            Song song = new Song();
            song.Name = fs.ReadString();
            song.Tempo = fs.ReadUInt16();
            int tsN = fs.ReadByte();
            int tsD = fs.ReadByte();
            int trackCount = fs.ReadByte();
            if (tsN < 0 || tsD < 0 || trackCount < 1)
            {
                throw new Exception();
                failed = true;
                return null;
            }
            song.TimeSignature = new Songwriting.TimeSignature(tsN, tsD);
            song.Tracks.Clear();
            for (int i = 0; i < trackCount; i++)
            {
                Track track = new Track();
                track.Name = fs.ReadString();
                track.Volume = fs.ReadFloat32();
                int tuningCount = fs.ReadByte();
                if (tuningCount < 0)
                {
                    throw new Exception();
                    failed = true;
                    return null;
                }
                string[] tuningNotes = new string[tuningCount];
                for (int ii = 0; ii < tuningCount; ii++)
                {
                    tuningNotes[ii] = fs.ReadString();
                }
                track.Tab.Tuning = new Songwriting.Tuning(tuningNotes);
                int trackFlagsInt = fs.ReadByte();
                if (trackFlagsInt < 0)
                {
                    throw new Exception();
                    failed = true;
                    return null;
                }
                TrackFlags trackFlags = (TrackFlags)(byte)trackFlagsInt;
                if (trackFlags.HasFlag(TrackFlags.Mute))
                {
                    track.Mute = true;
                }
                if (trackFlags.HasFlag(TrackFlags.Solo))
                {
                    track.Solo = true;
                }
                if (trackFlags.HasFlag(TrackFlags.NamedByUser))
                {
                    track.NamedByUser = true;
                }
                song.Tracks.Add(track);
            }
            int sectionCount = fs.ReadByte();
            if (sectionCount < 1)
            {
                throw new Exception();
                failed = true;
                return null;
            }
            SectionInfo[] sectionInfo = new SectionInfo[sectionCount];
            for (int i = 0; i < sectionCount; i++)
            {
                SectionInfo section = new SectionInfo();
                section.Name = fs.ReadString();
                section.TrackBeatLengths = new int[trackCount];
                for (int ii = 0; ii < trackCount; ii++)
                {
                    section.TrackBeatLengths[ii] = fs.ReadInt32();
                }
                sectionInfo[i] = section;
            }
            using (ZLibStream zlib = new ZLibStream(fs, CompressionMode.Decompress))
            {
                for (int trackIndex = 0; trackIndex < trackCount; trackIndex++)
                {
                    foreach (SectionInfo section in sectionInfo)
                    {
                        if (!readTrackSection(zlib, song.Tracks[trackIndex].Tab, trackIndex, section, song.TimeSignature))
                        {
                            failed = true;
                            return null;
                        }
                    }
                }
            }
            failed = false;
            return song;
        }

        private static bool readTrackSection(Stream stream, Tab tab, int trackIndex, SectionInfo section, TimeSignature ts)
        {
            int shIndex = tab.Count;
            tab.SetLength(tab.Count + 1, MusicalTimespan.Zero);
            SectionHead sh = new SectionHead();
            sh.Name = section.Name;
            tab[shIndex] = sh;
            int trackSectionLength = section.TrackBeatLengths[trackIndex];
            int beatsRead = 0;
            while (beatsRead < trackSectionLength)
            {
                int readMeasureResult = readMeasure(stream, tab, ts);
                if (readMeasureResult < 0)
                {
                    throw new Exception();
                    return false;
                }
                beatsRead += readMeasureResult;
            }
            return true;
        }

        private static int readMeasure(Stream stream, Tab tab, TimeSignature ts)
        {
            int divisionByte = stream.ReadByte();
            if (divisionByte < 0)
            {
                throw new Exception();
                return -1;
            }
            MusicalTimespan division = MusicalTimespan.DeserializeInt32(divisionByte);
            Tuning tuning = tab.Tuning;
            int beatCount = ts.MeasureLength / division;
            int measureStart = tab.Count;
            tab.SetLength(tab.Count + beatCount, division);
            for (int i = 0; i < beatCount; i++)
            {
                int beatIndex = measureStart + i;
                Beat beat = (Beat)tab[beatIndex];
                for (int stringIndex = 0; stringIndex < tuning.Count; stringIndex++)
                {
                    int fretByte = stream.ReadByte();
                    int sustainByte = stream.ReadByte();
                    if (fretByte < 0 || sustainByte < 0)
                    {
                        throw new Exception();
                        return -1;
                    }
                    if (sustainByte > 0)
                    {
                        MusicalTimespan sustainTime = MusicalTimespan.DeserializeInt32(sustainByte);
                        beat[new Fret(stringIndex, fretByte)] = sustainTime;
                    }
                }
            }
            return beatCount;
        }

        private class SectionInfo
        {
            public string Name { get; set; }
            public int[] TrackBeatLengths { get; set; }
        }
    }
    [Flags]
    enum TrackFlags : byte
    {
        None = 0,
        Mute = 1,
        Solo = 2,
        NamedByUser = 4
    }
}
