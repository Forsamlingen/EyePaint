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
        // Eye tracking engine.
        private readonly string interactorId = "EyePaint" + System.Threading.Thread.CurrentThread.ManagedThreadId; // TODO Make into property.
        private InteractionContext context;
        private InteractionSnapshot globalInteractorSnapshot;

        // User input alternatives.
        internal enum InputMode { MOUSE_AND_KEYBOARD, EYE_TRACKER };

        // Painting.
        private BaseFactory factory;
        private BaseRasterizer rasterizer;
        private Timer paint;

        // Painting toolbox.
        private List<PaintTool> paintTools;

        //TODO Move into its own file.
        internal struct PaintTool
        {
            public string name;
            public Color color;

            public PaintTool(string name, Color color)
            {
                this.name = name;
                this.color = color;
            }
        }
        private PaintTool currentTool;

        internal enum ModelType { TREE, CLOUD }; //TODO Move logic into Model and View.
        private const ModelType modelType = ModelType.TREE; //TODO Make into property (but make sure the property always resolves into some ModelType to avoid the program crashing).

        public EyePaintingForm()
        {
            InitializeComponent();

            paintTools = new List<PaintTool>();
            paintTools.Add(new PaintTool("Funny Test Brush", Color.Crimson));
            paintTools.Add(new PaintTool("Hilarious Test Pencil", Color.Blue));
            //TODO Get all paint tools. 
            //TODO Create buttons in paint tools toolbox.
            //TODO Gaze enable toolbox.
            foreach (var paintTool in paintTools)
            {
                Button button = new Button();
                button.Name = paintTool.name;
                button.Text = paintTool.name;
                button.Tag = paintTool.name;
                button.Click += (object s, EventArgs e) => { currentTool = paintTool; };
                PaintToolsPanel.Controls.Add(button);
            }

            // Program resolution.
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            // Choose user input mode and register event handlers, etc.
            useInputMode(InputMode.MOUSE_AND_KEYBOARD);

            // Create a global interactor context for the gaze point data stream.
            context = new InteractionContext(false);
            InitializeGlobalInteractorSnapshot();
            context.ConnectionStateChanged += OnConnectionStateChanged;
            context.RegisterEventHandler(HandleInteractionEvent);

            // Initialize model and view classes.
            switch (modelType) //TODO Use Activator instead.
            {
                case ModelType.CLOUD:
                    factory = new CloudFactory();
                    rasterizer = new CloudRasterizer(width, height);
                    break;
                case ModelType.TREE:
                    factory = new TreeFactory();
                    rasterizer = new TreeRasterizer(width, height);
                    break;
                default:
                    goto case ModelType.TREE;
            }

            // Create a paint event handler with a corresponding timer. The timer is the paint refresh interval (similar to rendering FPS).
            Paint += OnPaint;
            paint = new System.Windows.Forms.Timer();
            paint.Enabled = false;
            paint.Interval = 33; //TODO Make into property.
            paint.Tick += (object s, EventArgs e) => { factory.Grow(); Invalidate(); };
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Image image = getPainting();
            if (image != null) e.Graphics.DrawImageUnscaled(image, new Point(0, 0));
        }

        // Starts the paint timer.
        private void startPainting()
        {
            if (paint.Enabled) return;
            else paint.Enabled = true;
        }

        // Stops the timer.
        private void stopPainting()
        {
            paint.Enabled = false;
        }

        // Rasterizes the model and returns an image object.
        private Image getPainting()
        {
            Image image = rasterizer.Rasterize(factory);
            return image;
        }

        // Clears the canvas.
        private void resetPainting()
        {
            rasterizer.Undo();
            Invalidate();
        }

        // Writes rasterized image to a file
        private void storePainting()
        {
            Image image = getPainting();
            image.Save("painting.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void openToolBox()
        {
            PaintToolsPanel.BringToFront(); //TODO Animate.
        }

        private void closeToolBox()
        {
            PaintToolsPanel.SendToBack(); //TODO Animate.
        }

        private void getPaintTools()
        {
            // TODO Load all paint tools into the toolbox.
        }

        private void setPaintTool()
        {
            //TODO
            currentTool = paintTools[0];
        }

        private void setRandomPaintTool()
        {
            currentTool = paintTools[new Random().Next(paintTools.Count - 1)];
        }

        // Store a new point in the model, if painting is enabled.
        private void TrackPoint(Point p)
        {
            if (p.Y < 50) {
                openToolBox();                
                return;
            }
            else
                closeToolBox();

            if (paint.Enabled) factory.Add(p, currentTool.color, false);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            TrackPoint(new Point(e.X, e.Y));
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    startPainting();
                    break;
                default:
                    break;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    stopPainting();
                    break;
                default:
                    break;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    startPainting();
                    break;
                case Keys.Back:
                    resetPainting();
                    break;
                case Keys.R:
                    currentTool.color = Color.Crimson;
                    break;
                case Keys.G:
                    currentTool.color = Color.ForestGreen;
                    break;
                case Keys.B:
                    currentTool.color = Color.CornflowerBlue;
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
                    stopPainting();
                    break;
                default:
                    break;
            }
        }

        private void OpenControlPanelClick(object sender, EventArgs e)
        {
            Process.Start("C:\\Program Files\\Tobii\\EyeTracking\\Tobii.EyeTracking.ControlPanel.exe"); //TODO Don't assume default install location.
        }

        private void CloseSetupPanelClick(object sender, EventArgs e)
        {
            SetupPanel.Visible = SetupPanel.Enabled = false;
        }

        private void UseEyeTrackerClick(object sender, EventArgs e)
        {
            useInputMode(InputMode.EYE_TRACKER);
            SetupPanel.Visible = SetupPanel.Enabled = false;
        }

        private void UseMouseClick(object sender, EventArgs e)
        {
            useInputMode(InputMode.MOUSE_AND_KEYBOARD);
            SetupPanel.Visible = SetupPanel.Enabled = false;
        }

        private void OnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            switch (e.State)
            {
                case ConnectionState.Connected:
                    globalInteractorSnapshot.Commit(OnSnapshotCommitted);

                    Action a1 = () => SetupMessage.Text = "Status: " + e.State.ToString();
                    if (SetupMessage.InvokeRequired)
                        SetupMessage.Invoke(a1);
                    else
                        a1.Invoke();

                    Action a2 = () => SetupPanel.Visible = SetupPanel.Enabled = false;
                    if (SetupPanel.InvokeRequired)
                        SetupPanel.Invoke(a2);
                    else
                        a2.Invoke();

                    break;
                case ConnectionState.Disconnected:
                    break;
                case ConnectionState.ServerVersionTooHigh:
                    break;
                case ConnectionState.ServerVersionTooLow:
                    break;
                case ConnectionState.TryingToConnect:
                    Action a = () => SetupPanel.Visible = SetupPanel.Enabled = true;
                    if (SetupPanel.InvokeRequired)
                        SetupPanel.Invoke(a);
                    else
                        a.Invoke();
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

        private void useInputMode(InputMode inputMode)
        {
            // Deregister all input event handlers.
            if (context != null) context.DisableConnection();
            KeyDown -= OnKeyDown;
            KeyUp -= OnKeyUp;
            MouseMove -= OnMouseMove;
            MouseDown -= OnMouseDown;
            MouseUp -= OnMouseUp;

            switch (inputMode)
            {
                case InputMode.EYE_TRACKER: // Register event handlers for the eye tracker.
                    context.EnableConnection();
                    goto case InputMode.MOUSE_AND_KEYBOARD; // TODO Buy USB buttons and use them instead for the keyboard.
                case InputMode.MOUSE_AND_KEYBOARD: // Register event handlers for mouse and keyboard.
                    KeyDown += OnKeyDown;
                    KeyUp += OnKeyUp;
                    MouseMove += OnMouseMove;
                    MouseDown += OnMouseDown;
                    MouseUp += OnMouseUp;
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
