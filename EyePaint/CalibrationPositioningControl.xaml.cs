using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for CalibrationPositioningControl.xaml
    /// </summary>
    public partial class CalibrationPositioningControl : UserControl
    {
        static EyeTrackingEngine eyeTracker = new EyeTrackingEngine();

        public CalibrationPositioningControl()
        {
            InitializeComponent();
        }

        void onLoaded(object s, RoutedEventArgs e)
        {
            //TODO Use DependencyProperty instead.
            (new Task(() =>
            {
                while (true) Dispatcher.BeginInvoke(new Action(() =>
                {
                    //TODO Include the eye tracker angle in the mount when calculating the distance to the user's eyes.
                    Blur.Radius = Math.Abs(eyeTracker.distance - 550);
                    if (eyeTracker.distance < 400) Instructions.Text = "Sitt längre bak";
                    else if (eyeTracker.distance > 700) Instructions.Text = "Sitt närmre";
                    else ((ContentControl)Parent).Content = new CalibrationControl();
                })).Wait();
            })).Start();
        }
    }
}
