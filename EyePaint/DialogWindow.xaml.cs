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
        public DialogWindow(string instructions, Action a = null)
        {
            InitializeComponent();
            Instructions.Text = instructions;

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
