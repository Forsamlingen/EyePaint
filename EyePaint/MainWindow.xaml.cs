using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            (new Welcome()).ShowDialog(); //TODO Reshow the welcome window according to an inactivity timer.
            (new TrackBoxPositioningWindow()).ShowDialog();
            (new CalibrationWindow()).ShowDialog();
            (new CalibrationEvaluationWindow()).ShowDialog(); //TODO Don't continue to the Paint window if the calibration was bad, instead ask the user if they want to recalibrate.
            (new Paint()).ShowDialog();
        }
    }
}
