using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tobii.EyeX.Client;
using Tobii.EyeX.Framework;
using InteractorId = System.String;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for Paint.xaml
    /// </summary>
    public partial class Paint : Window
    {
        private const string InteractorId = "EyePaint";
        private InteractionSystem system;
        private InteractionContext context;
        private InteractionSnapshot globalInteractorSnapshot;

        Point gaze;
        bool paint = false;
        bool menuActive;
        bool isKeyDown = false; //TODO see if other soultion is possible?
        Dictionary<InteractorId, Button> gazeAwareButtons;

        //Painting
        int paintingWidth;
        int paintingHeight;
        RenderTargetBitmap painting;

        //Tools 
        List<PaintTool> paintTools;
        List<ColorTool> colorTools;

        //Buttons
        Button activeButton;
        List<Button> toolButtons;
        List<Button> colorButtons;

        Model model;
        View view;

        //Timers
        private DispatcherTimer paintTimer;
        private DispatcherTimer inactivityTimer;

        public Paint()
        {
            InitializeComponent();

            // Create a canvas for painting
            paintingHeight = (int)System.Windows.SystemParameters.PrimaryScreenHeight;
            paintingWidth = (int)(System.Windows.SystemParameters.PrimaryScreenWidth * 0.8);
            painting = new RenderTargetBitmap(paintingHeight, paintingWidth, 96, 96, PixelFormats.Pbgra32);

            // Interaction via eye tracker and mouse
            InitializeEyeTracking();
            InitializeMouseControl();

            // Set up model and view
            SettingsFactory sf = new SettingsFactory();
            paintTools = sf.getPaintTools();
            colorTools = sf.getColorTools();
            model = new Model(paintTools[0], colorTools[0]);
            view = new View(painting);

            // Set up GUI
            toolButtons = new List<Button>();
            colorButtons = new List<Button>();
            gazeAwareButtons = new Dictionary<InteractorId, Button>();
            paintingImage.Source = painting;
            InitializeMenu();

            //Initialize parameters
            menuActive = false;

            //MouseDown += (object s, MouseEventArgs e) => startPainting();
            //MouseUp += (object s, MouseEventArgs e) => stopPainting();

            //Initialize timers
            paintTimer = new DispatcherTimer();
            paintTimer.Interval = TimeSpan.FromMilliseconds(1);
            paintTimer.Tick += (object s, EventArgs e) =>
            {
                model.Grow(); RasterizeModel();
            };

            // Set timer for inactivity
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMinutes(15);
            inactivityTimer.Tick += (object s, EventArgs e) =>
            {
                //TODO implement
            };
        }

        void InitializeMenu()
        {
            int leftmargin = (int)menuPanel.Margin.Left;
            int rightmargin = (int)menuPanel.Margin.Right;
            int btnWidth = (paintingWidth - leftmargin - rightmargin) / (colorTools.Count() + paintTools.Count + paintToolPanel.Children.Count + colorToolPanel.Children.Count);

            //Add ColorTools
            DockPanel.SetDock(colorToolPanel, Dock.Left);
            foreach (ColorTool ct in colorTools)
            {
                Button btn = new Button();
                var brush = new ImageBrush();

                String path = Directory.GetCurrentDirectory() + "\\Resources\\" + ct.iconImage; //TODO ev change to resources
                //brush.ImageSource = new BitmapImage(new Uri(path)); //TODO this should be uncommented, I just didn't have any pictures to the buttons

                btn.Background = brush;
                btn.Width = btnWidth;
                btn.Click += (object s, RoutedEventArgs e) => { model.ChangeColorTool(ct); };
                colorToolPanel.Children.Add(btn);
                colorButtons.Add(btn); // TODO Q: What do we need this list for?
                gazeAwareButtons.Add(ct.name, btn);
            }

            //Add PaintTools
            DockPanel.SetDock(paintToolPanel, Dock.Right);
            foreach (PaintTool pt in paintTools)
            {
                Button btn = new Button();
                var brush = new ImageBrush();

                String path = Directory.GetCurrentDirectory() + "\\Resources\\" + pt.iconImage; //TODO ev change to resource
                //brush.ImageSource = new BitmapImage(new Uri(path));

                btn.Background = brush;
                btn.Width = btnWidth;
                btn.GotFocus += (object s, RoutedEventArgs e) => { btn.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)); };
                btn.LostFocus += (object s, RoutedEventArgs e) => { btn.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)); };
                btn.Click += (object s, RoutedEventArgs e) => { model.ChangePaintTool(pt); };
                paintToolPanel.Children.Add(btn);
                toolButtons.Add(btn); // TODO Q: What do we need this list for?
                gazeAwareButtons.Add(pt.name, btn);
            }

            saveButton.Width = btnWidth;
            setRandomBackgroundButton.Width = btnWidth;

            // Set focus/blur behavior for gaze aware buttons
            foreach (var kv in gazeAwareButtons)
            {
                Button btn = kv.Value;
                btn.GotFocus += (object s, RoutedEventArgs e) =>
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                };
                btn.LostFocus += (object s, RoutedEventArgs e) =>
                {
                    btn.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                };
            }
        }

        private void OnGaze(string interactorId, bool hasGaze)
        {
            var control = gazeAwareButtons[interactorId];
            Console.WriteLine(interactorId);
            if (control != null)
            {
                control.Focus();
            }
        }

        //ButtonMethods on click
        void OnSetRandomBackGroundClick(object sender, RoutedEventArgs e)
        {
            SetBackGroundToRandomColor();
            model.ResetModel();
        }

        void OnSaveClick(object sender, RoutedEventArgs e)
        {
            //TODO CHANGE
            //Application.Current.Shutdown();
            Window confirmBox = new ConfirmBox();
            confirmBox.ShowDialog();
            if (confirmBox.DialogResult.HasValue && confirmBox.DialogResult.Value)
            {
                Console.WriteLine("YEAAAAH");
            }
            else
            {
                Console.WriteLine("Noooooo");
            }
        }

        //Methods for keypress
        void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            isKeyDown = false;
            StopPainting();
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (isKeyDown) return;
            isKeyDown = true;
            StartPainting();
        }

        void StartPainting()
        {
            if (menuActive) return;
            if (paint) return;
            paint = true;
            paintTimer.Start();
            TrackGaze(gaze, paint, 0);
            inactivityTimer.Stop();
        }

        // Stop painting.
        void StopPainting()
        {
            paint = false;
            paintTimer.Stop();
            inactivityTimer.Start();
        }

        void TrackGaze(Point p, bool keep = true, int keyhole = 100)
        {
            var distance = Math.Sqrt(Math.Pow(gaze.X - p.X, 2) + Math.Pow(gaze.Y - p.Y, 2));
            if (distance < keyhole) return;
            gaze = p;
            if (keep) model.Add(gaze, true); //TODO Add alwaysAdd argument, or remove it completely from the function declaration.      
        }

        void RasterizeModel()
        {
            view.Rasterize(model.GetRenderQueue());
        }

        void SetBackGroundToRandomColor()
        {
            view.setBackGroundColorRandomly();
        }

        void SavePainting()
        {
            //TODO implement
        }

        /// <summary>
        /// Sets up mouse based interaction.
        /// </summary>
        void InitializeMouseControl()
        {
            this.MouseMove += (object s, MouseEventArgs e) =>
            {
                var mousePosition = new Point(Mouse.GetPosition(paintingImage).X, Mouse.GetPosition(paintingImage).Y);
                TrackGaze(mousePosition, paint, 0);
            };
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
        }

        /// <summary>
        /// Sets up the EyeX engine and enables eye tracking.
        /// </summary>
        void InitializeEyeTracking()
        {
            // initialize the EyeX Engine client library.
            system = InteractionSystem.Initialize(LogTarget.Trace);

            // create a context, register event handlers, and enable the connection to the engine.
            context = new InteractionContext(false);
            context.RegisterQueryHandlerForCurrentProcess(HandleQuery);
            context.RegisterEventHandler(HandleEvent);
            context.EnableConnection();

            // enable gaze point tracking over the entire window
            InitializePaintingInteractorSnapshot();
            context.ConnectionStateChanged += (object s, ConnectionStateChangedEventArgs ce) =>
            {
                if (ce.State == ConnectionState.Connected)
                {
                    globalInteractorSnapshot.Commit((InteractionSnapshotResult isr) => { });
                }
            };
        }

        /// <summary>
        /// Initializes the EyeX snapshot that handles the painting interaction.
        /// </summary>
        private void InitializePaintingInteractorSnapshot()
        {
            globalInteractorSnapshot = context.CreateSnapshot();
            globalInteractorSnapshot.CreateBounds(InteractionBoundsType.None);
            globalInteractorSnapshot.AddWindowId(Literals.GlobalInteractorWindowId);

            var interactor = globalInteractorSnapshot.CreateInteractor(InteractorId, Literals.RootId, Literals.GlobalInteractorWindowId);
            interactor.CreateBounds(InteractionBoundsType.None);

            var behavior = interactor.CreateBehavior(InteractionBehaviorType.GazePointData);
            var behaviorParams = new GazePointDataParams() { GazePointDataMode = GazePointDataMode.LightlyFiltered };
            behavior.SetGazePointDataOptions(ref behaviorParams);
        }

        /// <summary>
        /// Handles a query from the EyeX Engine.
        /// Note that this method is called from a worker thread, so it may not access any WPF Window objects.
        /// </summary>
        /// <param name="query">Query.</param>
        private void HandleQuery(InteractionQuery query)
        {
            var queryBounds = query.Bounds;
            double x, y, w, h;
            if (queryBounds.TryGetRectangularData(out x, out y, out w, out h))
            {
                // marshal the query to the UI thread, where WPF objects may be accessed.
                System.Windows.Rect r = new System.Windows.Rect((int)x, (int)y, (int)w, (int)h);
                this.Dispatcher.BeginInvoke(new Action<System.Windows.Rect>(HandleQueryOnUiThread), r);
            }
        }

        private void HandleQueryOnUiThread(System.Windows.Rect queryBounds)
        {
            IntPtr windowHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            var windowId = windowHandle.ToString();

            var snapshot = context.CreateSnapshot();
            snapshot.AddWindowId(windowId);
            var bounds = snapshot.CreateBounds(InteractionBoundsType.Rectangular);
            bounds.SetRectangularData(queryBounds.Left, queryBounds.Top, queryBounds.Width, queryBounds.Height);
            System.Windows.Rect queryBoundsRect = new System.Windows.Rect(queryBounds.Left, queryBounds.Top, queryBounds.Width, queryBounds.Height);

            foreach (var kv in gazeAwareButtons)
            {
                Button b = kv.Value;
                InteractorId id = kv.Key;
                CreateGazeAwareInteractor(id, b, Literals.RootId, windowId, snapshot, queryBoundsRect);
            }

            snapshot.Commit((InteractionSnapshotResult isr) => { });
        }

        private void CreateGazeAwareInteractor(InteractorId id, Control control, string parentId, string windowId, InteractionSnapshot snapshot, System.Windows.Rect queryBoundsRect)
        {
            var controlTopLeft = control.TranslatePoint(new Point(0, 0), this);
            var controlRect = new System.Windows.Rect(controlTopLeft, control.RenderSize);
            if (controlRect.IntersectsWith(queryBoundsRect))
            {
                var interactor = snapshot.CreateInteractor(id, parentId, windowId);
                var bounds = interactor.CreateBounds(InteractionBoundsType.Rectangular);
                bounds.SetRectangularData(controlRect.Left, controlRect.Top, controlRect.Width, controlRect.Height);
                interactor.CreateBehavior(InteractionBehaviorType.GazeAware);
            }
        }

        /// <summary>
        /// Handles an event from the EyeX Engine.
        /// Note that this method is called from a worker thread, so it may not access any WPF objects.
        /// </summary>
        /// <param name="@event">Event.</param>
        private void HandleEvent(InteractionEvent @event)
        {
            var interactorId = @event.InteractorId;
            foreach (var behavior in @event.Behaviors)
            {
                if (behavior.BehaviorType == InteractionBehaviorType.GazeAware)
                {
                    GazeAwareEventParams r;
                    if (behavior.TryGetGazeAwareEventParams(out r))
                    {
                        // marshal the event to the UI thread, where WPF objects may be accessed.
                        this.Dispatcher.BeginInvoke(new Action<string, bool>(OnGaze), interactorId, r.HasGaze != EyeXBoolean.False);
                    }
                }
                else if (behavior.BehaviorType == InteractionBehaviorType.GazePointData)
                {
                    GazePointDataEventParams r;
                    if (behavior.TryGetGazePointDataEventParams(out r))
                    {
                        TrackGaze(new Point(r.X, r.Y), paint, 50); //TODO Set keyhole size dynamically based on how bad the calibration is.
                        this.Dispatcher.BeginInvoke(new Action(() => RasterizeModel()));
                    }
                }
            }
        }
    }
}
