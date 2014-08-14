using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tobii.Gaze.Core;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        IEyeTracker iet;
        List<Point> gazePoints = new List<Point>();
        Dictionary<Point, Vector> calibrationPoints = new Dictionary<Point, Vector>();
        TimeSpan? time;

        [DllImport("User32.dll")]
        static public extern bool SetCursorPos(int X, int Y);

        void onStartup(object s, StartupEventArgs e)
        {
            connect();
        }

        void onEyeTrackerError(object s, EyeTrackerErrorEventArgs e)
        {
            connect();
        }

        void connect()
        {
            try
            {
                Uri url = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
                iet = new EyeTracker(url);
                iet.EyeTrackerError += onEyeTrackerError;
                iet.GazeData += onGazeData;
                Task.Factory.StartNew(() => iet.RunEventLoop());
                iet.Connect();
                iet.StartTracking();
            }
            catch (EyeTrackerException)
            {
                //error();
            }
            catch (NullReferenceException)
            {
                //error();
            }
        }

        void error()
        {
            //TODO Implement email error report.
            /*
            string to = "foo@.com";
            string from = "bar@.com";
            string subject = "Test subject";
            string body = "Test message.";
            MailMessage message = new MailMessage(from, to, subject, body);
            SmtpClient client = new SmtpClient(server);
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
            client.Send(message);
            */
            (new ErrorWindow()).Show();
        }

        void onGazeData(object s, GazeDataEventArgs e)
        {
            if (e.GazeData.TrackingStatus == TrackingStatus.NoEyesTracked) return;
            var left = (e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyLeft || e.GazeData.TrackingStatus == TrackingStatus.OnlyLeftEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Left.GazePointOnDisplayNormalized : new Point2D(0, 0);
            var right = (e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedProbablyRight || e.GazeData.TrackingStatus == TrackingStatus.OnlyRightEyeTracked || e.GazeData.TrackingStatus == TrackingStatus.OneEyeTrackedUnknownWhich) ? e.GazeData.Right.GazePointOnDisplayNormalized : new Point2D(0, 0);

            var newGazePoint = Dispatcher.Invoke(() =>
            {
                return new Point(
                    (int)Application.Current.MainWindow.ActualWidth * (left.X + right.X) / 2,
                    (int)Application.Current.MainWindow.ActualHeight * (left.Y + right.Y) / 2
                );
            });
            gazePoints.Add(newGazePoint);

            while (gazePoints.Count > 50) gazePoints.RemoveAt(0);

            // Reduce noise by averaging incoming gaze points.
            var gazePoint = new Point(
                gazePoints.Average(p => p.X),
                gazePoints.Average(p => p.Y)
            );

            if ((newGazePoint - gazePoint).Length > 100)
            {
                gazePoints.Clear();
                gazePoints.Add(newGazePoint);
                gazePoint = newGazePoint;
            }

            // Calibrate point with known offsets.
            var distances = calibrationPoints.Select(kvp => (gazePoint - kvp.Key).Length);
            var normalizedDistances = distances.Select(d => d / distances.Sum());
            foreach (var v in calibrationPoints.Zip(normalizedDistances, (kvp, d) => kvp.Value)) //TODO Scale by normalized distance.
            {
                gazePoint.Offset(v.X, v.Y);
            }

            SetCursorPos((int)gazePoint.X, (int)gazePoint.Y);
        }

        //TODO Fix gaze click sequence bug.
        void onGazeClick(object s, EventArgs e)
        {
            var c = s as Clock;

            if (time.HasValue && c.CurrentTime.HasValue && c.CurrentTime.Value < time.Value)
            {
                // Find button.
                var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                var focusedButton = FocusManager.GetFocusedElement(activeWindow) as Button;

                // Store calibration offset.
                if (gazePoints.Count > 0)
                {
                    var expectedPoint = focusedButton.PointToScreen(new Point(focusedButton.ActualWidth / 2, focusedButton.ActualHeight / 2));
                    var actualPoint = Mouse.GetPosition(activeWindow);
                    calibrationPoints[actualPoint] = expectedPoint - actualPoint;
                }

                // Raise click event.
                focusedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }

            time = (c.CurrentState == ClockState.Active) ? c.CurrentTime : null;
        }
    }
}
