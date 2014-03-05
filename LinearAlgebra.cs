using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    public static class LinearAlgebra
    {
        /**
         *Evaluates if the line between point A and B and the line between point C and D intersects
         *
         **/
        public static bool LineSegmentIntersect(Point A, Point B, Point C, Point D)
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
            double distAB = Get2DVectorLength(GetVector(A, B));
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
        /**
         * Return the convex hull for the given list of points 
         **/
        public static Stack<Point> GetConvexHull(Point[] points)
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

            s.Push(GrahamPointToPoint(grahamPoints[0]));
            s.Push(GrahamPointToPoint(grahamPoints[1]));
            s.Push(GrahamPointToPoint(grahamPoints[2]));

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
        struct GrahamPoint : IComparable<GrahamPoint>
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
        static int Ccw(Point p1, Point p2, Point p3)
        {
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
        }
        /*
         * Return an angle in radians between teh vectors (p1,p2) and (q1,q2)
         * If any of the vectors is of length zero, return zero
         */
        public static double GetAngleBetweenVectors(int[] P, int[] Q)
        {
            double lP = Get2DVectorLength(P);
            double lQ = Get2DVectorLength(Q);

            if (lP == 0 || lQ == 0) return 0;

            double v = Math.Acos((P[0] * Q[0] + P[1] * Q[1]) / (lP * lQ));
            return v;
        }

        /**
         * Return the length of the vector specified vector.
         **/
        public static double Get2DVectorLength(int[] vector)
        {
            double l = Math.Sqrt(Math.Pow(vector[0], 2) + Math.Pow(vector[1], 2));
            return l;
        }

        /**
         * Return a vector between the the two given points
         **/
        public static int[] GetVector(Point p1, Point p2)
        {
            int[] vector = new int[2] { p2.X - p1.X, p2.Y - p1.Y };
            return vector;
        }

        // Offsets the point 'p' 'distance' pixels from origo
        public static Point Offset(Point p, int offset)
        {
            Point origo = new Point(0, 0);
            int[] op = GetVector(origo, p);
            double old_l = Get2DVectorLength(op);
            double new_l = old_l + offset;
            double ratio = new_l / old_l;
            int x = Convert.ToInt32(p.X * ratio);
            int y = Convert.ToInt32(p.Y * ratio);
            return new Point(x, y);
        }

        /**
         * Transform the point to a coordinate system where the argumet origo have the cooridinate (0,0)
        **/
        public static Point TransformCoordinates(Point origo, Point point)
        {
            point.X -= origo.X;
            point.Y -= origo.Y;
            return point;
        }

        static Point GetLowestPoint(Point[] points)
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
        static Point GrahamPointToPoint(GrahamPoint grahamPoint)
        {
            Point point = new Point(grahamPoint.point.X, grahamPoint.point.Y);
            return point;
        }
    }
}
