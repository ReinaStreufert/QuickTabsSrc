using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    internal partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection selectionSection;

        private void setupSelectionSection()
        {
            selectionSection = new ContextSection();
            selectionSection.SectionName = "Selection";
            selectionSection.ToggleType = ToggleType.NotTogglable;
            ContextItem copy = new ContextItem(DrawingIcons.Copy, "Copy");
            copy.Selected = true;
            copy.Click += copyClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.C, copyClick));
            selectionSection.AddItem(copy);
            paste = new ContextItem(DrawingIcons.Paste, "Paste");
            paste.Selected = false;
            paste.Click += pasteClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.V, pasteClick));
            selectionSection.AddItem(paste);
            ContextItem shiftLeft = new ContextItem(DrawingIcons.ShiftLeft, "Shift beats left");
            shiftLeft.Selected = true;
            shiftLeft.DontCloseDropdown = true;
            shiftLeft.Click += shiftLeftClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.A, shiftLeftClick));
            selectionSection.AddItem(shiftLeft);
            ContextItem shiftRight = new ContextItem(DrawingIcons.ShiftRight, "Shift beats right");
            shiftRight.Selected = true;
            shiftRight.DontCloseDropdown = true;
            shiftRight.Click += shiftRightClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.D, shiftRightClick));
            selectionSection.AddItem(shiftRight);
            ContextItem shiftUp = new ContextItem(DrawingIcons.ShiftUp, "Shift strings up");
            shiftUp.Selected = true;
            shiftUp.DontCloseDropdown = true;
            shiftUp.Click += () => { shiftStrings(-1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.W, () => { shiftStrings(-1); }));
            selectionSection.AddItem(shiftUp);
            ContextItem shiftDown = new ContextItem(DrawingIcons.ShiftDown, "Shift strings down");
            shiftDown.Selected = true;
            shiftDown.DontCloseDropdown = true;
            shiftDown.Click += () => { shiftStrings(1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.S, () => { shiftStrings(1); }));
            selectionSection.AddItem(shiftDown);
            ContextItem fretUp = new ContextItem(DrawingIcons.Plus, "Shift frets up");
            fretUp.Selected = true;
            fretUp.DontCloseDropdown = true;
            fretUp.Click += () => { shiftFrets(1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.Oemplus, () => { shiftFrets(1); }));
            selectionSection.AddItem(fretUp);
            ContextItem fretDown = new ContextItem(DrawingIcons.Minus, "Shift frets down");
            fretDown.Selected = true;
            fretDown.DontCloseDropdown = true;
            fretDown.Click += () => { shiftFrets(-1); };
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, (Keys)189, () => { shiftFrets(-1); }));
            selectionSection.AddItem(fretDown);
            ContextItem clear = new ContextItem(DrawingIcons.Clear, "Clear beats");
            clear.Selected = true;
            clear.Click += clearClick;
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.None, Keys.Delete, clearClick));
            selectionShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.X, () => { copyClick(); clearClick(); }));
            ShortcutManager.AddShortcut(Keys.Control, Keys.A, selectAll);
            selectionSection.AddItem(clear);
        }
        private void copyClick()
        {
            clipboard = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    clipboard.Add(((Beat)Song.Tab[i]).Copy());
                }
            }
            if (!paste.Selected)
            {
                paste.Selected = true;
                updateUI();
                Invalidate();
            }
        }
        private int copyBeats(List<Beat> source, Selection destination) // return value: end index 
        {
            int indexInSource = 0;
            int copied = 0;
            int i;
            for (i = destination.SelectionStart; copied < destination.SelectionLength; i++)
            {
                if (i < 1)
                {
                    indexInSource++;
                    copied++;
                    if (indexInSource >= source.Count)
                    {
                        indexInSource = 0;
                    }
                    continue;
                }
                if (i >= Song.Tab.Count)
                {
                    break;
                }
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Song.Tab[i] = source[indexInSource].Copy();
                    indexInSource++;
                    copied++;
                    if (indexInSource >= source.Count)
                    {
                        indexInSource = 0;
                    }
                }
            }
            return i;
        }
        private int countBeatsInSelection(Selection selection)
        {
            int count = 0;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    count++;
                }
            }
            return count;
        }
        private void pasteClick()
        {
            if (clipboard == null)
            {
                return;
            }
            Selection selection = editor.Selection;
            if (selection.SelectionLength == 1 && clipboard.Count > 1)
            {
                selection.SelectionLength = clipboard.Count;
            }
            else
            {
                selection.SelectionLength = countBeatsInSelection(selection);
            }
            int endIndex = copyBeats(clipboard, selection);
            editor.Selection = new Selection(selection.SelectionStart, endIndex - selection.SelectionStart);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void clearClick()
        {
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Song.Tab[i] = new Beat();
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void shiftLeftClick()
        {
            List<Beat> selectionBeats = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    selectionBeats.Add((Beat)(Song.Tab[i]));
                }
            }
            int additive = 1;
            if (Song.Tab[selection.SelectionStart - additive].Type == Enums.StepType.SectionHead && selection.SelectionStart > 1)
            {
                additive = 2;
            }
            int endIndex = copyBeats(selectionBeats, new Selection(selection.SelectionStart - additive, selectionBeats.Count));
            if (Song.Tab[endIndex].Type == Enums.StepType.SectionHead)
            {
                endIndex++;
            }
            Song.Tab[endIndex] = new Beat();
            int newSelectionStart = selection.SelectionStart - additive;
            if (newSelectionStart < 1)
            {
                newSelectionStart = 1;
            }
            if (endIndex - newSelectionStart > 0)
            {
                editor.Selection = new Selection(newSelectionStart, endIndex - newSelectionStart);
            }
            else
            {
                editor.Selection = null;
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void shiftRightClick()
        {
            List<Beat> selectionBeats = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    selectionBeats.Add((Beat)(Song.Tab[i]));
                }
            }
            int additive = 1;
            if (selection.SelectionStart + additive < Song.Tab.Count && Song.Tab[selection.SelectionStart + additive].Type == Enums.StepType.SectionHead)
            {
                additive = 2;
            }
            int endIndex = copyBeats(selectionBeats, new Selection(selection.SelectionStart + additive, selectionBeats.Count));
            Song.Tab[selection.SelectionStart] = new Beat();
            int newSelectionStart = selection.SelectionStart + additive;
            if (newSelectionStart >= Song.Tab.Count)
            {
                editor.Selection = null;
            }
            else
            {
                editor.Selection = new Selection(newSelectionStart, endIndex - newSelectionStart);
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void shiftStrings(int additive)
        {
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Beat srcBeat = (Beat)(Song.Tab[i]);
                    Beat newBeat = new Beat();
                    newBeat.NoteLength = srcBeat.NoteLength;
                    foreach (Fret fret in srcBeat)
                    {
                        int newString = fret.String + additive;
                        if (newString >= 0 && newString < Song.Tab.Tuning.Count)
                        {
                            newBeat[new Fret(newString, fret.Space)] = true;
                        }
                    }
                    Song.Tab[i] = newBeat;
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        public void shiftFrets(int additive)
        {
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.Tab[i].Type == Enums.StepType.Beat)
                {
                    Beat srcBeat = (Beat)(Song.Tab[i]);
                    Beat newBeat = new Beat();
                    newBeat.NoteLength = srcBeat.NoteLength;
                    foreach (Fret fret in srcBeat)
                    {
                        int newSpace = fret.Space + additive;
                        if (newSpace >= 0)
                        {
                            newBeat[new Fret(fret.String, newSpace)] = true;
                        }
                    }
                    Song.Tab[i] = newBeat;
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void selectAll()
        {
            editor.Selection = new Selection(1, Song.Tab.Count - 1);
            editor.Refresh();
            History.PushState(Song, editor.Selection, false);
        }
    }
}
