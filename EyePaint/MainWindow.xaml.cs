using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
        DispatcherTimer paintTimer;
        RenderTargetBitmap bitmap;

        Point root;
        PointCollection leaves;
        Dictionary<Point, Point> parents;

        Tool tool;
        List<Tool> tools = new List<Tool> { Tools.Splatter, Tools.Flower, Tools.Neuron, Tools.Circle, Tools.Polygon, Tools.Snowflake };

        Color color;
        List<Color> colors = new List<Color> { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Black, Colors.White };

        public MainWindow()
        {
            InitializeComponent();
            rng = new Random();
            (paintTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(33), DispatcherPriority.Normal, (s, e) => update(), Dispatcher)).Stop();
            tool = tools.First();
            color = colors.First();
        }

        void Window_Loaded(object s, EventArgs e)
        {
            bitmap = new RenderTargetBitmap((int)(Drawing.ActualWidth), (int)(Drawing.ActualHeight), 96.0, 96.0, PixelFormats.Pbgra32);
        }

        void start(Point p)
        {
            root = p;
            paintTimer.Start();
            leaves = new PointCollection();
            for (int i = 0; i < rng.Next(tool.BranchCount / 2, tool.BranchCount + 1); ++i) leaves.Add(root);
            parents = new Dictionary<Point, Point>();
            parents.Add(root, root);
        }

        void update()
        {
            // Grow model.
            var newLeaves = new PointCollection();
            var newParents = new Dictionary<Point, Point>();
            var rotation = tool.Rotation * rng.NextDouble() * 2 * Math.PI;
            for (int i = 0; i < leaves.Count; ++i)
            {
                Point q = (leaves.Count == 0) ? root : leaves[i];
                var angle = i * (2 * Math.PI / leaves.Count) + rotation + (1 - tool.BranchStraightness) * rng.NextDouble() * 2 * Math.PI;
                var p = new Point(
                    q.X + tool.BranchLength * Math.Cos(angle),
                    q.Y + tool.BranchLength * Math.Sin(angle)
                );
                if (!newParents.ContainsKey(p)) newParents.Add(p, q);
                newLeaves.Add(p);
            }

            // Render model.
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                SolidColorBrush brush;

                // Root
                brush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                brush.Opacity = rng.NextDouble() * tool.CenterOpacity;
                var s = rng.NextDouble() * tool.CenterSize;
                dc.DrawEllipse(brush, null, root, s, s);

                if (leaves.Count > 0)
                {
                    // Edges
                    var gg = new GeometryGroup();
                    brush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                    brush.Opacity = tool.EdgesOpacity;
                    var pen = new Pen(brush, tool.EdgesThickness);
                    pen.EndLineCap = pen.StartLineCap = PenLineCap.Round;
                    pen.LineJoin = PenLineJoin.Round;
                    foreach (var leaf in leaves) gg.Children.Add(new LineGeometry(leaf, parents[leaf]));
                    dc.DrawGeometry(null, pen, gg);

                    // Vertices
                    brush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                    brush.Opacity = rng.NextDouble() * tool.VerticesOpacity;
                    foreach (var leaf in leaves)
                    {
                        var r = rng.NextDouble();
                        var eg = new EllipseGeometry(leaf, tool.VerticesSize * (r + tool.VerticesSquash * rng.NextDouble()), tool.VerticesSize * (r + tool.VerticesSquash * rng.NextDouble()));
                        dc.DrawGeometry(brush, null, eg);
                    }

                    // Hull
                    StreamGeometry sg = new StreamGeometry();
                    brush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                    brush.Opacity = rng.NextDouble() * tool.HullOpacity;
                    using (StreamGeometryContext gc = sg.Open())
                    {
                        gc.BeginFigure(leaves[0], true, true);
                        gc.PolyLineTo(leaves, true, true);
                    }
                    dc.DrawGeometry(brush, null, sg);
                }
            }

            // Persist update.
            leaves = newLeaves;
            parents = newParents;
            bitmap.Render(dv);
            var image = new Image();
            image.Source = bitmap;
            Drawing.Children.Clear(); //TODO Implement undo history.
            Drawing.Children.Add(image);
        }

        Color getRandomColor(Color? baseColor = null, double randomness = 1)
        {
            var c = Color.FromScRgb(1.0f, (float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            return (baseColor.HasValue) ? baseColor.Value + Color.Multiply(c, (float)randomness) : c;
        }

        void save()
        {
            var e = new PngBitmapEncoder();
            e.Frames.Add(BitmapFrame.Create(bitmap));
            using (var fs = System.IO.File.OpenWrite("image.png"))
            {
                e.Save(fs);
            }
        }

        void Drawing_MouseDown(object s, MouseButtonEventArgs e)
        {
            start(e.GetPosition(Drawing));
        }

        void Drawing_MouseUp(object s, MouseButtonEventArgs e)
        {
            paintTimer.Stop();
        }

        void Drawing_MouseLeave(object s, MouseEventArgs e)
        {
            paintTimer.Stop();
        }

        void Drawing_MouseMove(object s, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) { paintTimer.Stop(); return; }
            if ((root - e.GetPosition(Drawing)).Length > 20) start(e.GetPosition(Drawing));
        }

        void SaveButton_Click(object s, RoutedEventArgs e)
        {
            if (new DialogWindow("Spara bilden?").DialogResult.Value) save();
        }

        void ToolButton_Click(object s, RoutedEventArgs e)
        {
            tool = tools[(tools.IndexOf(tool) + 1) % tools.Count];
        }

        void ColorButton_Click(object s, RoutedEventArgs e)
        {
            color = colors[(colors.IndexOf(color) + 1) % colors.Count];
        }

        void RandomButton_Click(object s, RoutedEventArgs e)
        {
            color = getRandomColor();
            tool = new Tool
            {
                BranchCount = rng.Next(1, 20),
                BranchLength = rng.Next(5, 100),
                BranchStraightness = Math.Sqrt(rng.NextDouble()),
                Rotation = rng.NextDouble(),
                ColorVariety = rng.NextDouble(),
                CenterOpacity = rng.NextDouble(),
                EdgesOpacity = rng.NextDouble(),
                VerticesOpacity = rng.NextDouble(),
                HullOpacity = Math.Pow(rng.NextDouble(), 2),
                CenterSize = Math.Sqrt(rng.Next(1, 100)),
                EdgesThickness = Math.Sqrt(rng.Next(1, 10)),
                VerticesSize = rng.Next(1, 20),
                VerticesSquash = Math.Pow(rng.NextDouble(), 2),
            };
        }

        void onInactivity(object s, EventArgs e)
        {
            if (new DialogWindow("Börja om?").DialogResult.Value)
            {
                (new MainWindow()).Show();
                Close();
            }
        }
    }
}
