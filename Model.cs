using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    enum Model { Cloud, Tree, Square, Snowflake };

    // A factory element is a collection of groups of points, and a paint tool defining the appearance of the groups of points.
    abstract class FactoryElement
    {
        protected static Random random = new Random();
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
        const int MAX_GROWTH_RATE = 100;
        internal readonly Point center;
        int radius;

        public Cloud(Point p, PaintTool pt)
            : base(p, pt)
        {
            center = p;
            radius = 1;
        }

        // Randomly plot new points around the cloud's center.
        public override void Grow()
        {
            //int amount = (int)Math.Floor(MAX_GROWTH_RATE * paintTool.amplitude); TODO
            int amount = MAX_GROWTH_RATE / radius;

            Point[] points = new Point[amount];
            ++radius;
            while (--amount >= 0)
            {
                points[amount] = new Point(random.Next(center.X - radius, center.X + radius), random.Next(center.Y - radius, center.Y + radius));
            }
            pointGroups.Enqueue(points);
        }
    }

    class Tree : FactoryElement
    {
        const int MAX_BRANCHES = 10;
        const int MAX_AGE = 10;
        const int MAX_BRANCH_LENGTH = 300;

        int age, rotation;
        Dictionary<Point, Point> parents;
        Point[] leaves;

        public Tree(Point p, PaintTool pt)
            : base(p, pt)
        {
            age = 0;
            leaves = new Point[] { p };
            parents = new Dictionary<Point, Point>();
            rotation = random.Next((int)(2 * Math.PI));
        }

        // Add new leaves for each current leaf, effectively branching out the tree.
        public override void Grow()
        {
            ++age;
            if (age >= MAX_AGE) return;
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
                    int dx = (int)Math.Round(random.Next(branchLength) * Math.Cos(angle * idx + rotation));
                    int dy = (int)Math.Round(random.Next(branchLength) * Math.Sin(angle * idx + rotation));

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

    class Snowflake : FactoryElement
    {
        const int MAX_BRANCHES = 10;
        const int MAX_AGE = 5;
        const int MAX_BRANCH_LENGTH = 100;

        int age;
        Dictionary<Point, Point> parents;
        Point[] leaves;

        public Snowflake(Point p, PaintTool pt)
            : base(p, pt)
        {
            age = 0;
            leaves = new Point[] { p };
            parents = new Dictionary<Point, Point>();

        }

        // Add new leaves for each current leaf, effectively branching out the tree.
        public override void Grow()
        {
            ++age;
            if (age >= MAX_AGE) return;
            int branches = MAX_BRANCHES / age;
            int branchLength = random.Next(MAX_BRANCH_LENGTH);

            // Go through each leaf and branch out the tree, distributed evenly.
            Point[] newLeaves = new Point[leaves.Length * branches];
            Dictionary<Point, Point> newParents = new Dictionary<Point, Point>();
            int idx = 0;
            double angle = (2 * Math.PI) / (double)newLeaves.Length;
            int rotation = random.Next((int)(2 * Math.PI));
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

    class Square : FactoryElement
    {
        const int MAX_SIZE = 100;
        internal readonly Point center;
        int size;

        public Square(Point p, PaintTool pt)
            : base(p, pt)
        {
            center = p;
            size = 1;
        }

        // Randomly plot new points around the cloud's center.
        public override void Grow()
        {
            if (size > MAX_SIZE) return;
            size += random.Next(MAX_SIZE - size);
            pointGroups.Enqueue(new Point[] { 
                new Point(center.X - size, center.Y - size), 
                new Point(center.X - size, center.Y + size), 
                new Point(center.X + size, center.Y + size), 
                new Point(center.X + size, center.Y - size) 
            });
        }
    }

    // A factory creates and maintains several factory elements at once.
    class Factory
    {
        const int MAX_ELEMENTS = 3;
        static Random random = new Random();
        internal readonly LinkedList<FactoryElement> elements;

        public Factory()
        {
            elements = new LinkedList<FactoryElement>();
        }

        // Create a new factory element.
        public void Add(Point p, PaintTool pt)
        {
            switch (pt.model)
            {
                case Model.Tree:
                    elements.AddLast(new Tree(p, pt));
                    break;
                case Model.Cloud:
                    elements.AddLast(new Cloud(p, pt));
                    break;
                case Model.Square:
                    elements.AddLast(new Square(p, pt));
                    break;
                case Model.Snowflake:
                    elements.AddLast(new Snowflake(p, pt));
                    break;
            }
        }

        // Grow factory elements.
        public void Grow()
        {
            //TODO Collision detection.
            while (elements.Count > MAX_ELEMENTS) elements.RemoveFirst();
            foreach (var e in elements) e.Grow();
        }
    }
}
