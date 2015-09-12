using EyeXFramework;
using EyeXFramework.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Tobii.EyeX.Framework;

namespace EyePaint
{
  /// <summary>
  /// Eye tracking logic and gaze enabled UI elements.
  /// </summary>
  public partial class App : Application, INotifyPropertyChanged, IDisposable
  {
    public event PropertyChangedEventHandler PropertyChanged;
    readonly WpfEyeXHost eyeXHost = new WpfEyeXHost();
    bool isResettable, isUserPresent, isTrackingGaze;
    Dictionary<Point, Vector> offsets = new Dictionary<Point, Vector>();
    TimeSpan? time;
    PaintWindow paintWindow;
    public ResultWindow ResultWindow;
    Size resolution;

    [DllImport("User32.dll")]
    static public extern bool SetCursorPos(int X, int Y);

    /// <summary>
    /// Gets whether or not the application is hardware button resettable.
    /// </summary>
    public bool IsResettable
    {
      get { return isResettable; }
      set
      {
        isResettable = value;
        OnPropertyChanged("IsResettable");
      }
    }

    /// <summary>
    /// Gets whether or not the user is present.
    /// </summary>
    public bool IsUserPresent
    {
      get { return isUserPresent; }
      set
      {
        isUserPresent = value;
        OnPropertyChanged("IsUserPresent");
      }
    }

    /// <summary>
    /// Gets whether or not gaze is being tracked.
    /// </summary>
    public bool IsTrackingGaze
    {
      get { return isTrackingGaze; }
      set
      {
        isTrackingGaze = value;
        OnPropertyChanged("IsTrackingGaze");
      }
    }

    /// <summary>
    /// Connect to the eye tracker on application startup and open windows.
    /// </summary>
    void onStartup(object s, StartupEventArgs e)
    {
      IsResettable = false;
      IsUserPresent = false;
      IsTrackingGaze = false;
      eyeXHost.UserPresenceChanged += (object _s, EngineStateValue<UserPresence> _e) => RunOnMainThread(() =>
      {
        IsUserPresent = _e.IsValid && _e.Value == UserPresence.Present;
      });
      eyeXHost.GazeTrackingChanged += (object _s, EngineStateValue<GazeTracking> _e) => RunOnMainThread(() =>
      {
        IsTrackingGaze = _e.IsValid && _e.Value == GazeTracking.GazeTracked;
      });
      eyeXHost.Start();
      var gazePointDataStream = eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);
      gazePointDataStream.Next += (_s, _e) =>
      {
        var gaze = new Point(_e.X, _e.Y);

        // Calibrate average gaze point with known calibration points.
        if (offsets.Count == 1) gaze += offsets.Values.First();
        else
        {
          // Interpolate linearly between calibration points.
          var distances = offsets.Select(kvp => (gaze - kvp.Key).Length);
          var distancesRatios = distances.Select(d => d / distances.Sum());
          foreach (var v in offsets.Values.Zip(distancesRatios, (o, d) => (1.0 - d) * o))
            gaze += v;
          //TODO Create debug mode visualization of calibration offsets.
        }

        // Place the mouse cursor at the gaze point so mouse events can be used throughout the application.
        //TODO Don't move mouse cursor, create distinct gaze point throughout application instead and use EyeX WPF SDK events.
        SetCursorPos((int)gaze.X, (int)gaze.Y);
      };

      // Register global hardware reset button to any key but Escape.
      EventManager.RegisterClassHandler(typeof(Window), Window.KeyUpEvent, new RoutedEventHandler((_s, _e) =>
      {
        if ((_e as KeyEventArgs).Key == Key.Escape) new SettingsWindow();
        else if (IsResettable)
        {
          IsResettable = false;
          StartPaintSession();
        }
      }));

      Mouse.OverrideCursor = Cursors.None;

      ResultWindow = new ResultWindow();
      StartPaintSession();
    }

    /// <summary>
    /// Starts a paint session in a new window, destroying the previous gazePoint data and paint window if neccessary.
    /// </summary>
    public void StartPaintSession()
    {
      var w = new PaintWindow();
      w.ContentRendered += (s, e) =>
      {
        // Clear gaze data
        offsets.Clear();
        time = null;

        // Close previous paint window
        if (paintWindow != null) paintWindow.Close();

        // Use new paint window
        resolution = w.RenderSize;
        paintWindow = w;
      };
      w.Show();
    }

    /// <summary>
    /// When a gaze button is stared at for long enough a click event is raised on each animation completion.
    /// TODO Use built in EyeX WPF SDK ActivatableElements instead.
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
          var expectedPoint = focusedButton.PointToScreen(new Point(focusedButton.ActualWidth / 2, focusedButton.ActualHeight / 2));
          var actualPoint = Mouse.GetPosition(MainWindow);
          offsets[expectedPoint] = expectedPoint - actualPoint;

          // Raise click event.
          focusedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        time = (c.CurrentState == ClockState.Active) ? c.CurrentTime : null;
      }
      catch (Exception)
      {
        //TODO Handle
      }
    }

    void OnPropertyChanged(string name)
    {
      var handler = PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(name));
      }
    }

    /// <summary>
    /// Marshals the given operation to the UI thread.
    /// </summary>
    /// <param name="action">The operation to be performed.</param>
    static void RunOnMainThread(Action action)
    {
      if (Application.Current != null)
      {
        Application.Current.Dispatcher.BeginInvoke(action);
      }
    }

    public void Dispose()
    {
      //TODO Dispose event handlers.
      eyeXHost.Dispose();
    }
  }
}
