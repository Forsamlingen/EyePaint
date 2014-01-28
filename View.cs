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
        private readonly int stdOpacity = 255;
        private readonly int stdWidth = 5;

        internal ImageFactory(int width, int height)
        {
            background = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(background))
                g.FillRectangle(Brushes.White, 0, 0, width, height);

            image = new Bitmap(background);

            pen = new Pen(Color.White, 1);
        }

        internal Image RasterizeTree(LinkedList<EP_Tree> renderQueue)
        {
            while (renderQueue.Count() != 0)
            {
                DrawTree(renderQueue.First());
                renderQueue.RemoveFirst();
            }
            return image;
        }

        private void DrawLine(Point p1, Point p2 )
        {
            using (Graphics g = Graphics.FromImage(image))
                        g.DrawLine(
                            pen,
                            p1,
                            p2
                            );
        }
        private void DrawTree(EP_Tree tree)
        {
            pen.Color = Color.FromArgb(stdOpacity, tree.color.R, tree.color.G, tree.color.B);
            pen.Width = stdWidth;

            for (int i = 0; i < tree.nLeaves; i++)
            {
                Point parent = new Point(tree.previousGen[i].X, tree.previousGen[i].Y);
                Point leaf = new Point(tree.leaves[i].X, tree.leaves[i].Y);
                DrawLine(parent, leaf);
            }

        }

        internal Image RasterizeCloud(Stack<Cloud> model, Point[] points) // TODO change back to normal
        {
            Cloud c = model.Peek();
            int radius = c.GetRadius();
            pen.Color = Color.FromArgb(100, c.color.R, c.color.G, c.color.B);
            int scale = 10;
            pen.Width = scale * 2 * radius;

            using (Graphics g = Graphics.FromImage(image))
                foreach (Point point in points){
                    DrawLine(new Point(0,0),point); //TODO changto to optional setting
                    g.DrawEllipse(
                        pen,
                        point.X + radius,
                        point.Y + radius,
                        pen.Width,
                        pen.Width
                      );
                      //DrawTree(tree);
                }
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
