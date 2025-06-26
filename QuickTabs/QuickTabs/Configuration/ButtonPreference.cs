using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public class ButtonPreference : Preference
    {
        private string name;
        private MultiColorBitmap ctxIcon;

        public ButtonPreference(string name, MultiColorBitmap ctxIcon = null)
        {
            this.name = name;
            this.ctxIcon = ctxIcon;
        }

        public override string Name => name;
        public override bool CanBeAddedToContextMenu => (ctxIcon != null);
        public override bool SelfLabeled => true;
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
            Button button = new Button();
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.DodgerBlue;
            button.ForeColor = Color.White;
            button.Font = new Font(DrawingConstants.Montserrat, 9, FontStyle.Regular, GraphicsUnit.Point);
            int width;
            if (ctxIcon != null)
            {
                width = MaxPinnableControlWidth;
            } else
            {
                width = MaxUnpinnableControlWidth;
            }
            button.Size = new Size(width, DrawingConstants.SafeButtonHeight);
            button.Text = name;
            button.Click += (object sender, EventArgs e) => { notifyValueChange(); };
            return button;
        }
        protected override void setupContextItem(ContextItem item)
        {
            item.Selected = true;
            item.ExcludeFromToggle = true;
            item.Click += (ContextItem sender, ContextItem.ContextItemClickEventArgs e) => { notifyValueChange(); };
        }
    }
}
