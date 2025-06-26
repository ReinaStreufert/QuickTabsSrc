using QuickTabs.Configuration;
using QuickTabs.Songwriting;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public class Fretboard : Control
    {
        public override Color BackColor { get => DrawingConstants.UIAreaBackColor; set => base.BackColor = value; }

        public Song Song { get; set; } = null;
        private TabEditor editor;
        public TabEditor Editor
        {
            get
            {
                return editor;
            }
            set
            {
                editor = value;
                editor.SelectionChanged += updateFromSelectedBeat;
            }
        }

        private List<String> strings = new List<String>();
        private List<Button> buttons;
        private int viewportStart = 1;
        private int viewportLength = 10;
        private int fretAreaWidth = 0;
        private List<Fret> starredFrets = new List<Fret>();

        public Fretboard()
        {
            DoubleBuffered = true;
        }

        private void loadStrings()
        {
            if (Song == null) { return; }
            if (strings.Count != Song.FocusedTab.Tuning.Count)
            {
                starredFrets.Clear(); // TODO: figure out something to make this better in parallel tracks
            }
            strings = new List<String>();
            if (Song == null)
            {
                return;
            }
            for (int i = 0; i < Song.FocusedTab.Tuning.Count; i++)
            {
                strings.Add(new String() { Index = i });
            }
            fretAreaWidth = Width - DrawingConstants.FretboardButtonAreaWidth;
            float targetFretWidth = DrawingConstants.TargetFretWidth;
            if (QTPersistence.Current.ViewLargeFretboard)
            {
                targetFretWidth = targetFretWidth * 1.5F;
            }
            viewportLength = (int)Math.Round(fretAreaWidth / targetFretWidth);

            buttons = new List<Button>();

            Button leftButton = new Button();
            leftButton.Icon = DrawingIcons.LeftArrow;
            leftButton.Location = new Rectangle(fretAreaWidth, 0, DrawingConstants.FretboardButtonAreaWidth, Height / 2);
            leftButton.Highlighted = viewportStart != 1;
            leftButton.Large = true;
            leftButton.OnClick += () =>
            {
                if (viewportStart > 1)
                {
                    viewportStart--;
                }
                if (viewportStart == 1)
                {
                    leftButton.Highlighted = false;
                }
                else
                {
                    leftButton.Highlighted = true;
                }
                Invalidate();
            };
            buttons.Add(leftButton);
            Button rightButton = new Button();
            rightButton.Icon = DrawingIcons.RightArrow;
            rightButton.Location = new Rectangle(fretAreaWidth, Height / 2, DrawingConstants.FretboardButtonAreaWidth, Height / 2);
            rightButton.Highlighted = true;
            rightButton.Large = true;
            rightButton.OnClick += () =>
            {
                viewportStart++;
                if (viewportStart == 1)
                {
                    leftButton.Highlighted = false;
                }
                else
                {
                    leftButton.Highlighted = true;
                }
                Invalidate();
            };
            buttons.Add(rightButton);
        }

        private void updateSelectedBeat()
        {
            bool updateMade = false;
            // update first beat in selection with current fret inputs
            if (Song.FocusedTab[Editor.Selection.SelectionStart].Type == Enums.StepType.Beat)
            {
                Beat beat = (Beat)Song.FocusedTab[Editor.Selection.SelectionStart];
                //beat.NoteLength = noteLength;
                KeyValuePair<Fret, MusicalTimespan>[] selectedFrets = beat.ToArray();
                foreach (KeyValuePair<Fret, MusicalTimespan> fret in selectedFrets)
                {
                    beat[fret.Key] = MusicalTimespan.Zero;
                }
                foreach (String s in strings)
                {
                    if (s.SelectedFret > -1)
                    {
                        beat[new Fret(s.Index, s.SelectedFret)] = s.SustainTime;
                    }
                }
                updateMade = true;
            }
            if (updateMade)
            {
                Editor.Invalidate();
                History.PushState(Song, editor.Selection);
            }
        }
        private void updateFromSelectedBeat()
        {
            if (Editor == null) { return; }
            if (Editor.Selection == null)
            {
                foreach (String s in strings)
                {
                    s.SelectedFret = -1;
                }
                Invalidate();
                return;
            }
            if (Song.FocusedTab[Editor.Selection.SelectionStart].Type == Enums.StepType.Beat)
            {
                if (Song.FocusedTab.Tuning.Count != strings.Count)
                {
                    loadStrings();
                }
                Beat beat = (Beat)Song.FocusedTab[Editor.Selection.SelectionStart];
                foreach (String s in strings)
                {
                    s.SelectedFret = -1;
                    s.SustainTime = beat.BeatDivision;
                }
                foreach (KeyValuePair<Fret, MusicalTimespan> fret in beat)
                {
                    String s = strings[fret.Key.String];
                    s.SelectedFret = fret.Key.Space;
                    s.SustainTime = fret.Value;
                }
                Invalidate();
            }
        }

        private int getFretFromPoint(Point point, out int stringIndex)
        {
            int fretAreaHeight = Height - DrawingConstants.FretCountAreaHeight;
            if (!QTPersistence.Current.ViewFretCounter)
            {
                fretAreaHeight = Height;
            }
            float fretWidth = fretAreaWidth / (float)viewportLength;
            float stringHeight = fretAreaHeight / (float)strings.Count;
            int viewportFret = (int)Math.Floor(point.X / fretWidth);
            stringIndex = (int)Math.Floor(point.Y / stringHeight);
            if (viewportStart == 1 && point.X < DrawingConstants.FretZeroAreaWidth)
            {
                return 0;
            }
            else
            {
                return viewportFret + viewportStart;
            }
        }

        private float getFretX(int fret)
        {
            if (fret == 0 && viewportStart == 1)
            {
                return 0;
            }
            float fretWidth = fretAreaWidth / (float)viewportLength;
            int onScreenFret = fret - viewportStart;
            return onScreenFret * fretWidth + fretWidth / 2;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (editor.Selection == null)
            {
                return;
            }
            foreach (String s in strings)
            {
                s.HoveredFret = -1;
            }
            int fretAreaHeight = Height - DrawingConstants.FretCountAreaHeight;
            if (!QTPersistence.Current.ViewFretCounter)
            {
                fretAreaHeight = Height;
            }
            if (e.X < fretAreaWidth && e.Y < fretAreaHeight && e.X > 0 && e.Y > 0)
            {
                int stringIndex;
                int fretIndex = getFretFromPoint(e.Location, out stringIndex);
                strings[stringIndex].HoveredFret = fretIndex;
                foreach (Button button in buttons)
                {
                    button.Hovered = false;
                }
            }
            else
            {
                foreach (Button button in buttons)
                {
                    if (button.Location.Contains(e.Location))
                    {
                        button.Hovered = true;
                    }
                    else
                    {
                        button.Hovered = false;
                    }
                }
            }
            Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            foreach (String s in strings)
            {
                s.HoveredFret = -1;
            }
            foreach (Button button in buttons)
            {
                button.Hovered = false;
            }
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (editor.Selection == null)
            {
                return;
            }
            int fretAreaHeight = Height - DrawingConstants.FretCountAreaHeight;
            if (!QTPersistence.Current.ViewFretCounter)
            {
                fretAreaHeight = Height;
            }
            if (e.X < fretAreaWidth && e.Y < fretAreaHeight)
            {
                int stringIndex;
                int fretIndex = getFretFromPoint(e.Location, out stringIndex);
                if (e.Button == MouseButtons.Left)
                {
                    if (strings[stringIndex].SelectedFret == fretIndex)
                    {
                        strings[stringIndex].SelectedFret = -1;
                    }
                    else
                    {
                        strings[stringIndex].SelectedFret = fretIndex;
                        if (AudioEngine.Enabled && QTPersistence.Current.EnablePreviewPlay)
                        {
                            AudioEngine.AudioEngineTick action = null;
                            action = (timestamp, bufferDurationMS) =>
                            {
                                //AudioEngine.PlayKick(new Note("C4"), 100, 0.25F);
                                AudioEngine.PlayNote(Note.FromSemitones(Song.FocusedTab.Tuning.GetMusicalNote(stringIndex), fretIndex), 100, new ConstantVolume(1.0F));
                                AudioEngine.Tick -= action;
                            };
                            // why not just call playnote directly? threading errors!!! thats why.
                            AudioEngine.Tick += action;
                        }
                    }
                    updateSelectedBeat();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    Fret fret = new Fret(stringIndex, fretIndex);
                    if (starredFrets.Contains(fret))
                    {
                        starredFrets.Remove(fret);
                    }
                    else
                    {
                        starredFrets.Add(fret);
                    }
                }
            }
            else
            {
                foreach (Button button in buttons)
                {
                    if (button.Location.Contains(e.Location))
                    {
                        button.InvokeClick();
                    }
                }
            }
            Invalidate();
        }
        public override void Refresh()
        {
            base.Refresh();
            loadStrings();
            updateFromSelectedBeat();
            Invalidate();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Song == null)
            {
                return;
            }
            loadStrings();
            updateFromSelectedBeat();
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            float fretWidth = fretAreaWidth / (float)viewportLength;
            int fretAreaHeight = Height - DrawingConstants.FretCountAreaHeight;
            if (!QTPersistence.Current.ViewFretCounter)
            {
                fretAreaHeight = Height;
            }
            float stringHeight = fretAreaHeight / strings.Count;

            using (SolidBrush fretAreaBrush = new SolidBrush(DrawingConstants.FretAreaColor))
            using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
            using (SolidBrush fretAreaHoverBrush = new SolidBrush(DrawingConstants.FretAreaHighlightColor))
            {
                // draw fret area
                g.FillRectangle(fretAreaBrush, 0, 0, fretAreaWidth, fretAreaHeight);

                // draw fret lines
                using (Pen fretLinePen = new Pen(Color.White, DrawingConstants.FretLineWidth))
                {
                    for (int i = 1; i < viewportLength; i++)
                    {
                        float x = i * fretWidth;
                        g.DrawLine(fretLinePen, x, 0, x, fretAreaHeight);
                        //Console.WriteLine(x);
                    }
                }
                // draw fret numbers
                if (QTPersistence.Current.ViewFretCounter)
                {
                    using (SolidBrush textBrush = new SolidBrush(DrawingConstants.ContrastColor))
                    using (Font boldFont = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        for (int i = viewportStart; i < viewportStart + viewportLength; i++)
                        {
                            float x = getFretX(i);
                            SizeF textSize = g.MeasureString(i.ToString(), boldFont);
                            g.DrawString(i.ToString(), boldFont, textBrush, x - textSize.Width / 2, fretAreaHeight + DrawingConstants.FretCountAreaHeight / 2 - textSize.Height / 2);
                        }
                    }
                }
                // draw dots
                if (QTPersistence.Current.ViewNavDots)
                {
                    using (SolidBrush dotBrush = new SolidBrush(Color.White))
                    {
                        int currentFret = 0;
                        for (; ; )
                        {
                            bool fullBreak = false;
                            for (int i = 0; i < DrawingConstants.DotPattern.Length; i += 2)
                            {
                                currentFret += DrawingConstants.DotPattern[i];
                                int dotType = DrawingConstants.DotPattern[i + 1];
                                if (currentFret >= viewportStart + viewportLength)
                                {
                                    fullBreak = true;
                                    break;
                                }
                                if (currentFret >= viewportStart)
                                {
                                    float x = getFretX(currentFret);
                                    float y = fretAreaHeight / 2F;
                                    if (dotType == 0)
                                    {
                                        g.FillEllipse(dotBrush, x - DrawingConstants.DotRadius / 2, y - DrawingConstants.DotRadius / 2, DrawingConstants.DotRadius, DrawingConstants.DotRadius);
                                    }
                                    else
                                    {
                                        g.FillEllipse(dotBrush, x - DrawingConstants.DotRadius / 2, y - DrawingConstants.DotRadius / 2 - DrawingConstants.TwoDotSpacing, DrawingConstants.DotRadius, DrawingConstants.DotRadius);
                                        g.FillEllipse(dotBrush, x - DrawingConstants.DotRadius / 2, y - DrawingConstants.DotRadius / 2 + DrawingConstants.TwoDotSpacing, DrawingConstants.DotRadius, DrawingConstants.DotRadius);
                                    }
                                }
                            }
                            if (fullBreak)
                                break;
                        }
                    }
                }
                // draw strings
                using (Pen unselectedStringPen = new Pen(DrawingConstants.StringColor, DrawingConstants.FretLineWidth))
                using (Pen selectedStringPen = new Pen(DrawingConstants.HighlightBlue, DrawingConstants.FretLineWidth))
                {
                    for (int i = 0; i < strings.Count; i++)
                    {
                        String s = strings[i];
                        float y = i * stringHeight + stringHeight / 2;
                        g.DrawLine(unselectedStringPen, 0, y, fretAreaWidth, y);
                        if (s.SelectedFret > -1)
                        {
                            float x = getFretX(s.SelectedFret);
                            if (x < fretAreaWidth)
                            {
                                g.DrawLine(selectedStringPen, x, y, fretAreaWidth, y);
                                if (x >= 0)
                                {
                                    g.FillEllipse(fretAreaBrush, x - DrawingConstants.FretMarkerRadius, y - DrawingConstants.FretMarkerRadius, DrawingConstants.FretMarkerRadius * 2, DrawingConstants.FretMarkerRadius * 2);
                                    g.DrawEllipse(selectedStringPen, x - DrawingConstants.FretMarkerRadius, y - DrawingConstants.FretMarkerRadius, DrawingConstants.FretMarkerRadius * 2, DrawingConstants.FretMarkerRadius * 2);
                                }
                            }
                        }
                    }
                }
                // draw stars
                using (Pen starPen = new Pen(DrawingConstants.StarColor, DrawingConstants.FretLineWidth))
                {
                    foreach (Fret fret in starredFrets)
                    {
                        if (fret.Space >= viewportStart && fret.Space < viewportStart + viewportLength)
                        {
                            float y = fret.String * stringHeight + stringHeight / 2;
                            float x = getFretX(fret.Space);
                            g.FillEllipse(fretAreaBrush, x - DrawingConstants.FretMarkerRadius, y - DrawingConstants.FretMarkerRadius, DrawingConstants.FretMarkerRadius * 2, DrawingConstants.FretMarkerRadius * 2);
                            g.DrawEllipse(starPen, x - DrawingConstants.FretMarkerRadius, y - DrawingConstants.FretMarkerRadius, DrawingConstants.FretMarkerRadius * 2, DrawingConstants.FretMarkerRadius * 2);
                        }
                    }
                }
                // draw hovered frets
                for (int i = 0; i < strings.Count; i++)
                {
                    String s = strings[i];
                    float y = i * stringHeight + stringHeight / 2;
                    if (s.HoveredFret > -1)
                    {
                        float x = getFretX(s.HoveredFret);
                        if (x >= 0 && x < fretAreaWidth)
                        {
                            g.FillRectangle(fretAreaHoverBrush, x - fretWidth / 2, y - stringHeight / 2, fretWidth, stringHeight);
                        }
                    }
                }
                // draw buttons
                using (Pen buttonPen = new Pen(DrawingConstants.ButtonOutlineColor, DrawingConstants.FretboardButtonOutlineWidth))
                {
                    foreach (Button button in buttons)
                    {
                        if (button.Hovered)
                        {
                            g.FillRectangle(hoverBrush, button.Location);
                        }
                        g.DrawRectangle(buttonPen, button.Location);
                        Rectangle iconRect;
                        if (button.Large)
                        {
                            iconRect = new Rectangle(button.Location.X + button.Location.Width / 2 - DrawingConstants.MediumIconSize / 2, button.Location.Y + button.Location.Height / 2 - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        }
                        else
                        {
                            iconRect = new Rectangle(button.Location.X + button.Location.Width / 2 - DrawingConstants.SmallIconSize / 2, button.Location.Y + button.Location.Height / 2 - DrawingConstants.SmallIconSize / 2, DrawingConstants.SmallIconSize, DrawingConstants.SmallIconSize);
                        }
                        if (button.Highlighted)
                        {
                            g.DrawImage(button.Icon[DrawingConstants.ContrastColor], iconRect);
                        }
                        else
                        {
                            g.DrawImage(button.Icon[DrawingConstants.FadedGray], iconRect);
                        }
                    }
                }
            }

        }

        private class String
        {
            public int SelectedFret = -1;
            public int HoveredFret = -1;
            public MusicalTimespan SustainTime = MusicalTimespan.Zero;
            public int Index = 0;
        }

        private class Button
        {
            public Rectangle Location { get; set; }
            public MultiColorBitmap Icon { get; set; }
            public bool Hovered { get; set; } = false;
            public bool Highlighted { get; set; } = false;
            public bool Large { get; set; } = false;
            public event Action OnClick;

            public void InvokeClick()
            {
                OnClick?.Invoke();
            }
        }
    }
}
