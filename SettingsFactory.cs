using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    internal enum PaintToolType { TREE }

    internal class SettingsFactory
    {
        internal SettingsFactory()
        {
        }


        // Return available paintTools
        internal List<PaintTool> getPaintTools()
        {
            //TODO Load paint tools from a data store instead.
            List<PaintTool> paintTools = new List<PaintTool>();
            TreeTool woolTool = new TreeTool(0,
                                        "woolPaint",
                                        stringToToolType("TREE"),
                                        "",
                                        "WoolTree", 2, 500, 800, 25, 2, 0, 0);
            TreeTool polyTool = new TreeTool(1,
                                        "polyPaint",
                                        stringToToolType("TREE"),
                                        "",
                                        "PolyTree", 25, 7, 800, 25, 2, 0, 0);
            TreeTool modernArtTool = new TreeTool(2,
                                        "modernArtPaint",
                                        stringToToolType("TREE"),
                                        "",
                                        "ModernArtTree", 5, 4, 800, 100, 2, 5, 0);
            TreeTool cellNetTool = new TreeTool(3,
                                        "cellNetPaint",
                                        stringToToolType("TREE"),
                                        "",
                                        "CellNetTree", 25, 300, 800, 25, 2, 5, 0);
            paintTools.Add(woolTool);
            paintTools.Add(polyTool);
            paintTools.Add(modernArtTool);
            paintTools.Add(cellNetTool);
            return paintTools;
        }
        PaintToolType stringToToolType(string type)
        {
            switch (type)
            {

                case "TREE":
                    return PaintToolType.TREE;
                default:
                    throw new System.ArgumentException(type + " is an invalid tool PaintToolType");
            }
        }

        // Return available color tools    
        internal List<ColorTool> getColorTools()
        {
            List<ColorTool> colorTools = new List<ColorTool>();

            //TODO Load color tools from a data store instead.
            colorTools.Add(new ColorTool(1, "random", Color.Red, 0, 360, 0.9, 1, 0.5, 1));
            colorTools.Add(new ColorTool(1, "random", Color.Blue, 0, 360, 0.9, 1, 0.5, 1));
            colorTools.Add(new ColorTool(1, "random", Color.Yellow, 0, 360, 0.9, 1, 0.5, 1));
            colorTools.Add(new ColorTool(1, "random", Color.Green, 0, 360, 0.9, 1, 0.5, 1));

            return colorTools;
        }
    }

    internal class PaintTool
    {
        internal readonly int id;
        internal readonly string name; //Todo maybe ´skipp this property 
        internal readonly PaintToolType type;
        internal string iconImage; //Todo maybe skipp this property
        internal string renderObjectName;
        internal PaintTool(int id, string name, PaintToolType type, string pathToIconImage, string renderObjectName)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.iconImage = pathToIconImage;
            this.renderObjectName = renderObjectName;
        }
    }

    internal class TreeTool : PaintTool
    {
        internal readonly int branchLength;
        internal readonly int nLeaves;
        internal readonly int maxGeneration;
        internal readonly int opacity;
        internal readonly int branchWidth;
        internal readonly int hullWidth;
        internal readonly int leafSize;
        internal TreeTool(int id, string name,
                        PaintToolType type,
                        string pathToIconImage,
                        string renderObjectName,
                        int branchLength,
                        int nLeaves,
                        int maxGeneration,
                        int opacity,
                        int branchWidth,
                        int hullWidth,
                        int leafSize)
            : base(id, name, type, pathToIconImage, renderObjectName)
        {
            this.branchLength = branchLength;
            this.nLeaves = nLeaves;
            this.maxGeneration = maxGeneration;
            this.opacity = opacity;
            this.branchWidth = branchWidth;
            this.hullWidth = hullWidth;
            this.leafSize = leafSize;
        }
    }


    internal class ColorTool
    {
        internal readonly int id;
        internal readonly String name;
        internal readonly Color baseColor;
        readonly double minHue;
        readonly double maxHue;
        readonly double minSaturation;
        readonly double maxSaturation;
        readonly double minValue;
        readonly double maxValue;

        static Random rng = new Random();

        internal ColorTool(int id, string name,
                           Color defaultColor,
                           double minHue,
                           double maxHue,
                           double minSaturation,
                           double maxSaturation,
                           double minValue,
                           double maxValue
                        )
        {
            this.id = id;
            this.name = name;
            this.baseColor = defaultColor;
            this.minHue = minHue;
            this.maxHue = maxHue;
            this.minSaturation = minSaturation;
            this.maxSaturation = maxSaturation;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public Color getRandomShade(int opacity)
        {
            double randomHue = minHue + (rng.NextDouble() * (maxHue - minHue));
            double randomSaturation = minSaturation + (rng.NextDouble() * (maxSaturation - minSaturation));
            double randomValue = minValue + (rng.NextDouble() * (maxValue - minValue));
            Color c = ColorFromHSV(opacity, randomHue, randomSaturation, randomValue);
            return c;
        }


        Color ColorFromHSV(int opacity, double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(opacity, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(opacity, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(opacity, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(opacity, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(opacity, t, p, v);
            else
                return Color.FromArgb(opacity, v, p, q);
        }
    }
}





