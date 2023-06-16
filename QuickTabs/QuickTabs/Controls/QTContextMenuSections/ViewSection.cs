using QuickTabs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection viewSection;
        private bool compactContext = false;
        private bool largeFretboard = false;

        private void setupViewSection()
        {
            viewSection = new ContextSection();
            viewSection.SectionName = "View";
            viewSection.ToggleType = ToggleType.Togglable;
            ContextItem fretCounts = new ContextItem(DrawingIcons.Counter, "Fret counter");
            fretCounts.Selected = QTSettings.Current.ViewFretCounter;
            if (!fretCounts.Selected)
            {
                viewFretCountClick();
            }
            fretCounts.Click += viewFretCountClick;
            viewSection.AddItem(fretCounts);
            ContextItem dots = new ContextItem(DrawingIcons.Dots, "Fret navigation dots");
            dots.Selected = QTSettings.Current.ViewNavDots;
            if (!dots.Selected)
            {
                viewDotsClick();
            }
            dots.Click += viewDotsClick;
            viewSection.AddItem(dots);
            ContextItem compactContextMenu = new ContextItem(DrawingIcons.CompactContextMenu, "Compact context menu");
            compactContextMenu.Selected = QTSettings.Current.ViewCompactCtxMenu;
            if (compactContextMenu.Selected)
            {
                compactContextClick();
            }
            compactContextMenu.Click += compactContextClick;
            viewSection.AddItem(compactContextMenu);
            ContextItem largeFretboard = new ContextItem(DrawingIcons.LargeFretboard, "Large fretboard");
            largeFretboard.Selected = QTSettings.Current.ViewLargeFretboard;
            if (largeFretboard.Selected)
            {
                largeFretboardClick();
            }
            largeFretboard.Click += largeFretboardClick;
            viewSection.AddItem(largeFretboard);
            ContextItem darkMode = new ContextItem(DrawingIcons.DarkMode, "Dark mode");
            darkMode.Selected = QTSettings.Current.ViewDarkMode;
            if (!darkMode.Selected)
            {
                changeTheme();
            }
            darkMode.Click += changeTheme;
            viewSection.AddItem(darkMode);
            Sections.Add(viewSection);
        }
        private void viewDotsClick()
        {
            Fretboard.ViewDots = !Fretboard.ViewDots;
            Fretboard.Refresh();
            if (QTSettings.Current.ViewNavDots != Fretboard.ViewDots)
            {
                QTSettings.Current.ViewNavDots = Fretboard.ViewDots;
                QTSettings.Current.Save();
            }
        }
        private void viewFretCountClick()
        {
            Fretboard.ViewFretCounter = !Fretboard.ViewFretCounter;
            Fretboard.Refresh();
            if (QTSettings.Current.ViewFretCounter != Fretboard.ViewFretCounter)
            {
                QTSettings.Current.ViewFretCounter = Fretboard.ViewFretCounter;
                QTSettings.Current.Save();
            }
        }
        private void compactContextClick()
        {
            if (compactContext)
            {
                compactContext = false;
                this.Logo = DrawingIcons.QuickTabsLogo;
                this.Style = Enums.ContextMenuStyle.Responsive;
                MainForm.ContextMenuHeight = 160;
                MainForm.RefreshLayout();
            } else
            {
                compactContext = true;
                this.Logo = null;
                this.Style = Enums.ContextMenuStyle.Collapsed;
                MainForm.ContextMenuHeight = 80;
                MainForm.RefreshLayout();
            }
            if (QTSettings.Current.ViewCompactCtxMenu != compactContext)
            {
                QTSettings.Current.ViewCompactCtxMenu = compactContext;
                QTSettings.Current.Save();
            }
        }
        private void largeFretboardClick()
        {
            if (largeFretboard)
            {
                largeFretboard = false;
                MainForm.FretboardHeight = 440;
                MainForm.RefreshLayout();
            } else
            {
                largeFretboard = true;
                MainForm.FretboardHeight = 530;
                MainForm.RefreshLayout();
            }
            if (QTSettings.Current.ViewLargeFretboard != largeFretboard)
            {
                QTSettings.Current.ViewLargeFretboard = largeFretboard;
                QTSettings.Current.Save();
                Fretboard.Refresh();
            }
        }
        private void changeTheme()
        {
            if (DrawingConstants.CurrentTheme == Theme.DarkMode)
            {
                DrawingConstants.SetTheme(Theme.LightMode);
            } else
            {
                DrawingConstants.SetTheme(Theme.DarkMode);
            }
            deepInvalidate(MainForm); // invalidate EVERYTHING
            if (QTSettings.Current.ViewDarkMode != (DrawingConstants.CurrentTheme == Theme.DarkMode))
            {
                QTSettings.Current.ViewDarkMode = (DrawingConstants.CurrentTheme == Theme.DarkMode);
                QTSettings.Current.Save();
            }

        }
        private void deepInvalidate(Control control)
        {
            control.Invalidate();
            foreach (Control child in control.Controls)
            {
                deepInvalidate(child); // recursion moment
            }
        }
    }
}
