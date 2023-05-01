using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.UI
{
    internal class MouseTracker
    {
        private Point lastPoint;
        private bool isHovering = false;
        public Point LastPoint
        {
            get
            {
                if (!isHovering)
                {
                    throw new InvalidOperationException("Mouse is not over control");
                }
                return lastPoint;
            }
        }
        public bool IsHovering
        {
            get
            {
                return isHovering;
            }
        }
        public MouseTracker(Control control)
        {
            control.MouseMove += Control_MouseMove;
            control.MouseEnter += Control_MouseEnter;
            control.MouseLeave += Control_MouseLeave;
        }

        private void Control_MouseLeave(object? sender, EventArgs e)
        {
            isHovering = false;
        }

        private void Control_MouseEnter(object? sender, EventArgs e)
        {
            isHovering = true;
        }

        private void Control_MouseMove(object? sender, MouseEventArgs e)
        {
            isHovering = true;
            lastPoint = e.Location;
        }
    }
}
