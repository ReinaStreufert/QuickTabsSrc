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
            fretCounts.Selected = Properties.QuickTabs.Default.ViewFretCounter;
            if (!fretCounts.Selected)
            {
                viewFretCountClick();
            }
            fretCounts.Click += viewFretCountClick;
            viewSection.AddItem(fretCounts);
            ContextItem dots = new ContextItem(DrawingIcons.Dots, "Fret navigation dots");
            dots.Selected = Properties.QuickTabs.Default.ViewNavDots;
            if (!dots.Selected)
            {
                viewDotsClick();
            }
            dots.Click += viewDotsClick;
            viewSection.AddItem(dots);
            ContextItem compactContextMenu = new ContextItem(DrawingIcons.CompactContextMenu, "Compact context menu");
            compactContextMenu.Selected = Properties.QuickTabs.Default.ViewCompactCtxMenu;
            if (compactContextMenu.Selected)
            {
                compactContextClick();
            }
            compactContextMenu.Click += compactContextClick;
            viewSection.AddItem(compactContextMenu);
            ContextItem largeFretboard = new ContextItem(DrawingIcons.LargeFretboard, "Large fretboard");
            largeFretboard.Selected = Properties.QuickTabs.Default.ViewLargeFretboard;
            if (largeFretboard.Selected)
            {
                largeFretboardClick();
            }
            largeFretboard.Click += largeFretboardClick;
            viewSection.AddItem(largeFretboard);
            ContextItem darkMode = new ContextItem(DrawingIcons.DarkMode, "Dark mode");
            darkMode.Selected = Properties.QuickTabs.Default.ViewDarkMode;
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
            if (Properties.QuickTabs.Default.ViewNavDots != Fretboard.ViewDots)
            {
                Properties.QuickTabs.Default.ViewNavDots = Fretboard.ViewDots;
                Properties.QuickTabs.Default.Save();
            }
        }
        private void viewFretCountClick()
        {
            Fretboard.ViewFretCounter = !Fretboard.ViewFretCounter;
            Fretboard.Refresh();
            if (Properties.QuickTabs.Default.ViewFretCounter != Fretboard.ViewFretCounter)
            {
                Properties.QuickTabs.Default.ViewFretCounter = Fretboard.ViewFretCounter;
                Properties.QuickTabs.Default.Save();
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
            if (Properties.QuickTabs.Default.ViewCompactCtxMenu != compactContext)
            {
                Properties.QuickTabs.Default.ViewCompactCtxMenu = compactContext;
                Properties.QuickTabs.Default.Save();
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
            if (Properties.QuickTabs.Default.ViewLargeFretboard != largeFretboard)
            {
                Properties.QuickTabs.Default.ViewLargeFretboard = largeFretboard;
                Properties.QuickTabs.Default.Save();
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
            if (Properties.QuickTabs.Default.ViewDarkMode != (DrawingConstants.CurrentTheme == Theme.DarkMode))
            {
                Properties.QuickTabs.Default.ViewDarkMode = (DrawingConstants.CurrentTheme == Theme.DarkMode);
                Properties.QuickTabs.Default.Save();
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
