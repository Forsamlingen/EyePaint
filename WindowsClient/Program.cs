using System;
using System.Windows.Forms;
using Tobii.Gaze.Core;

namespace EyePaint
{
    public static class Program
    {
        private static EyeTrackingEngine _eyeTrackingEngine;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using (_eyeTrackingEngine = new EyeTrackingEngine())
                {
                    Application.Run(new EyeTrackingForm(_eyeTrackingEngine));
                }
            }
            catch (EyeTrackerException e)
            {
                MessageBox.Show(e.Message, "Failed loading application!");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error!");
            }
        }
    }
}
