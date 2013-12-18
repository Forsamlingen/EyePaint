namespace EyePaint
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Tobii.Gaze.Core;
    using System.Collections.Generic;
    using System.Diagnostics;

    struct Line
    {
        public readonly Pen pen;
        public readonly List<Point> points;

        public Line(Pen pen)
        {
            this.pen = pen;
            this.points = new List<Point>();
        }
    }

    public partial class WinFormsSample : Form
    {
        private readonly EyeTrackingEngine _eyeTrackingEngine; //TODO Remove underscore. Silly naming convention with an IDE.
        private Point _gazePoint; //TODO Remove underscore. Silly naming convention with an IDE.
        private Stack<Line> lines; //TODO Should this be a queue?
        private bool draw;
        private bool useMouse;
        private delegate void UpdateStateDelegate(EyeTrackingStateChangedEventArgs eyeTrackingStateChangedEventArgs);

        //TODO
        Stopwatch stopwatch;
        //void OnGreenButtonDown(object sender, EventArgs e);
        //void OnGreenButtonUp(object sender, EventArgs e);
        //void OnRedButtonDown(object sender, EventArgs e);
        //void OnRedButtonUp(object sender, EventArgs e);

        public WinFormsSample(EyeTrackingEngine eyeTrackingEngine)
        {
            InitializeComponent();
            Shown += OnShown;
            Paint += OnPaint;
            Move += OnMove;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            MouseMove += OnMouseMove;

            _eyeTrackingEngine = eyeTrackingEngine;
            _eyeTrackingEngine.StateChanged += StateChanged;
            _eyeTrackingEngine.GazePoint += GazePoint;
            _eyeTrackingEngine.Initialize();

            lines = new Stack<Line>();
            draw = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyCode == Keys.Space)
            {
                // Enable drawing.
                draw = true;

                // Prepare new line to draw.
                Pen pen = new System.Drawing.Pen(System.Drawing.Color.Black, 3); //TODO Let user select different drawing tools.
                lines.Push(new Line(pen));
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyCode == Keys.Space)
                draw = false;
        }

        private void OnMove(object sender, EventArgs eventArgs)
        {
            WarnIfOutsideEyeTrackingScreenBounds();
        }

        private void OnPaint(object sender, PaintEventArgs paintEventArgs)
        {
            // Get current eye position.
            var local = PointToClient(_gazePoint);

            // Mark current eye position.
            paintEventArgs.Graphics.FillEllipse(Brushes.Black, local.X - 25, local.Y - 25, 20, 20); //TODO Remove?

            // Store where the user is looking.
            if (draw) try
                {
                    Line currentLine = lines.Peek(); //TODO Figure out of currentLine is copied or just a reference?
                    currentLine.points.Add(local);
                }
                catch (InvalidOperationException e)
                {
                    //TODO Could not draw. What should we do here?
                }

            // Render drawing.
            foreach (Line line in lines)
                //if (line.points.Count > 1) //TODO Remove?
                paintEventArgs.Graphics.DrawCurve(line.pen, line.points.ToArray());
        }

        private void GazePoint(object sender, GazePointEventArgs gazePointEventArgs)
        {
            _gazePoint = new Point(gazePointEventArgs.X, gazePointEventArgs.Y);
            Invalidate();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (useMouse)
            {
                _gazePoint = new Point(e.X, e.Y);
                Invalidate(); //TODO Maybe only force rerendering if painting has changed? Might make for uneven performance though.

                //Console.WriteLine("Mouse position: " + _gazePoint.ToString()); //TODO Remove line.
            }
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
