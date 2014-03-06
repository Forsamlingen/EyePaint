namespace EyePaint
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;
    using InteractorId = System.String; //TODO typedef equivalent?

    public partial class EyePaintingForm : Form
    {
        InteractionContext context;
        InteractionSnapshot globalInteractorSnapshot;
        Dictionary<InteractorId, Button> gazeAwareButtons;
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
            gazeAwareButtons = new Dictionary<InteractorId, Button>();
            initializeGlobalInteractorSnapshot();
            context.ConnectionStateChanged += (object s, ConnectionStateChangedEventArgs e) => { if (e.State == ConnectionState.Connected) globalInteractorSnapshot.Commit((InteractionSnapshotResult isr) => { }); };
            context.RegisterQueryHandlerForCurrentProcess(handleInteractionQuery);
            context.RegisterEventHandler(handleInteractionEvent);
            context.EnableConnection();

            // Register user input event handlers.
            KeyDown += (object s, KeyEventArgs e) => { if (e.KeyCode == Keys.ControlKey) startPainting(); };
            KeyUp += (object s, KeyEventArgs e) => { if (e.KeyCode == Keys.ControlKey) stopPainting(); };
            MouseMove += (object s, MouseEventArgs e) => trackGaze(new Point(e.X, e.Y), paint, 0);
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
            paintTimer.Interval = 1;
            paintTimer.Enabled = true;
            paintTimer.Tick += (object s, EventArgs e) => { if (paint) model.Grow(); Invalidate(); };

            // Populate GUI with gaze enabled buttons.
            initializeMenu();
        }

        // Start painting.
        void startPainting()
        {
            paint = true;
            trackGaze(gaze, paint, 0);
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

        // Track gaze point if it's far away enough from the previous point, and add it to the model if the user wants to.
        void trackGaze(Point p, bool keep = true, int keyhole = 25)
        {
            var distance = Math.Sqrt(Math.Pow(gaze.X - p.X, 2) + Math.Pow(gaze.Y - p.Y, 2));
            if (distance < keyhole) return;
            gaze = p;
            if (keep) model.Add(gaze, true); //TODO Add alwaysAdd argument, or remove it completely from the function declaration.      
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
                Button b = new Button();
                b.Name = "button" + parent.Controls.Count;
                b.TabStop = false;
                b.Click += click;
                b.Width = b.Height = size;
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.Margin = new Padding(0);
                b.BackColor = color; //TODO Load button icon from file directory instead. //string directory = AppDomain.CurrentDomain.BaseDirectory;
                parent.Controls.Add(b);
                gazeAwareButtons.Add(b.Parent.Name + b.Name, b);
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
                var queryBounds = new Rectangle((int)x, (int)y, (int)w, (int)h);
                Action a = () =>
                {
                    // Prepare a new snapshot.
                    InteractionSnapshot s = context.CreateSnapshotWithQueryBounds(q);
                    s.AddWindowId(Handle.ToString());

                    // Create a new gaze aware interactor for buttons within the query bounds.
                    foreach (var e in gazeAwareButtons)
                    {
                        Button b = e.Value;
                        if (!b.Visible) continue;
                        InteractorId id = e.Key;
                        var buttonBounds = b.Parent.RectangleToScreen(b.Bounds);
                        if (buttonBounds.IntersectsWith(queryBounds))
                        {
                            Interactor i = s.CreateInteractor(id, Literals.RootId, Handle.ToString());
                            i.CreateBounds(InteractionBoundsType.Rectangular).SetRectangularData(
                                buttonBounds.Left,
                                buttonBounds.Top,
                                buttonBounds.Width,
                                buttonBounds.Height
                            );
                            i.CreateBehavior(InteractionBehaviorType.GazeAware);
                        }
                    }

                    // Send the snapshot to the eye tracking server.
                    s.Commit((InteractionSnapshotResult isr) => { });
                };

                BeginInvoke(a); // Run on UI thread.
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
                        trackGaze(new Point((int)r.X, (int)r.Y), paint, 200); //TODO Set keyhole size dynamically based on how bad the calibration is.
                }
                else if (behavior.BehaviorType == InteractionBehaviorType.GazeAware)
                {
                    GazeAwareEventParams r;
                    if (behavior.TryGetGazeAwareEventParams(out r))
                    {
                        bool hasGaze = r.HasGaze != EyeXBoolean.False;
                        Action a = () =>
                        {
                            gazeAwareButtons[e.InteractorId].BackColor = Color.Red; //TODO Implement visual aid.
                            gazeAwareButtons[e.InteractorId].PerformClick(); //TODO Implement a visual countdown before click.
                        };
                        if (hasGaze) BeginInvoke(a);
                    }
                }
        }
    }
}
