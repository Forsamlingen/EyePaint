using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    class BaseRasterizer
    {
        internal Image image, background;
        internal Pen pen;
        internal SolidBrush bgBrush = new SolidBrush(Color.Black);
        internal readonly int stdOpacity = 25;
        internal readonly int stdWidth = 2;
        internal readonly int stdRadius = 2;

        public BaseRasterizer(int width, int height)
        {
            background = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(background))
                g.FillRectangle(bgBrush, 0, 0, width, height);

            image = new Bitmap(background);
            pen = new Pen(Color.White, 1);
        }

        public virtual Image Rasterize(BaseFactory factory)
        {
            return image;
        }

        internal void DrawLine(Point p1, Point p2)
        {
            using (Graphics g = Graphics.FromImage(image))
                g.DrawLine(pen, p1, p2);
        }

        public virtual void Undo()
        {
            //TODO Don't clear the entire drawing, instead implement an undo history.
            Clear();
        }

        public virtual void Clear()
        {
            image = new Bitmap(background);
        }
    }

    class TreeRasterizer : BaseRasterizer
    {
        public TreeRasterizer(int width, int height)
            : base(width, height)
        {
        }

        public override Image Rasterize(BaseFactory f)
        {
            TreeFactory factory = f as TreeFactory;
            LinkedList<Tree> q = factory.getRenderQueue();
            while (q.Count() != 0)
            {
                Tree t = q.First();
                Stack<Point> s = factory.GetConvexHull(t);
                DrawTree(t);
                DrawConvexHull(t, s);
                q.RemoveFirst();
            }
            factory.ClearRenderQueue();
            return image;
        }

        private void DrawTree(Tree tree)
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

        private void DrawConvexHull(Tree tree, Stack<Point> convexHull)
        {
            Color c = Color.FromArgb(stdOpacity, tree.color.R, tree.color.G, tree.color.B);
            SolidBrush b = new SolidBrush(c);
            using (Graphics g = Graphics.FromImage(image))
                g.FillPolygon(b, convexHull.ToArray());
        }

        private void DrawBlopTree(Tree tree)
        {
            pen.Color = Color.FromArgb(stdOpacity, tree.color.R, tree.color.G, tree.color.B);
            pen.Width = stdWidth * 2 * stdRadius;
            for (int i = 0; i < tree.nLeaves; i++)
            {
                Point leaf = new Point(tree.leaves[i].X, tree.leaves[i].Y);
                DrawElipse(leaf);
            }
        }

        private void DrawElipse(Point point)
        {
            using (Graphics g = Graphics.FromImage(image))
                g.DrawEllipse(
                    pen,
                    point.X + stdRadius,
                    point.Y + stdRadius,
                    pen.Width,
                    pen.Width
                    );
        }

    }

    class CloudRasterizer : BaseRasterizer
    {
        public CloudRasterizer(int width, int height)
            : base(width, height)
        {
        }

        public override Image Rasterize(BaseFactory f)
        {
            CloudFactory factory = f as CloudFactory;

            Point[] points = new Point[factory.GetQueueLength()];
            int i = 0;
            while (factory.HasQueued())
                points[i++] = factory.GetQueued();

            try
            {
                Cloud c = factory.clouds.Peek();
                int radius = c.GetRadius();
                pen.Color = Color.FromArgb(100, c.color.R, c.color.G, c.color.B);
                int scale = 10;
                pen.Width = scale * 2 * radius;

                using (Graphics g = Graphics.FromImage(image))
                    foreach (Point point in points)
                    {
                        DrawLine(new Point(0, 0), point); //TODO changto to optional setting
                        g.DrawEllipse(
                            pen,
                            point.X + radius,
                            point.Y + radius,
                            pen.Width,
                            pen.Width
                          );
                    }
                return image;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
