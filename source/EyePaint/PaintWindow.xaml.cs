
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
  /// Model representing a paint stroke in the application. A stroke consists of several paint blobs that drip color.
  /// </summary>
  class PaintStroke
  {
    Point brushPosition;

    public List<PaintBlob> Blobs = new List<PaintBlob>();

    public PaintStroke(Point center, PaintTool tool)
    {
      Blobs.Add(new PaintBlob(center, tool));
    }

    public void Paint(Point brushPosition, PaintTool tool)
    {
      // Create new paint blob if brush has moved.
      if ((brushPosition - this.brushPosition).Length > tool.Spacing)
      {
        Blobs.Add(new PaintBlob(brushPosition, tool));
        this.brushPosition = brushPosition;
      }

      // Remove old paint blobs (like they would be dried up).
      while (Blobs.Count > EyePaint.Properties.Settings.Default.StrokeSize) Blobs.RemoveAt(0);

      // Let paint drip by growing each remaining paint blob.
      for (var i = 0; i < Blobs.Count; ++i)
      {
        var dripAmount = Math.Pow((i + 1) / (double)Blobs.Count, 2);
        Blobs[i].Drip(tool, dripAmount);
      }
    }
  }

  /// <summary>
  /// Model representing a dripping paint blob in the application.
  /// </summary>
  class PaintBlob
  {
    long generation;
    public Point Center;
    public PointCollection Children = new PointCollection();
    public Dictionary<Point, Point> Parents = new Dictionary<Point, Point>();

    public PaintBlob(Point center, PaintTool tool)
    {
      Center = center;
      var branches = 1 + Random.Next((int)((1 - tool.BranchesVariety) * tool.BranchesMaximum), (int)tool.BranchesMaximum);
      for (int i = 0; i < branches; ++i) Children.Add(Center);
      Parents[Center] = Center;
    }

    public void Drip(PaintTool tool, double amount)
    {
      if (generation > EyePaint.Properties.Settings.Default.PaintBlobLifespan) //TODO Set in seconds painted instead like an ADSR envelope.
      {
        Children.Clear();
        return;
      }

      ++generation;
      var stepLength = tool.BranchLength * amount * (1 - (Math.Sqrt(generation) / Math.Sqrt(EyePaint.Properties.Settings.Default.PaintBlobLifespan)));
      var rotation = tool.Rotation * Random.NextDouble() * 2 * Math.PI;
      for (int i = 0; i < Children.Count; ++i)
      {
        var angle =
            i * 2 * Math.PI / Children.Count
            + rotation
            + (1 - tool.BranchStraightness) * Random.NextDouble() * 2 * Math.PI;
        var p = new Point(
            Children[i].X + stepLength * Math.Cos(angle),
            Children[i].Y + stepLength * Math.Sin(angle)
        );
        Parents[p] = Children[i];
        Children[i] = p;
      }
    }
  }

  /// <summary>
  /// Interprets paint stroke into drawings.
  /// </summary>
  class PaintTool
  {
    public Color Color;
    public double
      Spacing,
      Rotation,
      ColorVariation,

      BranchesMaximum,
      BranchesVariety,
      BranchLength,
      BranchStraightness,

      CenterSize,
      CenterSquashVariety,

      VerticesSize,
      VerticesSquashVariety,

      CenterFillOpacity,
      CenterStrokeOpacity,
      EdgesOpacity,
      VerticesOpacity,
      VerticesStrokeOpacity,
      HullStrokeOpacity,
      HullFillOpacity;

    public PaintTool()
    {
      Color = generateRandomColor(null, 0.0, new double[] { 0.0, 0.33, 0.66 });
      ColorVariation = Random.NextDouble();

      BranchesMaximum = Random.Next(1, EyePaint.Properties.Settings.Default.MaximumDetail);
      BranchesVariety = Random.NextDouble();
      BranchLength = Math.Sqrt(Random.Next(1, 900));
      BranchStraightness = Random.NextDouble() > 0.5 ? 1 : Math.Sqrt(Random.NextDouble());
      Rotation = Random.NextDouble() * 2 - 1;

      VerticesSize = Random.NextDouble() > 0.5 ? 1 : Math.Sqrt(Random.Next(0, 100));
      VerticesSquashVariety = Math.Pow(Random.NextDouble(), 2);

      Spacing = BranchLength;

      CenterSize = BranchLength < 10 ? Random.Next(10, 200) : Math.Sqrt(Random.Next(1, 100));
      CenterSquashVariety = Math.Pow(Random.NextDouble(), 2);

      CenterFillOpacity = Math.Max(0, Random.NextDouble() * (Random.NextDouble() - CenterSize / 100.0));
      CenterStrokeOpacity = Math.Max(0, Random.NextDouble() * (Random.NextDouble() - CenterSize / 100.0));
      EdgesOpacity = BranchStraightness > 0.9 ? Random.NextDouble() : 0.1 * Random.NextDouble();
      VerticesOpacity = VerticesSize == 0 ? 0 : Random.NextDouble();
      VerticesStrokeOpacity = VerticesOpacity;
      HullFillOpacity = BranchStraightness < 0.75 || Rotation > 0.75 ? 0.01 * BranchStraightness * (1 - Rotation) : 0.1 * Random.NextDouble();
      HullStrokeOpacity = BranchStraightness < 0.75 || Rotation > 0.75 ? 0.1 * BranchStraightness * (1 - Rotation) : Random.NextDouble();

      var sumOpacity = CenterFillOpacity + CenterStrokeOpacity + EdgesOpacity + VerticesOpacity + HullFillOpacity + HullStrokeOpacity;
      CenterFillOpacity /= sumOpacity;
      CenterStrokeOpacity /= sumOpacity;
      EdgesOpacity /= sumOpacity;
      VerticesOpacity /= sumOpacity;
      HullStrokeOpacity /= sumOpacity;
      HullFillOpacity /= sumOpacity;
    }

    Color generateRandomColor(Color? baseColor = null, double randomness = 1.0, double[] preferedColors = null)
    {
      var H = Random.NextDouble();

      //TODO Test
      if (preferedColors != null)
      {
        H = Random.NextDouble() > 0.5 ? H : preferedColors[Random.Next(0, preferedColors.Length)];
      }

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

      // Mix colors
      if (baseColor.HasValue)
      {
        var c1 = baseColor.Value;
        var c2 = Color.Multiply(Color.FromRgb(R, G, B), (float)randomness);
        var brightness = System.Drawing.Color.FromArgb(baseColor.Value.A, baseColor.Value.R, baseColor.Value.G, baseColor.Value.B).GetBrightness();
        var c3 = (brightness >= 0.5) ? c1 + c2 : c1 - c2;
        return c3;
      }
      else
      {
        return Color.FromRgb(R, G, B);
      }
    }

    public DrawingVisual Draw(PaintStroke stroke)
    {
      var dv = new DrawingVisual();
      using (var dc = dv.RenderOpen())
      {
        foreach (var blob in stroke.Blobs)
        {
          if (blob.Children.Count == 0) continue;
          var hull = new StreamGeometry();
          var hullBrush = new SolidColorBrush(generateRandomColor(Color, ColorVariation));
          hullBrush.Opacity = HullFillOpacity;
          var hullPen = new Pen(new SolidColorBrush(generateRandomColor(Color, ColorVariation)), 1);
          hullPen.Brush.Opacity = HullStrokeOpacity;
          if (blob.Children.Count > 4) using (var sgc = hull.Open())
            {
              sgc.BeginFigure(blob.Children[0], true, true);
              sgc.PolyBezierTo(blob.Children, true, false);
            }
          dc.DrawGeometry(hullBrush, hullPen, hull);

          var edges = new GeometryGroup();
          var edgesPen = new Pen(new SolidColorBrush(generateRandomColor(Color, ColorVariation)), 0.1);
          edgesPen.Brush.Opacity = EdgesOpacity;
          foreach (var p in blob.Children) edges.Children.Add(new LineGeometry(p, blob.Parents[p]));
          dc.DrawGeometry(null, edgesPen, edges);

          var vertices = new GeometryGroup();
          var verticesBrush = new SolidColorBrush(generateRandomColor(Color, ColorVariation));
          verticesBrush.Opacity = VerticesOpacity;
          var verticesPen = new Pen(verticesBrush, 1);
          foreach (var p in blob.Children)
          {
            var r = Random.NextDouble();
            var sizeX = VerticesSize * r;
            var sizeY = VerticesSize * r;
            var eg = new EllipseGeometry(p, sizeX, sizeY);
            vertices.Children.Add(eg);
          }
          dc.DrawGeometry(verticesBrush, verticesPen, vertices);

          var centerBrush = new SolidColorBrush(generateRandomColor(Color, ColorVariation));
          centerBrush.Opacity = CenterFillOpacity;
          var centerPen = new Pen(new SolidColorBrush(generateRandomColor(Color, ColorVariation)), 1);
          centerPen.Brush.Opacity = CenterStrokeOpacity;
          var centerSizeX = CenterSize * CenterSquashVariety * Random.NextDouble();
          var centerSizeY = CenterSize * CenterSquashVariety * Random.NextDouble();
          dc.DrawEllipse(centerBrush, centerPen, blob.Center, centerSizeX, centerSizeY);
        }
      }

      var e = new DropShadowEffect();
      e.Color = generateRandomColor(Color, ColorVariation);
      e.RenderingBias = RenderingBias.Performance;
      e.BlurRadius = EyePaint.Properties.Settings.Default.BlurRadius;
      e.Opacity = 0.9;
      e.ShadowDepth = Random.NextDouble();
      e.Direction = Random.Next(0, 360);
      dv.Effect = e;

      return dv;
    }
  }

  /// <summary>
  /// Lets the user select different paintToolHistory and colorHistory and paint them onto a canvas.
  /// </summary>
  public partial class PaintWindow : Window
  {
    static Random rng = new Random();
    DispatcherTimer paintTimer;
    TimeSpan activePaintDuration;
    DateTime timeStrokeStart;
    Point brushPosition;
    List<Point> brushPositionHistory = new List<Point>();
    PaintStroke stroke;
    RenderTargetBitmap raster, icon;
    DrawingVisual layer;
    PaintTool tool;
    List<PaintTool> paintToolHistory = new List<PaintTool>();
    Dictionary<PaintTool, TimeSpan> paintToolUsage = new Dictionary<PaintTool, TimeSpan>();

    public PaintWindow()
    {
      InitializeComponent();

      // Update drawing several times per second.
      (paintTimer = new DispatcherTimer(
        TimeSpan.FromMilliseconds(1000.0 / Properties.Settings.Default.FPS),
        DispatcherPriority.Render,
        (_, __) =>
        {
          // Render drawing.
          paint(ref brushPosition, ref tool, ref stroke, ref raster, ref layer);

          // Require a couple of seconds of actively painting before allowing the user to publish the drawing.
          if (activePaintDuration.Seconds > Properties.Settings.Default.MinimumActivePaintTimeBeforeAllowingPublish) PublishButton.IsEnabled = true;
        },
        Dispatcher
      )).Stop();
    }

    protected override void OnContentRendered(EventArgs e)
    {
      // Initialize drawing and paint tool icon and add them to the XAML.
      Drawing.Source = raster = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32);
      PaintToolButtonIcon.Source = icon = new RenderTargetBitmap((int)PaintToolButton.ActualWidth, (int)PaintToolButton.ActualHeight, 96, 96, PixelFormats.Pbgra32);
      clear(icon);
      clear(raster);

      // Choose initial tool and color by simulating a button click.
      PaintToolButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

      base.OnContentRendered(e);

      // Perform initial offset calibration.
      while (!new CalibrationWindow(this).DialogResult.Value) ;

      // Allow user to reset the program and allow painting.
      Activate();
      PaintControls.IsEnabled = true;
      (App.Current as App).IsResettable = true;
      (App.Current as App).ResultWindow.SetPaintWindow(this);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);
      foreach (var w in OwnedWindows) (w as Window).Close();
    }

    void onCanvasMouseEnter(object s, MouseEventArgs e)
    {
      var gaze = e.GetPosition(s as Canvas);
      startPainting(gaze);
    }

    void onCanvasMouseLeave(object s, MouseEventArgs e)
    {
      stopPainting();
    }

    void onCanvasMouseMove(object s, MouseEventArgs e)
    {
      var gaze = e.GetPosition(s as Canvas);

      // Determine brush position with a moving average window.
      brushPositionHistory.Add(gaze);
      while (brushPositionHistory.Count > EyePaint.Properties.Settings.Default.Inertia) brushPositionHistory.RemoveAt(0);
      brushPosition = new Point(brushPositionHistory.Average(p => p.X), brushPositionHistory.Average(p => p.Y));

      // Begin new paint stroke if brush left paint.
      if (layer != null && !layer.ContentBounds.Contains(brushPosition))
      {
        stopPainting();
        Task.Delay(25).ContinueWith(_ => Dispatcher.Invoke(() => startPainting(brushPosition))); //TODO How long delay?
      }
    }

    void onPublishButtonClick(object s, EventArgs e)
    {
      if (new DialogWindow(this).DialogResult.Value)
      {
        new PublishWindow(this, Drawing.Source as RenderTargetBitmap);
      }
    }

    void onToolButtonClick(object s, EventArgs e)
    {
      // Sort history by usage and remove underused tools.
      paintToolHistory.OrderBy(c => paintToolUsage[c]);
      foreach (var c in paintToolHistory.ToList())
      {
        if (paintToolUsage[c].Seconds <= 0.1 * paintToolUsage.Max(kvp => kvp.Value).Seconds)
        {
          paintToolHistory.Remove(c);
          paintToolUsage.Remove(c);
        }
      }

      // Let a coin toss decide whether to pick a previous tool or a random one.
      if (paintToolHistory.Count > EyePaint.Properties.Settings.Default.PaintToolMemory && Random.NextDouble() < 0.5)
      {
        // Select next tool in paintToolHistory.
        tool = paintToolHistory.ElementAt((paintToolHistory.IndexOf(tool) + 1) % paintToolHistory.Count);
      }
      else
      {
        // Generate a new tool and remember it.
        tool = new PaintTool();
        paintToolHistory.Add(tool);
        paintToolUsage[tool] = TimeSpan.Zero;
      }

      // Update GUI button icon by sample drawing with the new tool.
      clear(icon);
      var center = new Point(PaintToolButton.ActualWidth / 2, PaintToolButton.ActualHeight / 2);
      var stroke = new PaintStroke(center, tool);
      var layer = new DrawingVisual();
      for (int i = 0; i < 10; ++i) paint(ref center, ref tool, ref stroke, ref icon, ref layer);
    }

    // Begin new paint stroke at center and start rendering drawing.
    void startPainting(Point center)
    {
      if (paintTimer.IsEnabled) return;
      stroke = new PaintStroke(center, tool);
      timeStrokeStart = DateTime.Now;
      brushPositionHistory.Add(center);
      paintTimer.Start();
    }

    // Stop rendering drawing, clear brush position and calculate paint tool usage.
    void stopPainting()
    {
      paintTimer.Stop();
      brushPositionHistory.Clear();
      layer = null; //TODO Why?
      var strokeDuration = DateTime.Now - timeStrokeStart;
      paintToolUsage[tool] += strokeDuration; //TODO Combine similar tools and find trends.
      activePaintDuration += strokeDuration;
    }

    void paint(ref Point brushPosition, ref PaintTool tool, ref PaintStroke stroke, ref RenderTargetBitmap drawing, ref DrawingVisual layer)
    {
      stroke.Paint(brushPosition, tool);
      var dv = tool.Draw(stroke);
      if (!dv.ContentBounds.IsEmpty)
      {
        drawing.Render(dv);
        layer = dv;
      }
    }

    void clear(RenderTargetBitmap canvas)
    {
      DrawingVisual dv = new DrawingVisual();
      using (DrawingContext dc = dv.RenderOpen()) dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, canvas.Width, canvas.Height));
      canvas.Render(dv);
    }
  }
}

