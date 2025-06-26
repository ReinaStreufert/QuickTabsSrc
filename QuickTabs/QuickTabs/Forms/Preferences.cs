using QuickTabs.Configuration;
using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickTabs.Forms
{
    public partial class Preferences : Form
    {
        public override Color BackColor { get => DrawingConstants.UIAreaBackColor; set => base.BackColor = value; }
        
        public Editor EditorForm { get; set; }
        public TabEditor TabEditor { get; set; }
        public Fretboard Fretboard { get; set; }
        public QuickTabsContextMenu ContextMenu { get; set; }

        private Panel contentPanel;
        private ToolTip toolTip;
        private PictureBox logoBox;

        public Preferences(Editor editorForm, TabEditor tabEditor, Fretboard fretboard, QuickTabsContextMenu contextMenu)
        {
            InitializeComponent();
            this.Size = new Size(DrawingConstants.PreferencesContentWidth + DrawingConstants.LargeMargin * 2, this.Height);
            this.MinimumSize = new Size(this.Size.Width, this.MinimumSize.Height);
            this.MaximumSize = new Size(this.Size.Width, Screen.FromControl(this).Bounds.Height);
            toolTip = new ToolTip();
            contentPanel = new Panel();
            contentPanel.BackColor = DrawingConstants.UIAreaBackColor;
            contentPanel.AutoScroll = true;
            contentPanel.AutoScrollMargin = new Size(0, DrawingConstants.LargeMargin);
            contentPanel.Location = new Point(0, 0);
            this.Controls.Add(contentPanel);
            EditorForm = editorForm;
            TabEditor = tabEditor;
            Fretboard = fretboard;
            ContextMenu = contextMenu;
            generateContent();
            OnSizeChanged(null);
            DrawingConstants.ThemeChanged += DrawingConstants_ThemeChanged;
        }

        private void DrawingConstants_ThemeChanged()
        {
            DrawingConstants.ApplyThemeToUIForm(this);
            logoBox.Image = DrawingIcons.QuickTabsLogo[DrawingConstants.ContrastColor];
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (contentPanel == null) { return; }
            contentPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height);
        }

        private void generateContent()
        {
            int centerX = this.ClientSize.Width / 2;
            int leftEdgeX = centerX - DrawingConstants.PreferencesContentWidth / 2;
            int leftMarginX = leftEdgeX + DrawingConstants.PreferencesControlLeftMargin;
            int y = DrawingConstants.LargeMargin;
            PreferenceCategory[] cats = PreferenceConfiguration.Config;
            foreach (PreferenceCategory category in cats)
            {
                Label title = new Label();
                title.BackColor = DrawingConstants.UIAreaBackColor;
                title.ForeColor = DrawingConstants.ContrastColor;
                title.Font = new Font(DrawingConstants.Montserrat, 12, FontStyle.Bold, GraphicsUnit.Point);
                title.Text = category.Title;
                title.Location = new Point(leftEdgeX, y);
                title.AutoSize = true;
                contentPanel.Controls.Add(title);
                y += title.Height;
                foreach (Preference pref in category.GeneratePage(EditorForm, TabEditor, ContextMenu, Fretboard))
                {
                    if (!pref.SelfLabeled)
                    {
                        Label prefLabel = new Label();
                        prefLabel.BackColor = DrawingConstants.UIAreaBackColor;
                        prefLabel.ForeColor = DrawingConstants.ContrastColor;
                        prefLabel.Font = new Font(DrawingConstants.Montserrat, 9, FontStyle.Regular, GraphicsUnit.Point);
                        prefLabel.Text = pref.Name;
                        prefLabel.Location = new Point(leftEdgeX, y);
                        prefLabel.AutoSize = true;
                        contentPanel.Controls.Add(prefLabel);
                        y += prefLabel.Height;
                    }
                    Margins ctrlMargins = pref.Margins;
                    y += ctrlMargins.Top;
                    Control ctrl = pref.GenerateControl();
                    ctrl.Location = new Point(leftMarginX + ctrlMargins.Left, y);
                    contentPanel.Controls.Add(ctrl);
                    y += ctrl.Height + ctrlMargins.Bottom;
                    if (pref.CanBeAddedToContextMenu)
                    {
                        IconToggle iconToggle = new IconToggle(DrawingIcons.Pin, QTPersistence.Current.ConfigPinnedPrefs.Contains(pref.Name));
                        iconToggle.Size = new Size(DrawingConstants.PreferencesPinButtonSize, DrawingConstants.PreferencesPinButtonSize);
                        int iconToggleX = leftMarginX + ctrl.Width + ctrlMargins.Right;
                        int iconToggleY = (ctrl.Location.Y + ctrl.Height / 2) - DrawingConstants.PreferencesPinButtonSize / 2;
                        iconToggle.Location = new Point(iconToggleX, iconToggleY);
                        iconToggle.ToggleStateChanged += (object sender, EventArgs e) => { pinToggleChanged(pref, iconToggle.ToggleState); };
                        iconToggle.MouseEnter += pinToggle_MouseEnter;
                        iconToggle.MouseLeave += pinToggle_MouseLeave;
                        contentPanel.Controls.Add(iconToggle);
                    }
                }
                y += DrawingConstants.PreferencesCatSpacing;
            }
            logoBox = new PictureBox();
            logoBox.SizeMode = PictureBoxSizeMode.StretchImage;
            logoBox.Image = DrawingIcons.QuickTabsLogo[DrawingConstants.ContrastColor];
            float aspRatio = DrawingIcons.QuickTabsLogo.Size.Width / (float)DrawingIcons.QuickTabsLogo.Size.Height;
            logoBox.Size = new Size((int)(aspRatio * DrawingConstants.PrefsLogoHeight), DrawingConstants.PrefsLogoHeight);
            logoBox.Location = new Point(centerX - logoBox.Size.Width / 2, y);
            contentPanel.Controls.Add(logoBox);
        }

        private void pinToggleChanged(Preference pref, bool toggleState)
        {
            string[] oldPinnedPrefs = QTPersistence.Current.ConfigPinnedPrefs;
            string[] newPinnedPrefs;
            if (toggleState)
            {
                newPinnedPrefs = new string[oldPinnedPrefs.Length + 1];
                oldPinnedPrefs.CopyTo(newPinnedPrefs, 0);
                newPinnedPrefs[newPinnedPrefs.Length - 1] = pref.Name;
            } else
            {
                newPinnedPrefs = new string[oldPinnedPrefs.Length - 1];
                int i = 0;
                foreach (string s in oldPinnedPrefs)
                {
                    if (s != pref.Name)
                    {
                        newPinnedPrefs[i] = s;
                        i++;
                    }
                }
            }
            QTPersistence.Current.ConfigPinnedPrefs = newPinnedPrefs;
            QTPersistence.Current.Save();
            ContextMenu.RefreshPinnedConfig();
        }

        private void pinToggle_MouseEnter(object? sender, EventArgs e)
        {
            Control control = (Control)sender;
            int ttX = control.Location.X;
            int ttY = control.Location.Y + control.Size.Height;
            toolTip.Show("Pin setting to context menu", contentPanel, ttX, ttY);
        }
        private void pinToggle_MouseLeave(object? sender, EventArgs e)
        {
            toolTip.Hide(this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            DrawingConstants.ThemeChanged -= DrawingConstants_ThemeChanged;
            ContextMenu.RefreshPinnedConfig();
        }
    }
}
