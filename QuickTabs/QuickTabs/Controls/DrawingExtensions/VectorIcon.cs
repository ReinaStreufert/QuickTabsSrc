using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Controls.DrawingExtensions
{
    internal class VectorIcon
    {
        private GraphicsPath internalGraphicsPath;
        private VectorIcon(GraphicsPath graphicsPath)
        {
            internalGraphicsPath = graphicsPath;
        }
        public static VectorIcon FromSVGPathData(string PathData)
        {
            GraphicsPath gp = new GraphicsPath();
            
            return new VectorIcon(gp);
        }
        private class PathTextParser
        {
            private string pathData;
            private int currentLocation = 0;
            public PathTextParser(string PathData)
            {
                pathData = PathData;
            }

        }
        private class PathCommand
        {
            public PathCommandType Command { get; set; }
            public bool ArgumentsRelative { get; set; }
            public PointF[] Arguments { get; set; }
        }
        private enum PathCommandType
        {
            MoveTo,
            ClosePath,
            LineTo,
            HorizontalLineTo,
            VerticalLineTo,
            CubicCurveTo,
            ConnectedCubicCurveTo,
            QuadraticCurveTo,
            ConnectedQuadraticCurveTo,
            EllipticalArcTo // dont support right now ?
        }
    }
}
