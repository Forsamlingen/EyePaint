using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EyePaint
{
    class PaintTool
    {
        static readonly Random random = new Random();
        internal readonly string name;
        internal Image icon, stamp;
        internal readonly Pen pen; // Contains settings for opacity, base color, width, etc.
        readonly List<Color> shades;
        internal readonly bool randomizeColor, drawEllipses, drawLines, drawPolygon, drawCurves, drawBeziers, drawStamps;
        internal readonly Model model;
        internal double amplitude; // [0.0..1.0]
        int a, d, r;
        Timer rise, fall;

        public PaintTool(string name, Image icon, Pen pen, Model model = Model.Cloud, bool randomizeColor = true, int shadesCount = 10, double colorVariance = 0.25, bool drawEllipses = true, bool drawLines = true, bool drawCurves = false, bool drawPolygon = false, bool drawStamps = false, Image stamp = null, int attack = 0, int decay = 0, double sustain = 1.0, int release = 100)
        {
            // Meta information
            this.name = name;
            this.icon = icon;

            // Model
            this.model = model;

            // Colors
            this.shades = getShades(pen.Color, colorVariance, shadesCount);
            this.pen = pen;
            this.randomizeColor = randomizeColor;

            // Shapes
            this.drawEllipses = drawEllipses;
            this.drawLines = drawLines;
            this.drawCurves = drawCurves;
            this.drawPolygon = drawPolygon;

            // Stamps
            this.drawStamps = drawStamps;
            this.stamp = stamp;

            // ADSR envelope
            //registerADSREnvelope(attack, decay, sustain, release);
            registerADSREnvelope(0, 0, 1.0, 0);
        }

        List<Color> getShades(Color baseColor, double colorVariance = 0.25, int shadesCount = 10)
        {
            List<Color> shades = new List<Color>();
            for (int i = 1; i <= shadesCount; ++i)
                shades.Add(Color.FromArgb(
                baseColor.A + (int)Math.Floor(colorVariance * random.Next(-baseColor.A, 255 - baseColor.A)),
                baseColor.R + (int)Math.Floor(colorVariance * random.Next(-baseColor.R, 255 - baseColor.R)),
                baseColor.G + (int)Math.Floor(colorVariance * random.Next(-baseColor.G, 255 - baseColor.G)),
                baseColor.B + (int)Math.Floor(colorVariance * random.Next(-baseColor.B, 255 - baseColor.B))
                ));
            return shades;
        }

        //TODO Use threads instead of timers. Timers lack precise enough timing.
        void registerADSREnvelope(int attack, int decay, double sustain, int release)
        {
            rise = new Timer();
            rise.Enabled = false;
            rise.Interval = 1;
            rise.Tick += (object o, EventArgs e) =>
            {
                if (++a < attack)
                {
                    amplitude += 1.0 / (double)attack;
                    return;
                }
                else if (++d < decay)
                {
                    amplitude -= (1.0 - sustain) / (double)decay;
                    return;
                }
                else
                {
                    amplitude = sustain;
                    rise.Enabled = false;
                }
            };

            fall = new Timer();
            fall.Enabled = false;
            fall.Interval = 1;
            fall.Tick += (object o, EventArgs e) =>
            {
                if (++r < release)
                {
                    amplitude -= sustain / (double)release;
                    return;
                }
                else
                {
                    amplitude = 0;
                    fall.Enabled = false;
                }
            };
        }

        public void StartPainting()
        {
            a = d = 0;
            if (randomizeColor) pen.Color = shades[random.Next(0, shades.Count() - 1)];
            rise.Enabled = true;
        }

        public void StopPainting()
        {
            r = 0;
            fall.Enabled = true;
        }
    }
}
