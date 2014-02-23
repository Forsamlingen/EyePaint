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

        Timer rise, fall;
        int amplitude, a, d, r;
        const int AMPLITUDE_MAX = 100;
        readonly int attack, decay, sustain, release;

        public PaintTool(string name, Image icon, Color color)
        {
            this.name = name;

            // ADSR envelope
            rise = new Timer();
            fall = new Timer();
            rise.Enabled = fall.Enabled = false;
            rise.Interval = fall.Interval = 1;

            rise.Tick += (object s, EventArgs e) =>
            {
                if (a < attack)
                {
                    ++a;
                    amplitude += AMPLITUDE_MAX / attack;
                    return;
                }
                else if (d < decay)
                {
                    ++d;
                    amplitude -= (AMPLITUDE_MAX - sustain) / decay;
                    return;
                }
                else rise.Enabled = false;
            };

            fall.Tick += (object s, EventArgs e) =>
            {
                if (r < release)
                {
                    ++r;
                    amplitude -= sustain / release;
                    return;
                }
                else fall.Enabled = false;
            };

            //TODO Don't hardcode values.
            amplitude = 0;
            attack = 10;
            decay = 10;
            sustain = 0;
            release = 10;

            // Shapes
            drawLines = true;
            drawHull = false;
            drawEllipses = false;
            alwaysAdd = true; //TODO Remove?

            // Colors
            pen = new Pen(Color.FromArgb(100, color), 3); //TODO Set default opacity and width somewhere else.
            shades = new List<Color>();
            SetShades(color);
        }

        public void SetShades(Color baseColor, int numberOfShades = 10)
        {
            //TODO Add more interesting palette generation than scaling opacity with the base color.
            for (int i = 1; i <= numberOfShades; ++i)
                shades.Add(Color.FromArgb(255 / i, baseColor));
        }

        public void StartPainting()
        {
            amplitude = a = d = r = 0;
            rise.Enabled = true;
        }

        public void StopPainting()
        {
            amplitude = a = d = r = 0;
            rise.Enabled = true;
        }

        internal int GetAmplitude()
        {
            return amplitude;
        }
    }
}
