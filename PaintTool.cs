using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EyePaint
{
    class PaintTool : IDisposable
    {
        internal string name;
        internal Image icon;
        internal readonly Pen pen; // Contains settings for opacity, base color, width, etc.
        internal readonly List<Color> shades;
        internal bool drawEllipses, drawLines, drawPolygon, drawCurves, drawStamps;

        // ADSR envelope
        internal double amplitude; // [0.0..1.0]
        int a, d, r;
        Timer rise, fall;
        public bool done;

        public PaintTool(string name, Image icon, Color color)
        {
            this.name = name;

            // Shapes
            drawLines = true;
            drawPolygon = false;
            drawEllipses = false;
            drawCurves = false;
            drawStamps = false; //TODO Implement.

            // Colors
            var c = Color.FromArgb(50, color);
            pen = new Pen(c, 1); //TODO Set default opacity and width somewhere else.
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            shades = GetShades(c);

            // ADSR envelope
            registerADSREnvelope(0, 0, 1, 0);
        }

        public List<Color> GetShades(Color baseColor, int numberOfShades = 10)
        {
            List<Color> shades = new List<Color>();
            Random random = new Random(); //TODO Don't allocate on each call.
            double offset = 0.5; //TODO Make into a parameter.
            for (int i = 1; i <= numberOfShades; ++i)
                shades.Add(Color.FromArgb(
                baseColor.A + (int)Math.Floor(offset * random.Next(-baseColor.A, 255 - baseColor.A)),
                baseColor.R + (int)Math.Floor(offset * random.Next(-baseColor.R, 255 - baseColor.R)),
                baseColor.G + (int)Math.Floor(offset * random.Next(-baseColor.G, 255 - baseColor.G)),
                baseColor.B + (int)Math.Floor(offset * random.Next(-baseColor.B, 255 - baseColor.B))
                ));
            return shades;
        }

        public void RandomShade()
        {
            Random random = new Random(); //TODO Don't allocate on each call.
            pen.Color = shades[random.Next(0, shades.Count() - 1)];
        }

        //TODO Use threads instead of timers. Timers lack precise timing.
        void registerADSREnvelope(int attack, int decay, double sustain, int release)
        {
            done = false;
            rise = new Timer();
            rise.Enabled = false;
            rise.Interval = 1;
            rise.Tick += (object o, EventArgs e) =>
            {
                Console.WriteLine("Amplitude: " + amplitude + ", Attack: " + a + ", Decay: " + d + ", Sustain: " + sustain + ", Release: " + r); //TODO Remove.
                if (a < attack)
                {
                    ++a;
                    amplitude += 1.0 / attack;
                    return;
                }
                else if (d < decay)
                {
                    ++d;
                    amplitude -= (1.0 - sustain) / decay;
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
                if (r < release)
                {
                    ++r;
                    amplitude -= (sustain / release);
                    return;
                }
                else
                {
                    amplitude = 0;
                    fall.Enabled = false;
                    done = true;
                }
            };
        }

        public void StartPainting()
        {
            a = d = 0;
            rise.Enabled = true;
        }

        public void StopPainting()
        {
            r = 0;
            fall.Enabled = true;
        }

        public void Dispose()
        {
            pen.Dispose();
            rise.Dispose();
            fall.Dispose();
        }
    }

}
