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
        InteractionSnapshot globalInteractorSnapshot;
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

            // Initialize eye tracking.
            context = new InteractionContext(false);
            initializeGlobalInteractorSnapshot();
            context.ConnectionStateChanged += (object s, ConnectionStateChangedEventArgs e) => { if (e.State == ConnectionState.Connected) globalInteractorSnapshot.Commit((InteractionSnapshotResult isr) => { }); };
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
            return view.Rasterize(model.GetRenderQueue());
        }

        // Clear view and model.
        void resetPainting()
        {
            model.ResetModel();
            view.Clear();
        }

        // Write rasterized image to a file.
        void storePainting()
        {
            string filename = DateTime.Now.TimeOfDay.TotalSeconds + ".png";
            //TODO Check if file with same name already exists, and if so: increment new file with index.
            getPainting().Save(filename, System.Drawing.Imaging.ImageFormat.Png);
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

        // Initialize a global interactor snapshot that this application responds to the EyeX engine with whenever queried.
        void initializeGlobalInteractorSnapshot()
        {
            globalInteractorSnapshot = context.CreateSnapshot();
            globalInteractorSnapshot.CreateBounds(InteractionBoundsType.None);
            globalInteractorSnapshot.AddWindowId(Literals.GlobalInteractorWindowId);
            var interactor = globalInteractorSnapshot.CreateInteractor(Application.ProductName, Literals.RootId, Literals.GlobalInteractorWindowId);
            interactor.CreateBounds(InteractionBoundsType.None);
            var behavior = interactor.CreateBehavior(InteractionBehaviorType.GazePointData);
            var parameters = new GazePointDataParams() { GazePointDataMode = GazePointDataMode.LightlyFiltered };
            behavior.SetGazePointDataOptions(ref parameters);
        }

        // Create a snapshot for the EyeX engine when queried.
        void handleInteractionQuery(InteractionQuery q)
        {
            double x, y, w, h;
            if (q.Bounds.TryGetRectangularData(out x, out y, out w, out h))
            {
                Action a = () =>
                {
                    // Prepare a new snapshot.
                    var snapshot = context.CreateSnapshot();
                    var queryBounds = new Rectangle((int)x, (int)y, (int)w, (int)h);
                    snapshot.CreateBounds(InteractionBoundsType.Rectangular).SetRectangularData(queryBounds.Left, queryBounds.Top, queryBounds.Width, queryBounds.Height);
                    var windowId = q.Handle.ToString();
                    snapshot.AddWindowId(windowId);

                    // Create a new gaze aware interactor for buttons within query bounds.
                    foreach (Button b in ColorToolsPanel.Controls)
                    {
                        var buttonBounds = b.Parent.RectangleToScreen(b.Bounds);
                        if (buttonBounds.IntersectsWith(queryBounds))
                        {
                            //TODO Get interactors from store when creating snapshots, instead of creating new interactors every time. Might improve performance.
                            var interactor = snapshot.CreateInteractor(
                                b.Name,
                                Literals.RootId,
                                windowId
                            );
                            interactor.SetZ(1); //TODO Set Z-index properly.
                            interactor.CreateBounds(InteractionBoundsType.Rectangular).SetRectangularData(
                                buttonBounds.Left,
                                buttonBounds.Top,
                                buttonBounds.Width,
                                buttonBounds.Height
                            );
                            interactor.CreateBehavior(InteractionBehaviorType.GazeAware);
                        }
                    }

                    // Send the snapshot to the eye tracking server.
                    snapshot.Commit((InteractionSnapshotResult isr) => { });
                };
                BeginInvoke(a);
            }
        }

        // Handle events from the EyeX engine.
        void handleInteractionEvent(InteractionEvent e)
        {
            foreach (var behavior in e.Behaviors)
                if (behavior.BehaviorType == InteractionBehaviorType.GazePointData)
                {
                    GazePointDataEventParams r;
                    if (behavior.TryGetGazePointDataEventParams(out r))
                        trackGaze(new Point((int)r.X, (int)r.Y), paint);
                }
                else if (behavior.BehaviorType == InteractionBehaviorType.GazeAware)
                {
                    Console.WriteLine("HEJ"); // TODO Remove.
                    GazeAwareEventParams r;
                    if (behavior.TryGetGazeAwareEventParams(out r))
                    {
                        bool hasGaze = r.HasGaze != EyeXBoolean.False;
                        if (hasGaze)
                        {
                            Button c = (Button)ColorToolsPanel.Controls[e.InteractorId];
                            c.PerformClick(); //TODO Implement a visual countdown before click.
                        }
                    }
                }

        }
    }
}
