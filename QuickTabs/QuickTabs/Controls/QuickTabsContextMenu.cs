using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal class QuickTabsContextMenu : ContextMenu
    {
        ContextSection fileSection;
        ContextSection viewSection;
        public QuickTabsContextMenu()
        {
            Logo = DrawingIcons.QuickTabsLogo;

            fileSection = new ContextSection();
            fileSection.SectionName = "File";
            fileSection.ToggleType = ToggleType.NotTogglable;
            ContextItem open = new ContextItem(DrawingIcons.OpenFile);
            open.Selected = true;
            fileSection.AddItem(open);
            ContextItem save = new ContextItem(DrawingIcons.SaveFile);
            save.Selected = true;
            fileSection.AddItem(save);
            ContextItem saveAs = new ContextItem(DrawingIcons.SaveFileAs);
            saveAs.Selected = true;
            fileSection.AddItem(saveAs);
            ContextItem newFile = new ContextItem(DrawingIcons.NewFile);
            newFile.Selected = true;
            fileSection.AddItem(newFile);
            ContextItem editMetadata = new ContextItem(DrawingIcons.EditMetadata);
            editMetadata.Selected = true;
            fileSection.AddItem(editMetadata);
            Sections.Add(fileSection);

            viewSection = new ContextSection();
            viewSection.SectionName = "View";
            viewSection.ToggleType = ToggleType.Togglable;
            ContextItem fretCounts = new ContextItem(DrawingIcons.Counter);
            fretCounts.Selected = true;
            viewSection.AddItem(fretCounts);
            ContextItem dots = new ContextItem(DrawingIcons.Dots);
            dots.Selected = true;
            viewSection.AddItem(dots);
            Sections.Add(viewSection);

            updateUI();
        }
    }
}
