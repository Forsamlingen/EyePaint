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

        // User input.
        internal enum InputMode { MOUSE_AND_KEYBOARD, EYE_TRACKER };

        // Painting.
        private Timer paint;
        private readonly Dictionary<int, PaintTool> paintTools; //All availble PaintTools maped agains there ID
        private readonly Dictionary<int, ColorTool> colorTools; //All availble ColorTools maped agains there ID
        private PaintTool currentPaintTool;
        private ColorTool currentColorTool;
        private Model model;
        private View view;

        public EyePaintingForm()
        {
            InitializeComponent();

            // Create an interactor context for the eye tracker engine.
            context = new InteractionContext(false);
            InitializeGlobalInteractorSnapshot();
            context.ConnectionStateChanged += OnConnectionStateChanged;
            context.RegisterQueryHandlerForCurrentProcess(HandleQuery);
            context.RegisterEventHandler(HandleInteractionEvent);
            context.EnableConnection();

            // Choose user input mode and register event handlers, etc.
            useInputMode(InputMode.MOUSE_AND_KEYBOARD);

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
            currentPaintTool = paintTools[randPaintToolId];
            currentColorTool = colorTools[randColorToolId];
            model = new Model(currentPaintTool, currentColorTool);
            view = new View(width, height);

            // Create a paint event handler with a corresponding timer. The timer is the paint refresh interval (similar to rendering FPS).
            Paint += (object s, PaintEventArgs e) => { Image image = getPainting(); if (image != null) e.Graphics.DrawImageUnscaled(image, new Point(0, 0)); };
            paint = new System.Windows.Forms.Timer();
            paint.Interval = 33; //TODO Make into property.
            paint.Enabled = false;
            paint.Tick += (object s, EventArgs e) => { model.Grow(); Invalidate(); };

            // Register setup panel button click handlers.
            OpenControlPanelButton.Click += (object s, EventArgs e) => { Process.Start("C:\\Program Files\\Tobii\\EyeTracking\\Tobii.EyeTracking.ControlPanel.exe"); }; //TODO Don't assume default install location.
            EnableEyeTrackerButton.Click += (object s, EventArgs e) => { SetupPanel.Visible = SetupPanel.Enabled = false; };
            EnableMouseButton.Click += (object s, EventArgs e) => { useInputMode(InputMode.MOUSE_AND_KEYBOARD); SetupPanel.Visible = SetupPanel.Enabled = false; };
            CloseSetupPanelButton.Click += (object s, EventArgs e) => { useInputMode(InputMode.EYE_TRACKER); SetupPanel.Visible = SetupPanel.Enabled = false; };
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

        // Starts the timer, enabling tick events.
        private void startPainting()
        {
            if (paint.Enabled) return;
            else paint.Enabled = true;
        }

        // Stops the timer, disabling tick events.
        private void stopPainting()
        {
            paint.Enabled = false;
        }

        // Rasterizes the model and returns an image object.
        private Image getPainting()
        {
            Image image = view.Rasterize(model.GetRenderQueue());
            // model.ClearRenderQueue();
            return image;
        }

        // Clears the canvas.
        private void resetPainting()
        {
            model.ResetModel();
            view.Clear();
            Invalidate();
        }

        // Writes rasterized image to a file.
        private void storePainting()
        {
            Image image = getPainting();
            image.Save("painting.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void setupPaintToolsToolbox()
        {
            //TODO
            throw new NotImplementedException();
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

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            TrackPoint(new Point(e.X, e.Y));
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
                case Keys.R: //TODO Take away after test
                    int newRandomToolID = getRandomPaintToolID();
                    changePaintTool(newRandomToolID);
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

        // Adds a new point to the model.
        private void TrackPoint(Point p)
        {
            if (paint.Enabled)
            {
                model.Add(p, true); //TODO alwaysAdd
            }
            else
            {
                //TODO Animate opening and closing?
                // if (p.Y < 50 && p.X < 50) PaintToolsPanel.Visible = true;
                //else PaintToolsPanel.Visible = false;
            }
        }

        //TODO Maybe change after agree on type of id for PaintTools
        private void changePaintTool(int newPaintToolID)
        {
            //Check if the new paint tool is the same as the present do nothing
            if (currentPaintTool.id == newPaintToolID) return;

            //Apply change to model
            model.ChangePaintTool(paintTools[newPaintToolID]);

            //Set present PaintTool to the new one
            currentPaintTool = paintTools[newPaintToolID];

        }

        private void changeColorTool(int newColorToolID)
        {
            model.ChangeColorTool(colorTools[newColorToolID]);
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

            //foreach (Control button in PaintToolsPanel.Controls) CreateGazeAwareInteractor(button, Literals.RootId, windowId, 1, snapshot, queryBoundsRect); TODO

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
                            TrackPoint(new Point((int)eventParams.X, (int)eventParams.Y)); //TODO Invoke required?
                        break;
                    case InteractionBehaviorType.GazeAware:
                        GazeAwareEventParams gazeAwareEventParams;
                        if (behavior.TryGetGazeAwareEventParams(out gazeAwareEventParams))
                        {
                            /* TODO
                            Button c = (Button)PaintToolsPanel.Controls[e.InteractorId]; // TODO Out of bounds?
                            bool hasGaze = gazeAwareEventParams.HasGaze != EyeXBoolean.False; //TODO Remove line?
                            Action a = () => c.PerformClick();
                            BeginInvoke(a);
                             */
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
