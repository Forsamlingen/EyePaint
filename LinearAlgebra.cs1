using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace EyePaint
{
    //TODO
    static class LinearAlgebra
    {
        /**
         * Return a stack with the points in the convex hull of the tree.
         * If number of leaves in the tree is less then 3 an empty stack is returned
         **/
        internal Stack<Point> GetConvexHull(Tree tree)
        {
            if (tree.points.Count < 3) return new Stack<Point>();
            else return GrahamScan(tree.points.ToArray());
        }

        // Return a point representing a leaf that is grown outwards from the root.
        Point getLeaf(Point parent, Point root)
        {
            parent = transformCoordinates(root, parent); // Move parent so that root is origo.
            Point origo = new Point(0, 0);
            int[] parentVector = getVector(origo, parent);
            int[] xAxisVector = new int[2] { 1, 0 };
            
            double r = getVectorLength(parentVector); // Euclidean distance.
            double maxAngle = Math.Atan(edgeLength / r); // Largest allowed angle going outwards from tree root.

            double v1 = getAngleBetweenVectors(parentVector, xAxisVector); // Scalar product.
            if (parentVector[1] < 0) v1 = 2 * Math.PI - v1; // Negative direction? Go anti-clockwise.
            double v2 = random.NextDouble() * 2 * maxAngle + (v1 - maxAngle); // Angle between child vector and X-axis. Outward bound from tree root.

            // In a triangle with corners at origo, the parent point and the leaf: calculate the triangle angles.
            double a1 = v2 - v1;
            double a2 = Math.Asin(r * Math.Sin(a1) / edgeLength);
            double a3 = Math.PI - a2 - a1;

            double c = edgeLength * Math.Sin(a3) / Math.Sin(a1); // Length of child vector.

            int[] childVector = { (int)Math.Round(c * Math.Cos(v2)), (int)Math.Round(c * Math.Sin(v2)) };

            // Calculate the coordinates for the leaf by transforming the child vector back to the original coordinate system
            return new Point(childVector[0] + root.X, childVector[1] + root.Y);
        }

        bool isInsideTree(Point p)
        {
            if (history.Count == 0) return false;

            var currentTree = (Tree)history.Peek();
            if (currentTree.points.Count < 3) return false;
            var root = currentTree.points[0];

            Point[] points = currentTree.points.ToArray(); //TODO Make sure to copy, not reference.

            // Transform leaves to a coordinate system where root is origo.
            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = offset(transformCoordinates(root, points[i]));
            }

            p = transformCoordinates(root, p);

            Stack<Point> s = GrahamScan(points);

            //check if a line (root-evalPoint) intersects with any of the lines representing the convex hull
            Point hullStart = s.Pop();
            Point p1 = hullStart;
            Point p2 = hullStart;//Needed to be assigned, should allways changed by while-loop below if nLeaves in tree>2
            Point origo = new Point(0, 0);
            while (s.Count() != 0)
            {
                p2 = s.Pop();
                if (LineSegmentIntersect(origo, p, p1, p2)) return false;
                p1 = p2;
            }

            if (LineSegmentIntersect(origo, p, hullStart, p2)) return false;

            return true;
        }

        bool LineSegmentIntersect(Point A, Point B, Point C, Point D)
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
            B = transformCoordinates(A, B);
            C = transformCoordinates(A, C);
            D = transformCoordinates(A, D);
            A = transformCoordinates(A, A);

            //  Discover the length of segment A-B.
            double distAB = getVectorLength(getVector(A, B));
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

        struct GrahamPoint : IComparable<GrahamPoint>
        {
            internal readonly double angle;
            internal readonly Point point;

            public GrahamPoint(double a, Point p)
            {
                angle = a;
                point = p;
            }

            public int CompareTo(GrahamPoint p)
            {
                if (angle > p.angle) return 1;
                else if (angle < p.angle) return -1;
                else return 0;
            }
        }

        Stack<Point> GrahamScan(Point[] points)
        {
            //Find point with lowex Y-coordinate
            Point minPoint = getLowestPoint(points);

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
                    double a = getAngleBetweenVectors(getVector(minPoint, refPoint), getVector(minPoint, points[i]));
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
                    if (ccw(nextToTop, top, grahamPoints[i].point) >= 0 || s.Count() < 2)
                    {
                        s.Push(top);
                        s.Push(grahamPoints[i].point);
                        notPushed = false;
                    }
                }
            }
            return s;
        }

        // Use cross-product to calculate if three points are a counter-clockwise. 
        // They are counter-clockwise if ccw > 0, clockwise if
        // ccw < 0, and collinear if ccw == 0
        int ccw(Point p1, Point p2, Point p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        }

        /*
         * Return an angle in radians between teh vectors (p1,p2) and (q1,q2)
         * If any of the vectors is of length zero, return zero
         */
        double getAngleBetweenVectors(int[] P, int[] Q)
        {
            double lP = getVectorLength(P);
            double lQ = getVectorLength(Q);

            if (lP == 0 || lQ == 0) return 0;

            double v = Math.Acos((P[0] * Q[0] + P[1] * Q[1]) / (lP * lQ));
            return v;
        }

        int[] getVector(Point p1, Point p2)
        {
            return new int[2] { p2.X - p1.X, p2.Y - p1.Y };
        }

        // Calculate Euclidean distance from origo.
        double getVectorLength(int[] vector)
        {
            int squareSum = 0;
            foreach (var dimension in vector) squareSum += dimension * dimension;
            return Math.Sqrt(squareSum);
        }

        // Offsets the point 'p' 'distance' pixels from origo
        Point offset(Point p)
        {
            Point origo = new Point(0, 0);
            int[] op = getVector(origo, p);
            double old_l = getVectorLength(op);
            double new_l = old_l + offsetDistance;
            double ratio = new_l / old_l;
            int x = Convert.ToInt32(p.X * ratio);
            int y = Convert.ToInt32(p.Y * ratio);
            return new Point(x, y);
        }

        //Transfor the point to a coordinate system where the argumet origo have the 
        //cooridinate (0,0)
        Point transformCoordinates(Point origo, Point point)
        {
            point.X -= origo.X;
            point.Y -= origo.Y;
            return point;
        }

        // Find the point with: 1) the smallest Y-coordinate; and 2) the smallest X-coordinate.
        Point getLowestPoint(Point[] points)
        {
            int y = points.Min(p => p.Y);
            int x = points.Where(p => p.Y == y).Min(p => p.X);
            return new Point(x, y); //TODO Don't create a new Point object.
        }
    }
}
