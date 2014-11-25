using FlickrNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EyePaint
{
  /// <summary>
  /// Model representing a paint blob in the application.
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
    public double MaxBranches, BranchStepLength, BranchStraightness, GenerationRotation, ColorVariety, VerticesSize, VerticesSquashVariety, CenterSize, CenterOpacity, EdgesOpacity, VerticesOpacity, HullOpacity;
  }

  /// <summary>
  /// Lets the user select different shapes and colors and paint them onto a canvas.
  /// </summary>
  public partial class PaintWindow : Window
  {
    Random rng = new Random();
    DispatcherTimer paintTimer;
    Tree model;
    Point gaze;
    List<Point> gazes = new List<Point>();
    Shape shape;
    Color color;
    HashSet<Shape> shapes = new HashSet<Shape>();
    HashSet<Color> colors = new HashSet<Color>();
    DateTime usage;
    Dictionary<Shape, TimeSpan> shapeUsage = new Dictionary<Shape, TimeSpan>();
    Dictionary<Color, TimeSpan> colorUsage = new Dictionary<Color, TimeSpan>();

    public PaintWindow()
    {
      InitializeComponent();
    }

    void onLoaded(object s, RoutedEventArgs e)
    {
      // Render clock. Note: single-threaded. Approximately 20 FPS.
      (paintTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(50), DispatcherPriority.Render, (_, __) => paint(ref this.model, Raster.Source as RenderTargetBitmap), Dispatcher)).Stop();

      // Initialize drawing.
      Raster.Source = createDrawing((int)ActualWidth, (int)ActualHeight);
    }

    void onContentRendered(object s, EventArgs e)
    {
      // Choose initial shape and color by simulating a button click.
      ShapeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
      ColorButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

      // Perform initial offset calibration.
      while (!new CalibrationWindow(this).DialogResult.Value) ;

      // Display paint marker.
      PaintMarker.Visibility = Visibility.Visible;

      // Reset window on user inactivity. Note: IsEnabled is used with the eye tracker to determine user status.
      IsEnabledChanged += (_s, _e) =>
      {
        var sb = (FindResource("InactivityStoryboard") as Storyboard);
        if (!(bool)_e.NewValue) sb.Begin();
        else sb.Stop();
      };

      (App.Current as App).Resettable = true;
    }

    void onUnloaded(object s, RoutedEventArgs e)
    {
      foreach (var w in OwnedWindows) (w as Window).Close();
    }

    void onInactivity(object s, EventArgs e)
    {
      if (IsVisible) (App.Current as App).Reset();
    }

    void onCanvasMouseDown(object s, MouseButtonEventArgs e)
    {
      gaze = calculateGaze(e.GetPosition(s as Canvas));
      startPainting();
      PaintMarker.Visibility = Visibility.Hidden;
      (PaintMarker.FindResource("GazePaintStoryboard") as Storyboard).Stop();
    }

    void onCanvasMouseUp(object s, MouseButtonEventArgs e)
    {
      stopPainting();
      PaintMarker.Visibility = Visibility.Visible;
      (PaintMarker.FindResource("GazePaintStoryboard") as Storyboard).Begin();
    }

    void onCanvasMouseEnter(object s, MouseEventArgs e)
    {
      (PaintMarker.FindResource("GazePaintStoryboard") as Storyboard).Begin();
    }

    void onCanvasMouseLeave(object s, MouseEventArgs e)
    {
      stopPainting();
      (PaintMarker.FindResource("GazePaintStoryboard") as Storyboard).Stop();
    }

    void onCanvasMouseMove(object s, MouseEventArgs e)
    {
      var p = calculateGaze(e.GetPosition(s as Canvas));

      Canvas.SetLeft(PaintMarker, p.X - PaintMarker.ActualWidth / 2);
      Canvas.SetTop(PaintMarker, p.Y - PaintMarker.ActualHeight / 2);

      if (paintTimer.IsEnabled && (model.Root - p).Length > Properties.Settings.Default.Spacing / 2)
        model = new Tree(p, (int)shape.MaxBranches, ref rng);
      if ((gaze - p).Length > Properties.Settings.Default.Spacing)
      {
        var sb = PaintMarker.FindResource("GazePaintStoryboard") as Storyboard;
        switch (sb.GetCurrentState())
        {
          case ClockState.Active: sb.Seek(TimeSpan.Zero); break;
          case ClockState.Filling: sb.Seek(TimeSpan.Zero); stopPainting(); break;
        }
      }
      gaze = p;
    }

    void onGazePaint(object s, EventArgs e)
    {
      startPainting();
    }

    void onPublishButtonClick(object s, EventArgs e)
    {
      var dw = new DialogWindow(this);
      dw.Closing += (_, __) =>
      {
        if (dw.DialogResult.Value)
        {
          (App.Current as App).Resettable = false;
          PaintControls.IsEnabled = false;
          (FindResource("SaveDrawingStoryboard") as Storyboard).Begin();
        }
      };
      dw.ShowDialog();
    }

    void onResetButtonClick(object s, EventArgs e)
    {
      var dw = new DialogWindow(this);
      dw.Closing += (_, __) => { if (dw.DialogResult.Value) (App.Current as App).Reset(); };
      dw.ShowDialog();
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
        var edgesOpacity = (branchStraightness > 0.75) ? rng.NextDouble() : 0.1 * rng.NextDouble();
        var verticesOpacity = (verticesSize == 0) ? 0 : rng.NextDouble();
        var hullOpacity = (branchStraightness < 0.75 || generationRotation > 0.75) ? 0.1 * branchStraightness * (1 - generationRotation) : rng.NextDouble();
        var sumOpacity = centerOpacity + edgesOpacity + verticesOpacity + hullOpacity;
        centerOpacity /= sumOpacity;
        edgesOpacity /= sumOpacity;
        verticesOpacity /= sumOpacity;
        hullOpacity /= sumOpacity;
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

    void onSaveDrawing(object s, EventArgs e)
    {
      // Save image to file system.
      var pbe = new PngBitmapEncoder();
      pbe.Frames.Add(BitmapFrame.Create(Raster.Source as RenderTargetBitmap));
      using (var fs = System.IO.File.OpenWrite("drawing.png")) pbe.Save(fs);

      // Upload image to Flickr.
      try
      {
        var bw = new BackgroundWorker();
        bw.WorkerReportsProgress = true;
        bw.ProgressChanged += (_s, _e) => ProgessBar.Value = _e.ProgressPercentage;
        bw.DoWork += (_s, _e) =>
        {
          // Login
          var f = new Flickr(Properties.Settings.Default.FlickrKey, Properties.Settings.Default.FlickrSecret);
          f.OAuthAccessToken = Properties.Settings.Default.FlickrAccessToken.Token;
          f.OAuthAccessTokenSecret = Properties.Settings.Default.FlickrAccessToken.TokenSecret;

          // Upload photo.
          f.OnUploadProgress += (__s, __e) => bw.ReportProgress(__e.ProcessPercentage);
          var photoId = f.UploadPicture("drawing.png", Properties.Settings.Default.FlickrTitle, Properties.Settings.Default.FlickrDescription, Properties.Settings.Default.FlickrTags, true, true, true);

          // Add photo to set. If set doesn't exist, create it first.
          var photosets = f.PhotosetsGetList().Where(set => set.Title == Properties.Settings.Default.FlickrPhotoset).ToList();
          if (photosets.Count == 0) f.PhotosetsCreate(Properties.Settings.Default.FlickrPhotoset, photoId);
          else f.PhotosetsAddPhoto(photosets[0].PhotosetId, photoId);
        };
        bw.RunWorkerCompleted += (_s, _e) =>
        {
          // Restart session.
          (App.Current as App).Resettable = true;
          (App.Current as App).Reset();
        };
        bw.RunWorkerAsync();
      }
      catch (Exception ex)
      {
        (App.Current as App).SendErrorReport("(EyePaint) Image Upload Error", "The application encountered an error when uploading an image. Error: " + ex.Message + ". The image is only available on the file system until the application is used again.");
        new ErrorWindow().ShowDialog();
      }
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
        usage = DateTime.Now;
        paintTimer.Start();
      }
    }

    void stopPainting()
    {
      if (paintTimer.IsEnabled)
      {
        paintTimer.Stop();
        var t = DateTime.Now - usage;
        shapeUsage[shape] += t;
        colorUsage[color] += t;
      }
    }

    RenderTargetBitmap createDrawing(int width, int height)
    {
      var d = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
      DrawingVisual dv = new DrawingVisual();
      using (DrawingContext dc = dv.RenderOpen()) dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, width, height));
      d.Render(dv);
      return d;
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
      //dv.Opacity = rng.NextDouble();
      var dse = new DropShadowEffect();
      dse.BlurRadius = 20;
      dse.ShadowDepth = 0;
      dse.Opacity = 0.9;
      dse.Color = createColor(color, shape.ColorVariety);
      dv.Effect = dse;

      // Persist update.
      model.Leaves = newLeaves;
      model.Parents = newParents;
      if (drawing != null) drawing.Render(dv);
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
      else
        return Color.FromRgb(R, G, B);
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

    void onPreviewKeyDown(object s, KeyEventArgs e)
    {
      if (e.Key == Key.Escape) new SettingsWindow();
      else Application.Current.Shutdown();
    }
  }
}
