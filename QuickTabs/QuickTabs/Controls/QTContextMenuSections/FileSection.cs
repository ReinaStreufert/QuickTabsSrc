using QuickTabs.Forms;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs.Controls
{
    internal partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection fileSection;

        private void setupFileSection()
        {
            fileSection = new ContextSection();
            fileSection.SectionName = "File";
            fileSection.ToggleType = ToggleType.NotTogglable;
            ContextItem newFile = new ContextItem(DrawingIcons.Reload, "New");
            newFile.Selected = true;
            newFile.Click += newClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.N, newClick);
            fileSection.AddItem(newFile);
            ContextItem open = new ContextItem(DrawingIcons.OpenFile, "Open...");
            open.Selected = true;
            open.Click += openClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.O, openClick);
            fileSection.AddItem(open);
            ContextItem save = new ContextItem(DrawingIcons.SaveFile, "Save");
            save.Selected = true;
            save.Click += saveClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.S, saveClick);
            fileSection.AddItem(save);
            ContextItem saveAs = new ContextItem(DrawingIcons.SaveFileAs, "Save as...");
            saveAs.Selected = true;
            saveAs.Click += saveAsClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.S, saveAsClick);
            fileSection.AddItem(saveAs);
            ContextItem export = new ContextItem(DrawingIcons.Export, "Export plain text...");
            export.Selected = true;
            export.Click += exportClick;
            ShortcutManager.AddShortcut(Keys.Control, Keys.E, exportClick);
            fileSection.AddItem(export);
            ContextItem documentProperties = new ContextItem(DrawingIcons.EditDocumentProperties, "Document properties...");
            documentProperties.Selected = true;
            documentProperties.Click += documentPropertiesClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.D, documentPropertiesClick);
            fileSection.AddItem(documentProperties);
            Sections.Add(fileSection);
        }

        private void saveClick()
        {
            FileManager.Save(Song);
            MainForm.Cursor = Cursors.WaitCursor;
            Timer t = new Timer();
            t.Interval = 150;
            t.Tick += (object sender, EventArgs e) =>
            {
                MainForm.Cursor = Cursors.Default;
                t.Stop();
                t.Dispose();
            };
            t.Start();
        }
        private void saveAsClick()
        {
            FileManager.SaveAs(Song);
        }
        private void openClick()
        {
            if (!FileManager.IsSaved)
            {
                using (UnsavedChanges unsavedChanges = new UnsavedChanges())
                {
                    unsavedChanges.Verb = "open a new file";
                    unsavedChanges.ShowDialog();
                    if (!unsavedChanges.Continue)
                    {
                        return;
                    }
                }
            }
            bool failed;
            Song openedSong = FileManager.Open(out failed);
            if (openedSong == null)
            {
                if (failed)
                {
                    using (GenericMessage errMessage = new GenericMessage())
                    {
                        errMessage.Text = "Could not open file";
                        errMessage.Message = "File format was not valid qtjson";
                        errMessage.ShowDialog(MainForm);
                    }
                }
                return;
            }
            History.ClearHistory();
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Stop();
                editor.PlayMode = false; // this gets done by the play cursor update timer, but theres a delay and we need to set the selection right after this
            }
            Song = openedSong;
            editor.QuietlySelect(new Selection(1, 1));
            MainForm.song = Song;
            editor.Song = Song;
            Fretboard.Song = Song;
            Fretboard.Refresh();
            editor.Refresh();
            editor.Selection = new Selection(1, 1);
            History.PushState(Song, editor.Selection, false);
        }
        private void newClick()
        {
            if (!FileManager.IsSaved)
            {
                using (UnsavedChanges unsavedChanges = new UnsavedChanges())
                {
                    unsavedChanges.Verb = "start a new tab";
                    unsavedChanges.ShowDialog();
                    if (!unsavedChanges.Continue)
                    {
                        return;
                    }
                }
            }
            FileManager.New();
            History.ClearHistory();
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Stop();
                editor.PlayMode = false;
            }
            Song.Name = "Untitled tab";
            Song.Tempo = 120;
            Song.TimeSignature = new TimeSignature(4, 4);
            Song.Tab.Tuning = Tuning.StandardGuitar;
            Song.Tab.SetLength(17);
            ((SectionHead)Song.Tab[0]).Name = "Untitled Section";
            for (int i = 1; i < 17; i++)
            {
                Song.Tab[i] = new Beat();
            }
            editor.Selection = new Selection(1, 1);
            editor.Refresh();
            Fretboard.Refresh();
            History.PushState(Song, editor.Selection, false);
        }
        private void exportClick()
        {
            using (ExportPlainText exportPlainText = new ExportPlainText())
            {
                exportPlainText.Song = Song;
                exportPlainText.ShowDialog();
            }
        }
        private void documentPropertiesClick()
        {
            bool changed;
            TimeSignature oldTs = Song.TimeSignature;
            using (DocumentProperties dp = new DocumentProperties())
            {
                dp.Song = Song;
                dp.ShowDialog();
                changed = dp.ChangesSaved;
            }
            if (changed)
            {
                if (tabPlayer != null && editor.PlayMode)
                {
                    if (Song.TimeSignature != oldTs)
                    {
                        if (tabPlayer.IsPlaying)
                        {
                            tabPlayer.Stop();
                        }
                        editor.PlayMode = false;
                    } else
                    {
                        tabPlayer.BPM = Song.Tempo;
                    }
                }
                if (editor.Selection != null && editor.Selection.SelectionStart + editor.Selection.SelectionLength >= Song.Tab.Count)
                {
                    editor.Selection = new Selection(1, 1);
                }
                editor.Refresh();
                Fretboard.Refresh();
                History.PushState(Song, editor.Selection);
            }
        }
    }
}
