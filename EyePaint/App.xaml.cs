using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Tobii.Gaze.Core;

namespace EyePaint
{
    public class TrackingStatusChangedEventArgs : EventArgs
    {
        public bool Tracking;
    }

    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        IEyeTracker iet;
        int width, height;
        List<Point> gazes;
#if DEBUG
        public Dictionary<Point, Vector> offsets;
#else
        Dictionary<Point, Vector> offsets;
#endif
        TimeSpan? time;
        public event EventHandler<TrackingStatusChangedEventArgs> TrackingStatusChanged;
        int notTracking;

        [DllImport("User32.dll")]
        static public extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Connects to the eye tracker on application startup.
        /// </summary>
        void onStartup(object s, StartupEventArgs e)
        {
            gazes = new List<Point>();
            offsets = new Dictionary<Point, Vector>();

            try
            {
                Uri url = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
                iet = new EyeTracker(url);
                iet.EyeTrackerError += onEyeTrackerError;
                iet.GazeData += onGazeData;
                Task.Factory.StartNew(() => iet.RunEventLoop());
                iet.Connect();
                iet.StartTracking();
                Mouse.OverrideCursor = Cursors.None;
            }
            catch (EyeTrackerException)
            {
                MessageBox.Show("Kameran verkar ha gått sönder. Prova att starta om datorn.");
                Application.Current.Shutdown();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Kameran verkar saknas. Säkerställ att den är inkopplad och prova att starta om datorn.");
                Application.Current.Shutdown();
            }
            finally
            {
                MainWindow = new MainWindow();
                MainWindow.Show();
                width = (int)MainWindow.ActualWidth;
                height = (int)MainWindow.ActualHeight;
            }
        }

        /// <summary>
        /// Handles error data from the eye tracker. The application will be halted, the user will see an error on the screen, and a notification email is sent to the museum's technical staff.
        /// </summary>
        void onEyeTrackerError(object s, EyeTrackerErrorEventArgs e)
        {
            new ErrorWindow();
            try
            {
                var message = new MailMessage(
                    EyePaint.Properties.Settings.Default.AdminEmail,
                    EyePaint.Properties.Settings.Default.AdminEmail,
                    "Tekniskt problem med stationen Måla med ögonen",
                    "Kameran verkar ha gått sönder. Prova att starta om datorn."
                );
                var client = new SmtpClient(EyePaint.Properties.Settings.Default.SmtpServer);
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                client.Send(message);
            }
            catch (Exception)
            {
                MessageBox.Show("Felmeddelande kunde ej skickas via epost. Kameran verkar ha gått sönder. Prova att starta om datorn.");
            }
        }

        /// <summary>
        /// Handles data from the eye tracker.
        /// </summary>
        void onGazeData(object s, GazeDataEventArgs e)
        {
            if (e.GazeData.TrackingStatus == TrackingStatus.NoEyesTracked)
            {
                // Determine if the user is inactive.
                if (++notTracking > 20 && TrackingStatusChanged != null) TrackingStatusChanged(this, new TrackingStatusChangedEventArgs { Tracking = false });
            }
            else
            {
                // Determine if the user reactived the session.
                if (notTracking > 0)
                {
                    if (TrackingStatusChanged != null) TrackingStatusChanged(this, new TrackingStatusChangedEventArgs { Tracking = true });
                    notTracking = 0;
                }

                // Retrieve available gaze information from the eye tracker.
                var left = (e.GazeData.TrackingStatus == TrackingStatus.BothEyesTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyLeft || e.GazeData.TrackingStatus == TrackingStatus.OnlyLeftEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Left.GazePointOnDisplayNormalized : new Point2D(0, 0);
                var right = (e.GazeData.TrackingStatus == TrackingStatus.BothEyesTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyRight || e.GazeData.TrackingStatus == TrackingStatus.OnlyRightEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Right.GazePointOnDisplayNormalized : new Point2D(0, 0);

                // Determine new gaze point position on the screen.
                var newGazePoint = new Point(width * (left.X + right.X) / 2, height * (left.Y + right.Y) / 2);
                gazes.Add(newGazePoint);

                // Calculate average gaze point (i.e. naive noise reduction).
                while (gazes.Count > EyePaint.Properties.Settings.Default.Stability) gazes.RemoveAt(0);
                var gazePoint = new Point(gazes.Average(p => p.X), gazes.Average(p => p.Y));

                // Calibrate average gaze point with known offsets.
#if !DEBUG
                //TODO Fix offset interpolation bug.
                if (offsets.Count == 1) gazePoint += offsets.Values.First();
                var distances = offsets.Select(kvp => (gazePoint - kvp.Key).Length);
                var distancesRatios = distances.Select(d => d / distances.Sum());
                foreach (var v in offsets.Values.Zip(distancesRatios, (o, d) => (1 - d) * o)) gazePoint += v;
#endif
                // Place the mouse cursor at the gaze point.
                SetCursorPos((int)gazePoint.X, (int)gazePoint.Y);
            }
        }

        /// <summary>
        /// Gaze click event for the GazeButton control template.
        /// </summary>
        void onGazeClick(object s, EventArgs e)
        {
            var c = s as Clock;

            if (time.HasValue && c.CurrentTime.HasValue && c.CurrentTime.Value < time.Value)
            {
                // Find button.
                var activeWindow = Application.Current.Windows.OfType<Window>().Single(x => x.IsActive);
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
        /// Clear previous gaze data.
        /// </summary>
        public void Clear()
        {
            TrackingStatusChanged = null;
            gazes.Clear();
            offsets.Clear();
            time = null;
            notTracking = 0;
        }

        /// <summary>
        /// Restart main window.
        /// </summary>
        public void Restart()
        {
            var mw = new MainWindow();
            mw.ContentRendered += (s, e) => { MainWindow.Close(); MainWindow = mw; };
            mw.Show();
        }

        /// <summary>
        /// Clear previous gaze data and restart the main window.
        /// </summary>
        public void Reset()
        {
            Clear();
            Restart();
        }
    }
}
