using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace EyePaint
{
    internal class PaintTool
    {
        //TODO Declare more fields.
        internal readonly string name;
        internal readonly Pen pen;
        internal bool drawEllipses, drawLines, drawHull;

        internal PaintTool(string name, Color color)
        {
            this.name = name;
            this.drawLines = true;
            this.drawHull = false;
            this.drawEllipses = false;
            this.pen = new Pen(Color.FromArgb(100, color), 10);
        }
    }
}
