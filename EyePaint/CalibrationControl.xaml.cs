using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Tobii.Gaze.Core;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for CalibrationControl.xaml
    /// </summary>
    public partial class CalibrationControl : UserControl
    {
        static EyeTrackingEngine eyeTracker = new EyeTrackingEngine();
        DispatcherTimer addCalibrationPointTimer;
        int measurement = 0, measurements = 3, scan = 0, scans = 2;
        double averageOffset;
        Point calibrationPoint, gazePoint;

        public CalibrationControl()
        {
            InitializeComponent();
            eyeTracker.GazePoint += (object s, GazePointEventArgs e) => gazePoint = e.GazePoint;
            addCalibrationPointTimer = new DispatcherTimer();
            addCalibrationPointTimer.Interval = new TimeSpan(0, 0, 1);
            addCalibrationPointTimer.Tick += (object sender, EventArgs e) =>
            {
                calibrationPoint = new Point((int)AnimatedTranslateTransform.X, (int)AnimatedTranslateTransform.Y);
                eyeTracker.addCalibrationPoint(calibrationPoint);
                averageOffset += Math.Sqrt(Math.Pow(calibrationPoint.X - gazePoint.X, 2) + Math.Pow(calibrationPoint.Y - gazePoint.Y, 2)) / measurements;
  
                //Info.Text = "Measurement: " + measurement + "/" + measurements + ", Average Offset: " + averageOffset + ", Scan:" + scan + "/" + scans;
                if (++measurement > measurements) stopScan();
            };
            startCalibration();
        }

        void startCalibration()
        {
            scan = 0;
            startScan();
        }

        void stopCalibration()
        {
            eyeTracker.stopCalibration();
            AppStateMachine.Instance.Next();
        }

        void startScan()
        {
            measurement = 0;
            averageOffset = 0;
            eyeTracker.startCalibration();
            CalibrationPoint.Visibility = Visibility.Visible;
            addCalibrationPointTimer.Start();
        }

        void stopScan()
        {
            eyeTracker.setCalibration();
            if (averageOffset <= 100) stopCalibration();
            else if (++scan < scans) startScan();
            else
            {
                Window confirmBox = new ConfirmBox("Kalibreringen blev dålig. Gör om?");
                confirmBox.ShowDialog();
                if (confirmBox.DialogResult.HasValue && confirmBox.DialogResult.Value) startCalibration();
            }
        }
    }
}
