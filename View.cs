using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    abstract class BaseRasterizer
    {
        protected Image image, background;

        public BaseRasterizer(int width, int height)
        {
            background = new Bitmap(width, height);
            using (Graphics g = System.Drawing.Graphics.FromImage(background))
                g.FillRectangle(Brushes.Black, 0, 0, width, height);
            image = new Bitmap(background);
        }

        abstract public Image Rasterize(BaseFactory factory);

        public void ClearImage()
        {
            image = new Bitmap(background);
        }

        public Image GetImage()
        {
            return image;
        }
    }

    class TreeRasterizer : BaseRasterizer
    {
        public TreeRasterizer(int width, int height) : base(width, height) { }

        public override Image Rasterize(BaseFactory factory)
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
                        var leaves = t.GetLeaves();
                        if (leaves.Count > 1)
                            foreach (var leaf in t.GetLeaves())
                                g.DrawLine(pt.pen, t.parents[leaf], leaf);
                    }

                    if (pt.drawHull)
                    {
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
            return GetImage();
        }
    }

    class CloudRasterizer : BaseRasterizer
    {
        public CloudRasterizer(int width, int height) : base(width, height) { }

        public override Image Rasterize(BaseFactory factory)
        {
            var f = (CloudFactory)factory;
            var q = f.GetRenderQueue();
            using (Graphics g = Graphics.FromImage(image)) while (q.Count() > 0)
                {
                    Cloud c = (Cloud)q.First();
                    foreach (Point p in c.points)
                    {
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
            return GetImage();
        }
    }
}
