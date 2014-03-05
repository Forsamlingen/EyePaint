namespace EyePaint
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Client.Interop;
    using Tobii.EyeX.Framework;

    public partial class EyePaintingForm : Form, IDisposable
    {

        // Eye tracking.
        private readonly string interactorId = "EyePaint" + System.Threading.Thread.CurrentThread.ManagedThreadId; // TODO Make into property.
        private InteractionContext context;
        private InteractionSnapshot globalInteractorSnapshot;
        private bool stableGaze;
        private Point gazePoint, latestPoint;
        private const int keyhole = 120; //TODO Make into property.

        // User input.
        private bool useMouse;
        private bool greenButtonPressed;

        // Painting.
        private Timer paint;

        private readonly Dictionary<int, PaintTool> paintTools; //All availble PaintTools maped agains there ID
        private readonly Dictionary<int, ColorTool> colorTools; //All availble ColorTools maped agains there ID

        private PaintTool presentPaintTool;
        private ColorTool presentColorTool;
        private Model model;
        private View view;


        public EyePaintingForm()
        {
            InitializeComponent();

            Shown += OnShown;
            Move += OnMove;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;

            // Create a context and enable the connection to the eye tracking engine.
            context = new InteractionContext(false);
            InitializeGlobalInteractorSnapshot();
            context.ConnectionStateChanged += OnConnectionStateChanged;
            context.RegisterEventHandler(HandleInteractionEvent);
            context.EnableConnection();

            // Start values.
            stableGaze = false;
            greenButtonPressed = false;
            gazePoint = new Point(0, 0);
            latestPoint = new Point(0, 0);

            // Program resolution.
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            //Initialize Model and View
            SettingFactory sf = new SettingFactory();
            paintTools = sf.getPaintTools();
            colorTools = sf.getColorTools();

            //TODO Change when we agreed on how init paintTool is chosed
            int randPaintToolId = getRandomPaintToolID();
            int randColorToolId = getRandomColorToolID();
            presentPaintTool = paintTools[randPaintToolId];
            presentColorTool = colorTools[randColorToolId];
            model = new Model(presentPaintTool, presentColorTool);
            view = new View(width, height);

            // Create a paint event with a corresponding timer. The timer is the paint refresh interval (i.e. similar to rendering FPS).
            Paint += OnPaint;
            paint = new System.Windows.Forms.Timer();
            paint.Interval = 33; //TODO Make into property.
            paint.Enabled = false;
            paint.Tick += onTick;
        }

        //Todo Take away after testing
        private int getRandomPaintToolID()
        {
            List<int> toolIDs = new List<int>(paintTools.Keys);
            Random rng = new Random();
            int randomIdIndex = rng.Next(toolIDs.Count);
            int randomID = toolIDs[randomIdIndex];
            return randomID;
        }

        //Todo Take away after testing
        private int getRandomColorToolID()
        {
            List<int> toolIDs = new List<int>(colorTools.Keys);
            Random rng = new Random();
            int randomIdIndex = rng.Next(toolIDs.Count);
            int randomID = toolIDs[randomIdIndex];
            return randomID;
        }


        // Grows the model and refreshes the canvas
        private void onTick(object sender, System.EventArgs e)
        {
            model.Grow();
            Invalidate();
        }

        // Starts the timer, enabling tick events
        private void startPaintingTimer()
        {
            if (paint.Enabled) return;

            if (!stableGaze) return;

            paint.Enabled = true;
        }

        // Stops the timer, disabling tick events
        private void stopPaintingTimer()
        {
            paint.Enabled = false;
        }

        // Rasterizes the model and returns an image object
        private Image getPainting()
        {
            Image image = view.Rasterize(model.GetRenderQueue());
           // model.ClearRenderQueue();
            return image;
        }

        // Clears the canvas
        private void resetPainting()
        {
            model.ResetModel();
            view.Clear();
            Invalidate();
        }

        // Writes rasterized image to a file
        private void storePainting()
        {
            Image image = getPainting();
            image.Save("painting.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (useMouse)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        OnGreenButtonUp(sender, e);
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (useMouse)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        OnGreenButtonDown(sender, e);
                        break;
                    default:
                        break;
                }
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
                //TODO Take away after test
                case Keys.R:
                    int newRandomToolID = getRandomPaintToolID();
                    ChangePaintTool(newRandomToolID);
                    break;
                case Keys.G:
                    break;
                case Keys.B:
                    break;
                case Keys.S:
                    storePainting();
                    break;
                case Keys.Escape:
                    SetupPanel.Visible = SetupPanel.Enabled = true;
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
            if (greenButtonPressed) return;

            greenButtonPressed = true;
            gazePoint = latestPoint;
            AddPoint(gazePoint, true);
            startPaintingTimer();
        }

        private void OnGreenButtonUp(object sender, EventArgs e)
        {
            greenButtonPressed = false;
            gazePoint = latestPoint;
            stopPaintingTimer();
        }

        private void OnRedButtonDown(object sender, EventArgs e)
        {
            resetPainting();
        }

        private void OnRedButtonUp(object sender, EventArgs e)
        {
            return;
        }

        private void OnMove(object sender, EventArgs e)
        {
            stableGaze = false; //TODO Neccessary?
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Image image = getPainting();
            if (image != null)
            {
                e.Graphics.DrawImageUnscaled(image, new Point(0, 0));
            }
        }

        // Adds a new point to the model.
        private void AddPoint(Point p, bool alwaysAdd = false)
        {
            model.Add(p, alwaysAdd);
        }

        // Stores a new point in 'latestPoint' and determines whether or not to add it.
        private void TrackPoint(Point p)
        {
            stableGaze = true;
            latestPoint = p;

            double distance = Math.Sqrt(Math.Pow(gazePoint.X - p.X, 2) + Math.Pow(gazePoint.Y - p.Y, 2));

            if (distance > keyhole)
            {
                gazePoint = p;
                if (greenButtonPressed) AddPoint(gazePoint);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (useMouse) TrackPoint(new Point(e.X, e.Y));
        }

        private void OnShown(object sender, EventArgs e)
        {
            BringToFront();
        }

        //TODO Maybe change after agree on type of id for PaintTools
        private void ChangePaintTool(int newPaintToolID)
        {

            //Check if the new paint tool is the same as the present do nothing
            if (presentPaintTool.id == newPaintToolID)
            {
                return;
            }


            //Apply change to model
            model.ChangePaintTool(paintTools[newPaintToolID]);

            //Set present PaintTool to the new one
            presentPaintTool = paintTools[newPaintToolID];

        }
        private void ChangeColorTool(int newColorToolID)
        {
            model.ChangeColorTool(colorTools[newColorToolID]);
        }

        private void OpenControlPanelClick(object sender, EventArgs e)
        {
            Process.Start("C:\\Program Files\\Tobii\\EyeTracking\\Tobii.EyeTracking.ControlPanel.exe"); //TODO Don't assume default install location.
        }

        private void CloseInfoPanelClick(object sender, EventArgs e)
        {
            SetupPanel.Visible = SetupPanel.Enabled = false;
        }

        private void UseEyeTrackerClick(object sender, EventArgs e)
        {
            useMouse = false;
            SetupPanel.Visible = SetupPanel.Enabled = false;
        }

        private void UseMouseClick(object sender, EventArgs e)
        {
            useMouse = true;
            SetupPanel.Visible = SetupPanel.Enabled = false;
        }

        private void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            switch (e.State)
            {
                case ConnectionState.Connected:
                    globalInteractorSnapshot.Commit(OnSnapshotCommitted);
                    Invoke(new Action(() =>
                    {
                        SetupMessage.Text = "Status: " + e.State.ToString();
                        SetupPanel.Visible = SetupPanel.Enabled = false;
                    }));
                    break;
                case ConnectionState.Disconnected:
                    break;
                case ConnectionState.ServerVersionTooHigh:
                    break;
                case ConnectionState.ServerVersionTooLow:
                    break;
                case ConnectionState.TryingToConnect:
                    Invoke(new Action(() =>
                    {
                        SetupPanel.Visible = SetupPanel.Enabled = true;
                    }));
                    break;
                default:
                    break;
            }
        }

        private void InitializeGlobalInteractorSnapshot()
        {
            globalInteractorSnapshot = context.CreateSnapshot();
            globalInteractorSnapshot.CreateBounds(InteractionBoundsType.None);
            globalInteractorSnapshot.AddWindowId(Literals.GlobalInteractorWindowId);

            var interactor = globalInteractorSnapshot.CreateInteractor(interactorId, Literals.RootId, Literals.GlobalInteractorWindowId);
            interactor.CreateBounds(InteractionBoundsType.None);

            var behavior = interactor.CreateBehavior(InteractionBehaviorType.GazePointData);
            var behaviorParams = new GazePointDataParams() { GazePointDataMode = GazePointDataMode.LightlyFiltered };
            behavior.SetGazePointDataOptions(ref behaviorParams);

            globalInteractorSnapshot.Commit(OnSnapshotCommitted);
        }

        private void OnSnapshotCommitted(InteractionSnapshotResult result)
        {
            Debug.Assert(result.ResultCode != SnapshotResultCode.InvalidSnapshot, result.ErrorMessage);
        }

        private void HandleInteractionEvent(InteractionEvent e)
        {
            foreach (var behavior in e.Behaviors)
                switch (behavior.BehaviorType)
                {
                    case InteractionBehaviorType.GazePointData:
                        OnGazePointData(behavior);
                        break;
                    default: // TODO Investigate which other interaction events are possible in EyeX.
                        break;
                }
        }

        private void OnGazePointData(InteractionBehavior behavior)
        {
            GazePointDataEventParams eventParams;
            if (behavior.TryGetGazePointDataEventParams(out eventParams))
            {
                TrackPoint(new Point((int)eventParams.X, (int)eventParams.Y));
            }
            else
            {
                Console.WriteLine("Failed to interpret gaze data event packet.");
            }
        }
    }
}
