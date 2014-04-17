using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EyePaint
{
    internal class View
    {
        internal Canvas canvas;

        internal View(RenderTargetBitmap image)
        {
            canvas = new Canvas(image);
        }

        internal void Rasterize(Queue<RenderObject> renderQueue)
        {
            while (renderQueue.Count() != 0)
            {
                RenderObject renderObject = renderQueue.Dequeue();
                renderObject.Rasterize(ref canvas); // TODO: Sometimes crashes program. Wrap in try/catch.
            }
        }

        internal void Clear()
        {
            Color whiteBg = Color.FromArgb(255, 255, 255, 255); 
        }

        internal void setBackGroundColorRandomly()
        {
            ColorTool ct = new ColorTool("random", "null", 0, 360, 0.9, 1, 0.9, 1);
            Color color = ct.getRandomShade(255);
            canvas.SetBackGroundColor(color);
        }
    }

    //TODO This is not a canvas. Rename the class. Better yet: merge the class's fields and methods with the View class.
    internal class Canvas
    {
        RenderTargetBitmap image;
        SolidColorBrush mySolidColorBrush = new SolidColorBrush();
        DrawingVisual drawingVisual = new DrawingVisual();
        Random rnd = new Random();

        internal Canvas(RenderTargetBitmap image)
        {
            this.image = image;
        }

        internal void DrawLine(Color color, int width, Point p1, Point p2)
        {
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            mySolidColorBrush.Color = color;
            Pen pen = new Pen(mySolidColorBrush, width);
            drawingContext.DrawLine(pen, p1, p2);
            drawingContext.Close();
            image.Render(drawingVisual);
        }

        internal void DrawElipse(Color color, int radius, Point point)
        {
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            mySolidColorBrush.Color = color;
            drawingContext.DrawEllipse(mySolidColorBrush, null, point, radius, radius);
            drawingContext.Close();
            image.Render(drawingVisual);
        }

        internal void DrawPolygon(Color color, Point[] vertices)
        {
            StreamGeometry streamGeometry = new StreamGeometry();
            using (StreamGeometryContext geometryContext = streamGeometry.Open())
            {
                geometryContext.BeginFigure(vertices[0], true, true); //TODO check if this can be made in a more safe way the [0]
                PointCollection points = new PointCollection();
                for (int i = 1; i < vertices.Count();i++)
                {
                    points.Add(vertices[i]);
                }
                geometryContext.PolyLineTo(points, true, true);
            }

            // Draw the polygon visual
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            
            drawingContext.DrawGeometry(mySolidColorBrush, null, streamGeometry);
            
            drawingContext.Close();
            image.Render(drawingVisual);
        }

        internal void SetBackGroundColor(Color color)
        {
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            byte red = (byte)rnd.Next(255);
            byte green = (byte)rnd.Next(255);
            byte blue = (byte)rnd.Next(255);
            mySolidColorBrush.Color = Color.FromArgb(255, red, green, blue);
            Size size = new Size(image.Width, image.Height);
            Rect rect = new Rect(size);
            drawingContext.DrawRectangle(mySolidColorBrush, null, rect);
            drawingContext.Close();
            image.Render(drawingVisual);
        }
    }
}
