using QuickTabs.Songwriting;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls.Tools
{
    internal class Fretboard : Control
    {
        public Song Song { get; set; } = null;
        public bool ViewFretCounter { get; set; } = true;
        public bool ViewDots { get; set; } = true;
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
        private int noteLength = 1;
        private Dictionary<MultiColorBitmap, int> noteLengthButtonsPrototype = new Dictionary<MultiColorBitmap, int>();
        private Dictionary<int, Button> noteLengthButtons = new Dictionary<int, Button>();
        private List<Fret> starredFrets = new List<Fret>();

        public Fretboard()
        {
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;
            noteLengthButtonsPrototype[DrawingIcons.EighthNote] = 1;
            noteLengthButtonsPrototype[DrawingIcons.QuarterNote] = 2;
            noteLengthButtonsPrototype[DrawingIcons.DottedQuarterNote] = 3;
            noteLengthButtonsPrototype[DrawingIcons.HalfNote] = 4;
            noteLengthButtonsPrototype[DrawingIcons.DottedHalfNote] = 6;
            noteLengthButtonsPrototype[DrawingIcons.WholeNote] = 8;
            noteLengthButtonsPrototype[DrawingIcons.DottedWholeNote] = noteLengthButtonsPrototype[DrawingIcons.WholeNote] + noteLengthButtonsPrototype[DrawingIcons.WholeNote] / 2;
        }

        private void loadStrings()
        {
            if (strings.Count != Song.Tab.Tuning.Count)
            {
                starredFrets.Clear();
            }
            strings = new List<String>();
            if (Song == null)
            {
                return;
            }
            for (int i = 0; i < Song.Tab.Tuning.Count; i++)
            {
                strings.Add(new String() { Index = i });
            }
            fretAreaWidth = this.Width - DrawingConstants.ButtonAreaWidth;
            viewportLength = (int)Math.Round(fretAreaWidth / DrawingConstants.TargetFretWidth);

            buttons = new List<Button>();

            Button leftButton = new Button();
            leftButton.Icon = DrawingIcons.LeftArrow;
            leftButton.Location = new Rectangle(fretAreaWidth, 0, DrawingConstants.ButtonAreaWidth / 2, this.Height / 2);
            leftButton.Highlighted = (viewportStart != 1);
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
                } else
                {
                    leftButton.Highlighted = true;
                }
                this.Invalidate();
            };
            buttons.Add(leftButton);
            Button rightButton = new Button();
            rightButton.Icon = DrawingIcons.RightArrow;
            rightButton.Location = new Rectangle(fretAreaWidth, this.Height / 2, DrawingConstants.ButtonAreaWidth / 2, this.Height / 2);
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
                this.Invalidate();
            };
            buttons.Add(rightButton);
            float noteLengthButtonHeight = this.Height / (float)(noteLengthButtonsPrototype.Count);
            int currentNoteLengthButton = 0;
            foreach (KeyValuePair<MultiColorBitmap, int> buttonPrototype in noteLengthButtonsPrototype)
            {
                Button button = new Button();
                button.Icon = buttonPrototype.Key;
                button.Highlighted = false;
                button.Large = false;
                button.Location = new Rectangle(fretAreaWidth + DrawingConstants.ButtonAreaWidth / 2, (int)(currentNoteLengthButton * noteLengthButtonHeight), DrawingConstants.ButtonAreaWidth / 2, (int)noteLengthButtonHeight);
                button.OnClick += () =>
                {
                    noteLength = buttonPrototype.Value;
                    updateSelectedBeat();
                    updateNoteLengthButtons();
                    if (editor.Selection.SelectionLength == 1)
                    {
                        BeatPlayer beatPlayer = new BeatPlayer((Beat)(Song.Tab[editor.Selection.SelectionStart]));
                        beatPlayer.BPM = Song.Tempo;
                        beatPlayer.Tuning = Song.Tab.Tuning;
                        beatPlayer.Start();
                    }
                    this.Invalidate();
                };
                noteLengthButtons[buttonPrototype.Value] = button;
                buttons.Add(button);
                currentNoteLengthButton++;
            }
        }

        private void updateNoteLengthButtons()
        {
            foreach (KeyValuePair<int, Button> noteLengthButton in noteLengthButtons)
            {
                if (noteLengthButton.Key == noteLength)
                {
                    noteLengthButton.Value.Highlighted = true;
                } else
                {
                    noteLengthButton.Value.Highlighted = false;
                }
            }
        }

        private void updateSelectedBeat()
        {
            bool updateMade = false;
            // update first beat in selection with current fret inputs
            if (Song.Tab[Editor.Selection.SelectionStart].Type == Enums.StepType.Beat)
            {
                Beat beat = (Beat)Song.Tab[Editor.Selection.SelectionStart];
                //beat.NoteLength = noteLength;
                Fret[] selectedFrets = beat.ToArray();
                foreach (Fret fret in selectedFrets)
                {
                    beat[fret] = false;
                }
                foreach (String s in strings)
                {
                    if (s.SelectedFret > -1)
                    {
                        beat[new Fret(s.Index, s.SelectedFret)] = true;
                    }
                }
                updateMade = true;
            }
            // update entire selection with correct note length values
            for (int i = Editor.Selection.SelectionStart; i < Editor.Selection.SelectionStart + Editor.Selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Beat beat = (Beat)Song.Tab[i];
                    beat.NoteLength = noteLength;
                    updateMade = true;
                }
            }
            if (updateMade)
            {
                Editor.Invalidate();
                History.PushState(Song, editor.Selection);
            }
        }
        private void updateFromSelectedBeat()
        {
            if (Editor.Selection == null)
            {
                foreach (String s in strings)
                {
                    s.SelectedFret = -1;
                }
                this.Invalidate();
                return;
            }
            if (Song.Tab[Editor.Selection.SelectionStart].Type == Enums.StepType.Beat)
            {
                Beat beat = (Beat)Song.Tab[Editor.Selection.SelectionStart];
                foreach (String s in strings)
                {
                    s.SelectedFret = -1;
                }
                bool beatEmpty = true;
                foreach (Fret fret in beat)
                {
                    beatEmpty = false;
                    strings[fret.String].SelectedFret = fret.Space;
                }
                if (!beatEmpty)
                {
                    noteLength = beat.NoteLength;
                }
                updateNoteLengthButtons();
                this.Invalidate();
            }
        }

        private int getFretFromPoint(Point point, out int stringIndex)
        {
            int fretAreaHeight = this.Height - DrawingConstants.FretCountAreaHeight;
            if (!ViewFretCounter)
            {
                fretAreaHeight = this.Height;
            }
            float fretWidth = fretAreaWidth / (float)viewportLength;
            float stringHeight = fretAreaHeight / ((float)strings.Count);
            int viewportFret = (int)Math.Floor(point.X / fretWidth);
            stringIndex = (int)Math.Floor(point.Y / stringHeight);
            if (viewportStart == 1 && point.X < DrawingConstants.FretZeroAreaWidth)
            {
                return 0;
            } else
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
            return (onScreenFret * fretWidth) + fretWidth / 2;
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
            int fretAreaHeight = this.Height - DrawingConstants.FretCountAreaHeight;
            if (!ViewFretCounter)
            {
                fretAreaHeight = this.Height;
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
            } else
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
            this.Invalidate();
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
            this.Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.Focus();
            if (editor.Selection == null)
            {
                return;
            }
            int fretAreaHeight = this.Height - DrawingConstants.FretCountAreaHeight;
            if (!ViewFretCounter)
            {
                fretAreaHeight = this.Height;
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
                        if (AudioEngine.Enabled)
                        {
                            Action action = null;
                            action = () =>
                            {
                                //AudioEngine.PlayKick(new Note("C4"), 100, 0.25F);
                                AudioEngine.PlayNote(Note.FromSemitones(Song.Tab.Tuning.GetMusicalNote(stringIndex), fretIndex), 100, 0.25F);
                                AudioEngine.Tick -= action;
                            };
                            // why not just call playnote directly? threading errors!!! thats why.
                            AudioEngine.Tick += action;
                        }
                    }
                    updateSelectedBeat();
                } else if (e.Button == MouseButtons.Right)
                {
                    Fret fret = new Fret(stringIndex, fretIndex);
                    if (starredFrets.Contains(fret))
                    {
                        starredFrets.Remove(fret);
                    } else
                    {
                        starredFrets.Add(fret);
                    }
                }
            } else
            {
                foreach (Button button in buttons)
                {
                    if (button.Location.Contains(e.Location))
                    {
                        button.InvokeClick();
                    }
                }
            }
            this.Invalidate();
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
            int fretAreaHeight = this.Height - DrawingConstants.FretCountAreaHeight;
            if (!ViewFretCounter)
            {
                fretAreaHeight = this.Height;
            }
            float stringHeight = fretAreaHeight / (strings.Count);

            using (SolidBrush fretAreaBrush = new SolidBrush(DrawingConstants.FretAreaColor))
            using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
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
                if (ViewFretCounter)
                {
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    using (Font boldFont = new Font("Montserrat", DrawingConstants.SmallTextSizePx, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        for (int i = viewportStart; i < viewportStart + viewportLength; i++)
                        {
                            float x = getFretX(i);
                            SizeF textSize = g.MeasureString(i.ToString(), boldFont);
                            g.DrawString(i.ToString(), boldFont, textBrush, x - textSize.Width / 2, fretAreaHeight + (DrawingConstants.FretCountAreaHeight / 2) - (textSize.Height / 2));
                        }
                    }
                }
                // draw dots
                if (ViewDots)
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
                        float y = (i * stringHeight) + stringHeight / 2;
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
                            float y = (fret.String * stringHeight) + stringHeight / 2;
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
                    float y = (i * stringHeight) + stringHeight / 2;
                    if (s.HoveredFret > -1)
                    {
                        float x = getFretX(s.HoveredFret);
                        if (x >= 0 && x < fretAreaWidth)
                        {
                            g.FillRectangle(hoverBrush, x - fretWidth / 2, y - stringHeight / 2, fretWidth, stringHeight);
                        }
                    }
                }
                // draw buttons
                using (Pen buttonPen = new Pen(DrawingConstants.ButtonOutlineColor, DrawingConstants.ButtonOutlineWidth))
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
                            iconRect = new Rectangle((button.Location.X + button.Location.Width / 2) - DrawingConstants.MediumIconSize / 2, (button.Location.Y + button.Location.Height / 2) - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
                        } else
                        {
                            iconRect = new Rectangle((button.Location.X + button.Location.Width / 2) - DrawingConstants.SmallIconSize / 2, (button.Location.Y + button.Location.Height / 2) - DrawingConstants.SmallIconSize / 2, DrawingConstants.SmallIconSize, DrawingConstants.SmallIconSize);
                        }
                        if (button.Highlighted)
                        {
                            g.DrawImage(button.Icon[Color.White], iconRect);
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
