using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class ButtonListPreference : Preference
    {
        private string name;
        private string[] items;

        public ButtonListPreference(string name, string[] items)
        {
            this.name = name;
            this.items = items;
        }

        public override string Name => name;
        public override bool CanBeAddedToContextMenu => false;
        public override bool SelfLabeled => false;
        public override Margins Margins
        {
            get
            {
                int pad = (int)(10 * DrawingConstants.CurrentScale);
                return new Margins(0, pad, pad, pad);
            }
        }
        protected override MultiColorBitmap contextIcon => throw new InvalidOperationException();

        public override Control GenerateControl()
        {
            ButtonList buttonList = new ButtonList(items);
            buttonList.Size = new Size(MaxUnpinnableControlWidth, DrawingConstants.SafeButtonHeight * 4);
            buttonList.ButtonClick += buttonList_ButtonClick;
            return buttonList;
        }

        private void buttonList_ButtonClick(object sender, ButtonListClickEventArgs e)
        {
            notifyValueChange(e.ButtonIndex.ToString());
        }

        protected override void setupContextItem(ContextItem item)
        {
            throw new InvalidOperationException();
        }
    }
}
