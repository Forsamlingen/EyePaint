using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    struct Tree
    {
        public readonly Color color;
        public readonly List<Point> points; //TODO This is a temporary data structure that should be replaced with something more tree like.
        public Tree(Point root, Color color)
        {
            points = new List<Point> { root };
            this.color = color;
        }
    }

    class TreeFactory
    {
        public readonly Stack<Tree> trees;

        public TreeFactory()
        {
            trees = new Stack<Tree>();
        }

        public void CreateTree(Point root, Color color)
        {
            Tree tree = new Tree(root, color);
            trees.Push(tree);
        }

        public void ExpandTree()
        {
            //TODO Add some random new branches and leaves to the top tree on the stack whenever this function is called.
            //trees.Peek().points.Enqueue(new Point(0, 0)); //TODO Remove this line, this is just a silly test.
        }
    }
}
