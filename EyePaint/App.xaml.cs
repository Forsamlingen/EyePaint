using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
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
        Queue<Point> gazePoints;
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
                gazePoints = new Queue<Point>();
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
            gazePoints.Enqueue(newGazePoint);

            var gazePoint = new Point(
                gazePoints.Average(x => x.X),
                gazePoints.Average(x => x.Y)
            );

            if ((newGazePoint - gazePoint).Length > 50)
            {
                gazePoint = newGazePoint;
                gazePoints.Clear();
            }

            var offset = gazePoint - previousGazePoint;
            if (offset.Length < 100) gazePoint += offset;

            SetCursorPos((int)gazePoint.X, (int)gazePoint.Y);
        }
    }
}
