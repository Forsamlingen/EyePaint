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

    public partial class EyePaintingForm : Form
    {
        InteractionContext context;
        InteractionSnapshot globalInteractor;
        Point gaze;
        bool paint;
        Timer paintTimer;
        List<PaintTool> paintTools;
        List<ColorTool> colorTools;
        Model model;
        View view;

        public EyePaintingForm()
        {
            InitializeComponent();

            // Create an interactor context for the eye tracker engine.
            context = new InteractionContext(false);
            initializeGlobalInteractor();
            context.ConnectionStateChanged += (object s, ConnectionStateChangedEventArgs e) => Debug.WriteLine("Eye tracker state: " + e.State.ToString());
            context.RegisterQueryHandlerForCurrentProcess(handleInteractionQuery);
            context.RegisterEventHandler(handleInteractionEvent);
            context.EnableConnection();

            // Register user input event handlers.        
            KeyDown += (object s, KeyEventArgs e) => { if (e.KeyCode == Keys.Space) startPainting(); };
            KeyUp += (object s, KeyEventArgs e) => { if (e.KeyCode == Keys.Space) stopPainting(); };
            MouseMove += (object s, MouseEventArgs e) => trackGaze(new Point(e.X, e.Y), paint);
            MouseDown += (object s, MouseEventArgs e) => startPainting();
            MouseUp += (object s, MouseEventArgs e) => stopPainting();

            // Initialize model and view.
            SettingsFactory sf = new SettingsFactory();
            paintTools = sf.getPaintTools();
            colorTools = sf.getColorTools();
            model = new Model(paintTools[0], colorTools[0]);
            view = new View(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            // Create a paint event handler with a corresponding timer. The timer is the paint refresh interval (similar to rendering FPS).
            Paint += (object s, PaintEventArgs e) => { e.Graphics.DrawImage(getPainting(), 0, 0); };
            paintTimer = new System.Windows.Forms.Timer();
            paintTimer.Interval = 30;
            paintTimer.Enabled = true;
            paintTimer.Tick += (object s, EventArgs e) => { if (paint) model.Grow(); Invalidate(); };

            // Populate GUI with gaze enabled buttons.
            initializeMenu();
        }

        // Start painting.
        void startPainting()
        {
            paint = true;
            trackGaze(gaze, paint);
        }

        // Stop painting.
        void stopPainting()
        {
            paint = false;
        }

        // Rasterize the model and return an image object.
        Image getPainting()
        {
            Image image = view.Rasterize(model.GetRenderQueue());
            // model.ClearRenderQueue(); TODO Is this dead code? If so: remove!
            return image;
        }

        // Clear view, model and canvas.
        void resetPainting()
        {
            model.ResetModel();
            view.Clear();
            Invalidate();
        }

        // Write rasterized image to a file.
        void storePainting()
        {
            Image image = getPainting();
            image.Save("painting.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        // Track gaze point, and add it to the model if the user wants to.
        void trackGaze(Point p, bool add = false)
        {
            gaze = p;
            if (add) model.Add(gaze, true); //TODO Add alwaysAdd argument, or remove it completely from the function declaration.      
        }

        // Registers GUI click event handlers and populates the GUI with buttons for choosing color/paint-tools.
        void initializeMenu()
        {
            // Register click event handlers for the program menu.
            NewSessionButton.Click += (object s, EventArgs e) => { Application.Restart(); }; //TODO Show confirmation dialog before committing to exiting the program.
            SavePaintingButton.Click += (object s, EventArgs e) => { storePainting(); }; //TODO Show confirmation dialog before commiting to store the painting.
            ClearPaintingButton.Click += (object s, EventArgs e) => { resetPainting(); }; //TODO Show confirmation dialog before clearing the drawing.
            ToolPaneToggleButton.Click += (object s, EventArgs e) =>
            {
                //ToolPaneToggleButton.BackgroundImage = ... //TODO Load button icon from file directory.
                ColorToolsPanel.Visible = PaintToolsPanel.Visible;
                PaintToolsPanel.Visible = !PaintToolsPanel.Visible;
            };

            // Populate the GUI with available paint tools and color tools.
            Random r = new Random(); //TODO Remove.
            Action<Panel, int, Color, EventHandler> appendButton = (Panel parent, int size, Color color, EventHandler click) =>
            {
                Button button = new Button();
                button.Click += click;
                button.Width = button.Height = size;
                button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Margin = new Padding(0);
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                button.BackColor = color; //TODO Load button icon from file directory instead.
                parent.Controls.Add(button);
            };

            foreach (var pt in paintTools)
            {
                appendButton(
                    PaintToolsPanel,
                    ProgramControlPanel.Controls[0].Height,
                    Color.FromArgb(r.Next(255), r.Next(255), r.Next(255)),
                    (object s, EventArgs e) => { model.ChangePaintTool(pt); }
                );
            }

            foreach (var ct in colorTools)
            {
                appendButton(
                    ColorToolsPanel,
                    ProgramControlPanel.Controls[0].Height,
                    ct.baseColor,
                    (object s, EventArgs e) =>
                    {
                        model.ChangeColorTool(ct);
                        Menu.BackColor = ct.baseColor; //TODO Implement original design with a snippet of the drawing with gaussian blur and 50% white opacity overlay instead.
                    }
                );
            }
        }

        // Create a global interactor for listening on the gaze point data stream.
        void initializeGlobalInteractor()
        {
            globalInteractor = context.CreateSnapshot();
            globalInteractor.CreateBounds(InteractionBoundsType.None);
            globalInteractor.AddWindowId(Literals.GlobalInteractorWindowId);
            globalInteractor.Commit((InteractionSnapshotResult result) => Debug.Assert(result.ResultCode != SnapshotResultCode.InvalidSnapshot, result.ErrorMessage));

            var interactor = globalInteractor.CreateInteractor(
                Application.ProductName + System.Threading.Thread.CurrentThread.ManagedThreadId,
                Literals.RootId,
                Literals.GlobalInteractorWindowId
            );
            interactor.CreateBounds(InteractionBoundsType.None);

            var behavior = interactor.CreateBehavior(InteractionBehaviorType.GazePointData);
            var behaviorParams = new GazePointDataParams() { GazePointDataMode = GazePointDataMode.LightlyFiltered };
            behavior.SetGazePointDataOptions(ref behaviorParams);
        }

        // TODO
        void handleInteractionQuery(InteractionQuery q)
        {
            Action<Rectangle> a = (Rectangle queryBounds) =>
            {
                var windowId = Handle.ToString();
                var snapshot = context.CreateSnapshot();
                snapshot.AddWindowId(windowId);
                var bounds = snapshot.CreateBounds(InteractionBoundsType.Rectangular);
                bounds.SetRectangularData(queryBounds.Left, queryBounds.Top, queryBounds.Width, queryBounds.Height);
                var queryBoundsRect = new Rectangle(queryBounds.Left, queryBounds.Top, queryBounds.Width, queryBounds.Height);
                //foreach (Control button in PaintToolsPanel.Controls) CreateGazeAwareInteractor(button, Literals.RootId, windowId, 1, snapshot, queryBoundsRect); TODO Generate interactor for each menu button.
                snapshot.Commit((InteractionSnapshotResult result) => Debug.Assert(result.ResultCode != SnapshotResultCode.InvalidSnapshot, result.ErrorMessage));
            };

            double x, y, w, h;
            if (q.Bounds.TryGetRectangularData(out x, out y, out w, out h))
            {
                var r = new Rectangle((int)x, (int)y, (int)w, (int)h);
                BeginInvoke(a, r);
            }
        }

        // TODO
        void handleInteractionEvent(InteractionEvent e)
        {
            foreach (var behavior in e.Behaviors)
                switch (behavior.BehaviorType)
                {
                    case InteractionBehaviorType.GazePointData:
                        GazePointDataEventParams eventParams;
                        if (behavior.TryGetGazePointDataEventParams(out eventParams))
                            trackGaze(new Point((int)eventParams.X, (int)eventParams.Y), paint);
                        break;
                    case InteractionBehaviorType.GazeAware:
                        GazeAwareEventParams gazeAwareEventParams;
                        if (behavior.TryGetGazeAwareEventParams(out gazeAwareEventParams))
                        {
                            /* TODO Handle interactors for each menu button.
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

        // TODO
        void addInteractor(Control control, string parentId, string windowId, double z, InteractionSnapshot snapshot, Rectangle queryBoundsRect)
        {
            var controlRect = control.Parent.RectangleToScreen(control.Bounds);

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
