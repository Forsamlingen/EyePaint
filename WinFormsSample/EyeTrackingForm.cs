namespace WinFormsSample
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Tobii.Gaze.Core;

    public partial class WinFormsSample : Form
    {
        private readonly EyeTrackingEngine _eyeTrackingEngine;
        private Point _gazePoint;
        private delegate void UpdateStateDelegate(EyeTrackingStateChangedEventArgs eyeTrackingStateChangedEventArgs);
    
        public WinFormsSample(EyeTrackingEngine eyeTrackingEngine)
        {
            InitializeComponent();
            Shown += OnShown;
            Paint += OnPaint;
            Move += OnMove;
            
            _eyeTrackingEngine = eyeTrackingEngine;
            _eyeTrackingEngine.StateChanged += StateChanged;
            _eyeTrackingEngine.GazePoint += GazePoint;
            _eyeTrackingEngine.Initialize();
        }

        private void OnMove(object sender, EventArgs eventArgs)
        {
            WarnIfOutsideEyeTrackingScreenBounds();
        }

        private void OnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            var local = PointToClient(_gazePoint);
            paintEventArgs.Graphics.FillEllipse(Brushes.Red, local.X - 25, local.Y - 25, 20, 20);
        }

        private void GazePoint(object sender, GazePointEventArgs gazePointEventArgs)
        {
            _gazePoint = new Point(gazePointEventArgs.X, gazePointEventArgs.Y);
            Invalidate();
        }
        
        private void OnShown(object sender, EventArgs eventArgs)
        {
            WarnIfOutsideEyeTrackingScreenBounds();
            BringToFront();
        }

        private void StateChanged(object sender, EyeTrackingStateChangedEventArgs eyeTrackingStateChangedEventArgs)
        {
            // Forward state change to UI thread
            if (InvokeRequired)
            {
                var updateStateDelegate = new UpdateStateDelegate(UpdateState);
                Invoke(updateStateDelegate, new object[] {eyeTrackingStateChangedEventArgs});        
            }
            else
            {
                UpdateState(eyeTrackingStateChangedEventArgs);
            }
        }

        private void UpdateState(EyeTrackingStateChangedEventArgs eyeTrackingStateChangedEventArgs)
        {
            if (!string.IsNullOrEmpty(eyeTrackingStateChangedEventArgs.ErrorMessage))
            {
                InfoMessage.Visible = false;
                ErrorMessagePanel.Visible = true;
                ErrorMessage.Text = eyeTrackingStateChangedEventArgs.ErrorMessage;
                Resolve.Enabled = eyeTrackingStateChangedEventArgs.CanResolve;
                Retry.Enabled = eyeTrackingStateChangedEventArgs.CanRetry; 
                return;
            }
            
            ErrorMessagePanel.Visible = false;

            if (eyeTrackingStateChangedEventArgs.EyeTrackingState != EyeTrackingState.Tracking)
            {
                InfoMessage.Visible = true;
                InfoMessage.Text = "Connecting to eye tracker, please wait...";
            }
            else
            {
                InfoMessage.Visible = false;
            }
        }

        private void OpenControlPanelClick(object sender, EventArgs e)
        {
            _eyeTrackingEngine.ResolveError();
        }

        private void RetryClick(object sender, EventArgs e)
        {
            _eyeTrackingEngine.Retry();
        }
        
        private void SuppressErrorMessageClick(object sender, EventArgs e)
        {
            ErrorMessagePanel.Visible = false;
        }

        private void WarnIfOutsideEyeTrackingScreenBounds()
        {
            var screenBounds = _eyeTrackingEngine.EyeTrackingScreenBounds;
            
            if (screenBounds.HasValue && (Bounds.Left > screenBounds.Value.Right || Bounds.Right < screenBounds.Value.X))
            {
                InfoMessage.Visible = true;
                InfoMessage.Text = "Warning!! Application window is outside of tracking area";
                InfoMessage.BringToFront();
            }
            else
            {
                InfoMessage.Visible = false;
            }
        }
    }
}
