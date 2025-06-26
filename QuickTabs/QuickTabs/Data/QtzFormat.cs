using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Data
{
    public class QtzFormat : FileFormat
    {
        public override string Extension => ".qtz";
        public override string Name => "QuickTabs Bytecode File (*.qtz)";
        public const ushort CurrentFormatVer = 1;

        private QtzVersion[] innerVersionTable = null;
        private QtzVersion[] versionTable
        {
            get
            {
                if (innerVersionTable == null)
                {
                    innerVersionTable = new QtzVersion[CurrentFormatVer + 1];
                    innerVersionTable[0] = new QtzFormatVer0();
                    innerVersionTable[1] = new QtzFormatVer1();
                }
                return innerVersionTable;
            }
        }
        private readonly byte[] signature = new byte[] { (byte)'Q', (byte)'T' };

        // verifies signature a version info then pass off to appropriate QtzVersion
        public override Song Open(string fileName, out bool failed)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length < 4)
                {
                    failed = true;
                    return null;
                }
                byte[] sig = fs.ReadBytes(2);
                if (!sig.SequenceEqual(signature))
                {
                    failed = true;
                    return null;
                }
                ushort formatVer = fs.ReadUInt16();
                if (formatVer > CurrentFormatVer)
                {
                    using (GenericMessage message = new GenericMessage())
                    {
                        message.Text = "Could not open file";
                        message.Message = "File was saved from a later, incompatible QuickTabs version. Restart with internet connection to update.";
                        message.ShowDialog();
                    }
                    failed = false;
                    return null;
                }
                QtzVersion ver = versionTable[formatVer];
                Song song;
                /*try
                {*/
                    song = ver.Open(fs, out failed);
                /*} catch
                {
                    failed = true;
                    return null;
                }*/
                return song;
            }
        }

        // implements the latest version of qtz. there is no backwards compatible saving, only opening.
        public override void Save(Song song, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                fs.Write(signature, 0, 2);
                fs.WriteUInt16(CurrentFormatVer);
                fs.WriteString(song.Name);
                fs.WriteUInt16((ushort)song.Tempo);
                fs.WriteByte((byte)song.TimeSignature.T1);
                fs.WriteByte((byte)song.TimeSignature.T2);
                fs.WriteByte((byte)song.Tracks.Count);
                foreach (Track track in song.Tracks)
                {
                    fs.WriteString(track.Name);
                    fs.WriteFloat32(track.Volume);
                    Tuning tuning = track.Tab.Tuning;
                    fs.WriteByte((byte)tuning.Count);
                    for (int i = 0; i < tuning.Count; i++)
                    {
                        fs.WriteString(tuning.GetMusicalNote(i).ToString());
                    }
                    TrackFlags flags = TrackFlags.None;
                    if (track.Solo)
                    {
                        flags |= TrackFlags.Solo;
                    }
                    if (track.Mute)
                    {
                        flags |= TrackFlags.Mute;
                    }
                    if (track.NamedByUser)
                    {
                        flags |= TrackFlags.NamedByUser;
                    }
                    fs.WriteByte((byte)flags);
                }
                Section[] sectionGraph = song.GetSectionGraph();
                fs.WriteByte((byte)sectionGraph.Length);
                foreach (Section section in sectionGraph)
                {
                    fs.WriteString(section.SectionName);
                    for (int i = 0; i < song.Tracks.Count; i++)
                    {
                        Track track = song.Tracks[i];
                        fs.WriteInt32(section.Content[track].Length);
                    }
                }
                using (ZLibStream zlib = new ZLibStream(fs, CompressionMode.Compress))
                {
                    foreach (Track track in song.Tracks)
                    {
                        foreach (Section section in sectionGraph)
                        {
                            Beat[] beatData = section.Content[track];
                            writeBeatData(zlib, beatData, song.TimeSignature, track.Tab.Tuning);
                        }
                    }
                }
            }
        }
        private static void writeBeatData(Stream stream, Beat[] beatData, TimeSignature ts, Tuning tuning)
        {
            MusicalTimespan measureLength = ts.MeasureLength;
            MusicalTimespan measureCounter = MusicalTimespan.Zero;
            bool divEncodeDue = true;
            foreach (Beat beat in beatData)
            {
                if (divEncodeDue)
                {
                    stream.WriteByte((byte)beat.BeatDivision.SerializeToInt32());
                    divEncodeDue = false;
                }
                StringState[] strings = new StringState[tuning.Count];
                for (int i = 0; i < strings.Length; i++)
                {
                    strings[i] = new StringState(0, MusicalTimespan.Zero);
                }
                foreach (KeyValuePair<Fret, MusicalTimespan> held in beat)
                {
                    strings[held.Key.String] = new StringState(held.Key.Space, held.Value);
                }
                foreach (StringState stringState in strings)
                {
                    stream.WriteByte((byte)stringState.Fret);
                    stream.WriteByte((byte)stringState.SustainTime.SerializeToInt32());
                }
                measureCounter += beat.BeatDivision;
                if (measureCounter >= measureLength)
                {
                    measureCounter = MusicalTimespan.Zero;
                    divEncodeDue = true;
                }
            }
        }
        private struct StringState
        {
            public int Fret;
            public MusicalTimespan SustainTime;
            public StringState(int fret, MusicalTimespan sustainTime)
            {
                Fret = fret;
                SustainTime = sustainTime;
            }
        }
    }
    public abstract class QtzVersion
    {
        public abstract Song Open(FileStream fs, out bool failed);
    }
}
