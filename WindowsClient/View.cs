using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    class ImageFactory
    {
        private Image image;
        private Pen pen;
        Random rng;

        internal ImageFactory(int width, int height)
        {
            image = new Bitmap(width, height);
            pen = new Pen(Color.Black, 1);
            rng = new Random();
        }

        internal Image RasterizeTrees(ref Stack<Tree> trees)
        {
            Graphics g = Graphics.FromImage(image);
            var tree = trees.Peek();

            pen.Color = Color.FromArgb(10, tree.color.R, tree.color.G, tree.color.B);
            pen.Width = (int)Math.Log10(2 * tree.radius);

            foreach (var point in tree.points)
                g.DrawEllipse(
                    pen,
                    point.X - (float)rng.NextDouble() * tree.radius,
                    point.Y - (float)rng.NextDouble() * tree.radius,
                    pen.Width + (float)rng.Next(-tree.radius, tree.radius),
                    pen.Width + (float)rng.Next(-tree.radius, tree.radius)
                  );

            g.Dispose(); // TODO Use using() {} instead.

            return image;
        }

        internal void Undo()
        {
            //TODO Don't clear the entire drawing, instead implement an undo history.
            Graphics g = Graphics.FromImage(image);
            Region r = new Region();
            r.MakeInfinite();
            g.FillRegion(Brushes.White, r);
        }
    }
}
