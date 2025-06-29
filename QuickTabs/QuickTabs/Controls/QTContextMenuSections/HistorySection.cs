﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection historySection;
        private ContextItem undo;
        private ContextItem redo;
        private ContextItem redoAlternate;

        private void setupHistorySection()
        {
            History.StateChange += historyStateChanged;
            historySection = new ContextSection();
            historySection.SectionName = "History";
            historySection.ToggleType = ToggleType.NotTogglable;
            undo = new ContextItem(DrawingIcons.Undo, "Undo");
            undo.Selected = false;
            undo.Click += undoClick;
            historyShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.Z, undoClick));
            historySection.AddItem(undo);
            redo = new ContextItem(DrawingIcons.Redo, "Redo");
            redo.Selected = false;
            redo.Click += redoClick;
            historyShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.Y, redoClick));
            historySection.AddItem(redo);
            redoAlternate = new ContextItem(DrawingIcons.RedoAlternate, "Alternate redo");
            redoAlternate.Selected = false;
            redoAlternate.Click += redoAlternateClick;
            historyShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.Y, redoAlternateClick));
            historySection.AddItem(redoAlternate);
            Sections.Add(historySection);
        }
        private void historyStateChanged()
        {
            bool changed = false;
            if (History.CanUndo != undo.Selected)
            {
                changed = true;
                undo.Selected = History.CanUndo;
            }
            if (History.CanRedo != redo.Selected)
            {
                changed = true;
                redo.Selected = History.CanRedo;
            }
            if (History.CanAlternateRedo != redoAlternate.Selected)
            {
                changed = true;
                redoAlternate.Selected = History.CanAlternateRedo;
            }
            if (changed)
            {
                this.Invalidate();
            }
        }
        private void undoClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => undoClick();
        private void undoClick()
        {
            if (!History.CanUndo)
            {
                return;
            }
            Selection newSelection;
            History.Undo(Song, out newSelection);
            refreshState(newSelection);
        }
        private void redoClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => redoClick();
        private void redoClick()
        {
            if (!History.CanRedo)
            {
                return;
            }
            Selection newSelection;
            History.Redo(Song, out newSelection);
            refreshState(newSelection);
        }
        private void redoAlternateClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => redoAlternateClick();
        private void redoAlternateClick()
        {
            if (!History.CanAlternateRedo)
            {
                return;
            }
            Selection newSelection;
            History.RedoAlternate(Song, out newSelection);
            refreshState(newSelection);
        }
        private void refreshState(Selection newSelection)
        {
            editor.QuietlySelect(newSelection);
            Fretboard.Refresh();
            editor.Refresh();
            editor.RefreshTrackView();
            editor.Selection = newSelection;
        }
    }
}
