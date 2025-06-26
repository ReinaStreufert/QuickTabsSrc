using QuickTabs.Forms;
using QuickTabs.Songwriting;
using QuickTabs.Synthesization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace QuickTabs.Controls
{
    public partial class QuickTabsContextMenu : ContextMenu
    {
        private ContextSection playbackSection;
        private ContextItem playPause;
        private ContextItem repeat;
        private ContextItem metronome;

        private void setupPlaybackSection()
        {
            playbackSection = new ContextSection();
            playbackSection.SectionName = "Player";
            if (AudioEngine.Enabled)
            {
                setupEnabledPlaybackSection();
            }
            else
            {
                playbackSection.ToggleType = ToggleType.NotTogglable;
                ContextItem downloadAsio = new ContextItem(DrawingIcons.Download, "Install ASIO driver...");
                downloadAsio.Selected = true;
                downloadAsio.Click += downloadAsioClick;
                playbackSection.AddItem(downloadAsio);
                ContextItem recheckAsio = new ContextItem(DrawingIcons.Reload, "Check for driver again...");
                recheckAsio.Selected = true;
                recheckAsio.Click += recheckAsioClick;
                playbackSection.AddItem(recheckAsio);

                AsioDownloader.DownloadFailed += AsioDownloader_DownloadFailed;
            }
            Sections.Add(playbackSection);
        }
        private void setupEnabledPlaybackSection()
        {
            playbackSection.ToggleType = ToggleType.Togglable;
            playPause = new ContextItem(DrawingIcons.PlayPause, "Play/pause");
            playPause.Selected = false;
            playPause.Click += playPauseClick;
            ShortcutManager.AddShortcut(Keys.None, Keys.Space, playPauseClick);
            playbackSection.AddItem(playPause);
            repeat = new ContextItem(DrawingIcons.Repeat, "Repeat");
            repeat.Selected = false;
            repeat.Click += repeatClick;
            playbackSection.AddItem(repeat);
            metronome = new ContextItem(DrawingIcons.Metronome, "Metronome");
            metronome.Selected = false;
            metronome.Click += metronomeClick;
            playbackSection.AddItem(metronome);
            ShortcutManager.AddShortcut(Keys.None, Keys.X, silencePressed);
        }

        private void playPauseClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => playPauseClick();
        private void playPauseClick()
        {
            if (SequencePlayer.PlayState == Enums.PlayState.Playing)
            {
                SequencePlayer.Stop();
            } else if (SequencePlayer.PlayState == Enums.PlayState.NotPlaying)
            {
                MusicalTimespan startPosition = MusicalTimespan.Zero;
                if (editor.Selection != null)
                {
                    startPosition = Song.FocusedTab.FindIndexTime(editor.Selection.SelectionStart);
                }
                SequencePlayer.Source = Song.Tracks;
                SequencePlayer.PlayFrom(startPosition);
                editor.PlayMode = true;
            }
            UpdateAvailableContent();
        }
        private void repeatClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => repeatClick();
        private void repeatClick()
        {
            SequencePlayer.Loop = !SequencePlayer.Loop;
        }
        private void metronomeClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => metronomeClick();
        private void metronomeClick()
        {
            SequencePlayer.MetronomeEnabled = !SequencePlayer.MetronomeEnabled;
        }
        private void silencePressed()
        {
            if (SequencePlayer.PlayState == Enums.PlayState.NotPlaying)
            {
                AudioEngine.AudioEngineTick eventHandler = null;
                eventHandler = (DateTime timestamp, float bufferDurationMS) =>
                {
                    AudioEngine.SilenceAll();
                    AudioEngine.Tick -= eventHandler;
                };
                AudioEngine.Tick += eventHandler;
                // because threads
            }
        }
        private void downloadAsioClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => downloadAsioClick();
        private void downloadAsioClick()
        {
            AsioDownloader.DownloadAndInstall();
        }
        private void recheckAsioClick(ContextItem sender, ContextItem.ContextItemClickEventArgs e) => recheckAsioClick();
        private void recheckAsioClick()
        {
            this.Cursor = Cursors.WaitCursor;
            AudioEngine.Initialize();
            this.Cursor = Cursors.Default;
            if (AudioEngine.Enabled)
            {
                playbackSection.ClearItems();
                setupEnabledPlaybackSection();
                updateUI();
                Invalidate();
            } else
            {
                using (GenericMessage genericMessage = new GenericMessage())
                {
                    genericMessage.Text = "ASIO not supported";
                    genericMessage.Message = "ASIO support was still not detected on your device. If you have installed ASIO4ALL, try restarting.";
                    genericMessage.ShowDialog();
                }
            }
        }
        private void AsioDownloader_DownloadFailed()
        {
            this.Invoke(() =>
            {
                using (GenericMessage genericMessage = new GenericMessage())
                {
                    genericMessage.Text = "ASIO4ALL auto download";
                    genericMessage.Message = "Could not automatically download ASIO4ALL installer. You will have to find it on the ASIO4ALL website.";
                    genericMessage.ShowDialog();
                }
            });
        }
    }
}
