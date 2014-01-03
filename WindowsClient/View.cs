using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    class ImageFactory
    {
        private Image image, background;
        private Pen pen;
        private Random rng;

        internal ImageFactory(int width, int height)
        {
            background = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(background))
                g.FillRectangle(Brushes.White, 0, 0, width, height);

            image = new Bitmap(background);

            pen = new Pen(Color.White, 1);
            rng = new Random();
        }

        internal Image Rasterize(Stack<Cloud> model, Point[] points)
        {
            Cloud c = model.Peek();
            int radius = c.GetRadius();
            pen.Color = Color.FromArgb(10, c.color.R, c.color.G, c.color.B);
            pen.Width = 2 * radius + rng.Next(10 * radius);

            using (Graphics g = Graphics.FromImage(image))
                foreach (Point point in points)
                    g.DrawEllipse(
                        pen,
                        point.X - (float)rng.NextDouble() * radius,
                        point.Y - (float)rng.NextDouble() * radius,
                        pen.Width + (float)rng.Next(-radius, radius),
                        pen.Width + (float)rng.Next(-radius, radius)
                      );

            return image;
        }

        internal void Undo()
        {
            //TODO Don't clear the entire drawing, instead implement an undo history.
            Clear();
        }

        internal void Clear()
        {
            image = new Bitmap(background);
        }
    }
}
