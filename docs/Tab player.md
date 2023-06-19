# Tab player

### About the tab player

The tab player lets you hear the tab you're editing to make sure the notes and timing are correct. It will also play individual notes when you place them on the fretboard, as well as all notes in the selected step when you click on a step or set the note length. The tab player uses ASIO to play low latency audio on your device. Many sound cards natively support ASIO, but some do not, in which case you'll need the ASIO4ALL driver to use the audio features of QuickTabs. See the ASIO4ALL section below.

### ASIO4ALL

If you need the ASIO4ALL driver and do not have it, QuickTabs will detect this and change the available options in the `Player` section of the context menu. If your `Player` section contains the normal `Play/pause`, `Loop`, and `Metronome` options then you do not need to do anything and the player is ready to use. If it contains `Install ASIO driver` and `Check for driver again`, you will need to select `Player->Install ASIO driver` before QuickTabs will be able to play audio. This will automatically download and open the ASIO4ALL installer. Once you have finished installing ASIO4ALL through the installer, use `Player->Check for driver again` to enable audio in QuickTabs.

### Using tab player

You may start playing from the currently selected beat using `Space` or `Player->Play/pause`. The player also allows you to enable and disable a metronome as well as loop mode through the `Player->Loop` and `Player->Metronome` context options. 