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
    internal class QtzFormat : FileFormat
    {
        public override string Extension => ".qtz";
        public override string Name => "QuickTabs Bytecode File (*.qtz)";

        private readonly byte[] signature = new byte[] { (byte)'Q', (byte)'T' };
        private const ushort formatVersion = 0;
        private readonly int[] noteLengthCodes = new int[] { 1, 2, 3, 4, 6, 8, 12 };

        public override Song Open(string fileName, out bool failed)
        {
            Song song = new Song();
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    // header
                    byte[] signatureBytes = new byte[2];
                    fs.Read(signatureBytes, 0, 2);
                    if (!signature.SequenceEqual(signatureBytes))
                    {
                        failed = true;
                        return null;
                    }
                    byte[] versionBytes = new byte[2];
                    fs.Read(versionBytes, 0, 2);
                    ushort fileVersion = BitConverter.ToUInt16(versionBytes, 0);
                    if (fileVersion > formatVersion)
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

                    // meta data
                    int nameLength = fs.ReadByte();
                    byte[] nameBytes = new byte[nameLength];
                    fs.Read(nameBytes, 0, nameLength);
                    song.Name = Encoding.UTF8.GetString(nameBytes);
                    byte[] tempoBytes = new byte[2];
                    fs.Read(tempoBytes, 0, 2);
                    if (!BitConverter.IsLittleEndian)
                    {
                        tempoBytes = tempoBytes.Reverse().ToArray();
                    }
                    song.Tempo = BitConverter.ToInt16(tempoBytes, 0);
                    int t1 = fs.ReadByte();
                    int t2 = fs.ReadByte();
                    song.TimeSignature = new TimeSignature(t1, t2);
                    int tuningCount = fs.ReadByte();
                    string[] tuningNotes = new string[tuningCount];
                    for (int i = 0; i < tuningCount; i++)
                    {
                        int noteNameLength = fs.ReadByte();
                        byte[] noteNameBytes = new byte[noteNameLength];
                        fs.Read(noteNameBytes, 0, noteNameLength);
                        tuningNotes[i] = Encoding.UTF8.GetString(noteNameBytes);
                    }
                    song.Tab.Tuning = new Tuning(tuningNotes);
                    byte[] stepCountBytes = new byte[4];
                    fs.Read(stepCountBytes, 0, 4);
                    if (!BitConverter.IsLittleEndian)
                    {
                        stepCountBytes = stepCountBytes.Reverse().ToArray();
                    }
                    song.Tab.SetLength(BitConverter.ToInt32(stepCountBytes, 0));

                    // section heads
                    int sectionHeadCount = fs.ReadByte();
                    for (int i = 0; i < sectionHeadCount; i++)
                    {
                        SectionHead sectionHead = new SectionHead();
                        byte[] positionBytes = new byte[4];
                        fs.Read(positionBytes, 0, 4);
                        if (!BitConverter.IsLittleEndian)
                        {
                            positionBytes = positionBytes.Reverse().ToArray();
                        }
                        int position = BitConverter.ToInt32(positionBytes, 0);
                        int sectionNameLength = fs.ReadByte();
                        byte[] sectionNameBytes = new byte[sectionNameLength];
                        fs.Read(sectionNameBytes, 0, sectionNameLength);
                        sectionHead.Name = Encoding.UTF8.GetString(sectionNameBytes);
                        song.Tab[position] = sectionHead;
                    }

                    // bitmask and tab data
                    byte[] bitmaskCompressedLengthBytes = new byte[4];
                    fs.Read(bitmaskCompressedLengthBytes, 0, 4);
                    if (!BitConverter.IsLittleEndian)
                    {
                        bitmaskCompressedLengthBytes = bitmaskCompressedLengthBytes.Reverse().ToArray();
                    }
                    int bitmaskCompressedLength = BitConverter.ToInt32(bitmaskCompressedLengthBytes, 0);
                    long bitmaskStart = fs.Position;
                    BitEditor bitmask = new BitEditor(song.Tab.BeatCount * (tuningCount + 3));
                    using (ZLibStream bitmaskZLib = new ZLibStream(fs, CompressionMode.Decompress, true))
                    {
                        bitmaskZLib.Read(bitmask.ByteArray, 0, bitmask.ByteArray.Length);
                    }
                    fs.Seek(bitmaskStart + bitmaskCompressedLength, SeekOrigin.Begin);
                    byte[] tabDataLengthBytes = new byte[4];
                    fs.Read(tabDataLengthBytes, 0, 4);
                    if (!BitConverter.IsLittleEndian)
                    {
                        tabDataLengthBytes = tabDataLengthBytes.Reverse().ToArray();
                    }
                    int tabDataLength = BitConverter.ToInt32(tabDataLengthBytes, 0);
                    byte[] tabData = new byte[tabDataLength];
                    using (ZLibStream tabDataZLib = new ZLibStream(fs, CompressionMode.Decompress, true))
                    {
                        tabDataZLib.Read(tabData, 0, tabData.Length);
                        tabDataZLib.Flush();
                    }
                    int bitmaskI = 0;
                    int tabdataI = 0;
                    foreach (Step step in song.Tab)
                    {
                        if (step.Type == Enums.StepType.Beat)
                        {
                            Beat beat = (Beat)step;
                            int noteLengthCode = bitmask.ReadBitsAsNumber(bitmaskI, 3);
                            bitmaskI += 3;
                            beat.NoteLength = getNoteLength(noteLengthCode);
                            for (int stringI = 0; stringI < tuningCount; stringI++)
                            {
                                if (bitmask[bitmaskI])
                                {
                                    beat[new Fret(stringI, tabData[tabdataI])] = true;
                                    tabdataI++;
                                }
                                bitmaskI++;
                            }
                        }
                    }
                } catch (IndexOutOfRangeException ex)
                {
                    failed = true;
                    return null;
                } catch (FormatException ex)
                {
                    failed = true;
                    return null;
                } catch (OverflowException ex)
                {
                    failed = true;
                    return null;
                }
            }
            failed = false;
            return song;
        }

        public override void Save(Song song, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                // header
                fs.Write(signature, 0, 2);
                byte[] versionBytes = BitConverter.GetBytes(formatVersion);
                fs.Write(versionBytes, 0, 2);

                // metadata
                byte[] nameBytes = Encoding.UTF8.GetBytes(song.Name);
                fs.WriteByte((byte)nameBytes.Length);
                fs.Write(nameBytes, 0, nameBytes.Length);
                byte[] tempoBytes = BitConverter.GetBytes((ushort)song.Tempo);
                if (!BitConverter.IsLittleEndian)
                {
                    tempoBytes = tempoBytes.Reverse().ToArray();
                }
                fs.Write(tempoBytes, 0, 2);
                fs.WriteByte((byte)song.TimeSignature.T1);
                fs.WriteByte((byte)song.TimeSignature.T2);
                Tuning tuning = song.Tab.Tuning;
                fs.WriteByte((byte)tuning.Count);
                for (int i = 0; i < tuning.Count; i++)
                {
                    string note = tuning.GetMusicalNote(i).ToString();
                    byte[] noteBytes = Encoding.UTF8.GetBytes(note);
                    fs.WriteByte((byte)noteBytes.Length);
                    fs.Write(noteBytes, 0, noteBytes.Length);
                }
                byte[] stepCountBytes = BitConverter.GetBytes(song.Tab.Count);
                if (!BitConverter.IsLittleEndian)
                {
                    stepCountBytes = stepCountBytes.Reverse().ToArray();
                }
                fs.Write(stepCountBytes, 0, 4);

                // section heads
                List<SectionHead> sectionHeads = new List<SectionHead>();
                foreach (Step step in song.Tab)
                {
                    if (step.Type == Enums.StepType.SectionHead)
                    {
                        sectionHeads.Add((SectionHead)step);
                    }
                }
                fs.WriteByte((byte)sectionHeads.Count);
                foreach (SectionHead sectionHead in sectionHeads)
                {
                    byte[] positionBytes = BitConverter.GetBytes(sectionHead.IndexWithinTab);
                    if (!BitConverter.IsLittleEndian)
                    {
                        positionBytes = positionBytes.Reverse().ToArray();
                    }
                    fs.Write(positionBytes, 0, 4);
                    byte[] sectionNameBytes = Encoding.UTF8.GetBytes(sectionHead.Name);
                    fs.WriteByte((byte)sectionNameBytes.Length);
                    fs.Write(sectionNameBytes, 0, sectionNameBytes.Length);
                }

                // bitmask and tab data
                BitEditor bitmask = new BitEditor(song.Tab.BeatCount * (tuning.Count + 3));
                List<byte> tabData = new List<byte>();
                int bitmaskI = 0;
                bool[] strings = new bool[tuning.Count];
                foreach (Step step in song.Tab)
                {
                    if (step.Type == Enums.StepType.Beat)
                    {
                        Beat beat = (Beat)step;
                        bitmask.WriteNumberAsBits(bitmaskI, 3, getNoteLengthCode(beat.NoteLength));
                        bitmaskI += 3;
                        for (int i = 0; i < strings.Length; i++)
                        {
                            strings[i] = false;
                        }
                        foreach (Fret fret in beat)
                        {
                            strings[fret.String] = true;
                            tabData.Add((byte)fret.Space);
                        }
                        for (int i = 0; i < strings.Length; i++)
                        {
                            bitmask[bitmaskI] = strings[i];
                            bitmaskI++;
                        }
                    }
                }
                byte[] bitmaskBytes = bitmask.ByteArray;
                byte[] tabDataBytes = tabData.ToArray();
                fs.Write(new byte[4], 0, 4); // will be bitmask compressed length
                long bitmaskStart = fs.Position;
                using (ZLibStream bitmaskZLib = new ZLibStream(fs, CompressionLevel.Optimal, true))
                {
                    bitmaskZLib.Write(bitmaskBytes, 0, bitmaskBytes.Length);
                }
                long tabDataStart = fs.Position;
                int bitmaskCompressedLength = (int)(tabDataStart - bitmaskStart);
                byte[] bitmaskCompressedLengthBytes = BitConverter.GetBytes(bitmaskCompressedLength);
                if (!BitConverter.IsLittleEndian)
                {
                    bitmaskCompressedLengthBytes = bitmaskCompressedLengthBytes.Reverse().ToArray();
                }
                fs.Seek(bitmaskStart - 4, SeekOrigin.Begin);
                fs.Write(bitmaskCompressedLengthBytes, 0, 4);
                fs.Seek(tabDataStart, SeekOrigin.Begin);
                byte[] tabDataLengthBytes = BitConverter.GetBytes(tabDataBytes.Length);
                if (!BitConverter.IsLittleEndian)
                {
                    tabDataLengthBytes = tabDataLengthBytes.Reverse().ToArray();
                }
                fs.Write(tabDataLengthBytes, 0, 4);
                using (ZLibStream tabDataZLib = new ZLibStream(fs, CompressionLevel.Optimal, true))
                {
                    tabDataZLib.Write(tabDataBytes, 0, tabDataBytes.Length);
                }
            }
        }

        private int getNoteLengthCode(int noteLength)
        {
            for (int i = 0; i < noteLengthCodes.Length; i++)
            {
                if (noteLengthCodes[i] == noteLength)
                {
                    return i;
                }
            }
            return 0;
        }
        private int getNoteLength(int code)
        {
            return noteLengthCodes[code];
        }
    }
}
