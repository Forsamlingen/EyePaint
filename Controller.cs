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

    partial class EyePaintingForm : Form, IDisposable
    {
        // Eye tracking engine
        InteractionContext context;
        readonly string interactorId = "EyePaint" + System.Threading.Thread.CurrentThread.ManagedThreadId;
        InteractionSnapshot globalInteractorSnapshot;

        // User input alternatives
        enum InputMode { MOUSE_AND_KEYBOARD, EYE_TRACKER };

        // Painting
        BaseFactory factory;
        BaseRasterizer rasterizer;
        Timer paint;
        PaintTool currentTool;
        enum ModelType { TREE, CLOUD }; //TODO Move logic into Model and View.
        const ModelType modelType = ModelType.TREE; //TODO Make into property (but make sure the property always resolves into some ModelType to avoid the program crashing).

        public EyePaintingForm()
        {
            InitializeComponent();

            // Setup paint tools and toolbox panel.
            setupPaintTools();

            // Choose user input mode and register event handlers, etc.
            useInputMode(InputMode.MOUSE_AND_KEYBOARD);

            // Create an interactor context for the eye tracker engine.
            context = new InteractionContext(false);
            InitializeGlobalInteractorSnapshot();
            context.RegisterQueryHandlerForCurrentProcess(HandleQuery);
            context.ConnectionStateChanged += OnConnectionStateChanged;
            context.RegisterEventHandler(HandleInteractionEvent);

            // Initialize model and view classes.
            switch (modelType)
            {
                case ModelType.CLOUD:
                    factory = new CloudFactory();
                    rasterizer = new CloudRasterizer(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    break;
                case ModelType.TREE:
                    factory = new TreeFactory();
                    rasterizer = new TreeRasterizer(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    break;
                default:
                    goto case ModelType.TREE;
            }

            // Create a paint event handler with a corresponding timer. The timer is the paint refresh interval (similar to rendering FPS).
            Paint += (object s, PaintEventArgs e) => { Image image = getPainting(); if (image != null) e.Graphics.DrawImageUnscaled(image, new Point(0, 0)); };
            paint = new System.Windows.Forms.Timer();
            paint.Enabled = false;
            paint.Interval = 33; //TODO Make into property.
            paint.Tick += (object s, EventArgs e) => { factory.Grow(); Invalidate(); };

            // Register setup panel button click handlers.
            OpenControlPanelButton.Click += (object s, EventArgs e) => { Process.Start("C:\\Program Files\\Tobii\\EyeTracking\\Tobii.EyeTracking.ControlPanel.exe"); }; //TODO Don't assume default install location.
            EnableEyeTrackerButton.Click += (object s, EventArgs e) => { SetupPanel.Visible = SetupPanel.Enabled = false; };
            EnableMouseButton.Click += (object s, EventArgs e) => { useInputMode(InputMode.MOUSE_AND_KEYBOARD); SetupPanel.Visible = SetupPanel.Enabled = false; };
            CloseSetupPanelButton.Click += (object s, EventArgs e) => { useInputMode(InputMode.EYE_TRACKER); SetupPanel.Visible = SetupPanel.Enabled = false; };
        }

        // Starts the paint timer.
        void startPainting()
        {
            if (paint.Enabled) return;
            else paint.Enabled = true;
        }

        // Stops the timer.
        void stopPainting()
        {
            paint.Enabled = false;
        }

        // Rasterizes the model and returns an image object.
        Image getPainting()
        {
            Image image = rasterizer.Rasterize(factory);
            return image;
        }

        // Clears the canvas.
        void resetPainting()
        {
            //TODO Clear model as well.
            rasterizer.ClearImage();
            Invalidate();
        }

        // Writes rasterized image to a file
        void storePainting()
        {
            Image image = getPainting();
            image.Save("painting.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        // Populates the paint tools toolbox with user selectable paint tools.
        void setupPaintTools()
        {
            //TODO Get all paint tools from a data store instead of automatically generating tools below.
            List<PaintTool> paintTools = new List<PaintTool>();
            Random rng = new Random();
            for (int i = 0; i < 20; ++i) paintTools.Add(new PaintTool("Test paint tool" + i, null, Color.FromArgb(255, rng.Next(255), rng.Next(255), rng.Next(255))));

            // Create buttons in paint tools toolbox.
            foreach (var paintTool in paintTools)
            {
                // Create a button for the paint tool.
                Button button = new Button();
                button.Click += (object s, EventArgs e) => { currentTool = paintTool; };
                int rows = 2; // TODO Make into a property.
                button.Height = button.Width = Screen.PrimaryScreen.Bounds.Width / (paintTools.Count / rows + 1);
                button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                button.Margin = new Padding(0);

                // Create sample drawing for the button thumbnail, if a paint tool icon doesn't already exist.
                if (paintTool.icon == null) {
                    //TODO Switch factory based on user choice.
                    BaseFactory sampleFactory = new TreeFactory();
                    BaseRasterizer sampleRasterizer = new TreeRasterizer(button.Width, button.Height);
                    sampleFactory.Add(new Point(button.Width / 2, button.Height / 2), paintTool);
                    for (int i = 0; i < 5; ++i) sampleFactory.Grow();
                    paintTool.icon = sampleRasterizer.Rasterize(sampleFactory);
                }
                button.BackgroundImage = paintTool.icon;

                // Add button to toolbox.
                PaintToolsPanel.Controls.Add(button);
            }

            // TODO Append "Get random tool" button.
        }

        // Store a new point in the model, if painting is enabled.
        void trackPoint(Point p)
        {
            if (paint.Enabled && currentTool != null)
            {
                factory.Add(p, currentTool);
            }
            else
            {
                //TODO Animate opening and closing?
                if (p.Y < 50 && p.X < 50) PaintToolsPanel.Visible = true;
                else PaintToolsPanel.Visible = false;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            trackPoint(new Point(e.X, e.Y));
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
                    currentTool.pen.Color = Color.Crimson;
                    break;
                case Keys.G:
                    currentTool.pen.Color = Color.ForestGreen;
                    break;
                case Keys.B:
                    currentTool.pen.Color = Color.CornflowerBlue;
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

        private void HandleQueryOnUiThread(Rectangle queryBounds)
        {
            var windowId = Handle.ToString();

            var snapshot = context.CreateSnapshot();
            snapshot.AddWindowId(windowId);

            var bounds = snapshot.CreateBounds(InteractionBoundsType.Rectangular);
            bounds.SetRectangularData(queryBounds.Left, queryBounds.Top, queryBounds.Width, queryBounds.Height);

            var queryBoundsRect = new Rectangle(queryBounds.Left, queryBounds.Top, queryBounds.Width, queryBounds.Height);

            foreach (Control button in PaintToolsPanel.Controls) CreateGazeAwareInteractor(button, Literals.RootId, windowId, 1, snapshot, queryBoundsRect);

            snapshot.Commit(OnSnapshotCommitted);
        }

        private void OnSnapshotCommitted(InteractionSnapshotResult result)
        {
            Debug.Assert(result.ResultCode != SnapshotResultCode.InvalidSnapshot, result.ErrorMessage);
        }

        private void HandleQuery(InteractionQuery query)
        {
            var queryBounds = query.Bounds;
            double x, y, w, h;
            if (queryBounds.TryGetRectangularData(out x, out y, out w, out h))
            {
                BeginInvoke(new Action<Rectangle>(HandleQueryOnUiThread), new Rectangle((int)x, (int)y, (int)w, (int)h));
            }
        }

        private void HandleInteractionEvent(InteractionEvent e)
        {
            foreach (var behavior in e.Behaviors)
                switch (behavior.BehaviorType)
                {
                    case InteractionBehaviorType.GazePointData:
                        GazePointDataEventParams eventParams;
                        if (behavior.TryGetGazePointDataEventParams(out eventParams))
                            trackPoint(new Point((int)eventParams.X, (int)eventParams.Y)); //TODO Invoke required?
                        break;
                    case InteractionBehaviorType.GazeAware:
                        GazeAwareEventParams gazeAwareEventParams;
                        if (behavior.TryGetGazeAwareEventParams(out gazeAwareEventParams))
                        {
                            Button c = (Button)PaintToolsPanel.Controls[e.InteractorId]; // TODO Out of bounds?
                            bool hasGaze = gazeAwareEventParams.HasGaze != EyeXBoolean.False; //TODO Remove line?
                            Action a = () => c.PerformClick();
                            BeginInvoke(a);
                        }
                        break;
                    default:
                        break;
                }
        }

        private void CreateGazeAwareInteractor(Control control, string parentId, string windowId, double z, InteractionSnapshot snapshot, Rectangle queryBoundsRect)
        {
            var controlRect = control.Bounds;
            controlRect = control.Parent.RectangleToScreen(controlRect);

            if (controlRect.IntersectsWith(queryBoundsRect))
            {
                var interactor = snapshot.CreateInteractor(control.Name, parentId, windowId);
                interactor.SetZ(z);

                var bounds = interactor.CreateBounds(InteractionBoundsType.Rectangular);
                bounds.SetRectangularData(controlRect.Left, controlRect.Top, controlRect.Width, controlRect.Height);

                interactor.CreateBehavior(InteractionBehaviorType.GazeAware);
            }
        }
    }
}
