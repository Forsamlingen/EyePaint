using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Tobii.Gaze.Core;

namespace EyePaint
{
    /// <summary>
    /// Class representing the user's gaze point on the screen.
    /// </summary>
    public class GazePointEventArgs : EventArgs
    {
        public GazePointEventArgs(int x, int y) { GazePoint = new Point(x, y); }
        public Point GazePoint { get; private set; }
    }

    /// <summary>
    /// Class representing the user's head distance from the eye tracker camera.
    /// </summary>
    public class HeadMovementEventArgs : EventArgs
    {
        public HeadMovementEventArgs(double d) { Distance = d; }
        public double Distance { get; private set; }
    }

    /// <summary>
    /// Uses the Tobii Gaze SDK to establish a connection to an eye tracker unit and places the mouse pointer at the primary screen gaze point so that all WPF mouse events are essentially gaze enabled.
    /// </summary>
    public class EyeTrackingEngine
    {
        public readonly IEyeTracker iet;
        public event EventHandler<GazePointEventArgs> GazePoint;
        public event EventHandler<HeadMovementEventArgs> HeadMovement;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public EyeTrackingEngine()
        {
            try
            {
                Uri url = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
                iet = new EyeTracker(url);
                iet.EyeTrackerError += onEyeTrackerError;
                iet.GazeData += onGazeData;
                (new Task(() => { iet.RunEventLoop(); })).Start();
                iet.Connect();
                iet.StartTracking();
            }
            catch (EyeTrackerException e)
            {
                //TODO Abort if eye tracker was broken or non-existant.
            }
        }

        public void startCalibration()
        {
            bool done = false;
            iet.StartCalibrationAsync(_ => done = true);
            while (!done) continue;
        }

        public void addCalibrationPoint(Point p)
        {
            bool done = false;
            iet.AddCalibrationPointAsync(new Point2D(p.X, p.Y), _ => done = true);
            //while (!done) continue;
        }

        public void setCalibration()
        {
            iet.ComputeAndSetCalibrationAsync(e => Debug.WriteLine(e.ToString())); //TODO Throw exception if calibration failed due to insufficient gaze data.
        }

        public void stopCalibration()
        {
            bool done = false;
            iet.StopCalibrationAsync(_ => done = true);
            while (!done) continue;
        }

        void onEyeTrackerError(object s, EyeTrackerErrorEventArgs e)
        {
            //TODO Handle connection errors.
        }

        void onGazeData(object s, GazeDataEventArgs e)
        {
            //TODO Add support for one-eyed users (pirate-mode, yeaarrrgh!).
            if (e.GazeData.TrackingStatus == TrackingStatus.NoEyesTracked) return;
            else if (e.GazeData.TrackingStatus == TrackingStatus.BothEyesTracked)
            {
                // Calculate the user's head's distance from the eye tracker camera.
                //TODO Include the eye tracker angle in the mount when calculating the distance to the user's eyes.
                raiseHeadMoved((e.GazeData.Left.EyePositionFromEyeTrackerMM.Z + e.GazeData.Right.EyePositionFromEyeTrackerMM.Z) / 2);

                // Set gaze point on the screen.
                var x = (int)(System.Windows.SystemParameters.WorkArea.Width * (e.GazeData.Left.GazePointOnDisplayNormalized.X + e.GazeData.Right.GazePointOnDisplayNormalized.X) / 2);
                var y = (int)(System.Windows.SystemParameters.WorkArea.Height * (e.GazeData.Left.GazePointOnDisplayNormalized.Y + e.GazeData.Right.GazePointOnDisplayNormalized.Y) / 2);
                raiseGazePoint(x, y);

                // Set mouse cursor at the gaze point on the screen, so WPF mouse events are triggered by the gaze.
                SetCursorPos(x, y);
            }
        }

        void raiseHeadMoved(double d)
        {
            var handler = HeadMovement;
            if (handler != null)
            {
                handler(this, new HeadMovementEventArgs(d));
            }
        }

        void raiseGazePoint(int x, int y)
        {
            var handler = GazePoint;
            if (handler != null)
            {
                handler(this, new GazePointEventArgs(x, y));
            }
        }
    }
}
