using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Songwriting
{
    class StringParser
    {
        private string str;
        private int currentLocation = 0;
        public StringParser(string Str)
        {
            str = Str;
        }
        public void Seek(int Offset, SeekOrigin Origin)
        {
            if (Origin == SeekOrigin.Begin)
            {
                if (Offset > str.Length - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                currentLocation = Offset;
            }
            else if (Origin == SeekOrigin.Current)
            {
                int absolute = currentLocation + Offset;
                if (absolute > str.Length - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                currentLocation = absolute;
            }
            else if (Origin == SeekOrigin.End)
            {
                int absolute = (str.Length - 1) + Offset;
                if (absolute > str.Length - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                currentLocation = absolute;
            }
        }
        public string Take(int Length, bool KeepPosition = false)
        {
            if (currentLocation + Length > str.Length)
            {
                throw new IndexOutOfRangeException();
            }
            string result = str.Substring(currentLocation, Length);
            if (!KeepPosition)
                currentLocation += Length;
            return result;
        }
        public string Peek(int Length)
        {
            return Take(Length, true);
        }
        public string TakeUntil(out string UsedSepeartor, params string[] Seperators)
        {
            int length = 0;
            for (;;)
            {
                foreach (string seperator in Seperators)
                {
                    if (Take(seperator.Length, true) == seperator)
                    {
                        //Console.WriteLine(seperator);
                        Seek(-length, SeekOrigin.Current);
                        string result = Take(length);
                        Seek(seperator.Length, SeekOrigin.Current);
                        UsedSepeartor = seperator;
                        return new string(result.Where(c => !char.IsControl(c)).ToArray());
                    }
                }
                Seek(1, SeekOrigin.Current);
                length++;
            }
        }
        public string TakeUntil(params string[] Seperators)
        {
            string output;
            return TakeUntil(out output, Seperators);
        }
        public int Position
        {
            get
            {
                return currentLocation;
            }
        }
    }
}
