# About QuickTabs

QuickTabs is an open-source guitar (or other guitar-like instrument) tab editor. As a songwriter with ADHD I found trying to jot down tabs quickly while the idea is fresh in my mind with a damn *text editor* to be next to impossible. I wrote QuickTabs as a solution to this. It is still in beta and currently limited to eighth notes as the smallest division, but it is still relatively featureful even including a synthesizer-based tab player. QuickTabs was written in C# and as of now is unfortanately only for Windows.

# Features

* Edit tabs by selecting eighth-note spaces on the tab, then manipulating it on the visual fretboard representation shown at the bottom.
* The length of the notes in the space can also be changed from the fretboard.
* Notes will play as you edit them so you can audibly confirm they are correct.
* QuickTabs includes a player that can play parts of your track or the entire track, so you can hear that the timing is all correct.
* Tabs may be split into clearly labelled sections ("Chorus", "Verse, "Whatever")
* Selection operations such as copy/paste, shift beats left and right, etc.
* Song name, BPM (tap tempo included), tuning, and time signature may be set and all features will adjust accordingly.
* Editing history (undo and redo)
* Export tabs as plain text in either standard tab style which doesn't allow for note lengths, or QuickTabs style which does. There are options to automatically include the metadata components you want at the top.
* Print tabs as they appear in the editor. This also enables exporting as PDF.
* Save and open in 3 formats:
    * `.qtjson`: Format i developed based on json to represent QuickTabs tabs.
    * `.qtz`: Bytecode version of `.qtjson`. Contrast: much much smaller files than json-based, but not hand readable or editable at all.
    * `.mid`: Midi tab format. This is an extension of Standard Midi File I developed which includes tab-specific info but is fully compatible with any generic midi supporting application. This compatibility only goes one way; Normal midi files do not have enough information to represent a string instrument tab.
* Automatically stays up to date to latest version.
* Basic view preference options (with persistence including over updates.)
* Probably some more stuff that im forgetting about.

Detailed information on all of these features can be found in [/docs/](/docs/)

# Prebuilt installer

To get started quickly you may download QuickTabs prebuilt [here](dead). On open QuickTabs will detect if it has not been installed on your machine and open to an installer if so. The installer will move QuickTabs to a permanent install directory, download its dependencies, and create start menu shortcuts if desired.

# Future plans

* Allow for beat divisions down to the 32nd note.
* Make note lengths per individual note instead of per each entire beat.
* Parallel track support (abilitity to notate multiple same-length tabs to be played at once, with individually set tuning. Player will support this change)
* Lyric notation
* Slide and hammer notation

# Why Windows Forms ??

Because I did not originally plan on sharing this project with anyone, but it got too useful to keep to myself. I had a lot of previous experience in Windows Forms and I like writing my own layout code much better than trying to fight with something like CSS. In the future when I get tired enough of GDI+'s poor performance I may choose to implement my own layout system with Direct2D using SharpDX and move away from winforms. Or I may explore cross-platform options. I am not sure yet and this is likely very in the future because it will be quite a bit of work.
