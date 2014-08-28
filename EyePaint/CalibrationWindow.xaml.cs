using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace EyePaint
{
    /// <summary>
    /// Used to reduce calibration offset errors amongst different users.
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        public CalibrationWindow()
        {
            InitializeComponent();
            ShowDialog();
        }

        void onClick(object s, RoutedEventArgs e)
        {
            Close();
        }

        void onContentVisible(object s, EventArgs e)
        {
            //TODO
            IsEnabled = ((App)Application.Current).Tracking;
            ((App)Application.Current).TrackingChanged += (_s, _e) => Dispatcher.Invoke(() => IsEnabled = _e.Tracking);
        }
    }
}
