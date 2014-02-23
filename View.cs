using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    class Rasterizer
    {
        protected Image image, background;

        public Rasterizer(int width, int height)
        {
            background = new Bitmap(width, height);
            using (Graphics g = System.Drawing.Graphics.FromImage(background))
                g.FillRectangle(Brushes.Black, 0, 0, width, height);
            image = new Bitmap(background);
        }

        public void RasterizeModel(BaseFactory f)
        {
            using (Graphics g = Graphics.FromImage(image))
                foreach (var e in f.Consume())
                {
                    var pt = e.GetPaintTool();
                    var pen = pt.pen;
                    var w = pen.Width;
                    while (e.CanConsume())
                    {
                        var points = e.Consume();
                        if (pt.drawLines && points.Length > 1)
                        {
                            g.DrawLines(pen, points);
                        }

                        if (pt.drawEllipses)
                        {
                            foreach (var p in points) g.DrawEllipse(pen, p.X - w / 2, p.Y - w / 2, w, w);
                        }
                    }
                }
        }

        public void ClearImage()
        {
            image = new Bitmap(background);
        }

        public Image GetImage()
        {
            return image;
        }
    }
}
