using System.Windows;

namespace EyePaint
{
    /// <summary>
    /// Displays editable settings to the museum staff.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = Properties.Settings.Default;
        }

        void onSaveButtonClick(object s, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            MessageBox.Show("Inställningarna har sparats.");
            Close();
        }
    }
}
