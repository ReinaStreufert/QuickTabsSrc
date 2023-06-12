using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Data
{
    internal class MidiFormat : FileFormat // note: midi is biiiiiig endian
    {
        public override string Extension => ".mid";

        public override string Name => "MIDI Tab File";

        public override Song Open(string fileName, out bool failed)
        {
            throw new NotImplementedException();
        }

        public override void Save(Song song, string fileName)
        {
            throw new NotImplementedException();
        }

        private class MidiStreamReader // MidiStreamReader assumes a delta time format of 2 ticks per quarter note (0x00 0x02)
        {
            private Stream stream;
            private long midiStart;

            public MidiStreamReader(Stream stream)
            {
                this.stream = stream;
                midiStart = stream.Position;
            }


        }
        private abstract class MidiMessage
        {
            public abstract MidiMessageType MessageType { get; }
            public abstract byte StatusIdentifier { get; }     // defines what to look for in status byte to recognize the specific midi message type
            public abstract byte StatusBitmask { get; }        // defines which bits of the status bytes to look in when checking if a status matches the identifier
            public abstract void ReadData(Stream stream);           // defines the message-type dependent operation that reads the correct length of data bytes for the message

            public byte Status { get; set; }
            public byte[] Data { get; set; }

            private static Type[] midiMessageTypes = new Type[] { typeof(NoteOnMessage), typeof(NoteOffMessage), typeof(ControlChangeMessage) };
            public static MidiMessage FromStream(Stream stream)
            {
                byte status = (byte)stream.ReadByte();
                foreach (Type type in midiMessageTypes)
                {
                    MidiMessage message = (MidiMessage)type.GetConstructor(new Type[0]).Invoke(null);
                    if ((status & message.StatusBitmask) == message.StatusIdentifier)
                    {
                        message.ReadData(stream);
                        return message;
                    }
                }
                return new InvalidMidiMessage();
            }
        }
        private class InvalidMidiMessage : MidiMessage
        {
            public override MidiMessageType MessageType => MidiMessageType.Unsupported;
            public override byte StatusIdentifier => 0x00;
            public override byte StatusBitmask => 0x00;

            public override void ReadData(Stream stream)
            {
                return;
            }
        }
        private abstract class ChannelMidiMessage : MidiMessage
        {
            public override byte StatusBitmask { get => 0xF0; }
            public int MidiChannel
            {
                get
                {
                    return Status & 0x0F;
                }
                set
                {
                    Status = (byte)((Status & 0xF0) | (value & 0x0F));
                }
            }
        }
        private abstract class NoteMessage : ChannelMidiMessage
        {
            public override void ReadData(Stream stream)
            {
                Data = new byte[2];
                stream.Read(Data, 0, 2);
            }
            public byte NoteNumber
            {
                get
                {
                    return Data[0];
                }
                set
                {
                    Data[0] = value;
                }
            }
            public byte Velocity
            {
                get
                {
                    return Data[1];
                }
                set
                {
                    Data[1] = value;
                }
            }
        }
        // all supported messages
        private class NoteOnMessage : NoteMessage
        {
            public override byte StatusIdentifier => 0x80;
            public override MidiMessageType MessageType => MidiMessageType.NoteOn;
        }
        private class NoteOffMessage : NoteMessage
        {
            public override byte StatusIdentifier => 0x90;
            public override MidiMessageType MessageType => MidiMessageType.NoteOff;
        }
        private class ControlChangeMessage : ChannelMidiMessage
        {
            public override byte StatusIdentifier => 0xB0;
            public override MidiMessageType MessageType => MidiMessageType.ControlChange;

            public override void ReadData(Stream stream)
            {
                Data = new byte[2];
                stream.Read(Data, 0, 2);
            }

            public byte ControllerNumber
            {
                get
                {
                    return Data[0];
                }
                set
                {
                    Data[0] = value;
                }
            }
            public byte NewValue
            {
                get
                {
                    return Data[1];
                }
                set
                {
                    Data[1] = value;
                }
            }
        }
        private class MetaMessage : MidiMessage
        {
            public override MidiMessageType MessageType => MidiMessageType.MetaEvent;
            public override byte StatusBitmask => 0xFF;
            public override byte StatusIdentifier => 0xFF;

            public MetaEventType MetaType { get; set; }

            public override void ReadData(Stream stream)
            {
                MetaType = (MetaEventType)stream.ReadByte();
                int dataLength = stream.ReadByte();
                Data = new byte[dataLength];
                stream.Read(Data, 0, dataLength);

                if ((byte)MetaType <= 0x07)
                {

                }
            }

            public abstract class MidiMetadata
            {
                public byte[] Data { get; set; }
            }
            public class MetadataString : MidiMetadata
            {
                public string Text
                {
                    get
                    {
                        return Encoding.ASCII.GetString(Data);
                    }
                    set
                    {
                        byte[] newData = Encoding.ASCII.GetBytes(value);
                        byte[] data = Data;
                        Array.Resize(ref data, newData.Length);
                        newData.CopyTo(data, 0);
                    }
                }
            }
        }

        private abstract class UnsupportedChannelMidiMessage1 : ChannelMidiMessage // 1 as in 1 byte of data
        {
            public override MidiMessageType MessageType => MidiMessageType.Unsupported;

            public override void ReadData(Stream stream)
            {
                Data = new byte[1];
                stream.Read(Data, 0, 1);
            }
        }
        private abstract class UnsupportedChannelMidiMessage2 : ChannelMidiMessage
        {
            public override MidiMessageType MessageType => MidiMessageType.Unsupported;

            public override void ReadData(Stream stream)
            {
                Data = new byte[2];
                stream.Read(Data, 0, 2);
            }
        }
        private abstract class UnsupportedMidiMessage : MidiMessage
        {
            public override byte StatusBitmask => 0xFF;
            public override MidiMessageType MessageType => MidiMessageType.Unsupported;
        }
        private abstract class UnsupportedMidiMessage0 : UnsupportedMidiMessage
        {
            public override void ReadData(Stream stream)
            {
                return;
            }
        }
        private abstract class UnsupportedMidiMessage1 : UnsupportedMidiMessage
        {
            public override void ReadData(Stream stream)
            {
                Data = new byte[1];
                stream.Read(Data, 0, 1);
            }
        }
        private abstract class UnsupportedMidiMessage2 : UnsupportedMidiMessage
        {
            public override void ReadData(Stream stream)
            {
                Data = new byte[2];
                stream.Read(Data, 0, 2);
            }
        }
        // all unsupported messages
        private class PolyphonicKeyPressureMessage : UnsupportedChannelMidiMessage2
        {
            public override byte StatusIdentifier => 0xA0;
        }
        private class ProgramChangeMessage : UnsupportedChannelMidiMessage1
        {
            public override byte StatusIdentifier => 0xC0;
        }
        private class ChannelPressureMessage : UnsupportedChannelMidiMessage1
        {
            public override byte StatusIdentifier => 0xD0;
        }
        private class PitchWheelMessage : UnsupportedChannelMidiMessage2
        {
            public override byte StatusIdentifier => 0xE0;
        }
        private class SysExMessage : UnsupportedMidiMessage
        {
            public override byte StatusIdentifier => 0xF0;

            public override void ReadData(Stream stream)
            {
                const byte terminator = 0b11110111;
                List<byte> bytes= new List<byte>();
                for (; ;)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte == terminator)
                    {
                        break;
                    } else
                    {
                        bytes.Add((byte)nextByte);
                    }
                }
                Data = bytes.ToArray();
            }
        }
        private class SongPositionMessage : UnsupportedMidiMessage2
        {
            public override byte StatusIdentifier => 0xF2;
        }
        private class SongSelectMessage : UnsupportedMidiMessage1
        {
            public override byte StatusIdentifier => 0xF3;
        }
        private class TuneRequestMessage : UnsupportedMidiMessage0
        {
            public override byte StatusIdentifier => 0xF6;
        }
        private class RealTimeMessage : UnsupportedMidiMessage0
        {
            public override byte StatusBitmask => 0xF8;     // ]
            public override byte StatusIdentifier => 0xF8;  // ] - this covers all IDs 0xF8 and above
        }
        private enum MidiMessageType
        {
            NoteOn,
            NoteOff,
            ControlChange,
            MetaEvent,
            Unsupported
        }
        private enum MetaEventType : byte
        {
            SequenceNumber = 0x00,
            Text = 0x01,
            CopyrightNotice = 0x02,
            SequenceName = 0x03,
            InstrumentName = 0x04,
            Lyric = 0x05,
            Marker = 0x06,
            CuePoint = 0x07, // end of string metadata types
            MidiChannelPrefix = 0x20,
            EndOfTrack = 0x2F,
            SetTempo = 0x51,
            SMPTEOffset = 0x54,
            TimeSignature = 0x58,
            KeySignature = 0x59
        }
    }
}
