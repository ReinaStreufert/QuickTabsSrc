using QuickTabs.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace QuickTabs.Controls
{
    public class TrackView : Control
    {
        public override Color BackColor { get => DrawingConstants.TrackViewBackColor; set => base.BackColor = value; }

        public Song Song { get; set; }
        public TabEditor ParentEditor { get; set; }
        public int[] TrackHeights { get; set; } = null;
        public int ParentScroll { get; set; } = 0;
        private List<UIElement> uiElements = new List<UIElement>();
        private ButtonElement hoveredButton = null;
        private SliderElement heldSlider = null;
        private float heldSliderStartValue;
        private Track draggedTrack = null;
        private int draggedTrackIndex;
        private bool trackOrderChanged = false;

        public TrackView()
        {
            this.DoubleBuffered = true;
        }

        public void UpdateUI()
        {
            if (TrackHeights.Length != Song.Tracks.Count)
            {
                ParentEditor.Refresh(); // get tab editor to update track heights
            }
            List<Track> tracks = Song.Tracks;
            uiElements.Clear();
            int y = 0;
            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];
                int height = TrackHeights[i];
                int textCenterX = this.Width / 2;
                int textCenterY = y + ((height - DrawingConstants.SafeButtonHeight * 2) / 2);
                int buttonY = y + height - DrawingConstants.SafeButtonHeight;
                int sliderY = buttonY - DrawingConstants.SafeButtonHeight;
                TrackAreaElement trackArea = new TrackAreaElement();
                trackArea.Rect = new Rectangle(0, y, this.Width, height);
                trackArea.Track = track;
                trackArea.TrackIndex = i;
                uiElements.Add(trackArea);
                TextElement trackName = new TextElement();
                trackName.Text = (i + 1).ToString("00") + " " + track.Name;
                trackName.Rect = new Rectangle(textCenterX, textCenterY, 0, 0);
                uiElements.Add(trackName);
                ControlAreaElement controlArea = new ControlAreaElement();
                controlArea.Rect = new Rectangle(0, sliderY, this.Width, DrawingConstants.SafeButtonHeight * 2);
                uiElements.Add(controlArea);
                SliderElement volumeSlider = new SliderElement();
                volumeSlider.MinValue = 0F;
                volumeSlider.MaxValue = 1.25F;
                volumeSlider.Value = track.Volume;
                volumeSlider.Rect = new Rectangle(0, sliderY, this.Width, DrawingConstants.SafeButtonHeight);
                volumeSlider.ValueChanged += () =>
                {
                    track.Volume = volumeSlider.Value;
                    this.Invalidate();
                };
                uiElements.Add(volumeSlider);
                ButtonElement muteButton = new ButtonElement();
                muteButton.Icon = DrawingIcons.Mute;
                muteButton.Rect = new Rectangle(0, buttonY, textCenterX, DrawingConstants.SafeButtonHeight);
                muteButton.On = track.Mute;
                muteButton.Click += () =>
                {
                    track.Mute = !track.Mute;
                    muteButton.On = track.Mute;
                    this.Invalidate();
                    if (!ParentEditor.PlayMode)
                        History.PushState(Song, ParentEditor.Selection, false);
                };
                uiElements.Add(muteButton);
                ButtonElement soloButton = new ButtonElement();
                soloButton.Icon = DrawingIcons.Solo;
                soloButton.Rect = new Rectangle(textCenterX, buttonY, textCenterX, DrawingConstants.SafeButtonHeight);
                soloButton.On = track.Solo;
                soloButton.Click += () =>
                {
                    track.Solo = !track.Solo;
                    soloButton.On = track.Solo;
                    this.Invalidate();
                    if (!ParentEditor.PlayMode)
                        History.PushState(Song, ParentEditor.Selection, false);
                };
                uiElements.Add(soloButton);
                y += height;
                if (i < tracks.Count - 1)
                {
                    SeperatorElement seperator = new SeperatorElement();
                    seperator.Rect = new Rectangle(0, y, this.Width, 0);
                    uiElements.Add(seperator);
                }
            }
        }
        private UIElement getUIElementFromPt(Point point)
        {
            for (int i = uiElements.Count - 1; i >= 0; i--)
            {
                UIElement element = uiElements[i];
                if (element.Rect.Contains(point.X, point.Y + ParentScroll))
                {
                    return element;
                }
            }
            return null;
        }
        private TrackAreaElement getTrackAreaFromPt(Point point)
        {
            foreach (TrackAreaElement trackArea in uiElements.OfType<TrackAreaElement>())
            {
                if (trackArea.Rect.Contains(point.X, point.Y + ParentScroll))
                {
                    return trackArea;
                }
            }
            return null;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.X < 0 || e.Y < 0)
            {
                return;
            }
            if (draggedTrack != null)
            {
                this.Cursor = Cursors.SizeNS;
                TrackAreaElement mouseArea = getTrackAreaFromPt(e.Location);
                if (mouseArea != null)
                {
                    if (mouseArea.Track != draggedTrack)
                    {
                        int heightDifference = TrackHeights[mouseArea.TrackIndex] - TrackHeights[draggedTrackIndex];
                        if (heightDifference > 0)
                        {
                            int scrollAdjustedY = e.Y + ParentScroll;
                            int lip = mouseArea.Rect.Bottom - scrollAdjustedY;
                            if (lip < heightDifference)
                            {
                                return;
                            }
                        }
                        bool isDraggedTrackFocused = (Song.FocusedTrack == draggedTrack);
                        bool isMouseTrackFocused = (Song.FocusedTrack == mouseArea.Track);
                        Song.Tracks[mouseArea.TrackIndex] = draggedTrack;
                        Song.Tracks[draggedTrackIndex] = mouseArea.Track;
                        draggedTrackIndex = mouseArea.TrackIndex;
                        trackOrderChanged = true;
                        if (isDraggedTrackFocused)
                        {
                            Song.FocusedTrack = draggedTrack;
                        }
                        if (isMouseTrackFocused)
                        {
                            Song.FocusedTrack = mouseArea.Track;
                        }
                        UpdateUI();
                        Invalidate();
                        ParentEditor.Refresh();
                    }
                }
                return;
            }
            if (heldSlider != null)
            {
                heldSlider.HandleX = e.X;
                return;
            }
            bool hit = false;
            UIElement element = getUIElementFromPt(e.Location);
            if (element != null && element.Type == UIElementType.Button)
            {
                hit = true;
                ButtonElement buttonElement = (ButtonElement)element;
                if (hoveredButton != element)
                {
                    buttonElement.Hovered = true;
                    if (hoveredButton != null)
                    {
                        hoveredButton.Hovered = false;
                    }
                    hoveredButton = buttonElement;
                    this.Invalidate();
                }
            }
            if (!hit && hoveredButton != null)
            {
                hoveredButton.Hovered = false;
                hoveredButton = null;
                this.Invalidate();
            }
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredButton != null)
            {
                hoveredButton.Hovered = false;
                hoveredButton = null;
                this.Invalidate();
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.X < 0 || e.Y < 0)
            {
                return;
            }
            UIElement element = getUIElementFromPt(e.Location);
            if (element == null)
            {
                return;
            }
            if (element.Type == UIElementType.Button)
            {
                ButtonElement buttonElement = (ButtonElement)element;
                buttonElement.InvokeClick();
            } else if (element.Type == UIElementType.TrackArea)
            {
                TrackAreaElement trackArea = (TrackAreaElement)element;
                draggedTrack = trackArea.Track;
                draggedTrackIndex = trackArea.TrackIndex;
                trackOrderChanged = false;
            } else if (element.Type == UIElementType.Slider)
            {
                SliderElement slider = (SliderElement)element;
                Point handlePt = new Point(slider.HandleX, slider.TrackY);
                int xDist = Math.Abs(handlePt.X - e.X);
                int yDist = Math.Abs(handlePt.Y - (e.Y + ParentScroll));
                if (xDist <= DrawingConstants.SliderHandleRadius && yDist <= DrawingConstants.SliderHandleRadius)
                {
                    heldSliderStartValue = slider.Value;
                    heldSlider = slider;
                }
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (draggedTrack != null)
            {
                this.Cursor = Cursors.Default;
                if (trackOrderChanged)
                {
                    History.PushState(Song, ParentEditor.Selection);
                }
                draggedTrack = null;
            }
            if (heldSlider != null)
            {
                if (heldSlider.Value != heldSliderStartValue)
                {
                    History.PushState(Song, ParentEditor.Selection);
                }
                heldSlider = null;
            }
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            TrackAreaElement trackArea = getTrackAreaFromPt(e.Location);
            if (trackArea != null)
            {
                using (TrackProperties trackProperties = new TrackProperties(trackArea.Track))
                {
                    trackProperties.ShowDialog();
                    if (trackProperties.ChangesMade)
                    {
                        ParentEditor.Selection = ParentEditor.Selection; // update fretboard, context menu, etc. for possible new tuning
                        ParentEditor.Refresh();
                        UpdateUI();
                        Invalidate();
                        History.PushState(Song, ParentEditor.Selection);
                    }
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.TranslateTransform(0, -ParentScroll);

            using (SolidBrush textBrush = new SolidBrush(DrawingConstants.ContrastColor))
            using (SolidBrush hoverBrush = new SolidBrush(DrawingConstants.HighlightColor))
            using (SolidBrush sliderHandleBrush = new SolidBrush(DrawingConstants.UIControlBackColor))
            using (SolidBrush buttonAreaBrush = new SolidBrush(DrawingConstants.UIAreaBackColor))
            using (Pen seperatorPen = new Pen(DrawingConstants.FadedGray, DrawingConstants.SeperatorLineWidth))
            using (Pen sliderPen = new Pen(DrawingConstants.TrackViewBackColor, DrawingConstants.SliderLineWidth))
            using (Font font = new Font(DrawingConstants.Montserrat, DrawingConstants.SmallTextSizePx, FontStyle.Regular, GraphicsUnit.Pixel))
            {
                sliderPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                sliderPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                foreach (UIElement element in uiElements)
                {
                    if (element.Type == UIElementType.Text)
                    {
                        TextElement textElement = (TextElement)element;
                        WrapLine[] lines = centerAndWrapText(g, font, textElement.Text, textElement.Rect.Location, this.Width - DrawingConstants.MediumMargin);
                        foreach (WrapLine line in lines)
                        {
                            g.DrawString(line.Text, font, textBrush, line.Point);
                        }
                    } else if (element.Type == UIElementType.Seperator)
                    {
                        g.DrawLine(seperatorPen, 0, element.Rect.Y, this.Width, element.Rect.Y);
                    } else if (element.Type == UIElementType.Button)
                    {
                        ButtonElement buttonElement = (ButtonElement)element;
                        float btnCenterX = buttonElement.Rect.X + buttonElement.Rect.Width / 2F;
                        float btnCenterY = buttonElement.Rect.Y + buttonElement.Rect.Height / 2F;
                        if (buttonElement.On)
                        {
                            g.DrawImage(buttonElement.Icon[DrawingConstants.ContrastColor], btnCenterX - DrawingConstants.SmallIconSize / 2F, btnCenterY - DrawingConstants.SmallIconSize / 2F, DrawingConstants.SmallIconSize, DrawingConstants.SmallIconSize);
                        } else
                        {
                            g.DrawImage(buttonElement.Icon[DrawingConstants.FadedGray], btnCenterX - DrawingConstants.SmallIconSize / 2F, btnCenterY - DrawingConstants.SmallIconSize / 2F, DrawingConstants.SmallIconSize, DrawingConstants.SmallIconSize);
                        }
                        if (buttonElement.Hovered)
                        {
                            g.FillRectangle(hoverBrush, buttonElement.Rect);
                        }
                    } else if (element.Type == UIElementType.ButtonArea)
                    {
                        g.FillRectangle(buttonAreaBrush, element.Rect);
                    } else if (element.Type == UIElementType.Slider)
                    {
                        SliderElement slider = (SliderElement)element;
                        float trackStartX = slider.TrackLeftEdge;
                        float trackEndX = trackStartX + slider.TrackWidth;
                        float trackY = slider.TrackY;
                        float handleX = slider.HandleX;
                        g.DrawLine(sliderPen, trackStartX, trackY, trackEndX, trackY);
                        g.FillEllipse(sliderHandleBrush, new RectangleF(handleX - DrawingConstants.SliderHandleRadius, trackY - DrawingConstants.SliderHandleRadius, DrawingConstants.SliderHandleRadius * 2, DrawingConstants.SliderHandleRadius * 2));
                    }
                }
            }
        }

        private static WrapLine[] centerAndWrapText(Graphics g, Font font, string text, Point centerPoint, int maxWidth)
        {
            List<WrapLine> lines = new List<WrapLine>();
            string[] words = text.Split(' ');
            WrapLine currentLine = new WrapLine();
            currentLine.Text = "";
            float lineY = 0;
            //float fontHeight = font.GetHeight(g);
            float currentLineHeight = 0F;
            bool firstWord = true;
            foreach (string word in words)
            {
                string proposedLine;
                if (firstWord)
                {
                    firstWord = false;
                    proposedLine = word;
                } else
                {
                    proposedLine = currentLine.Text + " " + word;
                }
                SizeF textSize = g.MeasureString(proposedLine, font);
                if (textSize.Width <= maxWidth)
                {
                    currentLine.Text = proposedLine;
                    if (textSize.Height > currentLineHeight)
                    {
                        currentLineHeight = textSize.Height;
                    }
                } else
                {
                    currentLine.Point = new PointF(0, lineY);
                    lines.Add(currentLine);
                    lineY += currentLineHeight * 1.25F;
                    currentLine = new WrapLine();
                    currentLine.Text = word;
                    currentLineHeight = textSize.Height;
                }
            }
            currentLine.Point = new PointF(0, lineY);
            lines.Add(currentLine);
            float height = lineY + currentLineHeight;
            float startY = centerPoint.Y - height / 2;
            for (int i = 0; i < lines.Count; i++)
            {
                WrapLine uncenteredLine = lines[i];
                WrapLine centeredLine = new WrapLine();
                centeredLine.Text = uncenteredLine.Text;
                SizeF lineSize = g.MeasureString(uncenteredLine.Text, font);
                float centeredX = centerPoint.X - lineSize.Width / 2;
                float centeredY = startY + uncenteredLine.Point.Y;
                centeredLine.Point = new PointF(centeredX, centeredY);
                lines[i] = centeredLine;
            }
            return lines.ToArray();
        }

        private struct WrapLine
        {
            public string Text;
            public PointF Point;
        }
        private abstract class UIElement
        {
            public abstract UIElementType Type { get; }
            public Rectangle Rect { get; set; }
        }
        private class TrackAreaElement : UIElement // not drawn
        {
            public override UIElementType Type => UIElementType.TrackArea;
            public Track Track { get; set; }
            public int TrackIndex { get; set; }
        }
        private class ControlAreaElement : UIElement
        {
            public override UIElementType Type => UIElementType.ButtonArea;
        }
        private class TextElement : UIElement // centered and wrapped, rect size is ignored
        {
            public override UIElementType Type => UIElementType.Text;
            public string Text { get; set; }
        }
        private class ButtonElement : UIElement
        {
            public override UIElementType Type => UIElementType.Button;
            public MultiColorBitmap Icon { get; set; }
            public bool Hovered { get; set; } = false;
            public bool On { get; set; }
            public event Action Click;
            public void InvokeClick()
            {
                Click?.Invoke();
            }
        }
        private class SeperatorElement : UIElement
        {
            public override UIElementType Type => UIElementType.Seperator;
        }
        private class SliderElement : UIElement
        {
            public override UIElementType Type => UIElementType.Slider;
            public float MinValue { get; set; }
            public float MaxValue { get; set; }
            private float value;
            public float Value
            {
                get
                {
                    return value;
                }
                set
                {
                    if (this.value != value)
                    {
                        this.value = value;
                        ValueChanged?.Invoke();
                    }
                }
            }
            public event Action ValueChanged;

            public int TrackWidth
            {
                get
                {
                    return Rect.Width - DrawingConstants.MediumMargin * 2;
                }
            }
            public int TrackLeftEdge
            {
                get
                {
                    return Rect.X + DrawingConstants.MediumMargin;
                }
            }
            public int TrackY
            {
                get
                {
                    return Rect.Y + Rect.Height / 2;
                }
            }
            public int HandleX
            {
                get
                {
                    float range = MaxValue - MinValue;
                    float valueMinNormalized = Value - MinValue;
                    float valueNormalized = valueMinNormalized / range;
                    return (int)Math.Round(TrackLeftEdge + TrackWidth * valueNormalized);
                }
                set
                {
                    int xEdgeNormalized = value - TrackLeftEdge;
                    float valueNormalized = xEdgeNormalized / (float)TrackWidth;
                    float range = MaxValue - MinValue;
                    float valueMinNormalized = range * valueNormalized;
                    float xConvertedToValue = valueMinNormalized + MinValue;
                    if (xConvertedToValue < MinValue)
                    {
                        Value = MinValue;
                    } else if (xConvertedToValue > MaxValue)
                    {
                        Value = MaxValue;
                    } else
                    {
                        Value = xConvertedToValue;
                    }
                }
            }
        }
        private enum UIElementType
        {
            Text,
            Button,
            Seperator,
            Slider,
            TrackArea,
            ButtonArea
        }
    }
}
