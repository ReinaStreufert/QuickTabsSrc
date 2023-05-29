using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs.Controls
{
    internal class TuningPicker : Control
    {
        public Tuning Tuning { get; set; }
        public int StringShift { get; set; } = 0;

        private List<Button> buttons = new List<Button>();
        private List<Button> tuningButtons = new List<Button>();
        private Button currentlyHovered = null;
        private Button currentlyEdited = null;
        private Timer cursorFlashTimer = new Timer();
        private bool cursorShowing = false;

        public TuningPicker()
        {
            this.DoubleBuffered = true;
            cursorFlashTimer.Interval = 500;
            cursorFlashTimer.Tick += CursorFlashTimer_Tick;
        }

        private void CursorFlashTimer_Tick(object? sender, EventArgs e)
        {
            cursorShowing = !cursorShowing;
            this.Invalidate();
        }

        private void updateUI()
        {
            buttons = new List<Button>();
            tuningButtons = new List<Button>();
            int columnCount = Tuning.Count + 2; // + 2 is for plus minus buttons on each side
            float buttonWidth = this.Width / (float)columnCount;
            float buttonHeight = this.Height / 2F;
            int plusY = 0;
            int minusY = (int)Math.Round(buttonHeight);
            int letterY = (int)Math.Round((this.Height / 2F) - (buttonHeight / 2F));
            for (int col = 0; col < columnCount; col++)
            {
                int x = (int)Math.Round(col * buttonWidth);
                if (col == 0 || col == columnCount - 1)
                {
                    bool beginOrEnd = false;
                    if (col > 0)
                    {
                        beginOrEnd = true;
                    }
                    Button plusButton = new Button();
                    plusButton.Location = new Rectangle(x, plusY, (int)buttonWidth, (int)buttonHeight);
                    plusButton.Icon = DrawingIcons.Plus;
                    plusButton.Click += () => { addLetter(beginOrEnd); };
                    Button minusButton = new Button();
                    minusButton.Location = new Rectangle(x, minusY, (int)buttonWidth, (int)buttonHeight);
                    minusButton.Icon = DrawingIcons.Minus;
                    minusButton.Click += () => { removeLetter(beginOrEnd); };
                    buttons.Add(plusButton);
                    buttons.Add(minusButton);
                } else
                {
                    Note note = Tuning.GetMusicalNote((Tuning.Count - (col - 1)) - 1);
                    Button letterButton = new Button();
                    letterButton.Location = new Rectangle(x, letterY, (int)buttonWidth, (int)buttonHeight);
                    letterButton.Note = note;
                    letterButton.Click += () => { editButton(letterButton); };
                    buttons.Add(letterButton);
                    tuningButtons.Add(letterButton);
                }
            }
        }
        private void addLetter(bool beginOrEnd) // false for begin, true for end
        {
            if (tuningButtons.Count >= 12)
            {
                return;
            }
            Button b = new Button();
            b.Note = new Note("C2");
            if (!beginOrEnd)
            {
                tuningButtons.Insert(0, b);
            } else
            {
                tuningButtons.Add(b);
                StringShift++;
            }
            updateTuningFromInputs();
            updateUI();
            Invalidate();
        }
        private void removeLetter(bool beginOrEnd) // false for begin, true for end
        {
            if (tuningButtons.Count <= 1)
            {
                return;
            }
            if (!beginOrEnd)
            {
                tuningButtons.RemoveAt(0);
            }
            else
            {
                tuningButtons.RemoveAt(tuningButtons.Count - 1);
                StringShift--;
            }
            updateTuningFromInputs();
            updateUI();
            Invalidate();
        }
        private void editButton(Button letterButton)
        {
            this.Focus();
            if (currentlyEdited != null)
            {
                stopEditing();
            }
            currentlyEdited = letterButton;
            currentlyEdited.Editing = true;
            currentlyEdited.Cleared = true;
            cursorShowing = true;
            this.Invalidate();
            if (!cursorFlashTimer.Enabled)
            {
                cursorFlashTimer.Start();
            }
        }
        private void stopEditing()
        {
            currentlyEdited.Editing = false;
            currentlyEdited.Cleared = false;
            currentlyEdited = null;
            cursorFlashTimer.Stop();
            updateTuningFromInputs();
            this.Invalidate();
        }
        private void updateTuningFromInputs()
        {
            Note[] notes = new Note[tuningButtons.Count];
            for (int i = 0; i < tuningButtons.Count; i++)
            {
                notes[i] = tuningButtons[i].Note;
            }
            Tuning = new Tuning(notes.Reverse().ToArray());
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            updateUI();
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (currentlyEdited != null)
            {
                Note note = currentlyEdited.Note;
                bool changeMade = true;
                if (e.KeyCode == Keys.A)
                {
                    note.NoteValue = NoteValue.A;
                } else if (e.KeyCode == Keys.B)
                {
                    note.NoteValue = NoteValue.B;
                } else if (e.KeyCode == Keys.C)
                {
                    note.NoteValue = NoteValue.C;
                } else if (e.KeyCode == Keys.D)
                {
                    note.NoteValue = NoteValue.D;
                } else if (e.KeyCode == Keys.E)
                {
                    note.NoteValue = NoteValue.E;
                } else if (e.KeyCode == Keys.F)
                {
                    note.NoteValue = NoteValue.F;
                } else if (e.KeyCode == Keys.G)
                {
                    note.NoteValue = NoteValue.G;
                } else
                {
                    changeMade = false;
                }
                if (currentlyEdited.Cleared && changeMade)
                {
                    currentlyEdited.Cleared = false;
                    note.NoteType = NoteType.Natural;
                    note.Octave = 0;
                } else if (!changeMade)
                {
                    changeMade = true;
                    if (e.KeyCode == Keys.D0)
                    {
                        note.Octave = 0;
                    } else if (e.KeyCode == Keys.D1)
                    {
                        note.Octave = 1;
                    } else if (e.KeyCode == Keys.D2)
                    {
                        note.Octave = 2;
                    } else if (e.KeyCode == Keys.D3)
                    {
                        if (e.Modifiers == Keys.Shift)
                        {
                            note.NoteType = NoteType.Sharp;
                        } else
                        {
                            note.Octave = 3;
                        }
                    } else if (e.KeyCode == Keys.D4)
                    {
                        note.Octave = 4;
                    } else if (e.KeyCode == Keys.D5)
                    {
                        note.Octave = 5;
                    } else if (e.KeyCode == Keys.D6)
                    {
                        note.Octave = 6;
                    } else if (e.KeyCode == Keys.D7)
                    {
                        note.Octave = 7;
                    } else if (e.KeyCode == Keys.D8)
                    {
                        note.Octave = 8;
                    } else if (e.KeyCode == Keys.D9)
                    {
                        note.Octave = 9;
                    } else if (e.KeyCode == Keys.Back)
                    {
                        note.NoteType = NoteType.Natural;
                    } else
                    {
                        changeMade = false;
                    }
                }
                if (changeMade)
                {
                    currentlyEdited.Note = note;
                    updateTuningFromInputs();
                    Invalidate();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (currentlyHovered != null)
            {
                currentlyHovered.Hovered = false;
                currentlyHovered = null;
            }
            foreach (Button button in buttons)
            {
                if (button.Location.Contains(e.Location))
                {
                    button.Hovered = true;
                    currentlyHovered = button;
                }
            }
            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            foreach (Button button in buttons)
            {
                if (button.Location.Contains(e.Location))
                {
                    button.InvokeClick();
                    return;
                }
            }
            // will only reach this code if click hit nothing
            if (currentlyEdited != null)
            {
                stopEditing();
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (currentlyEdited != null)
            {
                stopEditing();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (currentlyHovered != null)
            {
                currentlyHovered.Hovered = false;
                currentlyHovered = null;
            }
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            using (SolidBrush highlightBrush = new SolidBrush(DrawingConstants.HighlightColor))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (Font font = new Font("Montserrat", DrawingConstants.MediumTextSizePx, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                foreach (Button button in buttons)
                {
                    if (button.Hovered)
                    {
                        g.FillRectangle(highlightBrush, button.Location);
                    }
                    int centerX = button.Location.X + button.Location.Width / 2;
                    int centerY = button.Location.Y + button.Location.Height / 2;
                    if (button.Icon == null)
                    {
                        string text = button.Note.ToString();
                        SizeF textSize = g.MeasureString(text, font);
                        if (button.Cleared)
                        {
                            textSize = new SizeF(0, textSize.Height);
                            text = "";
                        }
                        float textX = centerX - textSize.Width / 2;
                        float textY = centerY - textSize.Height / 2;
                        g.DrawString(text, font, textBrush, textX, textY);
                        if (button.Editing && cursorShowing)
                        {
                            using (Pen pen = new Pen(Color.White, 1.0F))
                            {
                                g.DrawLine(pen, textX + textSize.Width, textY, textX + textSize.Width, textY + textSize.Height);
                            }
                        }
                    } else
                    {
                        g.DrawImage(button.Icon[Color.White], centerX - DrawingConstants.LargeIconSize / 2, centerY - DrawingConstants.LargeIconSize / 2, DrawingConstants.LargeIconSize, DrawingConstants.LargeIconSize);
                    }
                }
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            updateUI();
            Invalidate();
        }

        private class Button
        {
            public Rectangle Location { get; set; }
            public Note Note { get; set; }
            public MultiColorBitmap Icon { get; set; } = null;
            public event Action Click;
            public bool Hovered = false;
            public bool Editing = false;
            public bool Cleared = false;

            public void InvokeClick()
            {
                Click?.Invoke();
            }
        }
    }
}
