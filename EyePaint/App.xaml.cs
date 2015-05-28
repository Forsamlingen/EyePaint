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
    public static DependencyProperty TrackingProperty = DependencyProperty.Register("Tracking", typeof(bool), typeof(GlobalProperties));
    public bool Tracking
    {
      get { return (bool)GetValue(TrackingProperty); }
      set { SetValue(TrackingProperty, value); }
    }
  }

  /// <summary>
  /// Eye tracking logic and gaze enabled UI elements.
  /// </summary>
  public partial class App : Application
  {
    public IEyeTracker iet;
    List<Point> gazes = new List<Point>();
    Dictionary<Point, Vector> offsets = new Dictionary<Point, Vector>();
    TimeSpan? time;
    PaintWindow paintWindow;
    ResultWindow resultWindow;
    Size resolution;
    public GlobalProperties Globals { get; set; }
    bool tracking;
    public bool ResetButtonEnabled;
    int notTracking;

    [DllImport("User32.dll")]
    static public extern bool SetCursorPos(int X, int Y);

    public App()
    {
      // Register global hardware reset button to any key but Escape.
      EventManager.RegisterClassHandler(typeof(Window), Window.KeyUpEvent, new RoutedEventHandler((s, e) =>
      {
        if ((e as KeyEventArgs).Key == Key.Escape) new SettingsWindow();
        else if (ResetButtonEnabled)
        {
          ResetButtonEnabled = false;
          Reset();
        }
      }));

      // Clear gaze click data if hardware button was used to click buttons.
      EventManager.RegisterClassHandler(typeof(Button), Mouse.MouseDownEvent, new MouseButtonEventHandler((s, e) =>
      {
        time = null;
      }));
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
    /// Connect to the eye tracker and start gaze tracking. Return true iff a successful connection has been established.
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

          // Open paint controls.
          Mouse.OverrideCursor = Cursors.None;
          resultWindow = new ResultWindow();
          paintWindow = new PaintWindow();
          paintWindow.ContentRendered += (_, __) =>
          {
            paintWindow.Focus();
            paintWindow.Activate();
            resolution = paintWindow.RenderSize;
            MainWindow = paintWindow;
            resultWindow.SetPaintWindow(paintWindow);
            iet.StartTracking();
          };

          return true;
        }
        catch (Exception)
        {
          Thread.Sleep(1000);
        }
      return false;
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
    /// Handles data from the eye tracker. Places the mouse cursor at the average gaze point so that WPF mouse events can be used throughout the application.
    /// </summary>
    void onGazeData(object s, GazeDataEventArgs e)
    {
      // Determine if the user is present.
      if (e.GazeData.TrackingStatus == TrackingStatus.NoEyesTracked)
      {
        if (++notTracking > EyePaint.Properties.Settings.Default.Blink)
        {
          if (tracking)
          {
            tracking = false;
            Dispatcher.Invoke(() => Globals.Tracking = tracking);
          }
        }
      }
      else
      {
        if (notTracking > 0)
        {
          notTracking = 0;
          if (!tracking)
          {
            tracking = true;
            Dispatcher.Invoke(() => Globals.Tracking = tracking);
          }
        }
      }

      // Retrieve available gaze information from the eye tracker and place the mouse cursor at the position. 
      if (e.GazeData.TrackingStatus != TrackingStatus.NoEyesTracked)
      {
        //TODO Add support for one eye operation.
        var l = (e.GazeData.TrackingStatus == TrackingStatus.BothEyesTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyLeft || e.GazeData.TrackingStatus == TrackingStatus.OnlyLeftEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Left.GazePointOnDisplayNormalized : new Point2D(0, 0);
        var r = (e.GazeData.TrackingStatus == TrackingStatus.BothEyesTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyRight || e.GazeData.TrackingStatus == TrackingStatus.OnlyRightEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Right.GazePointOnDisplayNormalized : new Point2D(0, 0);

        // Determine new gaze point position on the screen.
        var newGazePoint = new Point(resolution.Width * (l.X + r.X) / 2, resolution.Height * (l.Y + r.Y) / 2);
        gazes.Add(newGazePoint);

        // Calculate average gaze point (i.e. naive noise reduction).
        while (gazes.Count > EyePaint.Properties.Settings.Default.Stability + 1) gazes.RemoveAt(0);
        var gazePoint = new Point(gazes.Average(p => p.X), gazes.Average(p => p.Y));

        // Calibrate average gaze point with known offsets.
        if (offsets.Count == 1) gazePoint += offsets.Values.First();
        var distances = offsets.Select(kvp => (gazePoint - kvp.Key).Length);
        var distancesRatios = distances.Select(d => d / distances.Sum());
        foreach (var v in offsets.Values.Zip(distancesRatios, (o, d) => (1.0 - d) * o))
          gazePoint += v;

        // Place the mouse cursor at the gaze point so mouse events can be used throughout the application.
        SetCursorPos((int)gazePoint.X, (int)gazePoint.Y);
      }
    }

    /// <summary>
    /// When a gaze button is stared at for long enough a click event is raised on each animation completion.
    /// </summary>
    void onGazeButtonFocused(object s, EventArgs e)
    {
      try
      {
        var c = s as Clock;

        if (time.HasValue && c.CurrentTime.HasValue && c.CurrentTime.Value < time.Value)
        {
          // Claim click for focused button.
          // TODO Fix time bug.
          time = null;
          Button focusedButton = FocusManager.GetFocusedElement(MainWindow) as Button;

          // Store calibration offset.
          if (gazes.Count > 0)
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
        MessageBox.Show("No button found upon gaze click. " + ex.Message);
      }
    }

    /// <summary>
    /// Store the offset between the expected gaze point where the user is assumed to be looking and the actual gaze point registered by the eye tracker.
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
    /// Clear previous gaze data.
    /// </summary>
    public void Reset()
    {
      var w = new PaintWindow();
      w.ContentRendered += (s, e) =>
      {
        gazes.Clear();
        offsets.Clear();
        time = null;
        resultWindow.SetPaintWindow(w);
        paintWindow.Close();
        paintWindow = w;
      };
      w.Show();
    }
  }
}
