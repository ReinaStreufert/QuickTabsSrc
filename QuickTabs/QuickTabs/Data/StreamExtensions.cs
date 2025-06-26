using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Data
{
    public static class StreamExtensions
    {
        private delegate T ByteNumberConverter<T>(byte[] bytes, int startIndex);
        private delegate byte[] NumberByteConverter<T>(T val);

        public static void WriteUInt16(this Stream stream, ushort val)
        {
            writeNumber(stream, val, BitConverter.GetBytes);
        }
        public static void WriteInt16(this Stream stream, short val)
        {
            writeNumber(stream, val, BitConverter.GetBytes);
        }
        public static void WriteUInt32(this Stream stream, uint val)
        {
            writeNumber(stream, val, BitConverter.GetBytes);
        }
        public static void WriteInt32(this Stream stream, int val)
        {
            writeNumber(stream, val, BitConverter.GetBytes);
        }
        public static void WriteFloat32(this Stream stream, float val)
        {
            writeNumber(stream, val, BitConverter.GetBytes);
        }
        public static void WriteUInt64(this Stream stream, ulong val)
        {
            writeNumber(stream, val, BitConverter.GetBytes);
        }
        public static void WriteInt64(this Stream stream, long val)
        {
            writeNumber(stream, val, BitConverter.GetBytes);
        }
        public static void WriteString(this Stream stream, string val)
        {
            if (val.Length > byte.MaxValue)
            {
                throw new ArgumentException("Max string exceeded");
            }
            stream.WriteByte((byte)val.Length);
            stream.Write(Encoding.UTF8.GetBytes(val));
        }
        public static ushort ReadUInt16(this Stream stream)
        {
            return readNumber(stream, 2, BitConverter.ToUInt16);
        }
        public static short ReadInt16(this Stream stream)
        {
            return readNumber(stream, 2, BitConverter.ToInt16);
        }
        public static uint ReadUInt32(this Stream stream)
        {
            return readNumber(stream, 4, BitConverter.ToUInt32);
        }
        public static int ReadInt32(this Stream stream)
        {
            return readNumber(stream, 4, BitConverter.ToInt32);
        }
        public static float ReadFloat32(this Stream stream)
        {
            return readNumber(stream, 4, BitConverter.ToSingle);
        }
        public static ulong ReadUInt64(this Stream stream)
        {
            return readNumber(stream, 8, BitConverter.ToUInt64);
        }
        public static long ReadInt64(this Stream stream)
        {
            return readNumber(stream, 8, BitConverter.ToInt64);
        }
        public static string ReadString(this Stream stream)
        {
            int stringLen = stream.ReadByte();
            if (stringLen < 0)
            {
                throw new EndOfStreamException();
            }
            byte[] stringBuf = new byte[stringLen];
            stream.Read(stringBuf, 0, stringLen);
            return Encoding.UTF8.GetString(stringBuf);
        }
        public static byte[] ReadBytes(this Stream stream, int length)
        {
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, 2);
            return bytes;
        }

        private static void writeNumber<T>(Stream stream, T val, NumberByteConverter<T> converter)
        {
            byte[] bytes = converter(val);
            if (!BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            stream.Write(bytes, 0, bytes.Length);
        }
        private static T readNumber<T>(Stream stream, int len, ByteNumberConverter<T> converter)
        {
            byte[] bytes = new byte[len];
            if (!BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
            stream.Read(bytes, 0, len);
            return converter(bytes, 0);
        }
    }
}
