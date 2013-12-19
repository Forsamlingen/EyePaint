using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    class Tree
    {
        internal readonly Color color;
        internal readonly List<Point> points; //TODO This is a temporary data structure that could be replaced with something more tree like.
        internal int radius; // Naive implementation that could be improved.
        internal Tree(Point root, Color color)
        {
            this.points = new List<Point> { root };
            this.color = color;
            this.radius = 1;
        }
    }

    class TreeFactory
    {
        internal readonly Stack<Tree> trees;
        Random randomNumberGenerator;

        internal TreeFactory()
        {
            trees = new Stack<Tree>();
            randomNumberGenerator = new Random();
        }

        internal void CreateTree(Point root, Color color)
        {
            Tree tree = new Tree(root, color);
            trees.Push(tree);
        }

        internal void ExpandTree()
        {
            var tree = trees.Peek();
            int cloud = ++tree.radius;
            int x = randomNumberGenerator.Next(tree.points[0].X - cloud, tree.points[0].X + cloud);
            int y = randomNumberGenerator.Next(tree.points[0].Y - cloud, tree.points[0].Y + cloud);
            tree.points.Add(new Point(x, y));
        }
    }
}
