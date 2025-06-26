using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class TwoStatePreference : Preference
    {
        private string name;
        private int persistenceKey;
        private MultiColorBitmap ctxIcon;
        private string falseState;
        private string trueState;

        public TwoStatePreference(string name, int persistenceKey, string falseState, string trueState, MultiColorBitmap ctxIcon)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.falseState = falseState;
            this.trueState = trueState;
            this.ctxIcon = ctxIcon;
        }
        public TwoStatePreference(string name, int persistenceKey, string falseState, string trueState)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.falseState = falseState;
            this.trueState = trueState;
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

        public override Control GenerateControl()
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Items.Add(falseState);
            comboBox.Items.Add(trueState);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Font = new Font(DrawingConstants.Montserrat, 9, FontStyle.Regular, GraphicsUnit.Point);
            if ((bool)QTPersistence.Current[persistenceKey])
            {
                comboBox.SelectedIndex = 1;
            } else
            {
                comboBox.SelectedIndex = 0;
            }
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
            ContextItem falseStateItem = new ContextItem(DrawingIcons.Dots, falseState);
            if ((bool)QTPersistence.Current[persistenceKey])
            {
                falseStateItem.Selected = false;
            } else
            {
                falseStateItem.Selected = true;
            }
            falseStateItem.Click += falseItemClick;
            subMenu.AddItem(falseStateItem);
            ContextItem trueStateItem = new ContextItem(DrawingIcons.Dots, trueState);
            if ((bool)QTPersistence.Current[persistenceKey])
            {
                trueStateItem.Selected = true;
            } else
            {
                trueStateItem.Selected = false;
            }
            trueStateItem.Click += trueItemClick;
            subMenu.AddItem(trueStateItem);
        }

        private void comboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex == 0)
            {
                QTPersistence.Current[persistenceKey] = false;
            } else
            {
                QTPersistence.Current[persistenceKey] = true;
            }
            QTPersistence.Current.Save();
            notifyValueChange();
        }
        private void falseItemClick(object sender, ContextItem.ContextItemClickEventArgs e)
        {
            QTPersistence.Current[persistenceKey] = false;
            QTPersistence.Current.Save();
            notifyValueChange();
        }
        private void trueItemClick(object sender, ContextItem.ContextItemClickEventArgs e)
        {
            QTPersistence.Current[persistenceKey] = true;
            QTPersistence.Current.Save();
            notifyValueChange();
        }
    }
}
