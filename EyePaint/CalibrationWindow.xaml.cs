using System;
using System.Windows;

namespace EyePaint
{
  /// <summary>
  /// Used to reduce calibration offset errors amongst different users.
  /// </summary>
  public partial class CalibrationWindow : Window
  {
    public CalibrationWindow(Window owner)
    {
      InitializeComponent();
      Owner = owner;
      ShowDialog();
    }

    void onClick(object s, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
