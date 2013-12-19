using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    class Cloud
    {
        internal readonly Color color;
        internal readonly List<Point> points;
        internal int radius;

        internal Cloud(Point root, Color color)
        {
            this.points = new List<Point> { root };
            this.color = color;
            this.radius = 1;
        }
    }

    class CloudFactory
    {
        internal readonly Stack<Cloud> clouds;
        Random randomNumberGenerator;

        internal CloudFactory()
        {
            clouds = new Stack<Cloud>();
            randomNumberGenerator = new Random();
        }

        internal void CreateCloud(Point root, Color color)
        {
            Cloud c = new Cloud(root, color);
            clouds.Push(c);
        }

        internal void GrowCloud(int amount)
        {
            var c = clouds.Peek();
            ++c.radius;

            for (int i = 0; i < amount; i++)
            {
                int x = randomNumberGenerator.Next(c.points[0].X - c.radius, c.points[0].X + c.radius);
                int y = randomNumberGenerator.Next(c.points[0].Y - c.radius, c.points[0].Y + c.radius);
                c.points.Add(new Point(x, y));                
            }
        }
    }
}
