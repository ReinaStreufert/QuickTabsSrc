using QuickTabs.Synthesization;
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
        private ContextSection playbackSection;
        private Synthesization.TabPlayer tabPlayer = null;

        private void setupPlaybackSection()
        {
            playbackSection = new ContextSection();
            playbackSection.SectionName = "Player";
            if (AudioEngine.Enabled)
            {
                playbackSection.ToggleType = ToggleType.Togglable;
                playPause = new ContextItem(DrawingIcons.PlayPause, "Play/pause");
                playPause.Selected = false;
                playPause.Click += () => { playPauseClick(false); };
                ShortcutManager.AddShortcut(Keys.None, Keys.Space, () => { playPauseClick(true); });
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
            else
            {
                playbackSection.ToggleType = ToggleType.NotTogglable;
                ContextItem downloadAsio = new ContextItem(DrawingIcons.Download, "Install ASIO driver...");
                downloadAsio.Selected = true;
                playbackSection.AddItem(downloadAsio);
                ContextItem recheckAsio = new ContextItem(DrawingIcons.Reload, "Check for driver again...");
                recheckAsio.Selected = true;
                playbackSection.AddItem(recheckAsio);
            }
            Sections.Add(playbackSection);
        }
        private void playPauseClick(bool fromShortcut) // this whole fromShortcut bullshit is because if you click the button directly, ContextMenu will already toggle it after this gets called. but if this is called from the shortcut, it will not.
        {
            if (tabPlayer == null || !tabPlayer.IsPlaying)
            {
                tabPlayer = new Synthesization.TabPlayer(Song.Tab);
                tabPlayer.BPM = Song.Tempo;
                tabPlayer.Loop = repeat.Selected;
                tabPlayer.MetronomeTimeSignature = Song.TimeSignature;
                tabPlayer.Metronome = metronome.Selected;
                if (editor.Selection == null)
                {
                    tabPlayer.Position = 1;
                }
                else
                {
                    tabPlayer.Position = editor.Selection.SelectionStart;
                }
                editor.PlayMode = true;
                tabPlayer.Start();
                selectionChanged(); // update measure and selection section availability
                Timer t = new Timer();
                t.Interval = 50;
                t.Tick += (object sender, EventArgs e) =>
                {
                    if (tabPlayer.IsPlaying)
                    {
                        int playerPosition = tabPlayer.Position;
                        if (editor.Selection == null || editor.Selection.SelectionStart != playerPosition)
                        {
                            editor.PlayCursor = tabPlayer.Position;
                            editor.Refresh();
                        }
                    }
                    else
                    {
                        if (playPause.Selected)
                        {
                            playPause.Selected = false;
                        }
                        selectionChanged(); // update measure and selection section availability
                        if (editor.PlayMode)
                        {
                            editor.PlayMode = false;
                            editor.Invalidate();
                        }
                        t.Stop();
                        t.Dispose();
                        History.PushState(Song, editor.Selection, false); // pushing an insignificant event because selection will be different once playing stops
                    }
                };
                t.Start();
                if (fromShortcut && !playPause.Selected)
                {
                    playPause.Selected = true;
                    this.Invalidate();
                }
            }
            else
            {
                tabPlayer.Stop();
                if (fromShortcut && playPause.Selected)
                {
                    playPause.Selected = false;
                    this.Invalidate();
                }
            }
        }
        private void repeatClick()
        {
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Loop = !tabPlayer.Loop;
            }
        }
        private void metronomeClick()
        {
            if (tabPlayer != null && tabPlayer.IsPlaying)
            {
                tabPlayer.Metronome = !tabPlayer.Metronome;
            }
        }
        private void silencePressed()
        {
            if (tabPlayer == null || !tabPlayer.IsPlaying)
            {
                Action eventHandler = null;
                eventHandler = () =>
                {
                    AudioEngine.SilenceAll();
                    AudioEngine.Tick -= eventHandler;
                };
                AudioEngine.Tick += eventHandler;
                // because threads
            }
        }
    }
}
