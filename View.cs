using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    internal abstract class BaseRasterizer
    {
        internal Image image, background;

        internal BaseRasterizer(int width, int height)
        {
            background = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(background)) g.FillRectangle(Brushes.Black, 0, 0, width, height);
            image = new Bitmap(background);
        }

        abstract internal Image Rasterize(BaseFactory factory);

        internal void Clear()
        {
            image = new Bitmap(background);
        }
    }

    internal class TreeRasterizer : BaseRasterizer
    {
        internal TreeRasterizer(int width, int height) : base(width, height) { }
        internal override Image Rasterize(BaseFactory factory)
        {
            var f = (TreeFactory)factory;
            Queue<FactoryElement> q = f.GetRenderQueue();
            using (Graphics g = Graphics.FromImage(image)) while (q.Count() > 0)
                {
                    Tree t = (Tree)q.First();
                    var pt = t.paintTool;

                    // Pick render methods based on paint tool settings.
                    if (pt.drawLines)
                    {
                        // Draw tree lines.
                        for (int i = 0; i < t.nLeaves; i++)
                        {
                            Point parent = new Point(t.previousGen[i].X, t.previousGen[i].Y);
                            Point leaf = new Point(t.points[i].X, t.points[i].Y);
                            g.DrawLine(pt.pen, parent, leaf);
                        }
                    }

                    if (pt.drawHull)
                    {
                        // Draw convex hull.
                        Stack<Point> convexHull = f.GetConvexHull(t);
                        g.FillPolygon(pt.pen.Brush, convexHull.ToArray());
                    }

                    if (pt.drawEllipses)
                    {
                        foreach (Point p in t.points)
                        {
                            var w = pt.pen.Width;
                            g.DrawEllipse(pt.pen, p.X - w / 2, p.Y - w / 2, w, w);
                        }
                    }

                    q.Dequeue();
                }
            f.ClearRenderQueue(); //TODO Neccessary? Probably not.
            return image;
        }

    }

    internal class CloudRasterizer : BaseRasterizer
    {
        internal CloudRasterizer(int width, int height) : base(width, height) { }
        internal override Image Rasterize(BaseFactory factory)
        {
            var f = (CloudFactory)factory;
            var q = f.GetRenderQueue();
            using (Graphics g = Graphics.FromImage(image)) while (q.Count() > 0)
                {
                    Cloud c = (Cloud)q.First();
                    foreach (Point p in c.points)
                    {
                        Console.WriteLine(p.ToString());
                        g.DrawEllipse(
                            c.paintTool.pen,
                            p.X - c.GetRadius() / 2,
                            p.Y - c.GetRadius() / 2,
                            c.paintTool.pen.Width,
                            c.paintTool.pen.Width
                            );
                    }
                    q.Dequeue();
                }
            return image;
        }
    }
}
