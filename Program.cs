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
            // Initialize the EyeX Engine client library and launch the paint form application.
            using (var system = InteractionSystem.Initialize(LogTarget.Trace)) Application.Run(new EyePaintingForm());
        }
    }
}
