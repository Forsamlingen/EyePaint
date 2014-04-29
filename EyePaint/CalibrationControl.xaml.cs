using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tobii.Gaze.Core;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for CalibrationControl.xaml
    /// </summary>
    public partial class CalibrationControl : UserControl
    {
        static EyeTrackingEngine eyeTracker = new EyeTrackingEngine();
        int calibrationStep = 10; //TODO Don't hardcode.

        public CalibrationControl()
        {
            InitializeComponent();
        }

        void onLoaded(object s, RoutedEventArgs e)
        {
            new Task(calibrate);
        }

        void onUnloaded(object s, RoutedEventArgs e)
        {
            //TODO
        }

        void calibrate()
        {
            eyeTracker.startCalibration();
            while (calibrationStep-- > 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MovementAnimation.Pause();
                    ShrinkAnimation.Begin();
                    eyeTracker.addCalibrationPoint(CalibrationPoint.Center);
                    MovementAnimation.Resume();
                })).Wait();
                Thread.Sleep(1000);
            }
            eyeTracker.stopCalibration();
        }
    }
}
