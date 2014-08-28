using System;
using System.Windows;

namespace EyePaint
{
    /// <summary>
    /// Used to display a yes/no dialog to the user.
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
            ShowDialog();
        }

        void onContentVisible(object s, EventArgs e)
        {
            IsEnabled = ((App)Application.Current).Tracking;
            ((App)Application.Current).TrackingChanged += (_s, _e) => Dispatcher.Invoke(() => IsEnabled = _e.Tracking);
        }

        void onConfirm(object s, EventArgs e)
        {
            DialogResult = true;
        }

        void onCancel(object s, EventArgs e)
        {
            DialogResult = false;
        }
    }
}
