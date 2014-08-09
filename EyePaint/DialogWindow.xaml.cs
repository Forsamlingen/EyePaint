using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        [DllImport("User32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        public DialogWindow(string instructions, string confirm, string cancel)
        {
            InitializeComponent();
            Instructions.Text = instructions;
            ConfirmButton.Tag = confirm;
            CancelButton.Tag = cancel;
            SetCursorPos(0, 0);
            ShowDialog();
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
