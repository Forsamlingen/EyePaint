using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyePaint
{
    public struct Tool
    {
        public int BranchCount { get; set; }
        public double BranchLength { get; set; }
        public double BranchStraightness { get; set; }
        public double Rotation { get; set; }
        public double ColorVariety { get; set; }
        public double CenterOpacity { get; set; }
        public double EdgesOpacity { get; set; }
        public double VerticesOpacity { get; set; }
        public double HullOpacity { get; set; }
        public double CenterSize { get; set; }
        public double EdgesThickness { get; set; }
        public double VerticesSize { get; set; }
        public double VerticesSquash { get; set; }
    }

    // Presets
    // TODO Don't initialize new objects every access.
    public class Tools
    {
        public static Tool Splatter
        {
            get
            {
                return new Tool
                {
                    BranchCount = 10,
                    BranchLength = 20,
                    BranchStraightness = 0.5,
                    Rotation = 0.5,
                    ColorVariety = 0.5,
                    CenterOpacity = 0,
                    EdgesOpacity = 0,
                    VerticesOpacity = 1,
                    HullOpacity = 0,
                    CenterSize = 0,
                    EdgesThickness = 0,
                    VerticesSize = 20,
                    VerticesSquash = 0,
                };
            }
        }

        public static Tool Flower
        {
            get
            {
                return new Tool
                {
                    BranchCount = 50,
                    BranchLength = 15,
                    BranchStraightness = 1,
                    Rotation = 1,
                    ColorVariety = 0.25,
                    CenterOpacity = 0.1,
                    EdgesOpacity = 1,
                    VerticesOpacity = 0.75,
                    HullOpacity = 0,
                    CenterSize = 25,
                    EdgesThickness = 2,
                    VerticesSize = 5,
                    VerticesSquash = 0,
                };
            }
        }

        public static Tool Neuron
        {
            get
            {
                return new Tool
                {
                    BranchCount = 10,
                    BranchLength = 50,
                    BranchStraightness = 0.8,
                    Rotation = 0,
                    ColorVariety = 0,
                    CenterOpacity = 0.01,
                    EdgesOpacity = 1,
                    VerticesOpacity = 0,
                    HullOpacity = 0,
                    CenterSize = 100,
                    EdgesThickness = 2,
                    VerticesSize = 0,
                    VerticesSquash = 0,
                };
            }
        }

        public static Tool Circle
        {
            get
            {
                return new Tool
                {
                    BranchCount = 100,
                    BranchLength = 10,
                    BranchStraightness = 1,
                    Rotation = 0.1,
                    ColorVariety = 0.25,
                    CenterOpacity = 0.25,
                    EdgesOpacity = 0.75,
                    VerticesOpacity = 0,
                    HullOpacity = 0,
                    CenterSize = 10,
                    EdgesThickness = 10,
                    VerticesSize = 0,
                    VerticesSquash = 0,
                };
            }
        }

        public static Tool Polygon
        {
            get
            {
                return new Tool
                {
                    BranchCount = 10,
                    BranchLength = 10,
                    BranchStraightness = 0.9,
                    Rotation = 0.25,
                    ColorVariety = 0.5,
                    CenterOpacity = 0,
                    EdgesOpacity = 0,
                    VerticesOpacity = 0,
                    HullOpacity = 0.1,
                    CenterSize = 0,
                    EdgesThickness = 0,
                    VerticesSize = 0,
                    VerticesSquash = 0,
                };
            }
        }

        public static Tool Snowflake
        {
            get
            {
                return new Tool
                {
                    BranchCount = 100,
                    BranchLength = 25,
                    BranchStraightness = 1,
                    Rotation = 1,
                    ColorVariety = 0,
                    CenterOpacity = 0,
                    EdgesOpacity = 1,
                    VerticesOpacity = 0,
                    HullOpacity = 0,
                    CenterSize = 0,
                    EdgesThickness = 1,
                    VerticesSize = 0,
                    VerticesSquash = 0,
                };
            }
        }
    }
}

