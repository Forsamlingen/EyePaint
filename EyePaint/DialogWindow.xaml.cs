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
