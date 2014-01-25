using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    public struct EP_Color
    {
        public int R, G, B;

        public EP_Color(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
    public struct EP_Point
    {
        public int X, Y;

        public EP_Point(int x, int y)
        {
            X = x;
            Y = y;
        }

    }

    internal struct EP_Tree
    {
        internal readonly EP_Color color; //TODO feedback
        internal readonly EP_Point root;
        internal int generation;
        internal EP_Point[] previousGen; //Parents of the present Leaves
        internal EP_Point[] leaves;
        internal readonly int edgeLength;
        internal readonly int nLeaves;

        public EP_Tree(EP_Color color, EP_Point root, int edgeLength, int nLeaves, EP_Point[] previousGen, EP_Point[] startLeaves)
        {
            this.color = color;
            this.root = root;
            this.edgeLength = edgeLength;
            this.nLeaves = nLeaves;
            this.previousGen = previousGen; //TODO method DefaultEP_Tree will create this //TODO feedback
            leaves = startLeaves;
            generation = 0;
        }
    }


    class TreeFactory
    {
        internal List<EP_Tree> oldTrees;
        private LinkedList<EP_Tree> renderQueue;
        private readonly int edgeLength = 50; // Constant to experiment with
        private readonly int nLeaves = 9;       //constant to experiment with
        private Random random = new Random();
        private EP_Tree currentTree;
        private bool treeAdded = false;
        public TreeFactory()
        {
            oldTrees = new List<EP_Tree>();
            renderQueue = new LinkedList<EP_Tree>();
        }

        internal void ClearRenderQueue()
        {
            renderQueue.Clear();
        }

        internal LinkedList<EP_Tree> getRenderQueue()
        {
            return renderQueue;
        }

        internal Boolean HasQueued()
        {
            if (renderQueue.Count() == 0)
                return false;
            else
                return true;
        }

        internal void AddTree(EP_Point root, EP_Color color)
        {
            oldTrees.Add(currentTree);
            EP_Tree tree = CreateDefaultTree(root, color);
            currentTree = tree;
            renderQueue.AddLast(tree);
            treeAdded = true;
        }

        /*
         * A default tree is the base of any tree. It consists of a root, 
         * where the gaze point is, surrounded by a set number of leaves to start with.
         */
        internal EP_Tree CreateDefaultTree(EP_Point root, EP_Color color)// TODO Change b to private
        {
            // All the start leaves will have the root as parent
            EP_Point[] previousGen = new EP_Point[nLeaves];
            for (int i = 0; i < nLeaves; i++)
                previousGen[i] = root;

            EP_Point[] startLeaves = new EP_Point[nLeaves];
            double v = 0;

            // Create a set number of leaves with the root of of the tree as parent to all of them
            for (int i = 0; i < nLeaves; i++)
            {

                int x = Convert.ToInt32(edgeLength * Math.Cos(v)) + root.X;
                int y = Convert.ToInt32(edgeLength * Math.Sin(v)) + root.Y;
                EP_Point leaf = new EP_Point(x, y);
                startLeaves[i] = leaf;
                v += 2 * Math.PI / nLeaves;
            }

            return new EP_Tree(color, root, edgeLength, nLeaves, previousGen, startLeaves);
        }

        /*
         * Update renderQuee with a EP-tree representing the next generation of the last tree created
         */
        internal void growTree()
        {
            if (treeAdded)
            {
                EP_Tree lastTree = currentTree;
                EP_Point[] newLeaves = new EP_Point[nLeaves];
                // Grow all branches
                for (int i = 0; i < nLeaves; i++)
                {
                    EP_Point newLeave = getLeave(lastTree.leaves[i], lastTree.root);
                    newLeaves[i] = newLeave;
                }
                EP_Tree grownTree = new EP_Tree(lastTree.color, lastTree.root, lastTree.edgeLength, lastTree.nLeaves, lastTree.leaves, newLeaves);
                grownTree.generation++;
                currentTree = grownTree;
                renderQueue.AddLast(currentTree);
            }
        }

        /*
         * Return a point representing a leave that is 
         * grown outwards from the root.
         */
        private EP_Point getLeave(EP_Point parent, EP_Point root)
        {
            //Declare a vector of length 1  from the root out on the positve x-axis.
            int[] xAxisVector = new int[2] { 1, 0 };

            //Transform to cooridninatesystem where root is origo
            parent.X = parent.X - root.X;
            parent.Y = parent.Y - root.Y;
            // parentVector is the vector between the root and the parent-point
            int[] parentVector = new int[2] { parent.X, parent.Y };

            // the child vector is the vector between the root and the leaf we want calculate the coordinates for 
            int[] childVector = new int[2];

            // r is the length of the parent vector
            double r = Math.Sqrt(Math.Pow(parentVector[0], 2) + Math.Pow(parentVector[1], 2));//TODO change variable name 

            //Calculate the angle v1 between parent vector and the x-axis vector
            //using the dot-product.
            double v1 = Math.Acos((parentVector[0] * xAxisVector[0] + parentVector[1] * xAxisVector[1]) / (r * 1));
            // If v1 is in the 3rd or 4th quadrant, we calculate the radians between the positive x-axis and the parent vector anti-clockwise
            if (parentVector[1] < 0)
            {
                v1 = 2 * Math.PI - v1;
            }
            // x is the maximal angle possible between the parent vector and the child vector if the tree is not alloved to grow inwards.
            double x = Math.Atan(edgeLength / r);
            //v2 is the angle between the child vector and the positve x-axis.
            //it is chosen randomly between the interval that only allows the tree to grow outwards
            double v2 = random.NextDouble() * 2 * x + (v1 - x);
            // In a triangle (T) with the corners at origo, the parent point and the leaf, v3 is the angle between the
            //child vector and the parent vector
            double v3 = v2 - v1;
            // v4 is the angle opposite to the parent vector in triangle T
            double v4 = Math.Asin(r * Math.Sin(v3) / edgeLength);
            // v5 is the last angle in triangle T. It is the angle opposite to the child vector.
            double v5 = Math.PI - v4 - v3;
            // c is the length of the child vector
            double c = edgeLength * Math.Sin(v5) / Math.Sin(v3);

            // Calculate the coordinates for the leaf and transform them back to the original coordinate system
            childVector[0] = Convert.ToInt32(c * Math.Cos(v2));
            childVector[1] = Convert.ToInt32(c * Math.Sin(v2));
            return new EP_Point(childVector[0] + root.X, childVector[1] + root.Y);
        }



        //OBS this method does not compute correct if the root isn´t in the convex hull of the trees leafs
        private bool pointInsideTree(EP_Point point, EP_Tree tree)
        {
            EP_Point[] points = tree.leaves;
            
            //Transform leaves to cordinate system where root is origo
            for (int i = 0; i < tree.nLeaves; i++)
            {
                points[i]= transformCoordinates(tree.root, points[i]);               
            }

            point = transformCoordinates(tree.root, point);

            Stack<EP_Point> s = GrahamScan(points);


            return false;

        }

        private Stack<EP_Point> GrahamScan(EP_Point[] points)
        {
            //Find point with lowex Y-coordinate

            EP_Point minPoint = getLowestPoint(points);

            //Declare a refpoint where the (minPoint,refPoint) is paral,ell to the x-axis
            EP_Point refPoint = new EP_Point(minPoint.X + 1, minPoint.Y);

            //Create a vector of GrahamPoints by calculate the angle a for each point in points
            //Where a is the angle beteen the two vectors (minPoint point) and (minPoint, Refpoint)

            int size = points.Count();
            GrahamPoint minGrahamPoint = new GrahamPoint(0, minPoint);
            GrahamPoint[] grahamPoints = new GrahamPoint[size];
            grahamPoints[0] = minGrahamPoint;
            for (int i = 1; i < points.Count(); i++)
            {
                double a = getAngleBetweenVectors(minPoint, refPoint, minPoint, points[i]);
                GrahamPoint grahamPoint = new GrahamPoint(a, points[i]);
                grahamPoints[i] = grahamPoint;
            }

            Array.Sort(grahamPoints);           

            Stack<EP_Point> s = new Stack<EP_Point>();


            s.Push(GrahamPointToEP_Point(grahamPoints[0]));
            s.Push(GrahamPointToEP_Point(grahamPoints[1]));
            s.Push(GrahamPointToEP_Point(grahamPoints[2]));

            EP_Point top;
            EP_Point nextToTop;
            for (int i = 3; i < points.Count(); i++)
            {
                bool notPushed = true;
                while (notPushed)
                {   
                     top = s.Pop();
                     nextToTop = s.Peek();
                    if (ccw(nextToTop, top, points[i]) >= 0)
                    {
                        s.Push(points[i]);
                        notPushed = false;
                    }

                }
            }          
            return s;
        }


        //Create struct to able to sort points by there angle a
        private struct GrahamPoint : IComparable
        {
            public double angle;
            public EP_Point point;

            public GrahamPoint(double angle, EP_Point point)
            {
                this.angle = angle;
                this.point = point;
            }

            public int CompareTo(GrahamPoint p1)
            {
                if (angle == p1.angle)
                {
                    return 0;
                }

                if (angle < p1.angle)
                {
                    return -1;
                }

                else //if(angle > p1.angle)
                {
                    return 1;
                }
            }

        }


        // Use cross-product to calculate if three points are a counter-clockwise. 
        // They are counter-clockwise if ccw > 0, clockwise if
        // ccw < 0, and collinear if ccw == 0
        private int ccw(EP_Point p1, EP_Point p2, EP_Point p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        }
        /*
         * Return an angle in radians between teh vectors (p1,p2) and (q1,q2)
         */
        // TODO HAndle Error from dividing by 0..if a vector is of length 0
        private double getAngleBetweenVectors(EP_Point p1, EP_Point p2, EP_Point q1, EP_Point q2)
        {
            int[] P = getVector(p1, p2);
            int[] Q = getVector(q1, q2);
            double lP = getVectorLength(P);
            double lQ = getVectorLength(Q);
            //Calculate the angle v between P and Q
            //using the dot-product.

            double v = Math.Acos((P[0] * Q[0] + P[1] * Q[1]) / (lP * lQ));
            return v;
        }

        private double getVectorLength(int[] vector)
        {
            double l = Math.Sqrt(Math.Pow(vector[0], 2) + Math.Pow(vector[1], 2));
            return l;
        }

        private int[] getVector(EP_Point p1, EP_Point p2)
        {
            int[] vector = new int[2] { p2.X - p1.X, p2.Y - p1.Y };
            return vector;
        }

        //Transfor the point to a coordinate system where the argumet origo have the 
        //cooridinate (0,0)
        private EP_Point transformCoordinates(EP_Point origo, EP_Point point)
        {
            point.X -= origo.X;
            point.Y -= origo.Y;
            
            return point;
        }

        private EP_Point getLowestPoint(EP_Point[] points)
        {
            EP_Point minPoint = points[0];

            for (int i = 0; i < points.Count(); i++)
            {
                if (points[i].Y < minPoint.Y)
                {
                    minPoint = points[i];
                }
                // if equal Y-coordinate, compare by X
                else if (minPoint.Y == points[i].Y)
                {

                    if (points[i].X < minPoint.X)
                    {
                        minPoint = points[i];
                    }
                }
            }
            return minPoint;
        }

        private EP_Point GrahamPointToEP_Point(GrahamPoint grahamPoint)
        {
            EP_Point ep_point = new EP_Point(grahamPoint.point.X, grahamPoint.point.Y);
            return ep_point;
        }
        
    }


    class Cloud
    {
        internal readonly Color color;
        internal readonly List<Point> points;
        private int radius;

        internal Cloud(Point root, Color color)
        {
            this.points = new List<Point> { root };
            this.color = color;
            this.radius = 1;
        }

        internal void IncreaseRadius()
        {
            radius++;
        }

        internal int GetRadius()
        {
            return radius;
        }
    }

    class CloudFactory
    {
        internal readonly Stack<Cloud> clouds;
        private Queue<Point> renderQueue;
        private Random randomNumberGenerator;

        internal CloudFactory()
        {
            clouds = new Stack<Cloud>();
            renderQueue = new Queue<Point>(); //TODO Exchange for buffer. Will probably require restructuring of program.
            randomNumberGenerator = new Random();
        }

        internal void AddCloud(Point center, Color color)
        {
            Cloud c = new Cloud(center, color);
            clouds.Push(c);
        }

        internal void GrowCloud(Cloud c, int amount)
        {
            c.IncreaseRadius();
            int radius = c.GetRadius();

            for (int i = 0; i < amount; i++)
            {
                int x = randomNumberGenerator.Next(c.points[0].X - radius, c.points[0].X + radius);
                int y = randomNumberGenerator.Next(c.points[0].Y - radius, c.points[0].Y + radius);
                c.points.Add(new Point(x, y)); //TODO Memory management!
                renderQueue.Enqueue(new Point(x, y));
            }
        }

        internal void GrowCloudRandomAmount(Cloud c, int maximum)
        {
            GrowCloud(c, randomNumberGenerator.Next(maximum));
        }

        internal bool HasQueued()
        {
            if (renderQueue.Count > 0)
                return true;
            else
                return false;
        }

        internal Point GetQueued()
        {
            return renderQueue.Dequeue();
        }

        internal int GetQueueLength()
        {
            return renderQueue.Count;
        }
    }
}
