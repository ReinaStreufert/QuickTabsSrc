using QuickTabs.Forms;
using QuickTabs.Songwriting;

namespace QuickTabs.Controls
{
    public partial class QuickTabsContextMenu
    {
        private ContextSection trackSection;
        private ContextItem removeTrack;
        private ContextItem trackProperties;

        private void setupTrackSection()
        {
            trackSection = new ContextSection();
            trackSection.SectionName = "Track";
            trackSection.ToggleType = ToggleType.NotTogglable;
            ContextItem addTrack = new ContextItem(DrawingIcons.AddMeasure, "Add track");
            addTrack.Selected = true;
            addTrack.Click += addTrackClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.Oemplus, addTrackClick);
            trackSection.AddItem(addTrack);
            removeTrack = new ContextItem(DrawingIcons.RemoveMeasure, "Remove track");
            removeTrack.Selected = false;
            removeTrack.Click += removeTrackClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, (Keys)189, removeTrackClick);
            trackSection.AddItem(removeTrack);
            ContextItem addAnnotationTrack = new ContextItem(DrawingIcons.Dots, "Add annotation track");
            addTrack.Selected = true;
            addTrack.Click += addAnnotationTrackClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.Oemplus, addTrackClick);
            trackSection.AddItem(addTrack);
            trackProperties = new ContextItem(DrawingIcons.EditDocumentProperties, "Edit track properties");
            trackProperties.Selected = true;
            trackProperties.Click += trackPropertiesClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.T, trackPropertiesClick);
            trackSection.AddItem(trackProperties);
            ContextItem printTrack = new ContextItem(DrawingIcons.Print, "Print track...");
            printTrack.Selected = true;
            printTrack.Click += printTrackClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.P, printTrackClick);
            trackSection.AddItem(printTrack);
            ContextItem export = new ContextItem(DrawingIcons.Export, "Export track plain text...");
            export.Selected = true;
            export.Click += exportTrackClick;
            ShortcutManager.AddShortcut(Keys.Control | Keys.Shift, Keys.E, exportTrackClick);
            trackSection.AddItem(export);
            Sections.Add(trackSection);
        }

        private void addTrackClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => addTrackClick();
        private void addTrackClick()
        {
            if (SequencePlayer.PlayState == Enums.PlayState.Playing)
            {
                return;
            }
            Track lastCurrentTrack = Song.Tracks[Song.Tracks.Count - 1];
            Track newTrack = new Track();
            Tab trackTab = newTrack.Tab;
            MusicalTimespan division = Song.TimeSignature.DefaultDivision;
            trackTab.Tuning = Tuning.StandardGuitar;
            newTrack.UpdateAutoName();
            Tab sourceTab = lastCurrentTrack.Tab;
            trackTab.SetLength(sourceTab.Count, null);
            for (int i = 0; i < sourceTab.Count; i++)
            {
                if (sourceTab[i].Type == Enums.StepType.SectionHead)
                {
                    SectionHead sourceSectionHead = (SectionHead)sourceTab[i];
                    SectionHead newSectionHead = new SectionHead();
                    newSectionHead.Name = sourceSectionHead.Name;
                    trackTab[i] = newSectionHead;
                }
                else if (sourceTab[i].Type == Enums.StepType.Beat)
                {
                    Beat sourceBeat = (Beat)sourceTab[i];
                    Beat destBeat = (Beat)trackTab[i];
                    destBeat.BeatDivision = sourceBeat.BeatDivision;
                }
            }
            Song.Tracks.Add(newTrack);
            Song.FocusedTrack = newTrack;
            editor.Selection = new Selection(1, 1);
            editor.Refresh();
            History.PushState(Song, editor.Selection);
        }

        private void removeTrackClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => removeTrackClick();
        private void removeTrackClick()
        {
            if (SequencePlayer.PlayState == Enums.PlayState.Playing)
            {
                return;
            }
            if (Song.Tracks.Count > 1 && editor.Selection != null)
            {
                int removeIndex = Song.FocusedTrackIndex;
                if (removeIndex == Song.Tracks.Count - 1)
                {
                    Song.FocusedTrackIndex--;
                }
                Song.Tracks.RemoveAt(removeIndex);
                editor.Selection = new Selection(1, 1);
                editor.Refresh();
                History.PushState(Song, editor.Selection);
            }
        }

        private void addAnnotationTrackClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => addAnnotationTrackClick();
        private void addAnnotationTrackClick()
        {
            Track newTrack = new Track();
            newTrack.IsAnnotationTrack = true;
            newTrack.UpdateAutoName();
            Song.Tracks.Add(newTrack);
            Song.FocusedTrack = newTrack;
            editor.Selection = new Selection(1, 1);
            editor.Refresh();
            History.PushState(Song, editor.Selection);
        }

        private void trackPropertiesClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => trackPropertiesClick();
        private void trackPropertiesClick()
        {
            if (SequencePlayer.PlayState == Enums.PlayState.Playing || Song.FocusedTrack.IsAnnotationTrack)
            {
                return;
            }
            if (editor.Selection != null)
            {
                using (TrackProperties trackProperties = new TrackProperties(Song.FocusedTrack))
                {
                    trackProperties.ShowDialog();
                    if (trackProperties.ChangesMade)
                    {
                        editor.Selection = editor.Selection; // update fretboard, context menu, etc. for possible new tuning
                        editor.Refresh();
                        editor.RefreshTrackView();
                        History.PushState(Song, editor.Selection);
                    }
                }
            }
        }

        private void printTrackClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => printTrackClick();
        private void printTrackClick()
        {
            using (PrintTab printTab = new PrintTab())
            {
                printTab.Song = Song;
                printTab.FocusedTrackOnly = true;
                printTab.ShowDialog();
            }
        }

        private void exportTrackClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => exportTrackClick();
        private void exportTrackClick()
        {
            using (ExportPlainText exportPlainText = new ExportPlainText())
            {
                exportPlainText.Song = Song;
                exportPlainText.FocusedTrackOnly = true;
                exportPlainText.ShowDialog();
            }
        }
    }
}
