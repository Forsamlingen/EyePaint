using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    abstract class FactoryElement
    {
        internal readonly PaintTool paintTool;
        internal readonly LinkedList<Point> points;

        public FactoryElement(PaintTool pt)
        {
            paintTool = pt;
            points = new LinkedList<Point>();
        }
    }

    abstract class BaseFactory
    {
        protected Stack<FactoryElement> history; //TODO Limit size (memory management).
        protected Queue<FactoryElement> renderQueue;

        // Create a new factory element.
        public abstract void Add(Point p, PaintTool pt);

        // Grow latest factory element.
        public abstract void Grow(int amount);

        public BaseFactory()
        {
            renderQueue = new Queue<FactoryElement>();
            history = new Stack<FactoryElement>();
        }

        public bool HasQueued()
        {
            return renderQueue.Count > 0;
        }

        public FactoryElement GetQueued()
        {
            return renderQueue.Dequeue();
        }
    }

    class Tree : FactoryElement
    {
        Random random; // TODO Every tree probably shouldn't have its own random number generator. Wasteful.
        Dictionary<Point, Point> parents;
        int age;

        internal Tree(PaintTool pt, Point p)
            : base(pt)
        {
            points.AddLast(p);
            age = 0;
            parents = new Dictionary<Point, Point>();
            random = new Random();
        }

        public Point GetParent(Point leaf)
        {
            return parents[leaf];
        }

        // Add new leaves for each current leaf, effectively branching out the tree.
        public void AddBranches(int branches = 1, int branchLength = 1)
        {
            ++age;
            int leaves = points.Count;
            for (int i = 0; i < leaves; ++i)
            {
                var leaf = points.First.Value;
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
                        //parents.Remove(leaf);
                    }

                    // Determine endpoint displacement.
                    int r = random.Next(1, branchLength);
                    int dx = directionX * r;
                    int dy = directionY * (branchLength - r);

                    // Construct and store branch's endpoint.
                    var newLeaf = new Point(leaf.X + dx, leaf.Y + dy);
                    parents[newLeaf] = leaf;
                    points.AddLast(newLeaf);
                }
                points.RemoveFirst();
            }
        }
    }

    class TreeFactory : BaseFactory
    {
        Random random = new Random();

        public override void Add(Point root, PaintTool pt)
        {
            //if (isInsideTree(root) && !pt.alwaysAdd) return; TODO Implement Linear Algebra library.
            Tree t = new Tree(pt, root);
            history.Push(t);
            renderQueue.Enqueue(t);
        }

        public override void Grow(int amount)
        {
            if (history.Count == 0)
                return;

            // Latest tree.
            Tree t = (Tree)history.Peek();

            // Branch out the latest tree.
            t.AddBranches(amount, 50);

            // Add modified tree to render queue, as its been updated.
            renderQueue.Enqueue(t);
        }
    }

    class Cloud : FactoryElement
    {
        int radius;

        public Cloud(PaintTool pt, Point center)
            : base(pt)
        {
            points.AddFirst(center);
            radius = 1;
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
            Cloud c = new Cloud(pt, p);
            renderQueue.Enqueue(c);
            history.Push(c);
        }

        public override void Grow(int amount)
        {
            if (history.Count > 0)
            {
                var cloud = (Cloud)history.Peek();
                var center = cloud.points.First.Value;
                int radius = cloud.GetRadius();
                cloud.IncreaseRadius(amount);
                while (--amount > 0) cloud.points.AddLast(new Point(
                    random.Next(center.X - radius, center.X + radius),
                    random.Next(center.Y - radius, center.Y + radius)
                ));
                renderQueue.Enqueue(cloud);
            }
        }
    }
}
