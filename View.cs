using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EyePaint
{
    internal class View
    {
        internal Image blank;
        internal Canvas imageObject;

        SolidBrush bgBrush = new SolidBrush(Color.White);

        internal View(int width, int height)
        {
            blank = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(blank)) g.FillRectangle(bgBrush, 0, 0, width, height);
            imageObject = new Canvas(new Bitmap(blank));
        }

        internal Image Rasterize(Queue<RenderObject> renderQueue)
        {
            while (renderQueue.Count() != 0)
            {
                RenderObject renderObject = renderQueue.Dequeue();
                renderObject.Rasterize(ref imageObject);
            }

            return imageObject.image;
        }

        internal void Clear()
        {
            imageObject.image = new Bitmap(blank);
        }
    }

    //TODO Don't use "using (Graphics g...) for each method call. Dispose costs.
    //TODO This is not a canvas. Rename the class. Better yet: merge the class's fields and methods with the View class.
    internal class Canvas
    {
        internal Image image;
        Pen pen = new Pen(Color.White);

        internal Canvas(Image image)
        {
            this.image = image;
        }

        internal void DrawLine(Color color, int width, Point p1, Point p2)
        {
            pen.Color = color;
            pen.Width = width;

            using (Graphics g = Graphics.FromImage(image))
                g.DrawLine(pen, p1, p2);
        }

        internal void DrawElipse(Color color, int radius, Point point)
        {
            pen.Color = color;
            using (Graphics g = Graphics.FromImage(image))
                g.DrawEllipse(
                    pen,
                    point.X + radius,
                    point.Y + radius,
                    pen.Width,
                    pen.Width
                    );
        }

        internal void DrawPolygon(Color color, Point[] vertices)
        {
            SolidBrush b = new SolidBrush(color);
            using (Graphics g = Graphics.FromImage(image))
                g.FillPolygon(b, vertices);
        }
    }
}
