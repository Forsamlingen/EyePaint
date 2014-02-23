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

        public FactoryElement(Point p, PaintTool pt)
        {
            paintTool = pt;
            pointGroups = new Queue<Point[]>();
            pointGroups.Enqueue(new Point[] { p });
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
        static Random random = random = new Random();

        public Cloud(Point p, PaintTool pt)
            : base(p, pt)
        {
            center = p;
            radius = 1;
        }

        public override void Grow()
        {
            const int MAX_NEW_POINTS = 10; //TODO Set somewhere else. But where? Hmm...

            // Interpret paint tool amplitude as number of new points to add to the cloud.
            int amount = (int)Math.Floor(MAX_NEW_POINTS * paintTool.amplitude);

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
        Dictionary<Point, Point> parents;
        LinkedList<Point> leaves;
        static Random random = random = new Random();

        public Tree(Point p, PaintTool pt)
            : base(p, pt)
        {
            leaves = new LinkedList<Point>();
            leaves.AddFirst(p);
            parents = new Dictionary<Point, Point>();
        }

        public Point GetParent(Point leaf)
        {
            return parents[leaf];
        }

        public Point[] GetLeaves()
        {
            return leaves.ToArray();
        }

        // Add new leaves for each current leaf, effectively branching out the tree.
        public override void Grow()
        {
            // Interpret paint tool amplitude as branch length and number of new branches.
            const int MAX_BRANCHES = 20; //TODO Set this somewhere else.
            const int MAX_BRANCH_LENGTH = 100; //TODO Set this somewhere else.
            int branches = (int)Math.Round(MAX_BRANCHES * paintTool.amplitude);
            int branchLength = MAX_BRANCH_LENGTH; // TODO Should each branch in the tree be of the same length? Or should it follow the paint tool amplitude somehow?

            // Go through each leaf and branch out the tree.
            LinkedList<Point> newLeaves = new LinkedList<Point>();
            Dictionary<Point, Point> newParents = new Dictionary<Point, Point>();
            foreach (var leaf in leaves)
            {
                for (int j = 0; j < branches; ++j)
                {
                    // Determine current branch growth direction.
                    int directionX, directionY;
                    if (!parents.ContainsKey(leaf))
                    {
                        directionX = Math.Sign(random.Next(-1, 1));
                        directionY = Math.Sign(random.Next(-1, 1));
                    }
                    else
                    {
                        directionX = Math.Sign(leaf.X - parents[leaf].X);
                        directionY = Math.Sign(leaf.Y - parents[leaf].Y);
                    }

                    // Determine endpoint displacement.
                    int r = random.Next(1, branchLength);
                    int dx = directionX * r;
                    int dy = directionY * (branchLength - r);

                    // Construct and store branch's endpoint.
                    var newLeaf = new Point(leaf.X + dx, leaf.Y + dy);
                    newParents[newLeaf] = leaf;
                    pointGroups.Enqueue(new Point[] { leaf, newLeaf });
                }
            }
            parents = newParents;
            leaves = newLeaves;
        }
    }

    // A factory creates and maintains several factory elements at once.
    class Factory<T> where T : FactoryElement
    {
        static Random random = random = new Random();
        protected readonly LinkedList<T> elements;

        public Factory()
        {
            elements = new LinkedList<T>();
        }

        // Create a new factory element.
        public void Add(Point p, PaintTool pt)
        {
            if (pt.done) return;
            var f = (T)Activator.CreateInstance(typeof(T), new object[] { p, pt });
            elements.AddLast(f);
        }

        // Grow factory elements.
        public void Grow()
        {
            //TODO Collision detection.
            foreach (T e in elements) e.Grow();
        }

        public LinkedList<T> Consume()
        {
            // Remove dead elements that are no longer changing.
            /*
            foreach (var e in elements)
                if (e.GetPaintTool().done)
                    elements.Remove(e);
            */

            return elements;
        }
    }
}
