using FlickrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EyePaint
{
    /// <summary>
    /// Model representing a paint stroke in the application.
    /// </summary>
    struct Tree
    {
        public Point Root;
        public PointCollection Leaves;
        public Dictionary<Point, Point> Parents;

        public Tree(Point p, int maxBranches, ref Random rng)
        {
            Root = p;
            Leaves = new PointCollection();
            Parents = new Dictionary<Point, Point>();
            for (int i = 0; i < rng.Next((maxBranches + 1) / 2, maxBranches + 1); ++i) Leaves.Add(Root);
            Parents[Root] = Root;
        }
    }

    /// <summary>
    /// Graphical interpretation parameters of paint strokes in the application.
    /// </summary>
    struct Shape
    {
        //TODO Add getters/setters with validation instead of using the public access modifier.
        public double MaxBranches, BranchStepLength, BranchStraightness, GenerationRotation, ColorVariety, VerticesSize, VerticesSquashVariety, CenterSize, CenterOpacity, EdgesOpacity, VerticesOpacity, HullOpacity;
    }

    /// <summary>
    /// Lets the user draw different shapes and colors.
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rng = new Random();
        DispatcherTimer paintTimer;
        Tree model;
        Point gaze;
        List<Point> gazes = new List<Point>();
        Shape shape;
        Color color;
        DateTime time;
        HashSet<Shape> shapes = new HashSet<Shape>();
        HashSet<Color> colors = new HashSet<Color>();
        Dictionary<Shape, TimeSpan> shapeUsage = new Dictionary<Shape, TimeSpan>();
        Dictionary<Color, TimeSpan> colorUsage = new Dictionary<Color, TimeSpan>();

        public MainWindow()
        {
            InitializeComponent();

            // Render clock. Note: single-threaded. Approximately 20 FPS.
            (paintTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Render, (_, __) => paint(ref this.model, Raster.Source as RenderTargetBitmap), Dispatcher)).Stop();
        }

        void onLoaded(object s, RoutedEventArgs e)
        {
            // Choose initial shape and color by simulating a button click.
            ShapeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            ColorButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        void onContentRendered(object s, EventArgs e)
        {
            // Initialize drawing.
            Raster.Source = createDrawing((int)ActualWidth, (int)ActualHeight);

            // Perform initial offset calibration.
            new CalibrationWindow();

            IsEnabled = ((App)Application.Current).Tracking;
            ((App)Application.Current).TrackingChanged += (_s, _e) => Dispatcher.Invoke(() => { IsEnabled = _e.Tracking; Focus(); });
        }

        void onKeyDown(object s, KeyEventArgs e)
        {
            if (e.IsRepeat) return;
            switch (e.Key)
            {
                case Key.Escape: // Show admin settings
                    (new SettingsWindow()).ShowDialog();
                    break;
                case Key.C: // Change color
                    ColorButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    break;
                case Key.S: // Change shape
                    ShapeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    break;
                case Key.P: // Publish drawing
                    if (new CountdownWindow().DialogResult.Value) { IsEnabled = false; ((Storyboard)FindResource("SaveDrawingAnimation")).Begin(); }
                    break;
                case Key.R: // Reset session
                    if (new CountdownWindow().DialogResult.Value) { IsEnabled = false; ((App)Application.Current).Reset(); }
                    break;
            }
        }

        void onCanvasMouseDown(object s, MouseButtonEventArgs e)
        {
            gaze = calculateGaze(e.GetPosition(s as Canvas));
            startPainting();
            GazeMarker.Visibility = Visibility.Hidden;
            ((Storyboard)GazeMarker.FindResource("GazePaintAnimation")).Stop();
        }

        void onCanvasMouseUp(object s, MouseButtonEventArgs e)
        {
            stopPainting();
            GazeMarker.Visibility = Visibility.Visible;
            ((Storyboard)GazeMarker.FindResource("GazePaintAnimation")).Begin();
        }

        void onCanvasMouseEnter(object s, MouseEventArgs e)
        {
            ((Storyboard)GazeMarker.FindResource("GazePaintAnimation")).Begin();
        }

        void onCanvasMouseLeave(object s, MouseEventArgs e)
        {
            stopPainting();
            ((Storyboard)GazeMarker.FindResource("GazePaintAnimation")).Stop();
        }

        void onCanvasMouseMove(object s, MouseEventArgs e)
        {
            var p = calculateGaze(e.GetPosition(s as Canvas));

            Canvas.SetLeft(GazeMarker, p.X - GazeMarker.ActualWidth / 2);
            Canvas.SetTop(GazeMarker, p.Y - GazeMarker.ActualHeight / 2);

            if (paintTimer.IsEnabled && (model.Root - p).Length > Properties.Settings.Default.Spacing) model = new Tree(p, (int)shape.MaxBranches, ref rng);
            if ((gaze - p).Length > Properties.Settings.Default.Spacing / 2)
            {
                var sb = (Storyboard)GazeMarker.FindResource("GazePaintAnimation");
                switch (sb.GetCurrentState())
                {
                    case ClockState.Active: sb.Seek(TimeSpan.Zero); break;
                    case ClockState.Filling: sb.Seek(TimeSpan.Zero); stopPainting(); break;
                }
            }
            gaze = p;
        }

        void onPublishButtonClick(object s, EventArgs e)
        {
            if (new DialogWindow().DialogResult.Value) { IsEnabled = false; ((Storyboard)FindResource("SaveDrawingAnimation")).Begin(); }
        }

        void onResetButtonClick(object s, EventArgs e)
        {
            if (new DialogWindow().DialogResult.Value) { IsEnabled = false; ((App)Application.Current).Reset(); }
        }

        void onShapeButtonClick(object s, EventArgs e)
        {
            // Sort shapes by usage.
            shapes.OrderBy(sh => shapeUsage[sh]);

            // Remove underused shapes.
            foreach (var sh in shapes.ToList())
            {
                if (shapeUsage[sh].Seconds < 0.1 * shapeUsage.Max(kvp => kvp.Value).Seconds || shapeUsage[sh] == TimeSpan.Zero)
                {
                    shapes.Remove(sh);
                    shapeUsage.Remove(sh);
                }
            }

            // Determine whether to pick a previous shape or generate a new.
            if (shapes.Count > 0 && rng.NextDouble() <= 0.01 * shapes.Count - 0.1)
            {
                // Pick a previously used shape.
                shape = shapes.ElementAt((shapes.ToList().IndexOf(shape) + 1) % shapes.Count); //TODO Verify that the index applies to the HashSet.
            }
            else
            {
                // Generate a new shape.
                var maxBranches = rng.Next(1, 100);
                var branchStepLength = Math.Sqrt(rng.Next(1, 1001));
                var branchStraightness = Math.Sqrt(rng.NextDouble());
                var generationRotation = rng.NextDouble();
                var colorVariety = rng.NextDouble();
                var verticesSize = Math.Sqrt(rng.Next(0, 101));
                var verticesSquashVariety = rng.NextDouble() * rng.NextDouble();
                var centerSize = (branchStepLength < 10) ? rng.Next(10, 101) : Math.Sqrt(rng.Next(1, 101));
                var centerOpacity = Math.Max(0, rng.NextDouble() * (rng.NextDouble() - centerSize / 100d));
                var edgesOpacity = rng.NextDouble() * rng.NextDouble() * branchStraightness;
                var verticesOpacity = (verticesSize == 0) ? 0 : rng.NextDouble();
                var hullOpacity = rng.NextDouble() * branchStraightness * generationRotation;
                var sumOpacity = centerOpacity + edgesOpacity + verticesOpacity + hullOpacity;
                centerOpacity /= sumOpacity; edgesOpacity /= sumOpacity; verticesOpacity /= sumOpacity; hullOpacity /= sumOpacity;
                shape = new Shape { MaxBranches = maxBranches, BranchStepLength = branchStepLength, BranchStraightness = branchStraightness, GenerationRotation = generationRotation, ColorVariety = colorVariety, VerticesSize = verticesSize, VerticesSquashVariety = verticesSquashVariety, CenterSize = centerSize, CenterOpacity = centerOpacity, EdgesOpacity = edgesOpacity, VerticesOpacity = verticesOpacity, HullOpacity = hullOpacity };
            }

            // Add shape.
            shapes.Add(shape);
            shapeUsage[shape] = TimeSpan.Zero;

            // Update GUI.
            updateIcons();
        }

        void onColorButtonClick(object s, EventArgs e)
        {
            // Sort colors by usage.
            colors.OrderBy(c => colorUsage[c]);

            // Remove underused colors.
            foreach (var c in colors.ToList())
            {
                if (colorUsage[c].Seconds < 0.1 * colorUsage.Max(kvp => kvp.Value).Seconds || colorUsage[c] == TimeSpan.Zero)
                {
                    colors.Remove(c);
                    colorUsage.Remove(c);
                }
            }

            // Either pick a previously used color or generate a new, based on how many previously used colors there are.
            color = (colors.Count > 0 && rng.NextDouble() <= 0.01 * colors.Count - 0.1) ? colors.ElementAt((colors.ToList().IndexOf(color) + 1) % colors.Count) : createColor(); //TODO Verify that the index applies to the HashSet.

            // Add color.
            colors.Add(color);
            colorUsage[color] = TimeSpan.Zero;

            // Update GUI.
            updateIcons();
        }

        void onGazePaint(object s, EventArgs e)
        {
            startPainting();
        }

        void onSaveDrawing(object s, EventArgs e)
        {
            var pbe = new PngBitmapEncoder();
            pbe.Frames.Add(BitmapFrame.Create(Raster.Source as RenderTargetBitmap));
            using (var fs = System.IO.File.OpenWrite("image.png")) pbe.Save(fs);
            //TODO Backend upload.
            //var f = new Flickr(Properties.Settings.Default.FlickrKey, Properties.Settings.Default.FlickrSecret);
            //var requestToken = f.OAuthGetRequestToken("oob");
            //System.Diagnostics.Process.Start(f.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write));
            //var accessToken = f.OAuthGetAccessToken(requestToken, VerifierTextBox.Text);
            //f.OAuthAccessToken = "";
            //f.OAuthAccessTokenSecret = "";
            //f.UploadPicture("image.png");
            ((App)Application.Current).Reset();
        }

        Point calculateGaze(Point p)
        {
            gazes.Add(p);
            while (gazes.Count > Properties.Settings.Default.Inertia) gazes.RemoveAt(0);
            return new Point(gazes.Average(_p => _p.X), gazes.Average(_p => _p.Y));
        }

        void startPainting()
        {
            if (!paintTimer.IsEnabled)
            {
                model = new Tree(gaze, (int)shape.MaxBranches, ref rng);
                time = DateTime.Now;
                paintTimer.Start();
            }
        }

        void stopPainting()
        {
            if (paintTimer.IsEnabled)
            {
                paintTimer.Stop();
                var t = DateTime.Now - time;
                shapeUsage[shape] += t;
                colorUsage[color] += t;
            }
        }

        RenderTargetBitmap createDrawing(int width, int height)
        {
            return new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        }

        void paint(ref Tree model, RenderTargetBitmap drawing)
        {
            // Grow model.
            var newLeaves = new PointCollection();
            var newParents = new Dictionary<Point, Point>();
            var rotation = shape.GenerationRotation * rng.NextDouble() * 2 * Math.PI;
            for (int i = 0; i < model.Leaves.Count; ++i)
            {
                var q = (model.Leaves.Count == 0) ? model.Root : model.Leaves[i];
                var angle =
                    i * (2 * Math.PI / model.Leaves.Count)
                    + rotation
                    + (1 - shape.BranchStraightness) * rng.NextDouble() * 2 * Math.PI;
                var p = new Point(
                    q.X + shape.BranchStepLength * Math.Cos(angle),
                    q.Y + shape.BranchStepLength * Math.Sin(angle)
                );
                newParents[p] = q;
                newLeaves.Add(p);
            }

            // Render model.
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var centerSize = rng.NextDouble() * shape.CenterSize;
                var centerBrush = new SolidColorBrush(createColor(color, shape.ColorVariety));
                centerBrush.Opacity = rng.NextDouble() * shape.CenterOpacity;
                var centerPen = new Pen(centerBrush, 1);
                dc.DrawEllipse(centerBrush, centerPen, model.Root, centerSize, centerSize);

                var edges = new GeometryGroup();
                var edgesPen = new Pen(new SolidColorBrush(createColor(color, shape.ColorVariety)), 1);
                edgesPen.StartLineCap = edgesPen.EndLineCap = PenLineCap.Round;
                edgesPen.LineJoin = PenLineJoin.Round;
                edgesPen.Brush.Opacity = shape.EdgesOpacity;
                foreach (var p in model.Leaves) edges.Children.Add(new LineGeometry(p, model.Parents[p]));
                dc.DrawGeometry(null, edgesPen, edges);

                var vertices = new GeometryGroup();
                var verticesBrush = new SolidColorBrush(createColor(color, shape.ColorVariety));
                verticesBrush.Opacity = rng.NextDouble() * shape.VerticesOpacity;
                var verticesPen = new Pen(verticesBrush, 1);
                foreach (var p in model.Leaves)
                {
                    var r = rng.NextDouble();
                    var eg = new EllipseGeometry(p, shape.VerticesSize * (r + shape.VerticesSquashVariety * rng.NextDouble()), shape.VerticesSize * (r + shape.VerticesSquashVariety * rng.NextDouble()));
                    vertices.Children.Add(eg);
                }
                dc.DrawGeometry(verticesBrush, verticesPen, vertices);

                var hull = new StreamGeometry();
                var hullBrush = new SolidColorBrush(createColor(color, shape.ColorVariety));
                hullBrush.Opacity = rng.NextDouble() * shape.HullOpacity;
                var hullPen = new Pen(hullBrush, 1);
                using (var sgc = hull.Open())
                {
                    sgc.BeginFigure(model.Leaves[0], true, true);
                    sgc.PolyLineTo(model.Leaves, true, true);
                }
                dc.DrawGeometry(hullBrush, hullPen, hull);
            }

            // Persist update.
            model.Leaves = newLeaves;
            model.Parents = newParents;
            drawing.Render(dv);
        }

        Color createColor(Color? baseColor = null, double randomness = 1)
        {
            // Generate a random color.
            var H = rng.NextDouble();
            var S = rng.NextDouble() > 0 || baseColor.HasValue ? 1.0 : 0;
            var V = rng.NextDouble() > 0 || baseColor.HasValue ? 1.0 : 0;
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

            // Mix colors if neccessary.
            if (baseColor.HasValue)
            {
                var c1 = baseColor.Value;
                var c2 = Color.Multiply(Color.FromRgb(R, G, B), (float)randomness);
                var brightness = System.Drawing.Color.FromArgb(baseColor.Value.A, baseColor.Value.R, baseColor.Value.G, baseColor.Value.B).GetBrightness();
                return (brightness >= 0.5) ? c1 + c2 : c1 - c2;
            }
            else return Color.FromRgb(R, G, B);
        }

        void updateIcons()
        {
            ColorButtonBackgroundBaseColor.Color = color;
            ColorButtonBackgroundColorVariety.Color = createColor(color, shape.ColorVariety);
            var toolIcon = new Image();
            toolIcon.Source = createDrawing((int)(ShapeButton.ActualWidth), (int)(ShapeButton.ActualHeight));
            var model = new Tree(new Point(ShapeButton.ActualWidth / 2, ShapeButton.ActualHeight / 2), (int)shape.MaxBranches, ref rng);
            for (int i = 0; i < 10; ++i) paint(ref model, toolIcon.Source as RenderTargetBitmap);
            ShapeButton.Content = toolIcon;
        }
    }
}
