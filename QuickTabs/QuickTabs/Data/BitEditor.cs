using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Data
{
    class BitEditor
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

        public BitEditor(int count)
        {
            length = count;
            bytes = new byte[(int)Math.Ceiling(count / 8F)];
        }
        public BitEditor(byte[] bytes)
        {
            length = bytes.Length * 8;
            this.bytes = bytes;
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
            for (int i = index + count - 1; i >= index; i--)
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
            for (int i = 0; i < count; i++)
            {
                this[index + i] = (value & 0x01) > 0;
                value >>= 1;
            }
        }

        private int findByteIndex(int index, out int positionInByte)
        {
            int byteIndex = (int)Math.Floor(index / 8F);
            positionInByte = index - byteIndex * 8;
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
