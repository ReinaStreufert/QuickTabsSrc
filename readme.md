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

