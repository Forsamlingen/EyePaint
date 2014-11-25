using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace EyePaint
{
  /// <summary>
  /// Displays a countdown before closing itself. Present alternative options during the countdown.
  /// </summary>
  public partial class StartupWindow : Window
  {
    public StartupWindow()
    {
      InitializeComponent();
      ShowDialog();
    }

    void onContentRendered(object s, EventArgs e)
    {
      (FindResource("CountdownAnimation") as Storyboard).Begin();
    }

    void onShowSettingsButtonClick(object s, RoutedEventArgs e)
    {
      (FindResource("CountdownAnimation") as Storyboard).Stop();
      new SettingsWindow();
      Close();
    }

    void onCountdownComplete(object s, EventArgs e)
    {
      Close();
    }
  }
}
