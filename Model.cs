using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    internal abstract class FactoryElement
    {
        internal readonly PaintTool paintTool;
        internal List<Point> points;
        internal FactoryElement(PaintTool paintTool)
        {
            this.paintTool = paintTool;
            points = new List<Point>();
        }
    }

    internal abstract class BaseFactory
    {
        private Queue<FactoryElement> renderQueue; //TODO The render queue should be a list of points, not FactoryElements.
        public abstract void Add(Point p, PaintTool pt, bool alwaysAdd = false); // TODO Remove bool.
        public abstract void Grow();

        public BaseFactory()
        {
            renderQueue = new Queue<FactoryElement>();
        }

        internal void ClearRenderQueue()
        {
            renderQueue.Clear();
        }

        internal Queue<FactoryElement> GetRenderQueue()
        {
            return renderQueue;
        }
    }

    internal class Tree : FactoryElement
    {
        internal readonly Point root;
        internal int generation;
        internal Point[] previousGen; //Parents of the present leaves
        internal readonly int edgeLength;
        internal readonly int nLeaves;// TODO Warning need to be >2 TODO Fix warning.

        internal Tree(PaintTool paintTool, Point root, int edgeLength, int nLeaves, Point[] previousGen, Point[] startLeaves)
            : base(paintTool)
        {
            this.root = root;
            this.edgeLength = edgeLength;
            this.nLeaves = nLeaves; //Warning need to be >2 TODO Fix warning.
            this.previousGen = previousGen;
            points = startLeaves.ToList();
            generation = 0;
        }
    }

    internal class TreeFactory : BaseFactory
    {
        internal List<Tree> oldTrees;
        private int maxGenerations = 100;           // controls the max size of a single tree
        private int offset_distance = 30;           // distance from the convex hull
        private readonly int edgeLength = 25;       // constant to experiment with
        private readonly int nLeaves = 7;           //constant to experiment with
        private Random random = new Random();
        private Tree currentTree;
        private bool treeAdded = false;

        public TreeFactory()
        {
            oldTrees = new List<Tree>();
        }

        /**
         * Return a stack with the points in the convex hull of the tree.
         * If number of leaves in the tree is less then 3 an empty stack is returned
         **/
        internal Stack<Point> GetConvexHull(Tree tree)
        {
            if (tree.nLeaves < 3) return new Stack<Point>();
            else return GrahamScan(tree.points.ToArray());
        }

        public override void Add(Point root, PaintTool pt, bool alwaysAdd = false)
        {
            if (alwaysAdd || !PointInsideTree(root))
            {
                oldTrees.Add(currentTree);
                Tree tree = CreateDefaultTree(root, pt);
                currentTree = tree;
                GetRenderQueue().Enqueue(tree);
                treeAdded = true;
            }
        }

        /*
         * A default tree is the base of any tree. It consists of a root, 
         * where the gaze point is, surrounded by a set number of leaves to start with.
         */
        private Tree CreateDefaultTree(Point root, PaintTool pt)
        {
            // All the start leaves will have the root as parent
            Point[] previousGen = new Point[nLeaves];
            for (int i = 0; i < nLeaves; i++)
                previousGen[i] = root;

            Point[] startLeaves = new Point[nLeaves];
            double v = 0;

            // Create a set number of leaves with the root of of the tree as parent to all of them
            for (int i = 0; i < nLeaves; i++)
            {
                int x = Convert.ToInt32(edgeLength * Math.Cos(v)) + root.X;
                int y = Convert.ToInt32(edgeLength * Math.Sin(v)) + root.Y;
                Point leaf = new Point(x, y);
                startLeaves[i] = leaf;
                v += 2 * Math.PI / nLeaves;
            }

            return new Tree(pt, root, edgeLength, nLeaves, previousGen, startLeaves);
        }

        /*
         * Update renderQuee with a EP-tree representing the next generation of the last tree created
         */
        public override void Grow()
        {
            if (treeAdded)
            {
                if (currentTree.generation > maxGenerations)
                {
                    return;
                }

                Tree lastTree = currentTree;
                Point[] newLeaves = new Point[nLeaves];
                // Grow all branches
                for (int i = 0; i < nLeaves; i++)
                {
                    Point newLeaf = GetLeaf(lastTree.points[i], lastTree.root);
                    newLeaves[i] = newLeaf;
                }

                Tree grownTree = new Tree(lastTree.paintTool, lastTree.root, lastTree.edgeLength, lastTree.nLeaves, lastTree.points.ToArray(), newLeaves);
                grownTree.generation = currentTree.generation + 1;
                currentTree = grownTree;
                GetRenderQueue().Enqueue(currentTree);
            }
        }

        /*
         * Return a point representing a leaf that is 
         * grown outwards from the root.
         */
        private Point GetLeaf(Point parent, Point root)
        {
            //Declare an origo point
            Point origo = new Point(0, 0);
            //Declare a vector of length 1  from the root out on the positve x-axis.
            int[] xAxisVector = new int[2] { 1, 0 };

            //Transform to cooridninatesystem where root is origo
            parent = TransformCoordinates(root, parent);
            // parentVector is the vector between the origo and the parent-point
            int[] parentVector = GetVector(origo, parent);

            // the child vector is the vector between the root and the leaf we want calculate the coordinates for 
            int[] childVector = new int[2];

            // r is the length of the parent vector
            double r = GetVectorLength(parentVector);

            //Calculate the angle v1 between parent vector and the x-axis vector
            //using the dot-product.
            double v1 = GetAngleBetweenVectors(parentVector, xAxisVector);

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
                Point p = TransformCoordinates(currentTree.root, currentTree.points[i]);
                points[i] = Offset(p);
            }

            evalPoint = TransformCoordinates(currentTree.root, evalPoint);

            Stack<Point> s = GrahamScan(points);

            //check if a line (root-evalPoint) intersects with any of the lines representing the convex hull
            Point hullStart = s.Pop();
            Point p1 = hullStart;
            Point p2 = hullStart;//Needed to be assigned, should allways changed by while-loop below if nLeaves in tree>2
            Point origo = new Point(0, 0);
            while (s.Count() != 0)
            {
                p2 = s.Pop();
                if (LineSegmentIntersect(origo, evalPoint, p1, p2))
                {
                    return false;
                }
                p1 = p2;
            }

            if (LineSegmentIntersect(origo, evalPoint, hullStart, p2))
            {
                return false;
            }

            return true;
        }

        private bool LineSegmentIntersect(Point A, Point B, Point C, Point D)
        {
            //check if any of the vectors are the 0-vector
            if ((A.X == B.X && A.Y == B.Y) || (C.X == D.X) && (C.Y == D.Y))
            {
                return false;
            }
            //  Fail if the segments share an end point.

            if (A.X == C.X && A.Y == C.Y || B.X == C.X && B.Y == C.Y
                || A.X == D.X && A.Y == D.Y || B.X == D.X && B.Y == D.Y)
            {
                return false;
            }
            //Trnsform all points to a coordinate system where A is origo
            B = TransformCoordinates(A, B);
            C = TransformCoordinates(A, C);
            D = TransformCoordinates(A, D);
            A = TransformCoordinates(A, A);

            //  Discover the length of segment A-B.
            double distAB = GetVectorLength(GetVector(A, B));
            //Change to double
            double Cx = C.X; double Cy = C.Y;
            double Dx = D.X; double Dy = D.Y;
            //  (2) Rotate the system so that point B is on the positive X axis.
            double theCos = B.X / distAB;
            double theSin = B.Y / distAB;
            double newX = Cx * theCos + Cy * theSin;
            Cy = Cy * theCos - Cx * theSin; Cx = newX;
            newX = Dx * theCos + Dy * theSin;
            Dy = Dy * theCos - Dx * theSin; Dx = newX;
            //  Fail if segment C-D doesn't cross line A-B.
            if (Cy < 0 && Dy < 0 || Cy >= 0 && Dy >= 0) return false;
            //  (3) Discover the position of the intersection point along line A-B.
            double ABpos = Dx + (Cx - Dx) * Dy / (Dy - Cy);
            //  Fail if segment C-D crosses line A-B outside of segment A-B.
            if (ABpos < 0 || ABpos > distAB) return false;
            //The line segments intersect
            return true;
        }


        private Stack<Point> GrahamScan(Point[] points)
        {
            //Find point with lowex Y-coordinate

            Point minPoint = GetLowestPoint(points);
            //Declare a refpoint where the (minPoint,refPoint) is paral,ell to the x-axis
            Point refPoint = new Point(minPoint.X + 10, minPoint.Y);

            //Create a vector of GrahamPoints by calculate the angle a for each point in points
            //Where a is the angle beteen the two vectors (minPoint point) and (minPoint, Refpoint)

            int size = points.Count();

            GrahamPoint minGrahamPoint = new GrahamPoint(0, minPoint);
            GrahamPoint[] grahamPoints = new GrahamPoint[size];
            grahamPoints[0] = minGrahamPoint;
            int gpIndex = 1;
            for (int i = 0; i < points.Count(); i++)
            {
                if (!(points[i].X == minPoint.X && points[i].Y == minPoint.Y))
                {
                    double a = GetAngleBetweenVectors(GetVector(minPoint, refPoint), GetVector(minPoint, points[i]));
                    GrahamPoint grahamPoint = new GrahamPoint(a, points[i]);
                    grahamPoints[gpIndex] = grahamPoint;
                    gpIndex++;
                }
            }

            Array.Sort(grahamPoints);
            Stack<Point> s = new Stack<Point>();

            for (int i = 0; i < 3; ++i) s.Push(new Point(grahamPoints[i].point.X, grahamPoints[i].point.Y));

            Point top;
            Point nextToTop;
            for (int i = 3; i < grahamPoints.Count(); i++)
            {
                bool notPushed = true;
                while (notPushed)
                {
                    top = s.Pop();
                    nextToTop = s.Peek();
                    if (Ccw(nextToTop, top, grahamPoints[i].point) >= 0 || s.Count() < 2)
                    {
                        s.Push(top);
                        s.Push(grahamPoints[i].point);
                        notPushed = false;
                    }
                }
            }
            return s;
        }

        //Create struct to able to sort points by there angle a
        private struct GrahamPoint : IComparable<GrahamPoint>
        {
            public double angle;
            public Point point;

            public GrahamPoint(double angle, Point point)
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
        private int Ccw(Point p1, Point p2, Point p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        }

        /*
         * Return an angle in radians between teh vectors (p1,p2) and (q1,q2)
         * If any of the vectors is of length zero, return zero
         */
        private double GetAngleBetweenVectors(int[] P, int[] Q)
        {
            double lP = GetVectorLength(P);
            double lQ = GetVectorLength(Q);

            if (lP == 0 || lQ == 0) return 0;

            double v = Math.Acos((P[0] * Q[0] + P[1] * Q[1]) / (lP * lQ));
            return v;
        }

        private double GetVectorLength(int[] vector)
        {
            double l = Math.Sqrt(Math.Pow(vector[0], 2) + Math.Pow(vector[1], 2));
            return l;
        }

        private int[] GetVector(Point p1, Point p2)
        {
            int[] vector = new int[2] { p2.X - p1.X, p2.Y - p1.Y };
            return vector;
        }

        // Offsets the point 'p' 'distance' pixels from origo
        private Point Offset(Point p)
        {
            Point origo = new Point(0, 0);
            int[] op = GetVector(origo, p);
            double old_l = GetVectorLength(op);
            double new_l = old_l + offset_distance;
            double ratio = new_l / old_l;
            int x = Convert.ToInt32(p.X * ratio);
            int y = Convert.ToInt32(p.Y * ratio);
            return new Point(x, y);
        }

        //Transfor the point to a coordinate system where the argumet origo have the 
        //cooridinate (0,0)
        private Point TransformCoordinates(Point origo, Point point)
        {
            point.X -= origo.X;
            point.Y -= origo.Y;
            return point;
        }

        private Point GetLowestPoint(Point[] points)
        {
            Point minPoint = points[0];

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
    }

    internal class Cloud : FactoryElement
    {
        private int radius;

        internal Cloud(PaintTool paintTool, Point root)
            : base(paintTool)
        {
            this.points.Add(root);
            this.radius = 1;
        }

        internal void IncreaseRadius(int amount = 1)
        {
            radius += amount;
        }

        internal int GetRadius()
        {
            return radius;
        }
    }

    class CloudFactory : BaseFactory
    {
        private Cloud cloud;
        private Random randomNumberGenerator;

        public CloudFactory()
        {
            randomNumberGenerator = new Random();
        }

        public override void Add(Point p, PaintTool pt, bool alwaysAdd = true)
        {
            cloud = new Cloud(pt, p);
            GetRenderQueue().Enqueue(cloud);
        }

        public override void Grow()
        {
            if (GetRenderQueue().Count > 0)
            {
                var cloud = (Cloud)GetRenderQueue().First();
                int amount = randomNumberGenerator.Next(10);
                cloud.IncreaseRadius(amount);
                int radius = cloud.GetRadius();
                for (int i = 0; i < amount; i++)
                {
                    int x = randomNumberGenerator.Next(cloud.points[0].X - radius, cloud.points[0].X + radius);
                    int y = randomNumberGenerator.Next(cloud.points[0].Y - radius, cloud.points[0].Y + radius);
                    cloud.points.Add(new Point(x, y));
                }
            }
        }
    }
}
