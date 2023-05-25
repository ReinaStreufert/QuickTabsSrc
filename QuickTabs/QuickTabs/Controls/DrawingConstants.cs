using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal static class DrawingConstants
    {
        public const int LargeMargin = 40;
        public const int MediumMargin = 30;
        public const int StepWidth = 26;
        public const int RowHeight = 30;
        public const int LeftMargin = 100; // pixels before beats start
        public const int PenWidth = 4;
        public const int BoldPenWidth = 6;
        public const float SmallTextSizePx = RowHeight * 0.8F;
        public const float TwoDigitTextSizePx = RowHeight * 0.5F;
        public const int SmallTextYOffset = 14;
        public const int TwoDigitTextYOffset = 10;
        //public const int SmallTextXOffset = 14;
        public const int StringOffsetForLetters = 30;
        public const int FretNotationXOffset = 11;
        public const int ToolSelectorWidth = 90;
        public static readonly Color FadedGray = Color.FromArgb(0x3D, 0x3D, 0x3D);
        public static readonly Color FretAreaColor = Color.FromArgb(0x1A, 0x1A, 0x1A);
        public static readonly Color HighlightBlue = Color.FromArgb(0x00, 0x9A, 0xE7);
        public static readonly Color StringColor = Color.FromArgb(0x25, 0x25, 0x25);
        public static readonly Color HighlightColor = Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF);
        public static readonly Color ButtonOutlineColor = Color.FromArgb(0x11, 0x11, 0x11);
        public static readonly Color StarColor = Color.FromArgb(0xe7, 0x70, 0x00);
        public static readonly Color LogoPatternColor = Color.FromArgb(0x30, 0x30, 0x30);
        public const int ButtonOutlineWidth = 3;
        public const int ButtonAreaWidth = 150;
        public const int FretCountAreaHeight = 50;
        public const int FretZeroAreaWidth = 25;
        public const int FretLineWidth = 5;
        public const int FretMarkerRadius = 10;
        public const int MediumIconSize = 40;
        public const int TwoDotSpacing = 60;
        public const int DotRadius = 20;
        public static readonly int[] DotPattern = new[] { 3, 0, 2, 0, 2, 0, 2, 0, 3, 1 }; // offs, [0 = 1 dot, 1 = 2 dots], ...
        public const int SectionSpacing = 50;
        public const int SeperatorLineWidth = 5;
        public const int SmallIconSize = 35;
        public static int LogoPatternSpacingSmall = 200;
        public static int LogoPatternSpacingLarge = 500;
        public static readonly Size LogoPatternSize = new Size(500, 110);
        public const float LogoPatternRotation = 20;
    }
}
