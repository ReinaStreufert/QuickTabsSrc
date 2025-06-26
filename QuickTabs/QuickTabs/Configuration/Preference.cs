using QuickTabs.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Configuration
{
    public abstract class Preference
    {
        public abstract string Name { get; }
        public abstract bool CanBeAddedToContextMenu { get; }
        public abstract bool SelfLabeled { get; }
        public abstract Margins Margins { get; }
        protected abstract MultiColorBitmap contextIcon { get; }

        public event Action<string> ValueChange;
        protected void notifyValueChange(string extraInfo = "")
        {
            ValueChange?.Invoke(extraInfo);
        }

        public abstract Control GenerateControl();
        public ContextItem GenerateContextItem()
        {
            if (!CanBeAddedToContextMenu)
            {
                throw new InvalidOperationException("Preference type cannot be added to context menu");
            }
            ContextItem item = new ContextItem(contextIcon, Name);
            setupContextItem(item);
            return item;
        }

        protected abstract void setupContextItem(ContextItem item);

        protected int MaxPinnableControlWidth
        {
            get
            {
                Margins margins = Margins;
                return DrawingConstants.PreferencesContentWidth - DrawingConstants.PreferencesPinButtonSize - margins.Right - SystemInformation.VerticalScrollBarWidth;
            }
        }
        protected int MaxUnpinnableControlWidth
        {
            get
            {
                Margins margins = Margins;
                return DrawingConstants.PreferencesContentWidth - margins.Right - SystemInformation.VerticalScrollBarWidth;
            }
        }
    }
}
