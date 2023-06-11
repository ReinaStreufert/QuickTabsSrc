using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal class QtzFormat : FileFormat
    {
        public override string Extension => ".qtz";
        public override string Name => "QuickTabs Bytecode File (*.qtz)";

        private readonly int[] noteLengthCodes = new int[] { 1, 2, 3, 4, 6, 8, 12 };

        public override Song Open(string fileName, out bool failed)
        {
            Song song = new Song();
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                try
                {
                    // meta data
                    int nameLength = fs.ReadByte();
                    byte[] nameBytes = new byte[nameLength];
                    fs.Read(nameBytes, 0, nameLength);
                    song.Name = Encoding.UTF8.GetString(nameBytes);
                    byte[] tempoBytes = new byte[2];
                    fs.Read(tempoBytes, 0, 2);
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
                    song.Tab.SetLength(BitConverter.ToInt32(stepCountBytes, 0));

                    // section heads
                    int sectionHeadCount = fs.ReadByte();
                    for (int i = 0; i < sectionHeadCount; i++)
                    {
                        SectionHead sectionHead = new SectionHead();
                        byte[] positionBytes = new byte[4];
                        fs.Read(positionBytes, 0, 4);
                        int position = BitConverter.ToInt32(positionBytes, 0);
                        int sectionNameLength = fs.ReadByte();
                        byte[] sectionNameBytes = new byte[sectionNameLength];
                        fs.Read(sectionNameBytes, 0, sectionNameLength);
                        sectionHead.Name = Encoding.UTF8.GetString(sectionNameBytes);
                        song.Tab[position] = sectionHead;
                    }

                    // bitmask and tab data
                    Bitmask bitmask = new Bitmask(song.Tab.BeatCount * (tuningCount + 3));
                    fs.Read(bitmask.ByteArray, 0, bitmask.ByteArray.Length);
                    byte[] tabData = new byte[fs.Length - fs.Position];
                    fs.Read(tabData, 0, tabData.Length);
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
                }
            }
            failed = false;
            return song;
        }

        public override void Save(Song song, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                // metadata
                byte[] nameBytes = Encoding.UTF8.GetBytes(song.Name);
                fs.WriteByte((byte)nameBytes.Length);
                fs.Write(nameBytes, 0, nameBytes.Length);
                byte[] tempoBytes = BitConverter.GetBytes((ushort)song.Tempo);
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
                    fs.Write(positionBytes, 0, 4);
                    byte[] sectionNameBytes = Encoding.UTF8.GetBytes(sectionHead.Name);
                    fs.WriteByte((byte)sectionNameBytes.Length);
                    fs.Write(sectionNameBytes, 0, sectionNameBytes.Length);
                }

                // bitmask and tab data
                Bitmask bitmask = new Bitmask(song.Tab.BeatCount * (tuning.Count + 3));
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
                fs.Write(bitmaskBytes, 0, bitmaskBytes.Length);
                fs.Write(tabDataBytes, 0, tabDataBytes.Length);
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

        private class Bitmask
        {
            private int length;
            private byte[] bytes;

            public byte[] ByteArray
            {
                get { return bytes; }
            }
            public int Count
            {
                get { return length; }
            }

            public Bitmask(int count)
            {
                length = count;
                bytes = new byte[(int)Math.Ceiling(count / 8F)];
            }

            public bool this[int index]
            {
                get
                {
                    if (index < 0 || index >= length)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    int bitIndex;
                    int byteIndex = findByteIndex(index, out bitIndex);
                    return getBit(bytes[byteIndex], bitIndex);
                }
                set
                {
                    if (index < 0 || index >= length)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    int bitIndex;
                    int byteIndex = findByteIndex(index, out bitIndex);
                    bytes[byteIndex] = modifyBit(bytes[byteIndex], bitIndex, value);
                }
            }
            public int ReadBitsAsNumber(int index, int count)
            {
                if (index < 0 || index + (count - 1) >= length)
                {
                    throw new IndexOutOfRangeException();
                }
                int result = 0;
                for (int i = index; i < index + count; i++)
                {
                    result <<= 1;
                    if (this[i])
                    {
                        result |= 0x01;
                    }
                }
                return result;
            }
            public void WriteNumberAsBits(int index, int count, int value)
            {
                if (index < 0 || index + (count - 1) >= length)
                {
                    throw new IndexOutOfRangeException();
                }
                for (int i = count - 1; i >= 0; i--)
                {
                    this[index + i] = (value & 0x01) > 0;
                    value >>= 1;
                }
            }

            private int findByteIndex(int index, out int positionInByte)
            {
                int byteIndex = (int)Math.Floor(index / 8F);
                positionInByte = index - (byteIndex * 8);
                return byteIndex;
            }
            private bool getBit(byte b, int index)
            {
                return (b & getFocus(index)) > 0;
            }
            private byte modifyBit(byte b, int index, bool val)
            {
                byte focus = getFocus(index);
                byte modified = (byte)(b | focus); // first make the bit on no matter what
                if (!val)
                {
                    modified = (byte)(modified ^ focus); // then we flip it if were trying to set to false
                }
                return modified;
            }
            private byte getFocus(int index)
            {
                return (byte)(0x01 << index);
            }
        }
    }
}
