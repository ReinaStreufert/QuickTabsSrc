using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal static class DrawingConstants // yeah this name makes no sense now that theres literally only a single constant in this class lolz
    {
        public static Theme CurrentTheme { get; private set; } = Theme.DarkMode;
        public static float CurrentScale { get; private set; } = 1.0F;
        public static FontFamily Montserrat { get; private set; }

        private static PrivateFontCollection fonts = new PrivateFontCollection();

        public static int LargeMargin { get; private set; } = 40;
        public static int MediumMargin { get; private set; } = 30;
        public static int StepWidth { get; private set; } = 26;
        public static int RowHeight { get; private set; } = 30;
        public static int LeftMargin { get; private set; } = 100; // pixels before beats start
        public static int PenWidth { get; private set; } = 4;
        public static int BoldPenWidth { get; private set; } = 6;
        public static float SmallTextSizePx { get; private set; } = 24F;
        public static float TwoDigitTextSizePx { get; private set; } = 19.5F;
        public static float MediumTextSizePx { get; private set; } = 39F;
        public static int SmallTextYOffset { get; private set; } = 14;
        public static float FretTextXOffset { get; private set; } = -4;
        //public const int SmallTextXOffset = 14;
        public static int StringOffsetForLetters { get; private set; } = 30;
        public static int FretNotationXOffset { get; private set; } = 11;
        public static int ToolSelectorWidth { get; private set; } = 90;

        private static readonly Color lightModeFadedGray = Color.FromArgb(0xAA, 0xAA, 0xAA);
        private static readonly Color darkModeFadedGray = Color.FromArgb(0x3D, 0x3D, 0x3D);
        public static Color FadedGray { get; private set; } = darkModeFadedGray;

        public static readonly Color FretAreaColor = Color.FromArgb(0x1A, 0x1A, 0x1A); // same in light mode
        public static readonly Color HighlightBlue = Color.FromArgb(0x00, 0x9A, 0xE7); // same in light mode
        public static readonly Color StringColor = Color.FromArgb(0x25, 0x25, 0x25); // same in light mode
        public static readonly Color FretAreaHighlightColor = Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF); // same in light mode
        public static readonly Color StarColor = Color.FromArgb(0xe7, 0x70, 0x00); // same in light mode

        private static readonly Color lightModeHighlightColor = Color.FromArgb(0x55, 0x00, 0x00, 0x00);
        private static readonly Color darkModeHighlightColor = Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF);
        public static Color HighlightColor { get; private set; } = darkModeHighlightColor;

        private static readonly Color lightModebuttonOutlineColor = Color.FromArgb(0xBF, 0xBF, 0xBF);
        private static readonly Color darkModeButtonOutlineColor = Color.FromArgb(0x11, 0x11, 0x11);
        public static Color ButtonOutlineColor { get; private set; } = darkModeButtonOutlineColor;

        public static readonly Color LightModeLogoPatternColor = Color.FromArgb(0xE8, 0xE8, 0xE8);
        public static readonly Color DarkModeLogoPatternColor = Color.FromArgb(0x30, 0x30, 0x30);

        private static readonly Color lightModeEditModeSelectionColor = Color.FromArgb(0x77, 0x00, 0x00, 0x00);
        private static readonly Color darkModeEditModeSelectionColor = Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF);
        public static Color EditModeSelectionColor { get; private set; } = darkModeEditModeSelectionColor;

        private static readonly Color lightModePlayModeSelectionColor = Color.FromArgb(0x77, 0x9C, 0xBE, 0x00);
        private static readonly Color darkModePlayModeSelectionColor = Color.FromArgb(0x77, 0xD8, 0xFF, 0x29);
        public static Color PlayModeSelectionColor { get; private set; } = darkModePlayModeSelectionColor;

        private static readonly Color lightModeTabEditorBackColor = Color.FromArgb(0xFF, 0xFF, 0xFF);
        private static readonly Color darkModeTabEditorBackColor = Color.FromArgb(0x22, 0x22, 0x22);
        public static Color TabEditorBackColor { get; private set; } = darkModeTabEditorBackColor;

        private static readonly Color lightModeEmptySpaceBackColor = Color.FromArgb(0xEE, 0xEE, 0xEE);
        private static readonly Color darkModeEmptySpaceBackColor = Color.FromArgb(0x33, 0x33, 0x33);
        public static Color EmptySpaceBackColor { get; private set; } = darkModeEmptySpaceBackColor;

        private static readonly Color lightModeUIAreaBackColor = Color.FromArgb(0xCC, 0xCC, 0xCC);
        private static readonly Color darkModeUIAreaBackColor = Color.FromArgb(0x00, 0x00, 0x00);
        public static Color UIAreaBackColor { get; private set; } = darkModeUIAreaBackColor; // this goes to the context menu back color and the back color of the freatboard button area

        private static readonly Color lightModeUIControlBackColor = Color.White;
        private static readonly Color darkModeUIControlBackColor = SystemColors.ControlDarkDark;
        public static Color UIControlBackColor { get; private set; } = darkModeUIControlBackColor;

        private static readonly Color lightModeContrastColor = Color.Black;
        private static readonly Color darkModeContrastColor = Color.White;
        public static Color ContrastColor { get; private set; } = darkModeContrastColor;

        public static int FretboardButtonOutlineWidth { get; private set; } = 3;
        public static int FretboardButtonAreaWidth { get; private set; } = 150;
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
        public static int DropdownWidth { get; private set; } = 450;
        public static int DropdownRowHeight { get; private set; } = 60;
        public static int DropdownIconArea { get; private set; } = 70;
        public static int ScrollbarLargeChange { get; private set; } = 100;
        public static int ScrollbarSmallChange { get; private set; } = 20;
        public static int PrintPreviewOutlineWidth { get; private set; } = 5;
        public static void Scale(float scale)
        {
            CurrentScale *= scale;
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
        public static void SetTheme(Theme theme)
        {
            if (theme == CurrentTheme)
            {
                return;
            }
            if (theme == Theme.LightMode)
            {
                FadedGray = lightModeFadedGray;
                HighlightColor = lightModeHighlightColor;
                ButtonOutlineColor = lightModebuttonOutlineColor;
                EditModeSelectionColor = lightModeEditModeSelectionColor;
                PlayModeSelectionColor = lightModePlayModeSelectionColor;
                TabEditorBackColor = lightModeTabEditorBackColor;
                EmptySpaceBackColor = lightModeEmptySpaceBackColor;
                UIAreaBackColor = lightModeUIAreaBackColor;
                UIControlBackColor = lightModeUIControlBackColor;
                ContrastColor = lightModeContrastColor;
            } else if (theme == Theme.DarkMode)
            {
                FadedGray = darkModeFadedGray;
                HighlightColor = darkModeHighlightColor;
                ButtonOutlineColor = darkModeButtonOutlineColor;
                EditModeSelectionColor = darkModeEditModeSelectionColor;
                PlayModeSelectionColor = darkModePlayModeSelectionColor;
                TabEditorBackColor = darkModeTabEditorBackColor;
                EmptySpaceBackColor = darkModeEmptySpaceBackColor;
                UIAreaBackColor = darkModeUIAreaBackColor;
                UIControlBackColor = darkModeUIControlBackColor;
                ContrastColor = darkModeContrastColor;
            }
            CurrentTheme = theme;
        }
        public static void LoadFonts()
        {
            if (File.Exists("fonts\\Montserrat-VariableFont_wght.ttf"))
            {
                fonts.AddFontFile("fonts\\Montserrat-VariableFont_wght.ttf");
                Montserrat = fonts.Families[0];
            } else
            {
                Montserrat = new FontFamily("Segoe UI");
            }
        }
        public static void ApplyThemeToUIForm(Control form)
        {
            form.BackColor = UIAreaBackColor;
            foreach (Control control in form.Controls)
            {
                Type type = control.GetType();
                Font font = new Font(Montserrat, control.Font.Size, control.Font.Style, control.Font.Unit);
                //control.Font.Dispose();
                control.Font = font;
                if (type == typeof(Label) || type == typeof(CheckBox) || type == typeof(LinkLabel))
                {
                    control.BackColor = UIAreaBackColor;
                    control.ForeColor = ContrastColor;
                    if (type == typeof(LinkLabel))
                    {
                        ((LinkLabel)control).LinkColor = ContrastColor;
                    }
                }
                if (type == typeof(TextBox) || type == typeof(NumericUpDown) || type == typeof(ListBox))
                {
                    control.BackColor = UIControlBackColor;
                    control.ForeColor = ContrastColor;
                }
                if (type == typeof(Panel))
                {
                    ApplyThemeToUIForm(control);
                }
            }
        }
    }
}
