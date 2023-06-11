using Newtonsoft.Json.Linq;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal static class FileManager
    {
        private static readonly FileFormat[] supportedFormats = new FileFormat[] { new QtJsonFormat(), new QtzFormat() };

        private static bool isSaved = true;
        public static bool IsSaved
        {
            get
            {
                return isSaved;
            }
            private set
            {
                bool oldValue = isSaved;
                isSaved = value;
                if (oldValue != value)
                {
                    FileStateChange?.Invoke();
                }
            }
        }
        private static string currentFilePath = "";
        public static string CurrentFilePath
        {
            get
            {
                return currentFilePath;
            }
            private set
            {
                string oldValue = currentFilePath;
                currentFilePath = value;
                if (oldValue != value)
                {
                    FileStateChange?.Invoke();
                }
            }
        }
        public static event Action FileStateChange;

        public static void Initialize()
        {
            History.SubstantialChange += History_SubstantialChange;
        }

        private static void History_SubstantialChange()
        {
            IsSaved = false;
        }

        private static FileFormat findFormat(string fileName)
        {
            string fileExt = Path.GetExtension(fileName);
            FileFormat usedFormat = null;
            foreach (FileFormat format in supportedFormats)
            {
                if (format.Extension == fileExt)
                {
                    usedFormat = format;
                }
            }
            if (usedFormat == null)
            {
                usedFormat = new QtJsonFormat();
            }
            return usedFormat;
        }
        private static string generateFilter()
        {
            StringBuilder sb = new StringBuilder();
            foreach (FileFormat format in supportedFormats)
            {
                sb.Append(format.Name);
                sb.Append("|*");
                sb.Append(format.Extension);
                sb.Append('|');
            }
            sb.Append("All Files (*.*)|*.*");
            return sb.ToString();
        }

        public static void Save(Song song)
        {
            if (CurrentFilePath == "")
            {
                SaveAs(song); return;
            }
            IsSaved = true;
            if (File.Exists(CurrentFilePath))
            {
                File.Delete(CurrentFilePath);
            }
            FileFormat usedFormat = findFormat(CurrentFilePath);
            usedFormat.Save(song, CurrentFilePath);
        }
        public static void New()
        {
            CurrentFilePath = "";
            IsSaved = true;
        }
        public static void SaveAs(Song song)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = generateFilter();
                saveDialog.DefaultExt = "qtjson";
                DialogResult saveResult = saveDialog.ShowDialog();
                if (saveResult == DialogResult.OK)
                {
                    CurrentFilePath = saveDialog.FileName;
                    Save(song);
                }
            }
        }
        public static Song Open(out bool failed)
        {
            string newFilePath;
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = generateFilter();
                DialogResult openResult = openDialog.ShowDialog();
                if (openResult != DialogResult.OK)
                {
                    failed = false;
                    return null;
                }
                newFilePath = openDialog.FileName;
            }

            FileFormat usedFormat = findFormat(newFilePath);
            bool openFailed;
            Song song = usedFormat.Open(newFilePath, out openFailed);

            if (openFailed)
            {
                failed = true;
                return null;
            } else
            {
                CurrentFilePath = newFilePath;
                IsSaved = true;
                failed = false;
                return song;
            }
        }
    }
    internal abstract class FileFormat
    {
        public abstract string Extension { get; }
        public abstract string Name { get; }
        public abstract Song Open(string fileName, out bool failed);
        public abstract void Save(Song song, string fileName);
    }
}
