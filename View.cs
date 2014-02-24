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

        public void RasterizeModel(Factory f)
        {
            using (Graphics g = Graphics.FromImage(image)) foreach (var e in f.elements)
                {
                    var pt = e.GetPaintTool();
                    var pen = pt.pen;
                    var w = pen.Width;

                    // Rasterize element's point groups.
                    while (e.CanConsume())
                    {
                        var points = e.Consume();

                        if (points.Length > 1)
                        {
                            if (pt.drawLines) g.DrawLines(pen, points);
                            if (pt.drawCurves) g.DrawCurve(pen, points);
                            if (pt.drawPolygon) g.DrawPolygon(pen, points);
                        }

                        foreach (var p in points)
                        {
                            if (pt.drawEllipses) g.DrawEllipse(pen, p.X - w / 2, p.Y - w / 2, w, w);
                            if (pt.drawStamps) g.DrawImage(pt.stamp, p);
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
