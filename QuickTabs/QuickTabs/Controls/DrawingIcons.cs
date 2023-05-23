using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal static class DrawingIcons
    {
        public static MultiColorBitmap QuickTabsLogo { get; private set; } = null;

        public static MultiColorBitmap EighthNote { get; private set; } = null;
        public static MultiColorBitmap QuarterNote { get; private set; } = null;
        public static MultiColorBitmap HalfNote { get; private set; } = null;
        public static MultiColorBitmap WholeNote { get; private set; } = null;

        public static MultiColorBitmap LeftArrow { get; private set; } = null;
        public static MultiColorBitmap RightArrow { get; private set; } = null;

        public static MultiColorBitmap Guitar { get; private set; } = null;
        public static MultiColorBitmap PlayPause { get; private set; } = null;
        public static MultiColorBitmap Record { get; private set; } = null;

        public static MultiColorBitmap OpenFile { get; private set; } = null;
        public static MultiColorBitmap SaveFile { get; private set; } = null;
        public static MultiColorBitmap SaveFileAs { get; private set; } = null;
        public static MultiColorBitmap NewFile { get; private set; } = null;
        public static MultiColorBitmap EditMetadata { get; private set; } = null;

        public static MultiColorBitmap Dots { get; private set; } = null;
        public static MultiColorBitmap Counter { get; private set; } = null;

        public static void LoadAll()
        {
            QuickTabsLogo = loadIcon("logo", Color.White);

            EighthNote = loadIcon("music-note-eighth", Color.White, DrawingConstants.FadedGray);
            QuarterNote = loadIcon("music-note-quarter", Color.White, DrawingConstants.FadedGray);
            HalfNote = loadIcon("music-note-half", Color.White, DrawingConstants.FadedGray);
            WholeNote = loadIcon("music-note-whole", Color.White, DrawingConstants.FadedGray);

            LeftArrow = loadIcon("arrow-left", Color.White, DrawingConstants.FadedGray);
            RightArrow = loadIcon("arrow-right", Color.White, DrawingConstants.FadedGray);

            Guitar = loadIcon("guitar-acoustic", Color.White, DrawingConstants.FadedGray);
            PlayPause = loadIcon("play-pause", Color.White, DrawingConstants.FadedGray);
            Record = loadIcon("record-circle", Color.White, DrawingConstants.FadedGray);

            OpenFile = loadIcon("folder-open-outline", Color.White, DrawingConstants.FadedGray);
            SaveFile = loadIcon("content-save-all-outline", Color.White, DrawingConstants.FadedGray);
            SaveFileAs = loadIcon("content-save-plus-outline", Color.White, DrawingConstants.FadedGray);
            NewFile = loadIcon("reload", Color.White, DrawingConstants.FadedGray);
            EditMetadata = loadIcon("pencil-outline", Color.White, DrawingConstants.FadedGray);

            Dots = loadIcon("circle-small", Color.White, DrawingConstants.FadedGray);
            Counter = loadIcon("numeric", Color.White, DrawingConstants.FadedGray);
        }
        private static MultiColorBitmap loadIcon(string iconName, params Color[] colors)
        {
            MultiColorBitmap result = new MultiColorBitmap((Bitmap)Bitmap.FromFile("icons\\" + iconName + ".png"));
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
