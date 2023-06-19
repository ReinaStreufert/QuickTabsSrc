# Saving, opening, and creating new songs

### Managing files in QuickTabs

QuickTabs allows you to save and open songs in 3 formats. You can save files using `Ctrl+S` or `File->Save`. If you are working on a song you opened, this will save it back to where you opened from. If you are working with a new never-been-saved song, it will open a file dialog. You may force the file dialog to open and save in a different location using `Ctrl+Shift+S` or `File->Save as`. You can open past saved files using `Ctrl+O` or `File->Open`.

### When to use different formats

**QuickTabs JSON Format**

Also known as `.qtjson`, this is the default save format. When in doubt, you pretty much cannot go wrong saving in this format. It is a direct representation of how QuickTabs remembers tab information serialized to json. Advantages of this format over others is it is very readable and editable from a text editor. Disadvantages include that is saves bigger files, but the files are still relatively tiny.

**QuickTabs Bytecode Format**

This is the bytecode and zlib compressed version of QuickTabs JSON Format. The extension for this format is `.qtz`. The only reason to save in this format over `.qtjson` is if you prefer very very tiny files. I have never been able to produce a `.qtz` that is larger than 1KB.

**Midi Tab Format**

This is an extension of Standard Midi File and is useful if you need to import the notes of your tab (other applications will not support the tab-specific data like tuning and which string to play on) into another application. You can also open from a non-modified midi tab file as it contains enough information to reconstruct the tab (not just a one way export.) You cannot use this format to attempt to open regular midi files saved by other applications, as normal midi files do not contain enough information to represent a tab.