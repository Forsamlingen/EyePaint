using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EyePaint
{

    internal class View
    {
        internal Image background;
        internal ImageObject imageObject;

        private SolidBrush bgBrush = new SolidBrush(Color.Black);
        private String RasterizerName;

        internal View(int width, int height)
        {
            //Initialize background
            background = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(background))
                g.FillRectangle(bgBrush, 0, 0, width, height);

            imageObject = new ImageObject(new Bitmap(background));

        }

        internal Image Rasterize(Queue<RenderObject> renderQueue)
        {
            foreach (RenderObject renderObject in renderQueue)
            {
                renderObject.Rasterize(ref imageObject);
            }
            return imageObject.image;
        }


        /**
         * Reset the image to the bakground color
         */
        internal void Clear()
        {
            imageObject.image = new Bitmap(background);
        }
    }

    //TODD Change Name!!
    internal class ImageObject
    {
        internal Image image;
        Pen pen = new Pen(Color.White);

        internal ImageObject(Image image)
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
