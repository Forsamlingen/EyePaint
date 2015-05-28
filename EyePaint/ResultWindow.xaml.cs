using System;
using System.Windows;
using System.Windows.Media;
using System.Linq;


namespace EyePaint
{
  /// <summary>
  /// Mirrors the drawing in realtime during painting on a secondary monitor.
  /// </summary>
  public partial class ResultWindow : Window
  {
    public ResultWindow()
    {
      InitializeComponent();
      Show();
    }

    public void SetPaintWindow(PaintWindow paintWindow)
    {
      Drawing.Source = paintWindow.Drawing.Source;
    }

    protected override void OnContentRendered(EventArgs e)
    {
      base.OnContentRendered(e);

      var secondaryScreen = System.Windows.Forms.Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();

      if (secondaryScreen == null) // Only one screen available, so minimize result window.
      {
        WindowState = WindowState.Minimized;
      }
      else // Maximize on secondary screen.
      {
        if (!IsLoaded) WindowStartupLocation = WindowStartupLocation.Manual;

        var workingArea = secondaryScreen.WorkingArea;
        Left = workingArea.Left;
        Top = workingArea.Top;
        Width = workingArea.Width;
        Height = workingArea.Height;

        // If window isn't loaded then maxmizing will result in the window displaying on the primary monitor
        if (IsLoaded) WindowState = WindowState.Maximized;
      }
    }

  }
}