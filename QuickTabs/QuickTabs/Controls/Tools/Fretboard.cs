using QuickTabs.Songwriting;
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

        private List<String> strings;
        private List<Button> buttons;
        private int viewportStart = 1;
        private int viewportLength = 10;
        private int fretAreaWidth = 0;
        private int noteLength = 1;
        private Dictionary<MultiColorBitmap, int> noteLengthButtonsPrototype = new Dictionary<MultiColorBitmap, int>();
        private Dictionary<int, Button> noteLengthButtons = new Dictionary<int, Button>();

        public Fretboard()
        {
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;
            noteLengthButtonsPrototype[DrawingIcons.EighthNote] = 1;
            noteLengthButtonsPrototype[DrawingIcons.QuarterNote] = 2;
            noteLengthButtonsPrototype[DrawingIcons.HalfNote] = 4;
            noteLengthButtonsPrototype[DrawingIcons.WholeNote] = 8;
        }

        private void loadStrings()
        {
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
            viewportLength = (int)Math.Round(fretAreaWidth / 125F);

            buttons = new List<Button>();

            Button leftButton = new Button();
            leftButton.Icon = DrawingIcons.LeftArrow;
            leftButton.Location = new Rectangle(fretAreaWidth, 0, DrawingConstants.ButtonAreaWidth / 2, this.Height / 2);
            leftButton.Highlighted = (viewportStart != 1);
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
                button.Location = new Rectangle(fretAreaWidth + DrawingConstants.ButtonAreaWidth / 2, (int)(currentNoteLengthButton * noteLengthButtonHeight), DrawingConstants.ButtonAreaWidth / 2, (int)noteLengthButtonHeight);
                button.OnClick += () =>
                {
                    noteLength = buttonPrototype.Value;
                    updateSelectedBeat();
                    updateNoteLengthButtons();
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
            if (Song.Tab[Editor.Selection.SelectionStart].Type == Enums.StepType.Beat)
            {
                Beat beat = (Beat)Song.Tab[Editor.Selection.SelectionStart];
                beat.NoteLength = noteLength;
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
                Editor.Invalidate();
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
                foreach (Fret fret in beat)
                {
                    strings[fret.String].SelectedFret = fret.Space;
                }
                noteLength = beat.NoteLength;
                updateNoteLengthButtons();
                this.Invalidate();
            }
        }

        private int getFretFromPoint(Point point, out int stringIndex)
        {
            int fretAreaHeight = this.Height - DrawingConstants.FretCountAreaHeight;
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
            if (e.X < fretAreaWidth && e.Y < fretAreaHeight)
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
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (editor.Selection == null)
            {
                return;
            }
            int fretAreaHeight = this.Height - DrawingConstants.FretCountAreaHeight;
            if (e.X < fretAreaWidth && e.Y < fretAreaHeight)
            {
                int stringIndex;
                int fretIndex = getFretFromPoint(e.Location, out stringIndex);
                if (strings[stringIndex].SelectedFret == fretIndex)
                {
                    strings[stringIndex].SelectedFret = -1;
                } else
                {
                    strings[stringIndex].SelectedFret = fretIndex;
                }
                updateSelectedBeat();
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
            float fretWidth = fretAreaWidth / (float)viewportLength;
            int fretAreaHeight = this.Height - DrawingConstants.FretCountAreaHeight;
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
                // draw dots
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
                        if (s.HoveredFret > -1)
                        {
                            float x = getFretX(s.HoveredFret);
                            if (x >= 0 && x < fretAreaWidth)
                            {
                                g.FillRectangle(hoverBrush, x - fretWidth / 2, y - stringHeight / 2, fretWidth, stringHeight);
                            }
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
                        Rectangle iconRect = new Rectangle((button.Location.X + button.Location.Width / 2) - DrawingConstants.MediumIconSize / 2, (button.Location.Y + button.Location.Height / 2) - DrawingConstants.MediumIconSize / 2, DrawingConstants.MediumIconSize, DrawingConstants.MediumIconSize);
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
            public event Action OnClick;

            public void InvokeClick()
            {
                OnClick?.Invoke();
            }
        }
    }
}
