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
        private Random rng;
        private readonly EyeTrackingEngine _eyeTrackingEngine; //TODO Remove underscore. Silly naming convention with an IDE.
        private Point _gazePoint; //TODO Remove underscore. Silly naming convention with an IDE.
        private CloudFactory cloudFactory;
        private ImageFactory imageFactory;
        private bool useMouse;
        private Timer paint;
        private Color currentColor;
        private readonly Color DEFAULT_COLOR = Color.Crimson;
        private delegate void UpdateStateDelegate(EyeTrackingStateChangedEventArgs eyeTrackingStateChangedEventArgs);

        public EyeTrackingForm(EyeTrackingEngine eyeTrackingEngine)
        {
            InitializeComponent();
            Shown += OnShown;
            Paint += OnPaint;
            Move += OnMove;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;

            _eyeTrackingEngine = eyeTrackingEngine;
            _eyeTrackingEngine.StateChanged += StateChanged;
            _eyeTrackingEngine.GazePoint += GazePoint;
            _eyeTrackingEngine.Initialize();

            int height = Screen.PrimaryScreen.Bounds.Height;
            int width = Screen.PrimaryScreen.Bounds.Width;
            imageFactory = new ImageFactory(width, height);
            cloudFactory = new CloudFactory(); //TODO This should be ERA's tree factory instead.

            rng = new Random();
            currentColor = DEFAULT_COLOR;

            paint = new System.Windows.Forms.Timer();
            paint.Interval = 33;
            paint.Enabled = false;
            paint.Tick += new EventHandler((object sender, System.EventArgs e) => { cloudFactory.GrowCloudRandomAmount(cloudFactory.clouds.Peek(), 10); Invalidate(); });
        }

        private void startPainting()
        {
            if (paint.Enabled)
                return;

            //TODO Make sure theres a gazepoint to paint with.

            setRandomPaintTool();
            paint.Enabled = true;
        }

        private void stopPainting()
        {
            paint.Enabled = false;
        }

        private void resetPainting()
        {
            imageFactory.Undo();
            Invalidate();
        }

        private void setRandomPaintTool()
        {
            //TODO Set more settings randomly than just the tool color.
            currentColor = Color.FromArgb(55 + rng.Next(200), rng.Next(255), rng.Next(255), rng.Next(255));
        }

        private void storePainting()
        {
            //TODO Call Henrik's IO library.
            throw new NotImplementedException();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (useMouse)
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        OnGreenButtonUp(sender, e);
                        break;
                    default:
                        break;
                }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (useMouse)
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        OnGreenButtonDown(sender, e);
                        break;
                    default:
                        break;
                }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (useMouse)
            {
                _gazePoint = new Point(e.X, e.Y);
                cloudFactory.AddCloud(PointToClient(_gazePoint), currentColor);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    OnGreenButtonDown(sender, e);
                    break;
                case Keys.Back:
                    OnRedButtonDown(sender, e);
                    break;
                case Keys.R:
                    currentColor = Color.Crimson;
                    break;
                case Keys.G:
                    currentColor = Color.ForestGreen;
                    break;
                case Keys.B:
                    currentColor = Color.CornflowerBlue;
                    break;
                case Keys.Enter:
                    storePainting();
                    break;
                default:
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    OnGreenButtonUp(sender, e);
                    break;
                case Keys.Back:
                    OnRedButtonUp(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void OnGreenButtonDown(object sender, EventArgs e)
        {
            startPainting();
        }


        private void OnGreenButtonUp(object sender, EventArgs e)
        {
            stopPainting();
        }

        private void OnRedButtonDown(object sender, EventArgs e)
        {
            resetPainting();
        }

        private void OnRedButtonUp(object sender, EventArgs e)
        {
            //TODO Define button behaviour.
        }

        private void OnMove(object sender, EventArgs e)
        {
            WarnIfOutsideEyeTrackingScreenBounds(); //TODO Neccessary?
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            try
            {
                Point[] points = new Point[cloudFactory.GetQueueLength()];
                int i = 0;
                while (cloudFactory.HasQueued())
                    points[i++] = cloudFactory.GetQueued();

                Image image = imageFactory.Rasterize(cloudFactory.clouds, points);
                e.Graphics.DrawImageUnscaled(image, new Point(0, 0));
            }
            catch (InvalidOperationException)
            {
                return; //TODO Improve exception handling.
            }
        }

        private void GazePoint(object sender, GazePointEventArgs e)
        {
            Point p1 = new Point(e.X, e.Y);
            Point p2 = _gazePoint; //TODO Is this a reference or does a wasteful copy occur?
            double distance = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            double KEYHOLE = 0; //TODO Make into class field.
            if (distance > KEYHOLE)
                _gazePoint = p1;

            //Point point = PointToClient(_gazePoint); TODO Neccessary?
            cloudFactory.AddCloud(_gazePoint, currentColor);
        }

        private void OnShown(object sender, EventArgs e)
        {
            WarnIfOutsideEyeTrackingScreenBounds();
            BringToFront();
        }

        private void StateChanged(object sender, EyeTrackingStateChangedEventArgs e)
        {
            // Forward state change to UI thread
            if (InvokeRequired)
            {
                var updateStateDelegate = new UpdateStateDelegate(UpdateState);
                Invoke(updateStateDelegate, new object[] { e });
            }
            else
            {
                UpdateState(e);
            }
        }

        private void UpdateState(EyeTrackingStateChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ErrorMessage))
            {
                InfoMessage.Visible = false;
                ErrorMessagePanel.Visible = true;
                ErrorMessage.Text = e.ErrorMessage;
                Resolve.Enabled = e.CanResolve;
                Retry.Enabled = e.CanRetry;
                return;
            }

            ErrorMessagePanel.Visible = false;

            if (e.EyeTrackingState != EyeTrackingState.Tracking)
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

        private void ExitClick(object sender, EventArgs e)
        {
            Environment.Exit(0);
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
            ErrorMessagePanel.Visible = false;
        }
    }
}
