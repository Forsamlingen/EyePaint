using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        void onSuccess(object s, RoutedEventArgs e)
        {
            ((ContentControl)Parent).Content = new CalibrationEvaluationControl();
        }

        void onLoaded(object s, RoutedEventArgs e)
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
