using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace EyePaint
{
    class PaintTool
    {
        internal string name;
        internal Image icon;
        internal readonly Pen pen; // Contains settings for opacity, base color, width, etc.
        internal readonly List<Color> shades;
        internal bool drawEllipses, drawLines, drawHull, alwaysAdd; //TODO Perhaps extend the PaintTool class with specific tools per model element (i.e. CloudTool, TreeTool).

        public PaintTool(string name, Image icon, Color color)
        {
            this.name = name;
            drawLines = true;
            drawHull = false;
            drawEllipses = true;
            alwaysAdd = true;
            pen = new Pen(Color.FromArgb(100, color), 10); //TODO Set opacity and width somewhere else.
            shades = new List<Color>();
            SetShades(color);
        }

        public void SetShades(Color baseColor, int numberOfShades = 10)
        {
            //TODO Add more interesting palette generation than scaling opacity with the base color.
            for (int i = 1; i <= numberOfShades; ++i)
                shades.Add(Color.FromArgb(255 / i, baseColor));
        }
    }
}
