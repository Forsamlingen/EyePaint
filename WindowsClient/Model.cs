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
        public Tree(Point root, Color color)
        {
            this.color = color;
        }
    }

    class TreeFactory
    {
        private Stack<Tree> trees;

        public TreeFactory()
        {
            trees = new Stack<Tree>();
        }

        public IEnumerable<Tree> getTrees()
        {
            return this.trees;
        }

        public void createTree(Point root, Color color)
        {
            Tree tree = new Tree(root, color);
            return;
        }

        public void expandTree()
        {
            //TODO Add new vertices and edges to the tree on top of the stack whenever this function is called.
            return;
        }
    }
}
