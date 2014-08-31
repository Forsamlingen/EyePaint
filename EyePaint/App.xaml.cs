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
        IEyeTracker iet;
        Size resolution;
        List<Point> gazes;
        Dictionary<Point, Vector> offsets;
        TimeSpan? time;
        PaintWindow paintWindow;
        ResultWindow resultWindow;
        public GlobalProperties Globals { get; set; }
        bool tracking;
        public bool Resettable;
        int notTracking;

        [DllImport("User32.dll")]
        static public extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Connect to the eye tracker on application startup.
        /// </summary>
        void onStartup(object s, StartupEventArgs e)
        {
            new SettingsWindow();

            Globals = new GlobalProperties();
            var connected = connect();
            if (!connected)
            {
                SendErrorReport("(EyePaint) Startup Error", "The eye tracker could not be found. Ensure that the cable is connected, restart the computer, and try launching the application again.");
                new ErrorWindow().ShowDialog();
            }
        }

        /// <summary>
        /// Connect to the eye tracker and start gaze tracking. Return true iff a successful connection has been established.
        /// </summary>
        bool connect()
        {
            int retry = 0;
            while (++retry < EyePaint.Properties.Settings.Default.ConnectionAttempts) try
                {
                    // Connect to eye tracker.
                    gazes = new List<Point>();
                    offsets = new Dictionary<Point, Vector>();
                    Uri url = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
                    iet = new EyeTracker(url);
                    iet.EyeTrackerError += onEyeTrackerError;
                    iet.GazeData += onGazeData;
                    Task.Factory.StartNew(() => iet.RunEventLoop());
                    iet.Connect();
                    iet.StartTracking();
                    Mouse.OverrideCursor = Cursors.None;

                    // Display image result on a secondary screen.
                    resultWindow = new ResultWindow();
                    if (System.Windows.Forms.SystemInformation.MonitorCount > 1)
                    {
                        var wa = System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
                        resultWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        resultWindow.Left = wa.Left;
                        resultWindow.Top = wa.Top;
                        resultWindow.Topmost = true;
                        resultWindow.Show();
                    }

                    // Open paint window.
                    paintWindow = new PaintWindow();
                    paintWindow.Loaded += (_, __) => resolution = new Size(paintWindow.ActualWidth, paintWindow.ActualHeight);
                    paintWindow.ContentRendered += (_, __) => resultWindow.SetImageSource(paintWindow.Raster.Source);
                    paintWindow.Show();
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
            var reconnected = connect();
            if (reconnected) ew.Close();
            else SendErrorReport("(EyePaint) Error " + e.ErrorCode, "The eye tracker encountered an error. Error: " + e.Message + ". Try rebooting the system.");
        }

        /// <summary>
        /// Handles data from the eye tracker. Places the mouse cursor at the average gaze point so that WPF mouse events can be used throughout the application.
        /// </summary>
        void onGazeData(object s, GazeDataEventArgs e)
        {
            if (e.GazeData.TrackingStatus == TrackingStatus.NoEyesTracked)
            {
                // Determine if the user is inactive or just blinking.
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
                // Determine if the user reactived the session.
                if (notTracking > 0)
                {
                    notTracking = 0;
                    if (!tracking)
                    {
                        tracking = true;
                        Dispatcher.Invoke(() => Globals.Tracking = tracking);
                    }
                }

                // Retrieve available gaze information from the eye tracker.
                var left = (e.GazeData.TrackingStatus == TrackingStatus.BothEyesTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyLeft || e.GazeData.TrackingStatus == TrackingStatus.OnlyLeftEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Left.GazePointOnDisplayNormalized : new Point2D(0, 0);
                var right = (e.GazeData.TrackingStatus == TrackingStatus.BothEyesTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyRight || e.GazeData.TrackingStatus == TrackingStatus.OnlyRightEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Right.GazePointOnDisplayNormalized : new Point2D(0, 0);

                // Determine new gaze point position on the screen.
                var newGazePoint = new Point(resolution.Width * (left.X + right.X) / 2, resolution.Height * (left.Y + right.Y) / 2);
                gazes.Add(newGazePoint);

                // Calculate average gaze point (i.e. naive noise reduction).
                while (gazes.Count > EyePaint.Properties.Settings.Default.Stability) gazes.RemoveAt(0);
                var gazePoint = new Point(gazes.Average(p => p.X), gazes.Average(p => p.Y));

                // Calibrate average gaze point with known offsets.
                if (offsets.Count == 1) gazePoint += offsets.Values.First();
                var distances = offsets.Select(kvp => (gazePoint - kvp.Key).Length);
                var distancesRatios = distances.Select(d => d / distances.Sum());
                foreach (var v in offsets.Values.Zip(distancesRatios, (o, d) => (1.0 - d) * o)) gazePoint += v;

                // Place the mouse cursor at the gaze point so mouse events can be used throughout the application.
                SetCursorPos((int)gazePoint.X, (int)gazePoint.Y);
            }
        }

        /// <summary>
        /// When a gaze button is stared at for long enough a click event is raised on each animation completion.
        /// </summary>
        void onGazeButtonFocused(object s, EventArgs e)
        {
            var c = s as Clock;

            if (time.HasValue && c.CurrentTime.HasValue && c.CurrentTime.Value < time.Value)
            {
                // Claim click.
                time = null;

                // Find button.
                var activeWindow = Application.Current.Windows.OfType<Window>().Single(w => w.IsActive);
                var focusedButton = FocusManager.GetFocusedElement(activeWindow) as Button;

                // Store calibration offset.
                if (gazes.Count > 0)
                {
                    var expectedPoint = focusedButton.PointToScreen(new Point(focusedButton.ActualWidth / 2, focusedButton.ActualHeight / 2));
                    var actualPoint = Mouse.GetPosition(activeWindow);
                    offsets[actualPoint] = expectedPoint - actualPoint;
                }
                // Raise click event.
                focusedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }

            time = (c.CurrentState == ClockState.Active) ? c.CurrentTime : null;
        }

        /// <summary>
        /// When a gaze button is touched a button click event is raised.
        /// </summary>
        void onGazeButtonTouched(object s, EventArgs e)
        {
            (s as Button).RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); //TODO Verify this works with an actual touch screen.
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
                MessageBox.Show("Email could not be sent. Make sure the application is configured correctly. Email: " + subject + " - " + body);
            }
        }

        /// <summary>
        /// Clear previous gaze data if neccessary.
        /// </summary>
        public void Reset()
        {
            if (Resettable)
            {
                offsets.Clear();
                time = null;
                var oldPaintWindow = paintWindow;
                paintWindow = new PaintWindow();
                paintWindow.ContentRendered += (s, e) =>
                {
                    oldPaintWindow.Close();
                    resultWindow.SetImageSource(paintWindow.Raster.Source);
                };
                paintWindow.Show();
            }
        }
    }
}
