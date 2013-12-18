namespace EyePaint
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Tobii.Gaze.Core;
    using System.Collections.Generic;
    using System.Diagnostics;

    public partial class EyeTrackingForm : Form
    {
        private readonly EyeTrackingEngine _eyeTrackingEngine; //TODO Remove underscore. Silly naming convention with an IDE.
        private Point _gazePoint; //TODO Remove underscore. Silly naming convention with an IDE.
        private bool gazeFixed;
        private TreeFactory treeFactory;
        private ImageFactory imageFactory;
        private bool useMouse;
        private Timer paint;
        private Color currentColor;
        private readonly Color DEFAULT_COLOR = Color.RoyalBlue;
        private delegate void UpdateStateDelegate(EyeTrackingStateChangedEventArgs eyeTrackingStateChangedEventArgs);

        public EyeTrackingForm(EyeTrackingEngine eyeTrackingEngine)
        {
            InitializeComponent();
            Shown += OnShown;
            Paint += OnPaint;
            Move += OnMove;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            MouseClick += OnMouseClick;

            _eyeTrackingEngine = eyeTrackingEngine;
            _eyeTrackingEngine.StateChanged += StateChanged;
            _eyeTrackingEngine.GazePoint += GazePoint;
            _eyeTrackingEngine.Initialize();

            int height = Screen.GetWorkingArea(this).Height;
            int width = Screen.GetWorkingArea(this).Width;
            imageFactory = new ImageFactory(width, height);
            treeFactory = new TreeFactory();

            currentColor = DEFAULT_COLOR;

            paint = new System.Windows.Forms.Timer();
            paint.Interval = 10;
            paint.Enabled = false;
            paint.Tick += new EventHandler((object sender, System.EventArgs e) => { treeFactory.ExpandTree(); Invalidate(); });
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (useMouse && !gazeFixed)
                _gazePoint = new Point(e.X, e.Y);
        }

        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            switch (eventArgs.KeyCode)
            {
                case Keys.Space:
                    OnGreenButtonDown(sender, eventArgs); // Simulate event.
                    break;
                case Keys.Back:
                    OnRedButtonDown(sender, eventArgs);
                    break;
                case Keys.R:
                    currentColor = Color.Red;
                    break;
                case Keys.G:
                    currentColor = Color.Green;
                    break;
                case Keys.B:
                    currentColor = Color.Blue;
                    break;
                default:
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs eventArgs)
        {
            switch (eventArgs.KeyCode)
            {
                case Keys.Space:
                    OnGreenButtonUp(sender, eventArgs); // Simulate event.
                    break;
                default:
                    break;
            }
        }

        private void OnGreenButtonDown(object sender, EventArgs eventArgs)
        {
            gazeFixed = true;
            treeFactory.CreateTree(PointToClient(_gazePoint), currentColor);
            paint.Enabled = true;
        }


        private void OnGreenButtonUp(object sender, EventArgs eventArgs)
        {
            gazeFixed = false;
            paint.Enabled = false;
        }

        private void OnRedButtonDown(object sender, EventArgs eventArgs)
        {
            imageFactory.Undo();
        }

        private void OnRedButtonUp(object sender, EventArgs eventArgs)
        {
            //TODO Define button behaviour.
        }

        private void OnMove(object sender, EventArgs eventArgs)
        {
            WarnIfOutsideEyeTrackingScreenBounds(); //TODO Neccessary?
        }

        private void OnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            try
            {
                var trees = treeFactory.trees;
                Image image = imageFactory.RasterizeTrees(trees);
                paintEventArgs.Graphics.DrawImageUnscaled(image, new Point(0, 0));
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        private void GazePoint(object sender, GazePointEventArgs gazePointEventArgs)
        {
            //TODO Add noise reduction and calibration.
            if (!gazeFixed)
                _gazePoint = new Point(gazePointEventArgs.X, gazePointEventArgs.Y);
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
                Invoke(updateStateDelegate, new object[] { eyeTrackingStateChangedEventArgs });
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

        private void EnableMouseClick(object sender, EventArgs e)
        {
            useMouse = true;
            ErrorMessagePanel.Visible = false; //TODO Verify that this is the best way to proceed to the painting canvas.
        }
    }
}
