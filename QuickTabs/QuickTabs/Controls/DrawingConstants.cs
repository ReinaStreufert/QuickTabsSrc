using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal static class DrawingConstants
    {
        public static int LargeMargin { get; private set; } = 40;
        public static int MediumMargin { get; private set; } = 30;
        public static int StepWidth { get; private set; } = 26;
        public static int RowHeight { get; private set; } = 30;
        public static int LeftMargin { get; private set; } = 100; // pixels before beats start
        public static int PenWidth { get; private set; } = 4;
        public static int BoldPenWidth { get; private set; } = 6;
        public static float SmallTextSizePx { get; private set; } = RowHeight * 0.8F;
        public static float TwoDigitTextSizePx { get; private set; } = RowHeight * 0.65F;
        public static float MediumTextSizePx { get; private set; } = RowHeight * 1.3F;
        public static int SmallTextYOffset { get; private set; } = 14;
        public static float FretTextXOffset { get; private set; } = -4;
        //public const int SmallTextXOffset = 14;
        public static int StringOffsetForLetters { get; private set; } = 30;
        public static int FretNotationXOffset { get; private set; } = 11;
        public static int ToolSelectorWidth { get; private set; } = 90;
        public static readonly Color FadedGray = Color.FromArgb(0x3D, 0x3D, 0x3D);
        public static readonly Color FretAreaColor = Color.FromArgb(0x1A, 0x1A, 0x1A);
        public static readonly Color HighlightBlue = Color.FromArgb(0x00, 0x9A, 0xE7);
        public static readonly Color StringColor = Color.FromArgb(0x25, 0x25, 0x25);
        public static readonly Color HighlightColor = Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF);
        public static readonly Color ButtonOutlineColor = Color.FromArgb(0x11, 0x11, 0x11);
        public static readonly Color StarColor = Color.FromArgb(0xe7, 0x70, 0x00);
        public static readonly Color LogoPatternColor = Color.FromArgb(0x30, 0x30, 0x30);
        public static readonly Color EditModeSelectionColor = Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF);
        public static readonly Color PlayModeSelectionColor = Color.FromArgb(0x77, 0xD8, 0xFF, 0x29);
        public static int ButtonOutlineWidth { get; private set; } = 3;
        public static int ButtonAreaWidth { get; private set; } = 150;
        public static float TargetFretWidth { get; private set; } = 125F;
        public static int FretCountAreaHeight { get; private set; } = 50;
        public static int FretZeroAreaWidth { get; private set; } = 25;
        public static int FretLineWidth { get; private set; } = 5;
        public static int FretMarkerRadius { get; private set; } = 10;
        public static int LargeIconSize { get; private set; } = 40;
        public static int TwoDotSpacing { get; private set; } = 60;
        public static int DotRadius { get; private set; } = 20;
        public static readonly int[] DotPattern = new[] { 3, 0, 2, 0, 2, 0, 2, 0, 3, 1 }; // offs, [0 = 1 dot, 1 = 2 dots], ...
        public static int SectionSpacing { get; private set; } = 50;
        public static int SeperatorLineWidth { get; private set; } = 5;
        public static int MediumIconSize { get; private set; } = 35;
        public static int SmallIconSize { get; private set; } = 30;
        public static int LogoPatternSpacingSmall { get; private set; } = 200;
        public static int LogoPatternSpacingLarge { get; private set; } = 500;
        public static Size LogoPatternSize { get; private set; } = new Size(500, 110);
        public const float LogoPatternRotation = 20;
        public static int DropdownWidth { get; private set; } = 400;
        public static int DropdownRowHeight { get; private set; } = 60;
        public static int DropdownIconArea { get; private set; } = 70;
        public static void Scale(float scale)
        {
            FieldInfo[] allFields = typeof(DrawingConstants).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            foreach (FieldInfo field in allFields)
            {
                if (field.IsLiteral) continue;
                if (field.FieldType == typeof(int))
                {
                    field.SetValue(null, (int)Math.Ceiling(((int)field.GetValue(null)) * scale)); // using ceiling because tiny sizes should rounded UP a pixel. for bigger sizes and positions, it doesnt matter.
                } else if (field.FieldType == typeof(float))
                {
                    field.SetValue(null, ((float)field.GetValue(null)) * scale);
                } else if (field.FieldType == typeof(Size))
                {
                    Size size = (Size)field.GetValue(null);
                    field.SetValue(null, new Size((int)(size.Width * scale), (int)(size.Height * scale)));
                }
            }
        }
    }
}
