namespace EyePaint
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Tobii.EyeX.Client;
    using Tobii.EyeX.Framework;
    using InteractorId = System.String;
    using System.Timers; //TODO typedef equivalent?

    public partial class EyePaintingForm : Form
    {
        InteractionContext context;
        InteractionSnapshot globalInteractorSnapshot;
        Dictionary<InteractorId, Button> gazeAwareButtons;
        Point gaze;
        bool paint;
        System.Windows.Forms.Timer paintTimer;
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
            context.ConnectionStateChanged += (object s, ConnectionStateChangedEventArgs e) => {
                if (e.State == ConnectionState.Connected)
                {
                    globalInteractorSnapshot.Commit((InteractionSnapshotResult isr) => { });
                }
            };
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

            // Create a paint event handler with a corresponding timer. The timer is the
            // paint refresh interval (similar to rendering FPS).
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
            if (paint) return;
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
            // TODO Check if file with same name already exists, and if so: increment
            // new file with index.
            getPainting().Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }

        // Track gaze point if it's far away enough from the previous point, and add it
        // to the model if the user wants to.
        void trackGaze(Point p, bool keep = true, int keyhole = 25)
        {
            var distance = Math.Sqrt(Math.Pow(gaze.X - p.X, 2) + Math.Pow(gaze.Y - p.Y, 2));
            if (distance < keyhole) return;
            gaze = p;
            if (keep) model.Add(gaze, true); //TODO Add alwaysAdd argument, or remove it completely from the function declaration.      
        }

        private bool confirm(String msg)
        {
            using (ConfirmBox dialog = new ConfirmBox())
            {
                DialogResult result = dialog.Show(this, msg);
                if (result == DialogResult.OK)
                {
                    return true;
                }
                return false;
            }
        }

        // Registers GUI click event handlers and populates the GUI with buttons for choosing color/paint-tools.
        void initializeMenu()
        {
            // Define and register click event handlers for the program menu.
            NewSessionButton.Click += (object s, EventArgs e) => {
                String msg = "Är du säker på att du vill starta om?";
                if (confirm(msg))
                {
                    Application.Restart();
                }
            };
            SavePaintingButton.Click += (object s, EventArgs e) => {
                String msg = "Vill du spara din målning?";
                if (confirm(msg))
                {
                    storePainting();
                }
            };
            ClearPaintingButton.Click += (object s, EventArgs e) => {
                String msg = "Vill du sudda allt och börja om?";
                if (confirm(msg))
                {
                    resetPainting();
                }
            };
            ToolPaneToggleButton.Click += (object s, EventArgs e) => {
                ColorToolsPanel.Visible = PaintToolsPanel.Visible;
                PaintToolsPanel.Visible = !PaintToolsPanel.Visible;
            };
            gazeAwareButtons.Add(NewSessionButton.Parent.Name + NewSessionButton.Name, NewSessionButton);
            gazeAwareButtons.Add(SavePaintingButton.Parent.Name + SavePaintingButton.Name, SavePaintingButton);
            gazeAwareButtons.Add(ClearPaintingButton.Parent.Name + ClearPaintingButton.Name, ClearPaintingButton);
            gazeAwareButtons.Add(ToolPaneToggleButton.Parent.Name + ToolPaneToggleButton.Name, ToolPaneToggleButton);

            // Populate the GUI with available paint tools and color tools.
            Random r = new Random(); //TODO Remove.
            Action<Panel, int, Color, EventHandler> appendButton = (Panel parent, int size, Color color, EventHandler onClick) =>
            {
                Button b = new Button();
                b.Name = "button" + parent.Controls.Count;
                b.Enter += onButtonFocus;
                b.Leave += onButtonBlur;
                b.Click += onClick;
                b.Click += onButtonClicked;
                b.TabStop = false;
                b.Width = b.Height = size;
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 10;
                b.FlatAppearance.BorderColor = Color.White;
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
                        MenuPanel.BackColor = ct.baseColor; //TODO Implement original design with a snippet of the drawing with gaussian blur and 50% white opacity overlay instead.
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
            {
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
                        Action a = () => clickCountdown(gazeAwareButtons[e.InteractorId]);
                        if (hasGaze) BeginInvoke(a);
                    }
                }
            }
        }

        // Focus the button, wait a short period of time and if the button is still focused: click it.
        void clickCountdown(Button b)
        {
            b.Focus();
            System.Timers.Timer waitBeforeClick = new System.Timers.Timer(1000); // One second.

            waitBeforeClick.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                Action a = () => { if (b.Focused) b.PerformClick(); };
                BeginInvoke(a);
                waitBeforeClick.Enabled = false;
            };
            waitBeforeClick.Enabled = true;
        }

        void onButtonFocus(object s, EventArgs e)
        {
            //TODO Animate
            var b = (Button)s;
            b.FlatAppearance.BorderColor = Color.Gray;
        }

        void onButtonBlur(object s, EventArgs e)
        {
            //TODO Animate
            var b = (Button)s;
            b.FlatAppearance.BorderColor = Color.White;
        }

        void onButtonClicked(object s, EventArgs e)
        {
            var b = (Button)s;
            b.FlatAppearance.BorderColor = Color.Black;
        }
    }
}
