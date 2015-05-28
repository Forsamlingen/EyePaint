using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

    protected override void OnContentRendered(EventArgs e)
    {
      base.OnContentRendered(e);
      (App.Current as App).MainWindow = this;
    }

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);
      (App.Current as App).MainWindow = Owner;
    }

    void onClick(object s, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
