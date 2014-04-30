using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for PositioningControl.xaml
    /// </summary>
    public partial class PositioningControl : UserControl
    {
        static EyeTrackingEngine eyeTracker = new EyeTrackingEngine();
        bool stable = false;
        const double OPTIMAL_DISTANCE_FROM_EYE_TRACKER = 500; //TODO Set this value for the actual hardware installation.

        public PositioningControl()
        {
            InitializeComponent();
        }

        void onLoaded(object s, RoutedEventArgs e)
        {
            eyeTracker.HeadMovement += onPositionChanged;
            Focus();
        }

        void onUnloaded(object s, RoutedEventArgs e)
        {
            eyeTracker.HeadMovement -= onPositionChanged;
        }

        void onKeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Space && stable) AppStateMachine.Instance.Next();
        }

        void onPositionChanged(object s, HeadMovementEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Blur.Radius = Math.Abs(e.Distance - OPTIMAL_DISTANCE_FROM_EYE_TRACKER);
                stable = 300 <= e.Distance && e.Distance <= 600;
            });
        }
    }
}
