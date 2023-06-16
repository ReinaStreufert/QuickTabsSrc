# Manipulating the tab

### Selection

Click on any step to select it, or click and drag to select multiple steps. You may also use the `A` and `D` keys to select the step directly to the left or right of your current selection. `Shift+A` or `Shift+D` widens your selection to the left or right.

### Modifying a step

The main way to modify a step is using the fretboard at the bottom of the window. Here you may set which strings are playing and which fret is held on each playing string, as well as the sustain time for the step. Detailed info on how to use the fretboard can be found in [/docs/Fretboard](/docs/Fretboard.md). You can also transform a step or multiple steps using the operations in the `Selection` section of the context menu.

### Adding/removing measures

You may add and remove measures using either `Measure->Add measure` and `Measure->Remove measure` or with the plus and minus keys. Add measure will insert a new measure between the next one and the current one, while remove will delete the current measure.

### Sections

Sections created and removed through `Measure->Add or split section` and `Measure->Collate section` or with `Shift+Plus` and `Shift+Minus`. If your selection starts in the last measure of the tab, add/split section will create a new section with one new measure at the end of the tab. Otherwise, add/split section will split the current section into two, breaking in between this measure and the next. To use collate section, you must first select the first step in a section. Collate section will join the current section to the end of the previous section. You can also rename a section using `Measure->Rename section`