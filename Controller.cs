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

    public partial class EyePaintingForm : GazeAwareForm
    {
        Point gaze;
        bool paint;
        System.Windows.Forms.Timer paintTimer;
        System.Windows.Forms.Timer inactivityTimer;
        List<PaintTool> paintTools;
        List<ColorTool> colorTools;
        Model model;
        View view;

        public EyePaintingForm() : base()
        {
            InitializeComponent();

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

            // Set timer for inactivity
            inactivityTimer = new System.Windows.Forms.Timer();
            inactivityTimer.Interval = 60000;
            inactivityTimer.Enabled = true;
            inactivityTimer.Tick += (object s, EventArgs e) =>
            {
                string msg = "Vill du starta om och börja på nytt?";
                if (confirm(msg))
                {
                    Application.Restart();
                }
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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
            ColorButton.Click += (object s, EventArgs e) => {
                ColorButton.Visible = false;
                ToolButton.Visible = true;
                ColorToolsPanel.Visible = PaintToolsPanel.Visible;
                PaintToolsPanel.Visible = !PaintToolsPanel.Visible;
            };
            ToolButton.Click += (object s, EventArgs e) =>
            {
                ColorButton.Visible = true;
                ToolButton.Visible = false;
                ColorToolsPanel.Visible = PaintToolsPanel.Visible;
                PaintToolsPanel.Visible = !PaintToolsPanel.Visible;
            };
            gazeAwareButtons.Add(NewSessionButton.Parent.Name + NewSessionButton.Name, NewSessionButton);
            gazeAwareButtons.Add(SavePaintingButton.Parent.Name + SavePaintingButton.Name, SavePaintingButton);
            gazeAwareButtons.Add(ClearPaintingButton.Parent.Name + ClearPaintingButton.Name, ClearPaintingButton);
            gazeAwareButtons.Add(ColorButton.Parent.Name + ColorButton.Name, ColorButton);
            gazeAwareButtons.Add(ToolButton.Parent.Name + ToolButton.Name, ToolButton);

            // Populate the GUI with available paint tools and color tools.
            Random r = new Random(); //TODO Remove.
            Action<Panel, Button, EventHandler> appendButton = (Panel parent, Button b, EventHandler onClick) =>
            {
                b.Name = "button" + parent.Controls.Count;
                b.Enter += onButtonFocus;
                b.Leave += onButtonBlur;
                b.Click += onClick;
                b.Click += onButtonClicked;
                b.TabStop = false;
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 10;
                b.FlatAppearance.BorderColor = Color.White;
                b.Margin = new Padding(0);
                parent.Controls.Add(b);
                gazeAwareButtons.Add(b.Parent.Name + b.Name, b);
            };

            Action<PaintTool> appendTool = (PaintTool pt) =>
            {
                Button b = new Button();
                b.Width = b.Height = ProgramControlPanel.Controls[0].Height;

                // Attempt to load icon file
                try
                {
                    Image icon = Image.FromFile(@"Resources/" + pt.iconImage, true);
                    b.Image = icon; //string directory = AppDomain.CurrentDomain.BaseDirectory;
                }
                catch (System.IO.FileNotFoundException)
                {
                }

                appendButton(
                    PaintToolsPanel,
                    b,
                    (object s, EventArgs e) =>
                    {
                        model.ChangePaintTool(pt);
                    }
                );
            };

            Action<ColorTool> appendColor = (ColorTool ct) =>
            {
                Button b = new Button();
                b.Width = b.Height = ProgramControlPanel.Controls[0].Height;
                b.BackColor = ct.baseColor;

                appendButton(
                    ColorToolsPanel,
                    b,
                    (object s, EventArgs e) =>
                    {
                        model.ChangeColorTool(ct);
                        // TODO Implement original design with a snippet of the drawing
                        // with gaussian blur and 50% white opacity overlay instead.
                    }
                );
            };

            foreach (var pt in paintTools)
            {
                appendTool(pt);
            }

            foreach (var ct in colorTools)
            {
                appendColor(ct);
            }
        }

        // Handle events from the EyeX engine.
        override protected void handleInteractionEvent(InteractionEvent e)
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
    }
}
