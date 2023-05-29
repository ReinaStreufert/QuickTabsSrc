using QuickTabs.Properties;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QuickTabs.Controls
{
    internal static class DrawingIcons
    {
        public static MultiColorBitmap QuickTabsLogo { get; private set; } = null;

        public static MultiColorBitmap EighthNote { get; private set; } = null;
        public static MultiColorBitmap QuarterNote { get; private set; } = null;
        public static MultiColorBitmap DottedQuarterNote { get; private set; } = null;
        public static MultiColorBitmap HalfNote { get; private set; } = null;
        public static MultiColorBitmap DottedHalfNote { get; private set; } = null;
        public static MultiColorBitmap WholeNote { get; private set; } = null;
        public static MultiColorBitmap DottedWholeNote { get; private set; } = null;

        public static MultiColorBitmap LeftArrow { get; private set; } = null;
        public static MultiColorBitmap RightArrow { get; private set; } = null;

        public static MultiColorBitmap Guitar { get; private set; } = null;
        public static MultiColorBitmap Record { get; private set; } = null;

        public static MultiColorBitmap OpenFile { get; private set; } = null;
        public static MultiColorBitmap SaveFile { get; private set; } = null;
        public static MultiColorBitmap SaveFileAs { get; private set; } = null;
        public static MultiColorBitmap Reload { get; private set; } = null;
        public static MultiColorBitmap EditDocumentProperties { get; private set; } = null;
        public static MultiColorBitmap Export { get; private set; } = null;
        public static MultiColorBitmap Settings { get; private set; } = null;

        public static MultiColorBitmap Dots { get; private set; } = null;
        public static MultiColorBitmap Counter { get; private set; } = null;
        public static MultiColorBitmap CompactContextMenu { get; private set; } = null;
        public static MultiColorBitmap LargeFretboard { get; private set; } = null;

        public static MultiColorBitmap AddMeasure { get; private set; } = null;
        public static MultiColorBitmap RemoveMeasure { get; private set; } = null;
        public static MultiColorBitmap AddSection { get; private set; } = null;
        public static MultiColorBitmap RemoveSection { get; private set; } = null;
        public static MultiColorBitmap Rename { get; private set; } = null;

        public static MultiColorBitmap Copy { get; private set; } = null;
        public static MultiColorBitmap Paste { get; private set; } = null;
        public static MultiColorBitmap ShiftLeft { get; private set; } = null;
        public static MultiColorBitmap ShiftRight { get; private set; } = null;
        public static MultiColorBitmap ShiftUp { get; private set; } = null;
        public static MultiColorBitmap ShiftDown { get; private set; } = null;
        public static MultiColorBitmap Plus { get; private set; } = null;
        public static MultiColorBitmap Minus { get; private set; } = null;
        public static MultiColorBitmap Clear { get; private set; } = null;

        public static MultiColorBitmap Undo { get; private set; } = null;
        public static MultiColorBitmap Redo { get; private set; } = null;
        public static MultiColorBitmap RedoAlternate { get; private set; } = null;

        public static MultiColorBitmap PlayPause { get; private set; } = null;
        public static MultiColorBitmap Repeat { get; private set; } = null;
        public static MultiColorBitmap Metronome { get; private set; } = null;
        public static MultiColorBitmap Download { get; private set; } = null;

        public static MultiColorBitmap Check { get; private set; } = null;

        public static void LoadAll()
        {
            QuickTabsLogo = loadIcon("logo", Color.White, DrawingConstants.LogoPatternColor);

            EighthNote = loadIcon("music-note-eighth", Color.White, DrawingConstants.FadedGray);
            QuarterNote = loadIcon("music-note-quarter", Color.White, DrawingConstants.FadedGray);
            DottedQuarterNote = loadIcon("music-note-quarter-dotted", Color.White, DrawingConstants.FadedGray);
            HalfNote = loadIcon("music-note-half", Color.White, DrawingConstants.FadedGray);
            DottedHalfNote = loadIcon("music-note-half-dotted", Color.White, DrawingConstants.FadedGray);
            WholeNote = loadIcon("music-note-whole", Color.White, DrawingConstants.FadedGray);
            DottedWholeNote = loadIcon("music-note-whole-dotted", Color.White, DrawingConstants.FadedGray);

            LeftArrow = loadIcon("arrow-left", Color.White, DrawingConstants.FadedGray);
            RightArrow = loadIcon("arrow-right", Color.White, DrawingConstants.FadedGray);

            Guitar = loadIcon("guitar-acoustic", Color.White, DrawingConstants.FadedGray);
            Record = loadIcon("record-circle", Color.White, DrawingConstants.FadedGray);

            OpenFile = loadIcon("folder-open-outline", Color.White);
            SaveFile = loadIcon("content-save-all-outline", Color.White);
            SaveFileAs = loadIcon("content-save-plus-outline", Color.White);
            Reload = loadIcon("reload", Color.White);
            EditDocumentProperties = loadIcon("pencil-outline", Color.White);
            Export = loadIcon("export", Color.White);
            Settings = loadIcon("cog", Color.White);

            Dots = loadIcon("circle-small", Color.White, DrawingConstants.FadedGray);
            Counter = loadIcon("numeric", Color.White, DrawingConstants.FadedGray);
            CompactContextMenu = loadIcon("arrow-expand-up", Color.White, DrawingConstants.FadedGray);
            LargeFretboard = loadIcon("arrow-expand-vertical", Color.White, DrawingConstants.FadedGray);

            AddMeasure = loadIcon("plus-box-outline", Color.White);
            RemoveMeasure = loadIcon("minus-box-outline", Color.White, DrawingConstants.FadedGray);
            AddSection = loadIcon("plus-box-multiple-outline", Color.White, DrawingConstants.FadedGray);
            RemoveSection = loadIcon("minus-box-multiple-outline", Color.White, DrawingConstants.FadedGray);
            Rename = loadIcon("rename-outline", Color.White);

            Copy = loadIcon("content-copy", Color.White);
            Paste = loadIcon("content-paste", Color.White, DrawingConstants.FadedGray);
            ShiftLeft = loadIcon("pan-left", Color.White);
            ShiftRight = loadIcon("pan-right", Color.White);
            ShiftUp = loadIcon("pan-up", Color.White);
            ShiftDown = loadIcon("pan-down", Color.White);
            Plus = loadIcon("plus", Color.White);
            Minus = loadIcon("minus", Color.White);
            Clear = loadIcon("close", Color.White);

            Undo = loadIcon("undo", Color.White, DrawingConstants.FadedGray);
            Redo = loadIcon("redo", Color.White, DrawingConstants.FadedGray);
            RedoAlternate = loadIcon("redo-variant", Color.White, DrawingConstants.FadedGray);

            PlayPause = loadIcon("play-pause", Color.White, DrawingConstants.FadedGray);
            Repeat = loadIcon("repeat", Color.White, DrawingConstants.FadedGray);
            Metronome = loadIcon("metronome", Color.White, DrawingConstants.FadedGray);
            Download = loadIcon("download-box", Color.White);

            Check = loadIcon("check", Color.White);

            FieldInfo[] allFields = typeof(DrawingIcons).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            bool fail = false;
            MultiColorBitmap x = null;
            foreach (FieldInfo field in allFields)
            {
                if (field.GetValue(null) == null)
                {
                    if (!fail)
                    {
                        x = loadXIcon(Color.White, DrawingConstants.FadedGray, DrawingConstants.LogoPatternColor);
                        fail = true;
                    }
                    field.SetValue(null, x);
                }
            }
            if (fail)
            {
                throw new FileNotFoundException();
            }
        }
        private static MultiColorBitmap loadIcon(string iconName, params Color[] colors)
        {
            string iconPath = "icons\\" + iconName + ".png";
            if (!File.Exists(iconPath))
            {
                return null;
            }
            MultiColorBitmap result = new MultiColorBitmap((Bitmap)Bitmap.FromFile(iconPath));
            foreach (Color color in colors)
            {
                result.AddColor(color);
            }
            return result;
        }
        private static MultiColorBitmap loadXIcon(params Color[] colors)
        {
            Bitmap x = new Bitmap(256, 256, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(x))
            {
                using (Pen p = new Pen(Color.Black, 10.0F))
                {
                    g.DrawLine(p, 0, 0, 256, 256);
                    g.DrawLine(p, 256, 0, 0, 256);
                }
            }
            MultiColorBitmap result = new MultiColorBitmap(x);
            foreach (Color color in colors)
            {
                result.AddColor(color);
            }
            return result;
        }
    }
    internal class MultiColorBitmap
    {
        private Bitmap originalBitmap;
        private Dictionary<Color, Bitmap> variations = new Dictionary<Color, Bitmap>();
        public MultiColorBitmap(Bitmap originalBitmap)
        {
            this.originalBitmap = originalBitmap;
        }
        public Bitmap this[Color color]
        {
            get
            {
                return variations[color];
            }
        }
        public Size Size
        {
            get
            {
                return originalBitmap.Size;
            }
        }
        public void AddColor(Color color)
        {
            Bitmap variation = new Bitmap(originalBitmap.Width, originalBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(variation))
            {
                g.Clear(Color.Transparent);
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                g.DrawImage(originalBitmap, 0, 0, originalBitmap.Width, originalBitmap.Height);
            }
            BitmapData bmpData = variation.LockBits(new Rectangle(0, 0, variation.Width, variation.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int dataLength = Math.Abs(bmpData.Stride) * bmpData.Height;
            IntPtr begin = bmpData.Scan0;
            for (int i = 0; i < dataLength; i += 4)
            {
                IntPtr alphaPtr = begin + i + 3;
                if (Marshal.ReadByte(alphaPtr) > 0)
                {
                    Marshal.WriteByte(begin + i, color.B);
                    Marshal.WriteByte(begin + i + 1, color.G);
                    Marshal.WriteByte(begin + i + 2, color.R);
                }
            }
            variation.UnlockBits(bmpData);
            variations[color] = variation;
        }
    }
}
