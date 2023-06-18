# History

### About history

History lets you easily undo mistakes, or freely experiment without having to worry about losing stuff. History also allows QuickTabs to know when you are about to throw away unsaved changes so it can warn you. History logs the current state every time you change anything about the song you're currently editing and allows you to easily move back and forth through it.

### Undo

Undo can be accessed through `Ctrl+Z` or `History->Undo`. It will go back in time by one change.

### Redo

You can use `Ctrl+Y` or `History->Redo` to go forward one state, aka undo the undo.

### Alternate redo

Alternate redo (`Ctrl+Shift+Y`, `History->Alternate redo`) is slightly difficult to understand. Alternate redo is only available when you go back in time, then start editing (creating a second future), and then go back again to the point at which there are two possible futures. Using regular redo at this point will redo the changes you most recently made where you created a new future. Alternate redo was created so you could still get back to the original timeline that you undid, so you can redo it from there. If this doesn't make much sense, don't worry, you do not need to use this feature.