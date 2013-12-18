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

        internal ImageFactory(int width, int height)
        {
            image = new Bitmap(width, height);
            pen = new Pen(Brushes.Black, 10);
        }

        internal Image RasterizeTrees(Stack<Tree> trees) // TODO Is a set of trees really neccessary?
        {
            Graphics g = Graphics.FromImage(image);

            while (trees.Count > 0) {
                var tree = trees.Pop();

                pen.Color = tree.color;
                int x = tree.points[0].X;
                int y = tree.points[0].Y;

                g.DrawEllipse(pen, x - 10, y - 10, 20, 20);
            }

            g.Dispose(); // TODO Use using() {} instead.

            return image;
        }

        internal void Undo()
        {
            //TODO Don't clear entire undo history.
            image = new Bitmap(image.Width, image.Height);
        }
    }
}
