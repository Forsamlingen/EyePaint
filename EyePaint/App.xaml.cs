using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //static EyeTrackingEngine eyeTracker = new EyeTrackingEngine(); //TODO Handle hardware errors gracefully.

        void onStartup(object sender, StartupEventArgs e)
        {
            (new MainWindow()).Show();
        }
    }
}
