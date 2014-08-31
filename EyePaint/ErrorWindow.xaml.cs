using System.Windows;
using System.Windows.Input;

namespace EyePaint
{
    /// <summary>
    /// Used to display an error message to the user when the appliction is experiencing a critical halt, such as hardware faults.
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }
    }
}
