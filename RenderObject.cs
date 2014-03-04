using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{
    abstract class RenderObject
    {
        internal abstract void Rasterize(ref ImageObject imageObeject);
    }

    abstract class Tree : RenderObject
    {
        internal readonly Point root;
        internal int generation;
        internal Point[] previousGen; //Parents of the present leaves
        internal Point[] leaves;
        internal readonly int branchLength;
        internal readonly int nLeaves;// TODO Warning need to be >2

        internal Color color;
        internal int branchWidth;
        internal int hullWidth;
        internal int leafSize;

        protected Tree(
                    Color color,
                    Point root,
                    int branchLength,
                    int nLeaves,
                    Point[] previousGen,
                    Point[] startLeaves,
                    int branchWidth,
                    int hullWidth,
                    int leafSize)
        {
            this.color = color;
            this.root = root;
            this.branchLength = branchLength;
            this.nLeaves = nLeaves; //Warning need to be >2
            this.previousGen = previousGen;
            leaves = startLeaves;
            generation = 0;
            this.branchWidth = branchWidth;
            this.hullWidth = hullWidth;
            this.leafSize = leafSize;
        }
        internal abstract override void Rasterize(ref ImageObject imageObject);

        protected void DrawBranches(ref ImageObject imageObject)
        {

            for (int i = 0; i < previousGen.Count(); i++)
            {
                Point parent = new Point(previousGen[i].X, previousGen[i].Y);
                Point leaf = new Point(leaves[i].X, leaves[i].Y);
                imageObject.DrawLine(color, branchWidth, parent, leaf);
            }
        }

        protected void DrawHull(ref ImageObject imageObject)
        {
            Stack<Point> convexHull = LinearAlgebra.GetConvexHull(leaves);
            Point startPoint = convexHull.Pop();
            Point p1 = startPoint;
            while (convexHull.Count() > 0)
            {
                Point p2 = convexHull.Pop();
                imageObject.DrawLine(color, hullWidth, p1, p2);
                p1 = p2;
                if (convexHull.Count == 0)
                {
                    imageObject.DrawLine(color, hullWidth, p1, startPoint);
                }
            }
        }

        protected void FillHull(ref ImageObject imageObject)
        {
            Stack<Point> convexHull = LinearAlgebra.GetConvexHull(leaves);
            imageObject.DrawPolygon(color, convexHull.ToArray());
        }

        protected void DrawLeaves(ref ImageObject imageObject)
        {
            for (int i = 0; i < leaves.Count(); i++)
            {
                Point leaf = new Point(leaves[i].X, leaves[i].Y);
                imageObject.DrawElipse(color, leafSize, leaf);
            }
        }
    }

    class PolyTree : Tree
    {

        internal PolyTree(
                     Color color,
                     Point root,
                     int branchLength,
                     int nLeaves,
                     Point[] previousGen,
                     Point[] startLeaves,
                     int branchWidth,
                     int hullWidth,
                     int leafSize)
            : base(
                     color, root, branchLength, nLeaves, previousGen, startLeaves, branchWidth, hullWidth, leafSize)
        {

        }

        internal override void Rasterize(ref ImageObject imageObject)
        {

            FillHull(ref imageObject);
        }


    }

    class WoolTree : Tree
    {
        internal WoolTree(
              Color color,
              Point root,
              int branchLength,
              int nLeaves,
              Point[] previousGen,
              Point[] startLeaves,
              int branchWidth,
              int hullWidth,
              int leafSize)
            : base(
                     color, root, branchLength, nLeaves, previousGen, startLeaves, branchWidth, hullWidth, leafSize)
        {

        }
        internal override void Rasterize(ref ImageObject imageObject)
        {
            DrawBranches(ref imageObject);
        }

    }

    class CellNetTree : Tree
    {
        internal CellNetTree(
              Color color,
              Point root,
              int branchLength,
              int nLeaves,
              Point[] previousGen,
              Point[] startLeaves,
              int branchWidth,
              int hullWidth,
              int leafSize)
            : base(
                     color, root, branchLength, nLeaves, previousGen, startLeaves, branchWidth, hullWidth, leafSize)
        {

        }
        internal override void Rasterize(ref ImageObject imageObject)
        {
            DrawBranches(ref imageObject);
            DrawLeaves(ref imageObject);
        }
    }

    class ModernArtTree : Tree
    {
        internal ModernArtTree(
              Color color,
              Point root,
              int branchLength,
              int nLeaves,
              Point[] previousGen,
              Point[] startLeaves,
              int branchWidth,
              int hullWidth,
              int leafSize)
            : base(
                     color, root, branchLength, nLeaves, previousGen, startLeaves, branchWidth, hullWidth, leafSize)
        {

        }
        internal override void Rasterize(ref ImageObject imageObject)
        {
            DrawHull(ref imageObject);
            DrawBranches(ref imageObject);
        }
    }



    //TODD Change Name!!
    internal class ImageObject
    {
        internal Image image;
        Pen pen = new Pen(Color.White);

        internal ImageObject(Image image)
        {
            this.image = image;
        }

        internal void DrawLine(Color color, int width, Point p1, Point p2)
        {
            pen.Color = color;
            pen.Width = width;

            using (Graphics g = Graphics.FromImage(image))
                g.DrawLine(pen, p1, p2);
        }

        internal void DrawElipse(Color color, int radius, Point point)
        {
            pen.Color = color;
            using (Graphics g = Graphics.FromImage(image))
                g.DrawEllipse(
                    pen,
                    point.X + radius,
                    point.Y + radius,
                    pen.Width,
                    pen.Width
                    );
        }

        internal void DrawPolygon(Color color, Point[] vertices)
        {
            SolidBrush b = new SolidBrush(color);
            using (Graphics g = Graphics.FromImage(image))
                g.FillPolygon(b, vertices);
        }

    }
}
