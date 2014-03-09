using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EyePaint
{
    public partial class ConfirmBox : Form
    {
        public ConfirmBox()
        {
            InitializeComponent();
        }

        public DialogResult Show(IWin32Window owner, string text)
        {
            TextBox.Text = text;
            return this.ShowDialog(owner);
        }
    }
}
