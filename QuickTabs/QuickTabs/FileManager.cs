using Newtonsoft.Json.Linq;
using QuickTabs.Forms;
using QuickTabs.Songwriting;
using QuickTabs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs
{
    internal static class FileManager
    {
        private static readonly FileFormat[] supportedFormats = new FileFormat[] { new QtJsonFormat(), new QtzFormat(), new MidiFormat() };

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
            string fileExt = Path.GetExtension(fileName).ToLower();
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
        private static string generateFilter(bool save, string extension, out int filterIndex) // note to self: FileDialog filter index is fucking one-based??? who thought that made any sense??? christ
        {
            StringBuilder sb = new StringBuilder();
            if (!save)
                sb.Append("QuickTabs Files (*.qtjson, *.qtz)|*.qtjson;*.qtz|");
            filterIndex = 1;
            for (int i = 0; i < supportedFormats.Length; i++)
            {
                FileFormat format = supportedFormats[i];
                sb.Append(format.Name);
                sb.Append("|*");
                sb.Append(format.Extension);
                sb.Append('|');
                if (format.Extension == extension)
                {
                    filterIndex = i + 1;
                }
            }
            sb.Append("All Files (*.*)|*.*");
            if (!save)
                filterIndex = 1;
            return sb.ToString();
        }

        public static void Save(Song song)
        {
            if (CurrentFilePath == "")
            {
                SaveAs(song); return;
            }
            if (File.Exists(CurrentFilePath))
            {
                File.Delete(CurrentFilePath);
            }
            FileFormat usedFormat = findFormat(CurrentFilePath);
            usedFormat.Save(song, CurrentFilePath);
            IsSaved = true;
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
                int filterIndex;
                string fileExt = Path.GetExtension(CurrentFilePath);
                saveDialog.Filter = generateFilter(true, fileExt, out filterIndex);
                saveDialog.FileName = Path.GetFileName(CurrentFilePath);
                saveDialog.FilterIndex = filterIndex;
                if (CurrentFilePath == "")
                {
                    saveDialog.DefaultExt = ".qtjson";
                } else
                {
                    saveDialog.DefaultExt = fileExt;
                }
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
                int filterIndex;
                openDialog.Filter = generateFilter(false, "", out filterIndex);
                openDialog.FilterIndex = filterIndex;
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
