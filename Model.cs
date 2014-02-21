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
        internal readonly List<Point> points;

        public FactoryElement(PaintTool pt)
        {
            paintTool = pt;
            points = new List<Point>();
        }
    }

    abstract class BaseFactory
    {
        protected Stack<FactoryElement> history; //TODO Limit size (memory management).
        protected Queue<FactoryElement> renderQueue;

        public abstract void Add(Point p, PaintTool pt);

        public abstract void Grow();

        public BaseFactory()
        {
            renderQueue = new Queue<FactoryElement>();
            history = new Stack<FactoryElement>();
        }

        public void ClearRenderQueue()
        {
            renderQueue.Clear();
        }

        public Queue<FactoryElement> GetRenderQueue()
        {
            return renderQueue;
        }
    }

    class Tree : FactoryElement
    {
        internal readonly Point root;
        internal readonly Dictionary<Point, Point> parents;
        Stack<Point> leaves;
        int age;

        internal Tree(PaintTool pt, Point r)
            : base(pt)
        {
            root = r;
            age = 0;
            parents = new Dictionary<Point, Point>();
            leaves = new Stack<Point>();
            leaves.Push(root);
        }

        // Increase the age of the tree.
        public void IncreaseAge(int amount = 1)
        {
            age += amount;
        }

        // Return tree's current age.
        public int GetAge()
        {
            return age;
        }

        // Add new leaves for each current leaf, effectively branching out the tree.
        public void AddBranches(int branches = 1, int branchLength = 1)
        {
            Stack<Point> newLeaves = new Stack<Point>();
            while (leaves.Count > 0)
            {
                var leaf = leaves.Pop();
                for (int i = 0; i < branches; ++i)
                {
                    var newLeaf = new Point(leaf.X + branchLength, leaf.Y + branchLength);
                    parents[newLeaf] = leaf;
                    newLeaves.Push(newLeaf);
                }
            }
            leaves = newLeaves;
            points.AddRange(leaves);
        }

        // Get all leaves in the tree.
        public Stack<Point> GetLeaves()
        {
            return leaves;
        }
    }

    class TreeFactory : BaseFactory
    {
        Random random = new Random();

        const int maxTreeAge = 100; //TODO Make into property.
        const int offsetDistance = 30; //TODO Make into property.
        const int edgeLength = 25; //TODO Make into property.
        const int leavesCount = 1; //TODO Make into property.

        public override void Add(Point root, PaintTool pt)
        {
            //if (isInsideTree(root) && !pt.alwaysAdd) return; TODO
            Tree t = new Tree(pt, root);
            history.Push(t);
            renderQueue.Enqueue(t);
        }

        public override void Grow()
        {
            if (history.Count == 0)
                return;

            // Latest tree.
            Tree t = (Tree)history.Peek();

            // Don't grow a tree past the limit.
            if (t.GetAge() > maxTreeAge) return;
            else t.IncreaseAge(1);

            // Branch out the latest tree.
            t.AddBranches(leavesCount, edgeLength);

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
            points.Add(center);
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

        public override void Grow()
        {
            if (history.Count > 0)
            {
                var cloud = (Cloud)history.Peek();
                var center = cloud.points[0];
                int radius = cloud.GetRadius();
                int amount = random.Next(10);
                cloud.IncreaseRadius(amount);
                while (--amount > 0) cloud.points.Add(new Point(
                    random.Next(center.X - radius, center.X + radius),
                    random.Next(center.Y - radius, center.Y + radius)
                ));
                renderQueue.Enqueue(cloud);
            }
        }
    }
}
