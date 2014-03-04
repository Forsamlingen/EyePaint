using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    /**
     * This class represent a model that constructs renderObjects that could
     * rasterize themself on a given bitmap.
     * Which renderObjects that are created  depend on the present PaintTool used.
     * 
     **/

    class Model
    {
        Dictionary<PaintToolType, BaseFactory> Factories;
        BaseFactory presentFactory;
        ColorTool presentColorTool;

        /**
         * Construct a model with the given PaintTool as start PaintTool 
         **/
        internal Model(PaintTool initPaintTool, ColorTool initColorTool)
        {
            setUpFactories(initColorTool);
            presentFactory = Factories[initPaintTool.type];
            presentFactory.ChangePaintTool(initPaintTool, initColorTool);
            presentColorTool = initColorTool;
        }

        internal Queue<RenderObject> GetRenderQueue()
        {
            return presentFactory.GetRenderQueue();
        }

        internal void ClearRenderQueue()
        {
            presentFactory.ClearRenderQueue();
        }
        internal void ChangePaintTool(PaintTool newPaintTool)
        {
            ClearRenderQueue();

            Factories[newPaintTool.type].ChangePaintTool(newPaintTool, presentColorTool);
            presentFactory = Factories[newPaintTool.type];

        }
        internal void ChangeColorTool(ColorTool newColorTool)
        {
            presentColorTool = newColorTool;
            presentFactory.changeColorTool(presentColorTool);

        }
        /**
         * Add a point to the model
         */
        internal void Add(Point point, bool alwaysAdd)
        {
            presentFactory.Add(point, alwaysAdd);
        }

        /**
         * Grow the present renderObject 
         */
        internal void Grow()
        {
            presentFactory.Grow();
        }



        /**
         * Set up the factories dictionary with all available PaintoolTypes mapped to their Factory.
         **/
        //TODO Add more factories here when more toolTypes are added
        private void setUpFactories(ColorTool initColorTool)
        {
            Factories = new Dictionary<PaintToolType, BaseFactory>();
            Factories.Add(PaintToolType.TREE, new TreeFactory(initColorTool));

        }

    }
    abstract class BaseFactory
    {
        protected ColorTool presentColorTool;

        protected BaseFactory(ColorTool initColorTool)
        {
            presentColorTool = initColorTool;
        }

        internal abstract void Add(Point p, bool alwaysAdd = false);
        internal abstract void Grow();

        internal abstract Queue<RenderObject> GetRenderQueue();

        internal abstract void ClearRenderQueue();

        //TODO see if logic for this could be done clearer
        internal abstract void ChangePaintTool(PaintTool newTool, ColorTool presentColorTool);

        internal abstract void changeColorTool(ColorTool newColorTool);

    }

    class TreeFactory : BaseFactory
    {
        //Todo check if possible to use tree instead
        private Queue<RenderObject> renderQueue;

        private int offset_distance = 0;           // distance from the convex hull

        private Random random = new Random();
        private Tree currentTree;
        private bool treeAdded = false;
        private TreeTool presentTreeTool;

        internal TreeFactory(ColorTool initColorTool)
            : base(initColorTool)
        {
            renderQueue = new Queue<RenderObject>();
            presentColorTool = initColorTool;
        }


        internal override void ClearRenderQueue()
        {
            renderQueue.Clear();
        }

        internal override Queue<RenderObject> GetRenderQueue()
        {
            return renderQueue;
        }


        internal override void Add(Point root, bool alwaysAdd = false)
        {
            if (alwaysAdd || !PointInsideTree(root))
            {

                Tree tree = CreateDefaultTree(root);
                currentTree = tree;
                renderQueue.Enqueue(tree);
                treeAdded = true;
            }
        }

        /*
         * Update renderQuee with a EP-tree representing the next generation of the last tree created
         */
        internal override void Grow()
        {
            if (treeAdded)
            {
                int maxGenerations = presentTreeTool.maxGeneration;
                int nLeaves = presentTreeTool.nLeaves;

                if (currentTree.generation > maxGenerations)
                {
                    return;
                }

                Tree lastTree = currentTree;
                Point[] newLeaves = new Point[nLeaves];
                // Grow all branches
                for (int i = 0; i < nLeaves; i++)
                {
                    Point newLeaf = GetLeaf(lastTree.leaves[i], lastTree.root, presentTreeTool.branchLength);
                    newLeaves[i] = newLeaf;
                }
                Tree grownTree = CreateTree(lastTree.color, lastTree.root, newLeaves, lastTree.leaves);
                grownTree.generation = currentTree.generation + 1;
                currentTree = grownTree;
                renderQueue.Enqueue(currentTree);
            }
        }

        /**
         * Apply the new tool to the Factory 
         */
        internal override void ChangePaintTool(PaintTool newTreeTool, ColorTool presentColorTool)
        {
            TreeTool newTool = (TreeTool)newTreeTool; // TODO Is there any way TODO this without casting?

            treeAdded = false;

            this.presentTreeTool = newTool;
            this.presentColorTool = presentColorTool;



        }
        internal override void changeColorTool(ColorTool newColorTool)
        {
            presentColorTool = newColorTool;
        }


        private Tree CreateTree(Color color, Point root, Point[] leaves, Point[] parents)
        {
            int branchLength = presentTreeTool.branchLength;
            int maxGeneration = presentTreeTool.maxGeneration;

            int branchWidth = presentTreeTool.branchWidth;
            int hullWidth = presentTreeTool.hullWidth;
            int leafSize = presentTreeTool.leafSize;

            //TODO How do this cleaner
            Tree tree = new PolyTree(color, root, branchLength, leaves.Count(), parents, leaves, branchWidth, hullWidth, leafSize);

            switch (presentTreeTool.renderObjectName)
            {
                case "PolyTree":
                    tree = new PolyTree(color, root, branchLength, leaves.Count(), parents, leaves, branchWidth, hullWidth, leafSize);
                    break;

                case "WoolTree":
                    tree = new WoolTree(color, root, branchLength, leaves.Count(), parents, leaves, branchWidth, hullWidth, leafSize);
                    break;
                case "CellNetTree":
                    tree = new CellNetTree(color, root, branchLength, leaves.Count(), parents, leaves, branchWidth, hullWidth, leafSize);
                    break;
                case "ModernArtTree":
                    tree = new ModernArtTree(color, root, branchLength, leaves.Count(), parents, leaves, branchWidth, hullWidth, leafSize);
                    break;
                default:
                    break;
            }
            return tree;


        }

        /*
         * A default tree is the base of any tree. It consists of a root, 
         * where the gaze point is, surrounded by a set number of leaves to start with.
         */

        private Tree CreateDefaultTree(Point root)
        {
            int nLeaves = presentTreeTool.nLeaves;
            int branchLength = presentTreeTool.branchLength;

            // All the start leaves will have the root as parent
            Point[] previousGen = new Point[nLeaves];
            for (int i = 0; i < nLeaves; i++)
                previousGen[i] = root;

            Point[] startLeaves = new Point[nLeaves];
            double v = 0;

            // Create a set number of leaves with the root of of the tree as parent to all of them
            for (int i = 0; i < nLeaves; i++)
            {
                int x = Convert.ToInt32(branchLength * Math.Cos(v)) + root.X;
                int y = Convert.ToInt32(branchLength * Math.Sin(v)) + root.Y;
                Point leaf = new Point(x, y);
                startLeaves[i] = leaf;
                v += 2 * Math.PI / nLeaves;
            }

            Color treeColor = presentColorTool.getRandomShade(presentTreeTool.opacity);
            return CreateTree(treeColor, root, startLeaves, previousGen);
        }


        /*
         * Return a point representing a leaf that is 
         * grown outwards from the root.
         */
        private Point GetLeaf(Point parent, Point root, int branchLength)
        {
            //Declare an origo point
            Point origo = new Point(0, 0);
            //Declare a vector of length 1  from the root out on the positve x-axis.
            int[] xAxisVector = new int[2] { 1, 0 };

            //Transform to cooridninatesystem where root is origo
            parent = LinearAlgebra.TransformCoordinates(root, parent);
            // parentVector is the vector between the origo and the parent-point
            int[] parentVector = LinearAlgebra.GetVector(origo, parent);

            // the child vector is the vector between the root and the leaf we want calculate the coordinates for 
            int[] childVector = new int[2];

            // r is the length of the parent vector
            double r = LinearAlgebra.Get2DVectorLength(parentVector);

            //Calculate the angle v1 between parent vector and the x-axis vector
            //using the dot-product.
            double v1 = LinearAlgebra.GetAngleBetweenVectors(parentVector, xAxisVector);

            // If v1 is in the 3rd or 4th quadrant, we calculate the radians between the positive x-axis and the parent vector anti-clockwise
            if (parentVector[1] < 0)
            {
                v1 = 2 * Math.PI - v1;
            }
            // x is the maximal angle possible between the parent vector and the child vector if the tree is not alloved to grow inwards.
            double x = Math.Atan(branchLength / r);
            //v2 is the angle between the child vector and the positve x-axis.
            //it is chosen randomly between the interval that only allows the tree to grow outwards
            double v2 = random.NextDouble() * 2 * x + (v1 - x);
            // In a triangle (T) with the corners at origo, the parent point and the leaf, v3 is the angle between the
            //child vector and the parent vector
            double v3 = v2 - v1;
            // v4 is the angle opposite to the parent vector in triangle T
            double v4 = Math.Asin(r * Math.Sin(v3) / branchLength);
            // v5 is the last angle in triangle T. It is the angle opposite to the child vector.
            double v5 = Math.PI - v4 - v3;
            // c is the length of the child vector
            double c = branchLength * Math.Sin(v5) / Math.Sin(v3);

            // Calculate the coordinates for the leaf and transform them back to the original coordinate system
            childVector[0] = Convert.ToInt32(c * Math.Cos(v2));
            childVector[1] = Convert.ToInt32(c * Math.Sin(v2));
            return new Point(childVector[0] + root.X, childVector[1] + root.Y);
        }

        /**
         * If nLeaves is less then 3 return always false
         * Otherwise returns if the evalPoint is inside tree 
         **/
        internal bool PointInsideTree(Point evalPoint)
        {
            if (!treeAdded)
            {
                return false;
            }
            if (currentTree.nLeaves < 3)
            {
                return false;
            }

            Point[] points = new Point[currentTree.nLeaves];
            //Transform leaves to cordinate system where root is origo
            for (int i = 0; i < currentTree.nLeaves; i++)
            {
                Point p = LinearAlgebra.TransformCoordinates(currentTree.root, currentTree.leaves[i]);
                points[i] = LinearAlgebra.Offset(p, offset_distance);
            }

            evalPoint = LinearAlgebra.TransformCoordinates(currentTree.root, evalPoint);



            Stack<Point> s = LinearAlgebra.GetConvexHull(points);

            //check if a line (root-evalPoint) intersects with any of the lines representing the convex hull
            Point hullStart = s.Pop();
            Point p1 = hullStart;
            Point p2 = hullStart;//Needed to be assigned, should allways changed by while-loop below if nLeaves in tree>2
            Point origo = new Point(0, 0);
            while (s.Count() != 0)
            {
                p2 = s.Pop();
                if (LinearAlgebra.LineSegmentIntersect(origo, evalPoint, p1, p2))
                {
                    return false;
                }
                p1 = p2;
            }

            if (LinearAlgebra.LineSegmentIntersect(origo, evalPoint, hullStart, p2))
            {
                return false;
            }

            return true;
        }

    }


}
