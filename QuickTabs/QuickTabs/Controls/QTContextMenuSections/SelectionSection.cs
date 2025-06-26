using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls
{
    public partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection selectionSection;
        private ContextItem paste;

        private void setupSelectionSection()
        {
            selectionSection = new ContextSection();
            selectionSection.SectionName = "Selection";
            selectionSection.ToggleType = ToggleType.NotTogglable;
            ContextItem copy = new ContextItem(DrawingIcons.Copy, "Copy");
            copy.Selected = true;
            copy.Click += copyClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.C, copyClick));
            selectionSection.AddItem(copy);
            paste = new ContextItem(DrawingIcons.Paste, "Paste");
            paste.Selected = false;
            paste.Click += pasteClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.V, pasteClick));
            selectionSection.AddItem(paste);
            ContextItem shiftLeft = new ContextItem(DrawingIcons.ShiftLeft, "Shift beats left");
            shiftLeft.Selected = true;
            shiftLeft.DontCloseDropdown = true;
            shiftLeft.Click += shiftLeftClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.A, shiftLeftClick));
            selectionSection.AddItem(shiftLeft);
            ContextItem shiftRight = new ContextItem(DrawingIcons.ShiftRight, "Shift beats right");
            shiftRight.Selected = true;
            shiftRight.DontCloseDropdown = true;
            shiftRight.Click += shiftRightClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.D, shiftRightClick));
            selectionSection.AddItem(shiftRight);
            ContextItem shiftUp = new ContextItem(DrawingIcons.ShiftUp, "Shift strings up");
            shiftUp.Selected = true;
            shiftUp.DontCloseDropdown = true;
            shiftUp.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) => { shiftStrings(-1); };
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.W, () => { shiftStrings(-1); }));
            selectionSection.AddItem(shiftUp);
            ContextItem shiftDown = new ContextItem(DrawingIcons.ShiftDown, "Shift strings down");
            shiftDown.Selected = true;
            shiftDown.DontCloseDropdown = true;
            shiftDown.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) => { shiftStrings(1); };
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.S, () => { shiftStrings(1); }));
            selectionSection.AddItem(shiftDown);
            ContextItem fretUp = new ContextItem(DrawingIcons.Plus, "Shift frets up");
            fretUp.Selected = true;
            fretUp.DontCloseDropdown = true;
            fretUp.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) => { shiftFrets(1); };
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, Keys.Oemplus, () => { shiftFrets(1); }));
            selectionSection.AddItem(fretUp);
            ContextItem fretDown = new ContextItem(DrawingIcons.Minus, "Shift frets down");
            fretDown.Selected = true;
            fretDown.DontCloseDropdown = true;
            fretDown.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) => { shiftFrets(-1); };
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Alt, (Keys)189, () => { shiftFrets(-1); }));
            selectionSection.AddItem(fretDown);
            ContextItem clear = new ContextItem(DrawingIcons.Clear, "Clear beats");
            clear.Selected = true;
            clear.Click += clearClick;
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.None, Keys.Delete, clearClick));
            selectionDependentShortcuts.Add(ShortcutManager.AddShortcut(Keys.Control, Keys.X, () => { copyClick(); clearClick(); }));
            ShortcutManager.AddShortcut(Keys.Control, Keys.A, selectAll);
            selectionSection.AddItem(clear);
        }

        private void copyClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => copyClick();
        private void copyClick()
        {
            clipboard = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    clipboard.Add(((Beat)Song.FocusedTab[i]).Copy());
                }
            }
            if (!paste.Selected)
            {
                paste.Selected = true;
                updateUI();
                Invalidate();
            }
        }
        private int copyBeats(List<Beat> source, Selection destination, bool preserveDestDivision = true) // return value: end index 
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
                if (i >= Song.FocusedTab.Count)
                {
                    break;
                }
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    Beat copy = source[indexInSource].Copy();
                    if (preserveDestDivision)
                    {
                        copy.BeatDivision = ((Beat)Song.FocusedTab[i]).BeatDivision;
                    }
                    foreach (KeyValuePair<Fret,MusicalTimespan> held in copy.ToArray())
                    {
                        if (held.Key.String >= Song.FocusedTab.Tuning.Count)
                        {
                            copy[held.Key] = MusicalTimespan.Zero;
                        }
                    }
                    Song.FocusedTab[i] = copy;
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
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    count++;
                }
            }
            return count;
        }
        private void pasteClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => pasteClick();
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
        private void clearBeat(int index)
        {
            Beat emptyBeat = new Beat();
            emptyBeat.BeatDivision = ((Beat)Song.FocusedTab[index]).BeatDivision;
            Song.FocusedTab[index] = emptyBeat;
        }
        private void clearClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => clearClick();
        private void clearClick()
        {
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    clearBeat(i);
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void shiftLeftClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => shiftLeftClick();
        private void shiftLeftClick()
        {
            List<Beat> selectionBeats = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    selectionBeats.Add((Beat)(Song.FocusedTab[i]));
                }
            }
            int additive = 1;
            if (Song.FocusedTab[selection.SelectionStart - additive].Type == Enums.StepType.SectionHead && selection.SelectionStart > 1)
            {
                additive = 2;
            }
            int endIndex = copyBeats(selectionBeats, new Selection(selection.SelectionStart - additive, selectionBeats.Count));
            if (Song.FocusedTab[endIndex].Type == Enums.StepType.SectionHead)
            {
                endIndex++;
            }
            clearBeat(endIndex);
            int newSelectionStart = selection.SelectionStart - additive;
            if (newSelectionStart < 1)
            {
                newSelectionStart = 1;
            }
            Debug.WriteLine("Old selection start: " + editor.Selection.SelectionStart);
            Debug.WriteLine("Old selection length: " + editor.Selection.SelectionLength);
            if (endIndex - newSelectionStart > 0)
            {
                int selectionLen = endIndex - newSelectionStart;
                if (Song.FocusedTab[newSelectionStart + selectionLen - 1].Type == Enums.StepType.SectionHead)
                {
                    selectionLen--;
                }
                editor.Selection = new Selection(newSelectionStart, selectionLen);
            }
            else
            {
                editor.Selection = null;
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void shiftRightClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => shiftRightClick();
        private void shiftRightClick()
        {
            List<Beat> selectionBeats = new List<Beat>();
            Selection selection = editor.Selection;
            for (int i = selection.SelectionStart; i < selection.SelectionStart + selection.SelectionLength; i++)
            {
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    selectionBeats.Add((Beat)(Song.FocusedTab[i]));
                }
            }
            int additive = 1;
            if (selection.SelectionStart + additive < Song.FocusedTab.Count && Song.FocusedTab[selection.SelectionStart + additive].Type == Enums.StepType.SectionHead)
            {
                additive = 2;
            }
            int endIndex = copyBeats(selectionBeats, new Selection(selection.SelectionStart + additive, selectionBeats.Count));
            clearBeat(selection.SelectionStart);
            int newSelectionStart = selection.SelectionStart + additive;
            if (newSelectionStart >= Song.FocusedTab.Count)
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
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    Beat srcBeat = (Beat)(Song.FocusedTab[i]);
                    Beat newBeat = new Beat();
                    newBeat.BeatDivision = srcBeat.BeatDivision;
                    foreach (KeyValuePair<Fret,MusicalTimespan> fret in srcBeat)
                    {
                        int newString = fret.Key.String + additive;
                        if (newString >= 0 && newString < Song.FocusedTab.Tuning.Count)
                        {
                            newBeat[new Fret(newString, fret.Key.Space)] = fret.Value;
                        }
                    }
                    Song.FocusedTab[i] = newBeat;
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
                if (Song.FocusedTab[i].Type == Enums.StepType.Beat)
                {
                    Beat srcBeat = (Beat)(Song.FocusedTab[i]);
                    Beat newBeat = new Beat();
                    newBeat.BeatDivision = srcBeat.BeatDivision;
                    foreach (KeyValuePair<Fret,MusicalTimespan> fret in srcBeat)
                    {
                        int newSpace = fret.Key.Space + additive;
                        if (newSpace >= 0)
                        {
                            newBeat[new Fret(fret.Key.String, newSpace)] = fret.Value;
                        }
                    }
                    Song.FocusedTab[i] = newBeat;
                }
            }
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection);
        }
        private void selectAll()
        {
            editor.Selection = new Selection(1, Song.FocusedTab.Count - 1);
            editor.Refresh();
            History.PushState(Song, editor.Selection, false);
        }
    }
}
