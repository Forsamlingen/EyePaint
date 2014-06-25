using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rng;
        RenderTargetBitmap bitmap;
        Point previous;
        List<string> shapeKeys, brushKeys;
        string shapeKey, brushKey;

        public MainWindow()
        {
            InitializeComponent();
            rng = new Random();
            shapeKeys = new List<String> { "SplatterDot", "InstantSquare"};
            brushKeys = new List<String> { "Green", "Red"};
            shapeKey = shapeKeys.First();
            brushKey = brushKeys.First();
        }

        void addShape(Canvas c, Point p)
        {
            var s = (Shape)FindResource(shapeKey);
            var b = (Brush)FindResource(brushKey);
            if (b is SolidColorBrush)
            {
                var color = (b as SolidColorBrush).Color;
                color = Color.FromArgb(color.A, (byte)(color.R * (rng.NextDouble() + 0.5)), (byte)(color.B * (rng.NextDouble() + 0.5)), (byte)(color.G * (rng.NextDouble() + 0.5)));
                (b as SolidColorBrush).Color = color;
            }
            s.Fill = b;
            s.SetValue(RenderTransformOriginProperty, new Point(rng.NextDouble(), rng.NextDouble()));
            Canvas.SetLeft(s, p.X - s.Width / 2);
            Canvas.SetTop(s, p.Y - s.Height / 2);
            c.Children.Add(s);
            if (c.Children[0] is Polyline) (c.Children[0] as Polyline).Points.Add(p); //TODO Test.
            if (c.Children.Count > 1000) rasterize(c); //TODO Calculate limit dynamically based on available memory.
        }

        void onShapeFadedOut(object s, EventArgs e)
        {
            Drawing.Children.Remove((Shape)s);
        }

        void rasterize(Canvas c)
        {
            bitmap = new RenderTargetBitmap((int)(c.ActualWidth), (int)(c.ActualHeight), 96.0, 96.0, PixelFormats.Pbgra32);
            bitmap.Render(c);
            c.Children.Clear();
            var ib = new ImageBrush(bitmap);
            c.Background = ib;
        }

        void save()
        {
            rasterize(Drawing);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var fileStream = File.OpenWrite("image.png"))
            {
                encoder.Save(fileStream);
            }
        }

        void clear()
        {
            Drawing.Children.Clear();
            Drawing.Background = Brushes.Black; //TODO Random background color.
        }

        void nextShape()
        {
            shapeKey = shapeKeys[(shapeKeys.IndexOf(shapeKey) + 1) % shapeKeys.Count];
        }

        void nextBrush()
        {
            brushKey = brushKeys[(brushKeys.IndexOf(brushKey) + 1) % brushKeys.Count];
        }

        void Drawing_MouseDown(object s, MouseButtonEventArgs e)
        {
            var c = s as Canvas;
            addShape(c, e.GetPosition(c));
        }

        void Drawing_MouseUp(object s, MouseButtonEventArgs e)
        {
            rasterize(Drawing);
        }

        void Drawing_MouseMove(object s, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            var c = s as Canvas;
            var p = e.GetPosition(c);
            if ((previous - p).Length > 4) addShape(c, e.GetPosition(c)); //TODO Set distance as spacing in paint tools.
            previous = p;
        }

        void ClearButton_Click(object s, RoutedEventArgs e)
        {
            if (new DialogWindow("Är du säker att du vill rensa bilden?").DialogResult.Value) clear();
        }

        void SaveButton_Click(object s, RoutedEventArgs e)
        {
            if (new DialogWindow("Vill du spara bilden?").DialogResult.Value) save();
        }

        void ShapeButton_Click(object s, RoutedEventArgs e)
        {
            nextShape();
        }

        void BrushButton_Click(object s, RoutedEventArgs e)
        {
            nextBrush();
        }
    }
}
