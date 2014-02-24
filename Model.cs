using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    // A factory element is a collection of groups of points, and a paint tool defining the appearance of the groups of points.
    abstract class FactoryElement
    {
        protected readonly PaintTool paintTool;
        protected readonly Queue<Point[]> pointGroups;
        protected readonly Random random;

        public FactoryElement(Point p, PaintTool pt, Random r)
        {
            paintTool = pt;
            pointGroups = new Queue<Point[]>();
            pointGroups.Enqueue(new Point[] { p });
            random = r;
        }

        public abstract void Grow();

        public PaintTool GetPaintTool()
        {
            return paintTool;
        }

        public bool CanConsume()
        {
            return pointGroups.Count > 0;
        }

        public Point[] Consume()
        {
            return pointGroups.Dequeue();
        }
    }

    class Cloud : FactoryElement
    {
        internal readonly Point center;
        int radius;

        public Cloud(Point p, PaintTool pt, Random r)
            : base(p, pt, r)
        {
            center = p;
            radius = 1;
        }

        public override void Grow()
        {
            const int MAX_NEW_POINTS = 10; //TODO Set somewhere else. But where? Hmm...

            // Interpret paint tool amplitude as number of new points to add to the cloud.
            //int amount = (int)Math.Floor(MAX_NEW_POINTS * paintTool.amplitude); TODO
            int amount = MAX_NEW_POINTS;

            // Randomly plot new points around the cloud's center.
            Point[] points = new Point[amount];
            radius += amount;
            while (--amount >= 0)
            {
                points[amount] = new Point(random.Next(center.X - radius, center.X + radius), random.Next(center.Y - radius, center.Y + radius));
            }
            pointGroups.Enqueue(points);
        }
    }

    class Tree : FactoryElement
    {
        int age, rotation;
        Dictionary<Point, Point> parents;
        Point[] leaves;

        public Tree(Point p, PaintTool pt, Random r)
            : base(p, pt, r)
        {
            age = 0;
            leaves = new Point[] { p };
            parents = new Dictionary<Point, Point>();
            rotation = random.Next((int)(2 * Math.PI));
        }

        public Point GetParent(Point leaf)
        {
            return parents[leaf];
        }

        public Point[] GetLeaves()
        {
            return leaves;
        }

        // Add new leaves for each current leaf, effectively branching out the tree.
        public override void Grow()
        {
            ++age;

            if (age > 5) return; //TODO Investigate performance hit.

            // Interpret paint tool amplitude as branch length and number of new branches.
            const int MAX_BRANCHES = 7; //TODO Set this somewhere else.
            const int MAX_BRANCH_LENGTH = 50; //TODO Set this somewhere else.
            int branches = MAX_BRANCHES / age;
            int branchLength = random.Next(MAX_BRANCH_LENGTH);

            // Go through each leaf and branch out the tree, distributed evenly.
            Point[] newLeaves = new Point[leaves.Length * branches];
            Dictionary<Point, Point> newParents = new Dictionary<Point, Point>();
            int idx = 0;
            double angle = (2 * Math.PI) / (double)newLeaves.Length;
            foreach (var leaf in leaves) for (int j = 0; j < branches; ++j)
                {
                    // Determine endpoint displacement.
                    int dx = (int)Math.Round(branchLength * Math.Cos(angle * idx + rotation));
                    int dy = (int)Math.Round(branchLength * Math.Sin(angle * idx + rotation));

                    // Construct and store branch's endpoint.
                    var newLeaf = new Point(leaf.X + dx, leaf.Y + dy);
                    newLeaves[idx++] = newLeaf;
                    newParents[newLeaf] = leaf;
                    pointGroups.Enqueue(new Point[] { newLeaf, leaf });
                }

            parents = newParents;
            leaves = newLeaves;
        }
    }

    // A factory creates and maintains several factory elements at once.
    class Factory<T> where T : FactoryElement
    {
        Random random;
        protected readonly LinkedList<T> elements;

        public Factory()
        {
            random = new Random();
            elements = new LinkedList<T>();
        }

        // Create a new factory element.
        public void Add(Point p, PaintTool pt)
        {
            if (pt.done) return;
            var f = (T)Activator.CreateInstance(typeof(T), new object[] { p, pt, random });
            elements.AddLast(f);
        }

        // Grow factory elements.
        public void Grow()
        {
            //TODO Collision detection.
            foreach (T e in elements) e.Grow();
            //if (elements.Count > 0) elements.Last.Value.Grow(); TODO
        }

        public LinkedList<T> Consume()
        {
            //TODO Remove dead elements that are no longer changing.
            /*
            foreach (var e in elements)
                if (e.GetPaintTool().done)
                    elements.Remove(e);
            */

            return elements;
        }
    }
}
