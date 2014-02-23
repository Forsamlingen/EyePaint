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

        public FactoryElement(PaintTool pt)
        {
            paintTool = pt;
            pointGroups = new Queue<Point[]>();
        }

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

    // A factory creates and maintains several factory elements at once.
    abstract class BaseFactory
    {
        protected readonly LinkedList<FactoryElement> elements;

        public BaseFactory()
        {
            elements = new LinkedList<FactoryElement>();
        }

        // Create a new factory element.
        public abstract void Add(Point p, PaintTool pt);

        // Grow factory elements.
        public abstract void Grow();

        public LinkedList<FactoryElement> Consume()
        {
            //TODO Remove dead elements that are no longer changing.
            //TODO Use iterator pattern instead.
            return elements;
        }
    }

    class Tree : FactoryElement
    {
        Dictionary<Point, Point> parents;
        LinkedList<Point> leaves;

        public Tree(PaintTool pt, Point p)
            : base(pt)
        {
            pointGroups.Enqueue(new Point[] { p });
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
        public void AddBranches(Random random)
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

    class TreeFactory : BaseFactory
    {
        Random random = new Random();

        public override void Add(Point root, PaintTool pt)
        {
            //if (isInsideTree(root) && !pt.alwaysAdd) return; TODO Implement Linear Algebra library.
            elements.AddLast(new Tree(pt, root));
        }

        public override void Grow()
        {
            foreach (Tree t in elements)
            {
                t.AddBranches(random);
            }
        }
    }

    class Cloud : FactoryElement
    {
        internal readonly Point center;
        int radius;

        public Cloud(PaintTool pt, Point c)
            : base(pt)
        {
            center = c;
            radius = 1;
            pointGroups.Enqueue(new Point[] { c });
        }

        public void GrowCloud(Random random)
        {
            int amount = (int)(10 * paintTool.amplitude); //TODO Convert properly.
            Point[] points = new Point[amount];
            radius += amount;
            while (--amount > 0)
            {
                points[amount] = new Point(random.Next(center.X - radius, center.X + radius), random.Next(center.Y - radius, center.Y + radius));
            }
            pointGroups.Enqueue(points);
        }

        public void IncreaseRadius(int amount = 1)
        {
            radius += amount;
        }

        public int GetRadius()
        {
            return radius;
        }

    }

    class CloudFactory : BaseFactory
    {
        Random random;

        public CloudFactory()
        {
            random = new Random();
        }

        public override void Add(Point p, PaintTool pt)
        {
            elements.AddLast(new Cloud(pt, p));
        }

        public override void Grow()
        {
            foreach (Cloud c in elements)
            {
                c.GrowCloud(random);
            }
        }
    }
}
