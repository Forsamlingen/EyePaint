using System;
using System.Windows.Forms;
using Tobii.EyeX.Client;

namespace EyePaint
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the EyeX Engine client library and launch the paint form application.
            try
            {
                using (var system = InteractionSystem.Initialize(LogTarget.Trace))
                {
                    Application.Run(new EyePaintingForm());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Failed loading application!");
            }
        }
    }
}
