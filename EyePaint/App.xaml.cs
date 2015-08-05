using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Tobii.Gaze.Core;

namespace EyePaint
{
  /// <summary>
  /// Globally XAML bindable properties
  /// </summary>
  public class GlobalProperties : DependencyObject
  {
    public static DependencyProperty IsUserPresentProperty = DependencyProperty.Register("IsUserPresent", typeof(bool), typeof(GlobalProperties));
    public bool IsUserPresent
    {
      get { return (bool)GetValue(IsUserPresentProperty); }
      set { SetValue(IsUserPresentProperty, value); }
    }

    public static DependencyProperty IsResettableProperty = DependencyProperty.Register("IsResettable", typeof(bool), typeof(GlobalProperties));
    public bool IsResettable
    {
      get { return (bool)GetValue(IsResettableProperty); }
      set { SetValue(IsResettableProperty, value); }
    }

    public static DependencyProperty GazeXProperty = DependencyProperty.Register("GazeX", typeof(double), typeof(GlobalProperties));
    public double GazeX
    {
      get { return (double)GetValue(GazeXProperty); }
      set { SetValue(GazeXProperty, value); }
    }

    public static DependencyProperty GazeYProperty = DependencyProperty.Register("GazeY", typeof(double), typeof(GlobalProperties));
    public double GazeY
    {
      get { return (double)GetValue(GazeYProperty); }
      set { SetValue(GazeYProperty, value); }
    }

    public static DependencyProperty ChaseXProperty = DependencyProperty.Register("ChaseX", typeof(double), typeof(GlobalProperties));
    public double ChaseX
    {
      get { return (double)GetValue(ChaseXProperty); }
      set { SetValue(ChaseXProperty, value); }
    }

    public static DependencyProperty ChaseYProperty = DependencyProperty.Register("ChaseY", typeof(double), typeof(GlobalProperties));
    public double ChaseY
    {
      get { return (double)GetValue(ChaseYProperty); }
      set { SetValue(ChaseYProperty, value); }
    }

    public static DependencyProperty IsChasingProperty = DependencyProperty.Register("IsChasing", typeof(bool), typeof(GlobalProperties));
    public bool IsChasing
    {
      get { return (bool)GetValue(IsChasingProperty); }
      set { SetValue(IsChasingProperty, value); }
    }
  }

  /// <summary>
  /// Eye tracking logic and center enabled UI elements.
  /// </summary>
  public partial class App : Application
  {
    IEyeTracker iet;
    List<Point> samples = new List<Point>();
    List<Point> gazes = new List<Point>();
    Dictionary<Point, Vector> offsets = new Dictionary<Point, Vector>();
    TimeSpan? time;
    PaintWindow paintWindow;
    ResultWindow resultWindow;
    Size resolution;
    public GlobalProperties Globals { get; set; }
    int timesNoEyesTracked;
    bool tracking;

    [DllImport("User32.dll")]
    static public extern bool SetCursorPos(int X, int Y);

    public App()
    {
      // Register global hardware reset button to any key but Escape.
      EventManager.RegisterClassHandler(typeof(Window), Window.KeyUpEvent, new RoutedEventHandler((s, e) =>
      {
        if ((e as KeyEventArgs).Key == Key.Escape) new SettingsWindow();
        else if (Globals.IsResettable)
        {
          Globals.IsResettable = false;
          Reset();
        }
      }));

      Mouse.OverrideCursor = Cursors.None;
    }

    /// <summary>
    /// Connect to the eye tracker on application startup.
    /// </summary>
    void onStartup(object s, StartupEventArgs e)
    {
      Globals = new GlobalProperties();
      var ew = new ErrorWindow();
      ew.Show();
      if (connect()) ew.Close();
      else SendErrorReport("(EyePaint) Startup Error", "The eye tracker could not be found. Ensure that the USB cable is connected and that both the Tobii EyeX and the Tobii USB service drivers are installed. Then restart the computer and try launching the application again.");
    }

    /// <summary>
    /// Connect to the eye tracker and start center tracking. Return true iff a successful connection has been established.
    /// </summary>
    bool connect()
    {
      int retry = 0;
      while (++retry < EyePaint.Properties.Settings.Default.ConnectionAttempts)
        try
        {
          // Connect to eye tracker.
          Uri url = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
          iet = new EyeTracker(url);
          iet.EyeTrackerError += onEyeTrackerError;
          iet.GazeData += onGazeData;
          Task.Factory.StartNew(() => iet.RunEventLoop());
          iet.Connect();
          iet.StartTracking();

          //Mouse.OverrideCursor = Cursors.None; //TODO Remove?

          // Initialize paint controls.
          resultWindow = new ResultWindow();
          startPaintSession();

          return true;
        }
        catch (Exception)
        {
          Thread.Sleep(1000);
        }
      return false;
    }

    /// <summary>
    /// Starts a paint session in a new window, destroying the previous gazePoint data and paint window if neccessary.
    /// </summary>
    void startPaintSession()
    {
      var w = new PaintWindow();
      w.ContentRendered += (s, e) =>
      {
        // Clear gaze data
        samples.Clear();
        gazes.Clear();
        offsets.Clear();
        time = null;

        // Close previous paint window
        if (paintWindow != null) paintWindow.Close();

        // Use new paint window
        resolution = w.RenderSize;
        resultWindow.SetPaintWindow(w);
        paintWindow = w;
      };
      w.Show();
    }

    /// <summary>
    /// Handles error data from the eye tracker. The application will be halted and the user will see an error on the screen while the application attempts to reestablish a connection to the eye tracker. If a connection cannot be reestablished a notification email is sent to the admin email.
    /// </summary>
    void onEyeTrackerError(object s, EyeTrackerErrorEventArgs e)
    {
      var ew = new ErrorWindow();
      ew.Show();
      if (connect()) ew.Close();
      else SendErrorReport("(EyePaint) Eye Tracker Error " + e.ErrorCode, "The eye tracker encountered an error. Error: " + e.Message + ". Try rebooting the system.");
    }

    /// <summary>
    /// Handles data from the eye tracker. Places the mouse cursor at the average center point so that WPF mouse events can be used throughout the application.
    /// </summary>
    void onGazeData(object s, GazeDataEventArgs e)
    {
      // Determine if the user is present.
      if (e.GazeData.TrackingStatus == TrackingStatus.NoEyesTracked)
      {
        if (++timesNoEyesTracked > EyePaint.Properties.Settings.Default.Blink)
        {
          if (tracking)
          {
            tracking = false;
            Dispatcher.Invoke(() => Globals.IsUserPresent = tracking);
          }
        }
      }
      else
      {
        if (timesNoEyesTracked > 0)
        {
          timesNoEyesTracked = 0;
          if (!tracking)
          {
            tracking = true;
            Dispatcher.Invoke(() => Globals.IsUserPresent = tracking);
          }
        }
      }

      // Retrieve available center information from the eye tracker and place the mouse cursor at the position. 
      if (e.GazeData.TrackingStatus != TrackingStatus.NoEyesTracked)
      {

        // Determine new center point position on the screen.
        var l = e.GazeData.Left.GazePointOnDisplayNormalized;
        var r = e.GazeData.Right.GazePointOnDisplayNormalized;
        switch (e.GazeData.TrackingStatus)
        {
          case TrackingStatus.BothEyesTracked:
            samples.Add(new Point(resolution.Width * (l.X + r.X) / 2, resolution.Height * (l.Y + r.Y) / 2));
            break;
          case TrackingStatus.OneEyeTrackedUnknownWhich:
          case TrackingStatus.OneEyeTrackedProbablyLeft:
          case TrackingStatus.OnlyLeftEyeTracked:
            samples.Add(new Point(resolution.Width * l.X, resolution.Height * l.Y));
            break;
          case TrackingStatus.OneEyeTrackedProbablyRight:
          case TrackingStatus.OnlyRightEyeTracked:
            samples.Add(new Point(resolution.Width * r.X, resolution.Height * r.Y));
            break;
        }

        // Perform noise reduction with a short moving average window.
        while (samples.Count > EyePaint.Properties.Settings.Default.Stability + 1) samples.RemoveAt(0);
        var gaze = new Point(samples.Average(p => p.X), samples.Average(p => p.Y));

        // Calibrate average gaze point with known offsets.
        if (offsets.Count == 1) gaze += offsets.Values.First();
        var distances = offsets.Select(kvp => (gaze - kvp.Key).Length);
        var distancesRatios = distances.Select(d => d / distances.Sum());
        foreach (var v in offsets.Values.Zip(distancesRatios, (o, d) => (1.0 - d) * o))
          gaze += v;

        // Keep gaze point within primary screen.
        if (gaze.X < 0) gaze.X = 0;
        if (gaze.X > resolution.Width) gaze.X = resolution.Width;
        if (gaze.Y < 0) gaze.Y = 0;
        if (gaze.Y > resolution.Height) gaze.Y = resolution.Height;

        // Place the mouse cursor at the gaze point so mouse events can be used throughout the application.         
        SetCursorPos((int)gaze.X, (int)gaze.Y);

        // Perform gaze point inertia with a long moving average window.
        /*TODO REmove
        Dispatcher.Invoke(() =>
        {
          if (Globals.IsChasing)
          {
            gazes.Add(gaze);
            while (gazes.Count > EyePaint.Properties.Settings.Default.Inertia) gazes.RemoveAt(0);
            var chase = new Point(gazes.Average(gazePoint => gazePoint.X), gazes.Average(gazePoint => gazePoint.Y));
            Dispatcher.Invoke(() => Globals.ChaseX = chase.X);
            Dispatcher.Invoke(() => Globals.ChaseY = chase.Y);
          }
        });*/
      }
    }

    /// <summary>
    /// When a center button is stared at for long enough a click event is raised on each animation completion.
    /// </summary>
    void onGazeButtonFocused(object s, EventArgs e)
    {
      try
      {
        var c = s as Clock;

        if (time.HasValue && c.CurrentTime.HasValue && c.CurrentTime.Value < time.Value)
        {
          // Claim click for focused button.
          time = null;
          Button focusedButton = FocusManager.GetFocusedElement(MainWindow) as Button;

          // Store calibration offset.
          if (samples.Count > 0)
          {
            var expectedPoint = focusedButton.PointToScreen(new Point(focusedButton.ActualWidth / 2, focusedButton.ActualHeight / 2));
            var actualPoint = Mouse.GetPosition(MainWindow);
            storeCalibrationPoint(expectedPoint, actualPoint);
          }

          // Raise click event.
          focusedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        time = (c.CurrentState == ClockState.Active) ? c.CurrentTime : null;
      }
      catch (Exception ex)
      {
        //TODO Improve error handling.
        var error = "No button found upon gaze click. " + ex.Message;
        MessageBox.Show(error);
        SendErrorReport("(EyePaint) Interaction Error", error);
      }
    }

    /// <summary>
    /// Store the offset between the expected center point where the user is assumed to be looking and the actual center point registered by the eye tracker.
    /// </summary>
    void storeCalibrationPoint(Point expectedPoint, Point actualPoint)
    {
      offsets[actualPoint] = expectedPoint - actualPoint;
    }

    /// <summary>
    /// Attempt to send an error report via email to an application defined admin email. If email could not be sent the error message will be displayed in a message box instead.
    /// </summary>
    public void SendErrorReport(string subject, string body)
    {
      try
      {
        var m = new MailMessage(EyePaint.Properties.Settings.Default.AdminEmail, EyePaint.Properties.Settings.Default.AdminEmail, subject, body);
        var c = new SmtpClient(EyePaint.Properties.Settings.Default.SmtpServer);
        c.Credentials = CredentialCache.DefaultNetworkCredentials;
        c.Send(m);
      }
      catch (Exception)
      {
        MessageBox.Show("Email could not be sent. Make sure the application is configured correctly.");
        MessageBox.Show(subject + " - " + body);
      }
    }

    /// <summary>
    /// Clear average center data.
    /// </summary>
    public void Reset()
    {
      startPaintSession();
    }

    /// <summary>
    /// Clear the chase point queue.
    /// </summary>
    public void ClearChaseQueue()
    {
      gazes.Clear();
    }
  }
}
