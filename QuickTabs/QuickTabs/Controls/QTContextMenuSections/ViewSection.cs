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
            fretCounts.Selected = true;
            fretCounts.Click += viewFretCountClick;
            viewSection.AddItem(fretCounts);
            ContextItem dots = new ContextItem(DrawingIcons.Dots, "Fret navigation dots");
            dots.Selected = true;
            dots.Click += viewDotsClick;
            viewSection.AddItem(dots);
            ContextItem compactContextMenu = new ContextItem(DrawingIcons.CompactContextMenu, "Compact context menu");
            compactContextMenu.Selected = false;
            compactContextMenu.Click += compactContextClick;
            viewSection.AddItem(compactContextMenu);
            ContextItem largeFretboard = new ContextItem(DrawingIcons.LargeFretboard, "Large fretboard");
            largeFretboard.Selected = false;
            largeFretboard.Click += largeFretboardClick;
            viewSection.AddItem(largeFretboard);
            Sections.Add(viewSection);
        }
        private void viewDotsClick()
        {
            Fretboard.ViewDots = !Fretboard.ViewDots;
            Fretboard.Refresh();
        }
        private void viewFretCountClick()
        {
            Fretboard.ViewFretCounter = !Fretboard.ViewFretCounter;
            Fretboard.Refresh();
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
        }
        private void largeFretboardClick()
        {
            if (largeFretboard)
            {
                largeFretboard = false;
                MainForm.FretboardHeight = 350;
                MainForm.RefreshLayout();
            } else
            {
                largeFretboard = true;
                MainForm.FretboardHeight = 440;
                MainForm.RefreshLayout();
            }
        }
    }
}
