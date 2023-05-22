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

        public Fretboard()
        {
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;
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
        }

        private void updateSelectedBeat()
        {
            if (Song.Tab[Editor.Selection.SelectionStart].Type == Enums.StepType.Beat)
            {
                Beat beat = (Beat)Song.Tab[Editor.Selection.SelectionStart];
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
                this.Invalidate();
            }
        }

        private int getFretFromPoint(Point point, out int stringIndex)
        {
            float fretWidth = fretAreaWidth / (float)viewportLength;
            float stringHeight = this.Height / ((float)strings.Count + 2);
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
            if (e.X < fretAreaWidth)
            {
                int stringIndex;
                int fretIndex = getFretFromPoint(e.Location, out stringIndex);
                strings[stringIndex].HoveredFret = fretIndex;
            } else
            {
                // TODO: buttons hover
            }
            this.Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            foreach (String s in strings)
            {
                s.HoveredFret = -1;
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
            if (e.X < fretAreaWidth)
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
                // TODO: button click
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
            float stringHeight = this.Height / (strings.Count + 2);
            int fretAreaHeight = this.Height;

            using (SolidBrush fretAreaBrush = new SolidBrush(DrawingConstants.FretAreaColor))
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
                // draw strings
                using (Pen unselectedStringPen = new Pen(DrawingConstants.StringColor, DrawingConstants.FretLineWidth))
                using (Pen selectedStringPen = new Pen(DrawingConstants.HighlightBlue, DrawingConstants.FretLineWidth))
                using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
                {
                    for (int i = 0; i < strings.Count; i++)
                    {
                        String s = strings[i];
                        float y = (i * stringHeight) + stringHeight / 2;
                        if (s.SelectedFret > -1)
                        {
                            g.DrawLine(selectedStringPen, 0, y, fretAreaWidth, y);
                            float x = getFretX(s.SelectedFret);
                            if (x >= 0 && x < fretAreaWidth)
                            {
                                g.FillEllipse(fretAreaBrush, x - DrawingConstants.FretMarkerRadius, y - DrawingConstants.FretMarkerRadius, DrawingConstants.FretMarkerRadius * 2, DrawingConstants.FretMarkerRadius * 2);
                                g.DrawEllipse(selectedStringPen, x - DrawingConstants.FretMarkerRadius, y - DrawingConstants.FretMarkerRadius, DrawingConstants.FretMarkerRadius * 2, DrawingConstants.FretMarkerRadius * 2);
                            }
                        }
                        else
                        {
                            g.DrawLine(unselectedStringPen, 0, y, fretAreaWidth, y);
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
            public event Action OnClick;

            public void InvokeClick()
            {
                OnClick?.Invoke();
            }
        }
    }
}
