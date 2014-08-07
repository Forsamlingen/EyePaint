using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
        PointCollection gazePoints = new PointCollection();
        List<Vector> offsets = new List<Vector>();
        static public bool EyeTracking { get; private set; }

        [DllImport("User32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        public App()
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
                EyeTracking = true;
            }
            catch (EyeTrackerException) { EyeTracking = false; }
            catch (NullReferenceException) { EyeTracking = false; }
        }

        void onEyeTrackerError(object s, EyeTrackerErrorEventArgs e)
        {
            EyeTracking = false; //TODO Retry eye tracker connection, and eventually notify the museum staff.
        }

        void onGazeData(object s, GazeDataEventArgs e)
        {
            if (e.GazeData.TrackingStatus != TrackingStatus.BothEyesTracked) return; //TODO Implement one-eye operation.

            var previousGazePoint = Dispatcher.Invoke<Point>(() => { return Mouse.GetPosition(Application.Current.MainWindow); });

            var newGazePoint = Dispatcher.Invoke<Point>(() =>
            {
                return new Point(
                    Application.Current.MainWindow.ActualWidth * (e.GazeData.Left.GazePointOnDisplayNormalized.X + e.GazeData.Right.GazePointOnDisplayNormalized.X) / 2,
                    Application.Current.MainWindow.ActualHeight * (e.GazeData.Left.GazePointOnDisplayNormalized.Y + e.GazeData.Right.GazePointOnDisplayNormalized.Y) / 2
                );
            });
            gazePoints.Add(newGazePoint);

            var gazePoint = new Point(
                gazePoints.Average(x => x.X),
                gazePoints.Average(x => x.Y)
            );

            //TODO Verify that vector length cannot be negative.
            if ((newGazePoint - gazePoint).Length > 50)
            {
                gazePoint = newGazePoint;
                gazePoints.Clear();
                gazePoints.Add(newGazePoint);
            }

            /* TODO Experiment with exploiting mouse chase to live calibrate.
            var offset = gazePoint - previousGazePoint;
            if (offset.Length < 100) gazePoint += offset;
            */

            SetCursorPos((int)gazePoint.X, (int)gazePoint.Y);
        }

        void onGazeClick(object s, EventArgs e)
        {
            var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            var focusedButton = FocusManager.GetFocusedElement(activeWindow) as Button;
            focusedButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            if (gazePoints.Count > 0)
            {
                var expectedPoint = focusedButton.PointToScreen(new Point(focusedButton.ActualWidth / 2, focusedButton.ActualHeight / 2));
                var actualPoint = new Point(
                    gazePoints.Average(x => x.X),
                    gazePoints.Average(x => x.Y)
                );
                offsets.Add(actualPoint - expectedPoint);
            }
        }

        Point calibrate(Point pointToCalibrate, PointCollection calibrationPoints)
        {
            var calibratedPoint = new Point(pointToCalibrate.X, pointToCalibrate.Y); // TODO Remove uneccessary copy.
            //TODO Weight by distance to calibration point.
            //var distances = calibrationPoints.Select(p => (pointToCalibrate - p).Length);
            //var normalizedDistances = distances.Select(d => d / distances.Average());
            foreach (var o in offsets) calibratedPoint.Offset(o.X, o.Y);
            return calibratedPoint;
        }
    }
}
