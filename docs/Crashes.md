# Crashes

### Crash handling in QuickTabs

In the (hopefully) unlikely event that an unhandled exception occurs, QuickTabs will attempt to fail gracefully. First it will gather info about the exception to log it, then it will attempt to check if you have unsaved data in the editor. If so, QuickTabs will try to include the unsaved work in the log so it can be recovered. Finally, it will attempt to restart QuickTabs as it exits. On restart, QuickTabs will display the error information and offer to recover unsaved work if available.

### Crash logs

All crash logs will end up in `[System drive]:\Users\[You]\AppData\Local\QuickTabs\crash-reports\logged` once they have been shown to the user by QuickTabs. Still "active" crash logs (ones that the user has not seen by reopening QuickTabs) are kept in the parent directory. Crash logs are named by the date and time that they occurred (`month-day-year-hour(24)-minute-second`).