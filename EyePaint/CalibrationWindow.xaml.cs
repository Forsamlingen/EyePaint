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
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : UserControl
    {
        IEyeTracker iet;
        int calibrationStep = 10; //TODO Don't hardcode. Make into a resource property instead.

        public CalibrationWindow()
        {
            InitializeComponent();

            try
            {
                Uri url = new EyeTrackerCoreLibrary().GetConnectedEyeTracker();
                iet = new EyeTracker(url);
                Thread t = new Thread(() => { iet.RunEventLoop(); });
                t.Start();
                iet.Connect();
                iet.StartCalibrationAsync(addCalibrationPoints);
            }
            catch (EyeTrackerException)
            {
                exit(false);
            }
            catch (NullReferenceException)
            {
                exit(false);
            }
        }

        void exit(bool status)
        {
        }

        void stopCalibrationCallback(ErrorCode e)
        {
            iet.Disconnect();
            exit(true);
        }

        void computeAndSetCalibrationCallback(ErrorCode e)
        {
            iet.StopCalibrationAsync(stopCalibrationCallback);
        }

        void addCalibrationPoints(ErrorCode e)
        {
            while (calibrationStep-- > 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CalibrationAnimation.Pause();
                    var p = new Point2D(CalibrationPoint.Center.X, CalibrationPoint.Center.Y);
                    iet.AddCalibrationPointAsync(p, addCalibrationPoints);
                    CalibrationAnimation.Resume();
                })).Wait();
                Thread.Sleep(1000);
            }
            iet.ComputeAndSetCalibrationAsync(computeAndSetCalibrationCallback);
        }

        private void GazePoint_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Over");
            ContentControl ctrl = (ContentControl)Parent;
            ctrl.Content = new Paint();
        }
    }
}
