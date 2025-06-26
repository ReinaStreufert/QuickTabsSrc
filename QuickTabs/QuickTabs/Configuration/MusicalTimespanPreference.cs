using QuickTabs.Controls;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class MusicalTimespanPreference : Preference
    {
        private string name;
        private int persistenceKey;
        private MultiColorBitmap ctxIcon;

        public MusicalTimespanPreference(string name, int persistenceKey, MultiColorBitmap ctxIcon)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.ctxIcon = ctxIcon;
        }
        public MusicalTimespanPreference(string name, int persistenceKey)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.ctxIcon = null;
        }

        public override string Name => name;
        public override bool CanBeAddedToContextMenu => (ctxIcon != null);
        public override bool SelfLabeled => false;
        public override Margins Margins
        {
            get
            {
                int pad = (int)(10 * DrawingConstants.CurrentScale);
                return new Margins(0, pad, pad, pad);
            }
        }
        protected override MultiColorBitmap contextIcon => ctxIcon;

        private MusicalTimespan getCurrentValue()
        {
            long serialized = (long)QTPersistence.Current[persistenceKey]; // for some reason Newtonsonft.JSON makes all integers int 64s after they have been saved then reloaded?
            int serialized32 = Convert.ToInt32(serialized);
            return MusicalTimespan.DeserializeInt32(serialized32);
        }

        public override Control GenerateControl()
        {
            int maxControlWidth;
            if (ctxIcon != null)
            {
                maxControlWidth = MaxPinnableControlWidth;
            } else
            {
                maxControlWidth = MaxUnpinnableControlWidth;
            }
            NoteLengthTableRow[] noteLengthValueTable = DrawingIcons.NoteLengthValueTable;
            IconPicker notePicker = new IconPicker();
            notePicker.Height = maxControlWidth / noteLengthValueTable.Length;
            MusicalTimespan prefValue = getCurrentValue();
            for (int i = 0; i < noteLengthValueTable.Length; i++)
            {
                NoteLengthTableRow noteLengthInfo = noteLengthValueTable[i];
                notePicker.Icons.Add(noteLengthInfo.NoteLengthIcon);
                if (prefValue == noteLengthInfo.Timespan)
                {
                    notePicker.SelectedIndex = i;
                }
            }
            notePicker.SelectedIndexChanged += notePicker_SelectedIndexChanged;
            return notePicker;
        }
        protected override void setupContextItem(ContextItem item)
        {
            item.Selected = true;
            item.ExcludeFromToggle = true;
            item.DontCloseDropdown = true;
            ContextSection subMenu = new ContextSection();
            item.Submenu = subMenu;

            subMenu.ToggleType = ToggleType.Radio;
            NoteLengthTableRow[] noteLengthValueTable = DrawingIcons.NoteLengthValueTable;
            MusicalTimespan prefValue = getCurrentValue();
            for (int i = 0; i < noteLengthValueTable.Length; i++)
            {
                NoteLengthTableRow noteLengthInfo = noteLengthValueTable[i];
                ContextItem subItem = new ContextItem(noteLengthInfo.NoteLengthIcon, noteLengthInfo.FullName);
                subItem.Selected = (prefValue == noteLengthInfo.Timespan);
                subItem.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) => { contextValueChange(noteLengthInfo.Timespan); };
                subMenu.AddItem(subItem);
            }
        }

        private void notePicker_SelectedIndexChanged(object? sender, EventArgs e)
        {
            IconPicker notePicker = (IconPicker)sender;
            QTPersistence.Current[persistenceKey] = (long/*why*/)DrawingIcons.NoteLengthValueTable[notePicker.SelectedIndex].Timespan.SerializeToInt32();
            QTPersistence.Current.Save();
            notifyValueChange();
        }
        private void contextValueChange(MusicalTimespan newValue)
        {
            QTPersistence.Current[persistenceKey] = (long/*why*/)newValue.SerializeToInt32();
            QTPersistence.Current.Save();
            notifyValueChange();
        }
    }
}
