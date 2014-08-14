using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;

namespace EyePaint
{
    public struct Tree
    {
        public Point root;
        public PointCollection leaves;
        public Dictionary<Point, Point> parents;
    }

    public struct Shape
    {
        public int maxBranches, strokeThickness;
        public double branchStepLength, branchStraightness, generationRotation, colorVariety, verticesSize, verticesSquashVariety, centerSize, centerOpacity, edgesOpacity, verticesOpacity, hullOpacity;
    }

    public class Presets
    {
        public HashSet<Shape> shapes;
        public HashSet<Color> colors;

        public static Presets Load()
        {
            using (var fs = new FileStream("Presets.json", FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var jrwf = JsonReaderWriterFactory.CreateJsonReader(fs, XmlDictionaryReaderQuotas.Max))
                {
                    return (Presets)(new DataContractJsonSerializer(typeof(Presets))).ReadObject(jrwf);
                }
            }
        }

        public void Save()
        {
            using (var fs = new FileStream("Presets.json", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var xdw = JsonReaderWriterFactory.CreateJsonWriter(fs))
                {
                    (new DataContractJsonSerializer(GetType())).WriteObject(xdw, this);
                }
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rng;
        DateTime time;
        TimeSpan timePainted;
        DispatcherTimer paintTimer;
        Tree model;
        Point gaze;
        Shape shape;
        Color color;
        Presets presets;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            var p = new Presets();
            p.colors = new HashSet<Color> { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow, Colors.White, Colors.Black };
            p.shapes = new HashSet<Shape> { new Shape { maxBranches = 100, strokeThickness = 1, branchStepLength = 10, branchStraightness = 1, generationRotation = 1, colorVariety = 1, verticesSize = 10, verticesSquashVariety = 0, centerSize = 100, centerOpacity = 1, edgesOpacity = 1, verticesOpacity = 1, hullOpacity = 1 } };
            p.Save();
            KeyDown += (s, e) => { presets.shapes.Add(shape); presets.Save(); };
#endif
        }

        void onContentRendered(object s, EventArgs e)
        {
            App.SetCursorPos(0, 0);
            rng = new Random();
            presets = Presets.Load();
            color = presets.colors.ElementAt(rng.Next(presets.colors.Count));
            shape = presets.shapes.ElementAt(rng.Next(presets.shapes.Count));
            (paintTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(33), DispatcherPriority.Normal, (_, __) => updateDrawing(ref model, (RenderTargetBitmap)Raster.Source), Dispatcher)).Stop();
            clearDrawing();
            updateIcons();
        }

        void onPreviewMouseDown(object s, MouseButtonEventArgs e)
        {
            ((Storyboard)FindResource("InactivityAnimation")).Seek(TimeSpan.Zero);
        }

        void onCanvasMouseDown(object s, MouseButtonEventArgs e)
        {
            gaze = e.GetPosition(s as Canvas);
            startPainting();
            ((Storyboard)GazePaintMarker.FindResource("GazePaintAnimation")).Stop();
        }

        void onCanvasMouseUp(object s, MouseButtonEventArgs e)
        {
            stopPainting();
            ((Storyboard)GazePaintMarker.FindResource("GazePaintAnimation")).Begin();
        }

        void onCanvasMouseEnter(object s, MouseEventArgs e)
        {
            ((Storyboard)GazePaintMarker.FindResource("GazePaintAnimation")).Begin();
        }

        void onCanvasMouseLeave(object s, MouseEventArgs e)
        {
            stopPainting();
            ((Storyboard)GazePaintMarker.FindResource("GazePaintAnimation")).Stop();
        }

        void onCanvasMouseMove(object s, MouseEventArgs e)
        {
            var p = e.GetPosition(s as Canvas);
            ((Storyboard)FindResource("InactivityAnimation")).Seek(TimeSpan.Zero);
            Canvas.SetLeft(GazePaintMarker, p.X - GazePaintMarker.ActualWidth / 2);
            Canvas.SetTop(GazePaintMarker, p.Y - GazePaintMarker.ActualHeight / 2);
            if ((gaze - p).Length > 50)
            {
                gaze = p;
                if (paintTimer.IsEnabled) model = createTree(gaze);
                var sb = (Storyboard)GazePaintMarker.FindResource("GazePaintAnimation");
                if (sb.GetCurrentState() == ClockState.Filling) stopPainting();
                if (sb.GetCurrentState() != ClockState.Stopped) sb.Seek(TimeSpan.Zero);
            }
            if (!paintTimer.IsEnabled) gaze = p;
        }

        void onStartButtonClick(object s, EventArgs e)
        {
            (s as Button).Visibility = Visibility.Hidden;
            ((Storyboard)FindResource("InactivityAnimation")).Begin();
            Blur.Radius = 0;
            PaintControls.IsEnabled = true;
        }

        void onShapeButtonClick(object s, EventArgs e)
        {
            if (timePainted > TimeSpan.FromSeconds(10)) presets.shapes.Add(shape);
            timePainted = TimeSpan.Zero;
            var candidates = presets.shapes.Where(c => !c.Equals(shape)).ToList();
            shape = (rng.NextDouble() <= 0.5 && candidates.Count > 1) ? candidates.ElementAt(rng.Next(candidates.Count)) : new Shape
            {
                maxBranches = rng.Next(1, 101),
                strokeThickness = (int)Math.Sqrt(rng.Next(1, 10)),
                branchStepLength = Math.Sqrt(rng.Next(10, 101)),
                branchStraightness = Math.Sqrt(rng.NextDouble()),
                generationRotation = rng.NextDouble(),
                colorVariety = rng.NextDouble(),
                verticesSize = Math.Sqrt(rng.Next(1, 51)),
                verticesSquashVariety = Math.Pow(rng.NextDouble(), 2),
                centerSize = Math.Sqrt(rng.Next(1, 101)),
                centerOpacity = Math.Pow(rng.NextDouble(), 2),
                edgesOpacity = rng.NextDouble(),
                verticesOpacity = rng.NextDouble(),
                hullOpacity = Math.Pow(rng.NextDouble(), 2),
            };
            updateIcons();
        }

        void onColorButtonClick(object s, EventArgs e)
        {
            if (timePainted > TimeSpan.FromSeconds(10)) presets.colors.Add(color);
            timePainted = TimeSpan.Zero;
            var candidates = presets.colors.Where(c => c != color).ToList();
            color = (rng.NextDouble() <= 0.5 && candidates.Count > 1) ? candidates.ElementAt(rng.Next(candidates.Count)) : createColor();
            updateIcons();
        }

        void onInactivity(object s, EventArgs e)
        {
            (new MainWindow()).Show();
            Close();
        }

        void onGazePaint(object s, EventArgs e)
        {
            startPainting();
        }

        void startPainting()
        {
            model = createTree(gaze);
            paintTimer.Start();
            time = DateTime.Now;
        }

        void stopPainting()
        {
            paintTimer.Stop();
            timePainted += DateTime.Now - time;
        }

        Tree createTree(Point p)
        {
            var t = new Tree { root = p, leaves = new PointCollection(), parents = new Dictionary<Point, Point>() };
            for (int i = 0; i < rng.Next((shape.maxBranches + 1) / 2, shape.maxBranches + 1); ++i) t.leaves.Add(t.root);
            t.parents[t.root] = t.root;
            return t;
        }

        void updateDrawing(ref Tree model, RenderTargetBitmap drawing)
        {
            // Grow model.
            var newLeaves = new PointCollection();
            var newParents = new Dictionary<Point, Point>();
            var rotation = shape.generationRotation * rng.NextDouble() * 2 * Math.PI;
            for (int i = 0; i < model.leaves.Count; ++i)
            {
                var q = (model.leaves.Count == 0) ? model.root : model.leaves[i];
                var angle =
                    i * (2 * Math.PI / model.leaves.Count)
                    + rotation
                    + (1 - shape.branchStraightness) * rng.NextDouble() * 2 * Math.PI;
                var p = new Point(
                    q.X + shape.branchStepLength * Math.Cos(angle),
                    q.Y + shape.branchStepLength * Math.Sin(angle)
                );
                newParents[p] = q;
                newLeaves.Add(p);
            }

            // Render model.
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var centerSize = rng.NextDouble() * shape.centerSize;
                var centerBrush = new SolidColorBrush(createColor(color, shape.colorVariety));
                centerBrush.Opacity = rng.NextDouble() * shape.centerOpacity;
                var centerPen = new Pen(new SolidColorBrush(createColor(color, shape.colorVariety)), 1);
                centerPen.Brush.Opacity = rng.NextDouble() * shape.centerOpacity;
                dc.DrawEllipse(centerBrush, centerPen, model.root, centerSize, centerSize);

                var edges = new GeometryGroup();
                var edgesPen = new Pen(new SolidColorBrush(createColor(color, shape.colorVariety)), shape.strokeThickness);
                edgesPen.Brush.Opacity = shape.edgesOpacity;
                foreach (var leaf in model.leaves) edges.Children.Add(new LineGeometry(leaf, model.parents[leaf]));
                dc.DrawGeometry(null, edgesPen, edges);

                var vertices = new GeometryGroup();
                var verticesBrush = new SolidColorBrush(createColor(color, shape.colorVariety));
                verticesBrush.Opacity = rng.NextDouble() * shape.verticesOpacity;
                var verticesPen = new Pen(verticesBrush, 1);
                foreach (var leaf in model.leaves)
                {
                    var r = rng.NextDouble();
                    var eg = new EllipseGeometry(leaf, shape.verticesSize * (r + shape.verticesSquashVariety * rng.NextDouble()), shape.verticesSize * (r + shape.verticesSquashVariety * rng.NextDouble()));
                    vertices.Children.Add(eg);
                }
                dc.DrawGeometry(verticesBrush, verticesPen, vertices);

                var hull = new StreamGeometry();
                var hullBrush = new SolidColorBrush(createColor(color, shape.colorVariety));
                hullBrush.Opacity = rng.NextDouble() * shape.hullOpacity;
                var hullPen = new Pen(hullBrush, shape.strokeThickness);
                using (var sgc = hull.Open())
                {
                    sgc.BeginFigure(model.leaves[0], true, true);
                    sgc.PolyLineTo(model.leaves, true, true);
                }
                dc.DrawGeometry(hullBrush, hullPen, hull);
            }

            // Persist update.
            model.leaves.Clear();
            model.parents.Clear();
            foreach (var l in newLeaves) model.leaves.Add(l);
            foreach (var kvp in newParents) model.parents.Add(kvp.Key, kvp.Value);
            drawing.Render(dv);

        }

        void saveDrawing()
        {
            var e = new PngBitmapEncoder();
            e.Frames.Add(BitmapFrame.Create((RenderTargetBitmap)Raster.Source));
            using (var fs = System.IO.File.OpenWrite("image.png")) e.Save(fs);
        }

        void clearDrawing()
        {
            Raster.Source = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32);
        }

        Color createColor(Color? baseColor = null, double randomness = 1)
        {
            var H = rng.NextDouble();
            var S = 1.0;
            var V = 1.0;
            byte R, G, B;

            if (S == 0)
            {
                R = (byte)(V * 255);
                G = (byte)(V * 255);
                B = (byte)(V * 255);
            }
            else
            {
                var h = (H * 6 == 6) ? 0 : H * 6;
                var i = (int)Math.Floor(h);
                var d1 = V * (1 - S);
                var d2 = V * (1 - S * (h - i));
                var d3 = V * (1 - S * (1 - (h - i)));

                double r, g, b;
                switch (i)
                {
                    case 0: r = V; g = d3; b = d1; break;
                    case 1: r = d2; g = V; b = d1; break;
                    case 2: r = d1; g = V; b = d3; break;
                    case 3: r = d1; g = d2; b = V; break;
                    case 4: r = d3; g = d1; b = V; break;
                    default: r = V; g = d1; b = d2; break;
                }

                R = (byte)(r * 255);
                G = (byte)(g * 255);
                B = (byte)(b * 255);
            }

            var c = Color.FromRgb(R, G, B);
            return (baseColor.HasValue) ? baseColor.Value + Color.Multiply(c, (float)randomness) : c;
        }

        void updateIcons()
        {
            ColorButton.Background = new SolidColorBrush(color);
            var toolIcon = new Image();
            toolIcon.Source = new RenderTargetBitmap((int)(ShapeButton.ActualWidth), (int)(ShapeButton.ActualHeight), 96.0, 96.0, PixelFormats.Pbgra32);
            var t = createTree(new Point(ShapeButton.ActualWidth / 2, ShapeButton.ActualHeight / 2));
            for (int i = 0; i < 10; ++i) updateDrawing(ref t, (RenderTargetBitmap)toolIcon.Source);
            ShapeButton.Content = toolIcon;
        }
    }
}
