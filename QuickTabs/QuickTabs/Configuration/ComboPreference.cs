using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class ComboPreference : Preference
    {
        private string name;
        private int persistenceKey;
        private string[] options;
        private int persistenceAdditive;
        private MultiColorBitmap ctxIcon;

        public ComboPreference(string name, int persistenceKey, string[] options, int persistenceAdditive = 0)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.options = options;
            this.persistenceAdditive = persistenceAdditive;
            this.ctxIcon = null;
        }
        public ComboPreference(string name, int persistenceKey, string[] options, MultiColorBitmap ctxIcon, int persistenceAdditive = 0)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.options = options;
            this.ctxIcon = ctxIcon;
            this.persistenceAdditive = persistenceAdditive;
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

        public override Control GenerateControl()
        {
            ComboBox comboBox = new ComboBox();
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Font = new Font(DrawingConstants.Montserrat, 9, FontStyle.Regular, GraphicsUnit.Point);
            foreach (string s in options)
            {
                comboBox.Items.Add(s);
            }
            long currentVal = (long)QTPersistence.Current.AsioOutputDevice - persistenceAdditive;
            comboBox.SelectedIndex = (int)currentVal;
            int width;
            if (ctxIcon != null)
            {
                width = MaxPinnableControlWidth;
            } else
            {
                width = MaxUnpinnableControlWidth;
            }
            comboBox.Width = width;
            comboBox.SelectedIndexChanged += comboBox_SelectedIndexChanged;
            return comboBox;
        }
        protected override void setupContextItem(ContextItem item)
        {
            item.Selected = true;
            item.ExcludeFromToggle = true;
            item.DontCloseDropdown = true;
            ContextSection subMenu = new ContextSection();
            item.Submenu = subMenu;

            subMenu.ToggleType = ToggleType.Radio;
            for (int i = 0; i < options.Length; i++)
            {
                string s = options[i];
                ContextItem ctxItem = new ContextItem(DrawingIcons.Dots, s);
                long currentVal = (long)QTPersistence.Current.AsioOutputDevice - persistenceAdditive;
                ctxItem.Selected = ((int)currentVal == i);
                int ctxClickI = i;
                ctxItem.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) => { contextItemIndexChange(ctxClickI); };
                subMenu.AddItem(ctxItem);
            }
        }

        private void comboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            long newVal = comboBox.SelectedIndex + persistenceAdditive;
            QTPersistence.Current[persistenceKey] = newVal;
            QTPersistence.Current.Save();
            notifyValueChange();
        }
        private void contextItemIndexChange(int index)
        {
            long newVal = index + persistenceAdditive;
            QTPersistence.Current[persistenceKey] = newVal;
            QTPersistence.Current.Save();
            notifyValueChange();
        }
    }
}
