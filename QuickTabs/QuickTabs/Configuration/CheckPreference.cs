using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class CheckPreference : Preference
    {
        private string name;
        private int persistenceKey;
        private MultiColorBitmap ctxIcon;

        public CheckPreference(string name, int persistenceKey, MultiColorBitmap ctxIcon)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.ctxIcon = ctxIcon;
        }
        public CheckPreference(string name, int persistenceKey)
        {
            this.name = name;
            this.persistenceKey = persistenceKey;
            this.ctxIcon = null;
        }

        public override string Name => name;
        public override bool CanBeAddedToContextMenu => (ctxIcon != null);
        public override bool SelfLabeled => true;
        public override Margins Margins => new Margins(0, 0, 0, 0);
        protected override MultiColorBitmap contextIcon => ctxIcon;

        public override Control GenerateControl()
        {
            CheckBox checkBox = new CheckBox();
            checkBox.BackColor = DrawingConstants.UIAreaBackColor;
            checkBox.ForeColor = DrawingConstants.ContrastColor;
            checkBox.Font = new Font(DrawingConstants.Montserrat, 9, FontStyle.Regular, GraphicsUnit.Point);
            checkBox.Checked = (bool)QTPersistence.Current[persistenceKey];
            checkBox.Text = Name;
            checkBox.AutoSize = true;
            checkBox.CheckedChanged += checkBox_CheckChanged;
            return checkBox;
        }

        protected override void setupContextItem(ContextItem item)
        {
            item.Selected = (bool)QTPersistence.Current[persistenceKey];
            item.Click += contextItemClick;
        }

        private void checkBox_CheckChanged(object sender, EventArgs e)
        {
            QTPersistence.Current[persistenceKey] = ((CheckBox)sender).Checked;
            QTPersistence.Current.Save();
            notifyValueChange();
        }
        private void contextItemClick(object sender, ContextItem.ContextItemClickEventArgs e)
        {
            QTPersistence.Current[persistenceKey] = !(bool)(QTPersistence.Current[persistenceKey]);
            QTPersistence.Current.Save();
            notifyValueChange();
        }
    }
}
