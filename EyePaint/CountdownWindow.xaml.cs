using System;
using System.Windows;

namespace EyePaint
{
    /// <summary>
    /// Used to display a countdown to the user before returning a dialog result. The user can abort the countdown with a keypress.
    /// </summary>
    public partial class CountdownWindow : Window
    {
        public CountdownWindow()
        {
            InitializeComponent();
            ShowDialog();
        }

        void onConfirm(object s, EventArgs e)
        {
            //TODO Fix repeated restart presses bug.
            DialogResult = true;
        }

        void onCancel(object s, EventArgs e)
        {
            DialogResult = false;
        }
    }
}
