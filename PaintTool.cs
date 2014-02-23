using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EyePaint
{
    class PaintTool
    {
        internal string name;
        internal Image icon;
        internal readonly Pen pen; // Contains settings for opacity, base color, width, etc.
        internal readonly List<Color> shades;
        internal bool drawEllipses, drawLines, drawHull, alwaysAdd; //TODO Perhaps extend the PaintTool class with specific tools per model element (i.e. CloudTool, TreeTool).

        // ADSR envelope
        internal double amplitude; // [0.0..1.0]
        int a, d, r;
        Timer rise, fall;

        public PaintTool(string name, Image icon, Color color)
        {
            this.name = name;

            // Shapes
            drawLines = true;
            drawHull = false;
            drawEllipses = true;
            alwaysAdd = true; //TODO Remove?

            // Colors
            pen = new Pen(Color.FromArgb(100, color), 10); //TODO Set default opacity and width somewhere else.
            shades = new List<Color>();
            SetShades(color);

            // ADSR envelope
            RegisterADSREnvelope(10, 1, 0.5, 10);
        }

        public void SetShades(Color baseColor, int numberOfShades = 10)
        {
            //TODO Add more interesting palette generation than scaling opacity with the base color.
            for (int i = 1; i <= numberOfShades; ++i)
                shades.Add(Color.FromArgb(255 / i, baseColor));
        }

        //TODO Use threads instead of timers. Timers lack precise timing.
        private void RegisterADSREnvelope(int attack, int decay, double sustain, int release)
        {
            rise = new Timer();
            rise.Enabled = false;
            rise.Interval = 1;
            rise.Tick += (object o, EventArgs e) =>
            {
                Console.WriteLine("Amplitude: " + amplitude + ", Attack: " + a + ", Decay: " + d + ", Sustain: " + sustain + ", Release: " + r);
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
    }

}
