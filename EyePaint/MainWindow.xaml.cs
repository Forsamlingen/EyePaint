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
using System.Windows.Media.Animation;
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
        Tree model;
        Tool tool;
        List<Tool> tools = new List<Tool> { Tools.Splatter, Tools.Flower, Tools.Neuron, Tools.Circle, Tools.Polygon, Tools.Snowflake };
        Color color;
        List<Color> colors = new List<Color> { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Black, Colors.White };

        public MainWindow()
        {
            InitializeComponent();
            rng = new Random();
            (paintTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(33), DispatcherPriority.Normal, (s, e) => updateDrawing(model, (RenderTargetBitmap)Raster.Source), Dispatcher)).Stop();
            tool = tools.First();
            color = colors.First();
        }

        void onWindowLoaded(object s, EventArgs e)
        {
            Raster.Source = new RenderTargetBitmap((int)(Drawing.ActualWidth), (int)(Drawing.ActualHeight), 96.0, 96.0, PixelFormats.Pbgra32);
        }

        void onContentRendered(object s, EventArgs e)
        {
            ColorButton.Background = new SolidColorBrush(color);
            ToolButton.Content = generateIcon();
        }

        Tree startDrawing(Point p)
        {
            var t = new Tree { root = p, leaves = new PointCollection(), parents = new Dictionary<Point, Point>() };
            for (int i = 0; i < rng.Next((tool.BranchCount + 1) / 2, tool.BranchCount + 1); ++i) t.leaves.Add(t.root);
            t.parents.Add(t.root, t.root);
            return t;
        }

        void updateDrawing(Tree t, RenderTargetBitmap drawing)
        {
            // Grow model.
            var newLeaves = new PointCollection();
            var newParents = new Dictionary<Point, Point>();
            var rotation = tool.Rotation * rng.NextDouble() * 2 * Math.PI;
            for (int i = 0; i < t.leaves.Count; ++i)
            {
                var q = (t.leaves.Count == 0) ? t.root : t.leaves[i];
                var angle = i * (2 * Math.PI / t.leaves.Count) + rotation + (1 - tool.BranchStraightness) * rng.NextDouble() * 2 * Math.PI;
                var p = new Point(
                    q.X + tool.BranchLength * Math.Cos(angle),
                    q.Y + tool.BranchLength * Math.Sin(angle)
                );
                if (!newParents.ContainsKey(p)) newParents.Add(p, q);
                newLeaves.Add(p);
            }

            // Render model.
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var centerBrush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                centerBrush.Opacity = rng.NextDouble() * tool.CenterOpacityVariety;
                var centerSize = rng.NextDouble() * tool.CenterSize;
                dc.DrawEllipse(centerBrush, null, t.root, centerSize, centerSize);

                var edges = new GeometryGroup();
                var edgesBrush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                edgesBrush.Opacity = tool.EdgesOpacity;
                var pen = new Pen(edgesBrush, tool.EdgesThickness);
                pen.EndLineCap = pen.StartLineCap = PenLineCap.Round;
                pen.LineJoin = PenLineJoin.Round;
                foreach (var leaf in t.leaves) edges.Children.Add(new LineGeometry(leaf, t.parents[leaf]));
                dc.DrawGeometry(null, pen, edges);

                var vertices = new GeometryGroup();
                var verticesBrush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                verticesBrush.Opacity = rng.NextDouble() * tool.VerticesOpacityVariety;
                foreach (var leaf in t.leaves)
                {
                    var r = rng.NextDouble();
                    var eg = new EllipseGeometry(leaf, tool.VerticesSize * (r + tool.VerticesSquashVariety * rng.NextDouble()), tool.VerticesSize * (r + tool.VerticesSquashVariety * rng.NextDouble()));
                    vertices.Children.Add(eg);
                }
                dc.DrawGeometry(verticesBrush, null, vertices);

                var hull = new StreamGeometry();
                var hullBrush = new SolidColorBrush(getRandomColor(color, tool.ColorVariety));
                hullBrush.Opacity = rng.NextDouble() * tool.HullOpacityVariety;
                using (var sgc = hull.Open())
                {
                    sgc.BeginFigure(t.leaves[0], true, true);
                    sgc.PolyLineTo(t.leaves, true, true);
                }
                dc.DrawGeometry(hullBrush, null, hull);
            }

            // Persist update.
            t.leaves.Clear();
            t.parents.Clear();
            foreach (var l in newLeaves) t.leaves.Add(l);
            foreach (var kvp in newParents) t.parents.Add(kvp.Key, kvp.Value);
            drawing.Render(dv);
        }

        Color getRandomColor(Color? baseColor = null, double randomness = 1)
        {
            var c = Color.FromScRgb(1.0f, (float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            return (baseColor.HasValue) ? baseColor.Value + Color.Multiply(c, (float)randomness) : c;
        }

        void saveDrawing()
        {
            var e = new PngBitmapEncoder();
            e.Frames.Add(BitmapFrame.Create((RenderTargetBitmap)Raster.Source));
            using (var fs = System.IO.File.OpenWrite("image.png")) e.Save(fs);
        }

        void onDrawingMouseDown(object s, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(Drawing);
            model = startDrawing(p);
            paintTimer.Start();
        }

        void onDrawingMouseUp(object s, MouseButtonEventArgs e)
        {
            paintTimer.Stop();
        }

        void onDrawingMouseLeave(object s, MouseEventArgs e)
        {
            paintTimer.Stop();
        }

        void onDrawingMouseMove(object s, MouseEventArgs e)
        {
            var p = e.GetPosition(Drawing);
            Canvas.SetLeft(PaintIndicator, p.X - PaintIndicator.ActualWidth / 2);
            Canvas.SetTop(PaintIndicator, p.Y - PaintIndicator.ActualHeight / 2);
            if (e.LeftButton != MouseButtonState.Pressed) { paintTimer.Stop(); return; } //TODO
            if ((this.model.root - p).Length > 50) model = startDrawing(p);
            paintTimer.Start();
        }

        void onStartButtonClick(object s, EventArgs e)
        {
            var b = s as Button;
            b.IsEnabled = false;
            b.Visibility = Visibility.Hidden;
            Blur.Radius = 0;
            GUI.IsEnabled = true;
        }

        void onSaveButtonClick(object s, EventArgs e)
        {
            if (new DialogWindow("Har du ritat färdigt och vill publicera bilden?", "Ja, jag är färdig.", "Nej, jag är inte färdig.").DialogResult.Value) saveDrawing();
            //TODO Test: else if (new DialogWindow("Vill du rensa bilden och börja om?", "Ja, jag vill börja om.", "Nej, jag vill gå tillbaka.").DialogResult.Value) Raster.Source = new RenderTargetBitmap((int)(Drawing.ActualWidth), (int)(Drawing.ActualHeight), 96.0, 96.0, PixelFormats.Pbgra32);
        }

        void onToolButtonClick(object s, EventArgs e)
        {
            tool = tools[(tools.IndexOf(tool) + 1) % tools.Count];
            (s as Button).Content = generateIcon();
        }

        void onColorButtonClick(object s, EventArgs e)
        {
            color = colors[(colors.IndexOf(color) + 1) % colors.Count];
            (s as Button).Background = new SolidColorBrush(color);
            ToolButton.Content = generateIcon();
        }

        void onRandomButtonClick(object s, EventArgs e)
        {
            color = getRandomColor();
            tool = new Tool
            {
                BranchCount = rng.Next(1, 100),
                BranchLength = rng.Next(1, 100),
                BranchStraightness = Math.Sqrt(rng.NextDouble()),
                Rotation = rng.NextDouble(),
                ColorVariety = rng.NextDouble(),
                CenterOpacityVariety = rng.NextDouble(),
                EdgesOpacity = rng.NextDouble(),
                VerticesOpacityVariety = rng.NextDouble(),
                HullOpacityVariety = Math.Pow(rng.NextDouble(), 2),
                CenterSize = Math.Sqrt(rng.Next(1, 100)),
                EdgesThickness = Math.Sqrt(rng.Next(1, 10)),
                VerticesSize = rng.Next(1, 20),
                VerticesSquashVariety = Math.Pow(rng.NextDouble(), 2),
            };

            (s as Button).Content = generateIcon();
            ToolButton.Content = generateIcon();
            ColorButton.Background = new SolidColorBrush(color);
        }

        void onInactivity(object s, EventArgs e)
        {
            if (new DialogWindow("Vill du rensa bilden och börja om?", "Ja, jag vill börja om.", "Nej, jag vill gå tillbaka.").DialogResult.Value)
            {
                var mw = new MainWindow();
                mw.Loaded += (_, __) => Close();
                mw.Show();
            }
        }

        Image generateIcon()
        {
            var toolIcon = new Image();
            toolIcon.Source = new RenderTargetBitmap((int)(ToolButton.ActualWidth), (int)(ToolButton.ActualHeight), 96.0, 96.0, PixelFormats.Pbgra32);
            var t = startDrawing(new Point(ToolButton.ActualWidth / 2, ToolButton.ActualHeight / 2));
            for (int i = 0; i < 10; ++i) updateDrawing(t, (RenderTargetBitmap)toolIcon.Source);
            return toolIcon;
        }

        void onLostFocus(object s, RoutedEventArgs e)
        {
            foreach (var sb in Resources.OfType<Storyboard>()) sb.Pause(); //TODO
        }


        void onGotFocus(object s, RoutedEventArgs e)
        {
            foreach (var sb in Resources.OfType<Storyboard>()) sb.Resume(); //TODO
        }

        void onGazePaintStart(object s, EventArgs e)
        {
            var p = Mouse.GetPosition(Application.Current.MainWindow); //TODO Don't use Application.Current.MainWindow.
            model = startDrawing(p);
            paintTimer.Start();
        }

        //TODO Implement.
        void onGazePaintStop(object s, EventArgs e)
        {
            paintTimer.Stop();
        }
    }
}
