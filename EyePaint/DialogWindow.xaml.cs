using System;
using System.ComponentModel;
using System.Windows;

namespace EyePaint
{
  /// <summary>
  /// Used to display a yes/no dialog to the user.
  /// </summary>
  public partial class DialogWindow : Window
  {
    public DialogWindow(Window owner)
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
