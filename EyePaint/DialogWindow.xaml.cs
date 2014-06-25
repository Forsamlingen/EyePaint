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
        //TODO Remove.
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public DialogWindow(string instructions, Action a = null)
        {
            InitializeComponent();
            Instructions.Text = instructions;
            SetCursorPos(-1, -1); //TODO Remove.

            if (a != null)
            {
                Buttons.Visibility = Visibility.Collapsed;
                FadeIn.Completed += (s, e) => { a(); Close(); };
            }

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

        void onClose(object s, EventArgs e)
        {
            Close();
        }
    }
}
