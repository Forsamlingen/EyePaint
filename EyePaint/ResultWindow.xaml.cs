using System.Windows;
using System.Windows.Media;

namespace EyePaint
{
    /// <summary>
    /// Used to display the resulting drawing in its own window.
    /// </summary>
    public partial class ResultWindow : Window
    {
        public ResultWindow()
        {
            InitializeComponent();
        }

        public void SetImageSource(ImageSource imageSource) {
            Result.Source = imageSource;
        }
    }
}
