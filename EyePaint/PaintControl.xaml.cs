using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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
    public partial class PaintControl : UserControl, IDisposable
    {
        private const string InteractorId = "EyePaint";
        private InteractionSystem system;
        private InteractionContext context;
        private InteractionSnapshot globalInteractorSnapshot;
        private int queryHandlerTicket;
        private int eventHandlerTicket;

        Point gaze;
        bool paintingActive = false;
        Button activeButton;
        Button activePaintTool;
        Button activeColorTool;
        Dictionary<InteractorId, Button> gazeAwareButtons;

        //Painting
        int paintingWidth;
        int paintingHeight;
        RenderTargetBitmap painting;

        //Tools 
        List<PaintTool> paintTools;
        List<ColorTool> colorTools;

        Model model;
        View view;

        //Timers
        private DispatcherTimer paintTimer;
        private DispatcherTimer inactivityTimer;

        // initialize the EyeX Engine client library.
        public PaintControl()
        {
            InitializeComponent();

            // Create a canvas for painting
            paintingWidth = (int)(System.Windows.SystemParameters.PrimaryScreenWidth);
            paintingHeight = (int)(System.Windows.SystemParameters.PrimaryScreenHeight * 0.8);
            painting = new RenderTargetBitmap(paintingWidth, paintingHeight, 96, 96, PixelFormats.Pbgra32);

            // Interaction via eye tracker and mouse
            system = AppStateMachine.Instance.System;
            InitializeEyeTracking();
            InitializeMouseControl();

            // Set up model and view
            SettingsFactory sf = new SettingsFactory();
            paintTools = sf.getPaintTools();
            colorTools = sf.getColorTools();
            model = new Model(paintTools[0], colorTools[0]);
            view = new View(painting);

            // Set up GUI
            gazeAwareButtons = new Dictionary<InteractorId, Button>();
            paintingImage.Source = painting;
            InitializeMenu();

            //Initialize timers
            paintTimer = new DispatcherTimer();
            paintTimer.Interval = TimeSpan.FromMilliseconds(1);
            paintTimer.Tick += (object s, EventArgs e) =>
            {
                model.Grow();
                RasterizeModel();
            };

            // Set timer for inactivity
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMinutes(1);
            inactivityTimer.Tick += (object s, EventArgs e) =>
            {
                Reset();
            };
        }

        void InitializeMenu()
        {
            int btnWidth = 2*(int)saveButton.BorderThickness.Right +
                (paintingWidth / (colorTools.Count + paintTools.Count + systemPanel.Children.Count));

            //Add ColorTools
            DockPanel.SetDock(colorToolPanel, Dock.Left);
            foreach (ColorTool ct in colorTools)
            {
                Button btn = new Button();
                var brush = new ImageBrush();

                String path = Directory.GetCurrentDirectory() + "\\Resources\\" + ct.iconImage;
                brush.ImageSource = new BitmapImage(new Uri(path));
                brush.Stretch = System.Windows.Media.Stretch.None;

                btn.Name = ct.name;
                btn.Background = brush;
                btn.BorderThickness = new System.Windows.Thickness(0, 0, 0, 4);
                btn.BorderBrush = Brushes.Transparent;
                btn.FocusVisualStyle = null;
                btn.Width = btnWidth;
                btn.Click += (object s, RoutedEventArgs e) =>
                {
                    activeColorTool.BorderBrush = Brushes.Transparent;
                    activeColorTool = btn;
                    activeColorTool.BorderBrush = Brushes.Black;
                    model.ChangeColorTool(ct);
                };
                colorToolPanel.Children.Add(btn);
                gazeAwareButtons.Add(btn.Name, btn);
            }

            //Add PaintTools
            DockPanel.SetDock(paintToolPanel, Dock.Right);
            foreach (PaintTool pt in paintTools)
            {
                Button btn = new Button();
                var brush = new ImageBrush();

                String path = Directory.GetCurrentDirectory() + "\\Resources\\" + pt.iconImage;
                brush.ImageSource = new BitmapImage(new Uri(path));
                brush.Stretch = System.Windows.Media.Stretch.None;

                btn.Name = pt.name;
                btn.Background = brush;
                btn.BorderThickness = new System.Windows.Thickness(0, 0, 0, 4);
                btn.BorderBrush = Brushes.Transparent;
                btn.FocusVisualStyle = null;
                btn.Width = btnWidth;
                btn.Click += (object s, RoutedEventArgs e) =>
                {
                    activePaintTool.BorderBrush = Brushes.Transparent;
                    model.ChangePaintTool(pt);
                    btn.BorderBrush = Brushes.Black;
                    activePaintTool = btn;
                };
                paintToolPanel.Children.Add(btn);
                gazeAwareButtons.Add(btn.Name, btn);
            }

            saveButton.Width = btnWidth;
            setRandomBackgroundButton.Width = btnWidth;

            gazeAwareButtons.Add(saveButton.Name, saveButton);
            gazeAwareButtons.Add(setRandomBackgroundButton.Name, setRandomBackgroundButton);

            // Set active buttons
            activePaintTool = gazeAwareButtons[paintTools[0].name];
            activeColorTool = gazeAwareButtons[colorTools[0].name];
            activePaintTool.BorderBrush = new SolidColorBrush(Color.FromRgb(0,0,0));
            activeColorTool.BorderBrush = new SolidColorBrush(Color.FromRgb(0,0,0));

            // Bind events to gaze aware buttons
            foreach (var kv in gazeAwareButtons)
            {
                Button btn = kv.Value;
                btn.PreviewKeyDown += (object s, KeyEventArgs e) => { gazeAwareButton_PreviewKeyDown(s, e); };
                btn.GotFocus += (object s, RoutedEventArgs e) =>
                {
                    System.Windows.Media.Effects.DropShadowEffect eff = new System.Windows.Media.Effects.DropShadowEffect();
                    eff.Color = Color.FromRgb(180, 180, 180);
                    eff.Direction = 270;
                    eff.BlurRadius = 16;
                    btn.Effect = eff;
                };
                btn.LostFocus += (object s, RoutedEventArgs e) =>
                {
                    btn.Effect = null;
                };
            }
        }

        #region PaintControl actions

        /// <summary>
        /// Event handler for gaze aware events, which are events triggered
        /// when the user looks at a gaze aware button.
        /// </summary>
        /// <param name="interactorId">ID of the gaze aware button</param>
        /// <param name="hasGaze">Flag indicating if the user is looking at the button</param>
        private void TrackInteractor(string interactorId, bool hasGaze)
        {
            Button btn = gazeAwareButtons[interactorId];
            if (btn != null)
            {
                btn.Focus();
                activeButton = btn;
            }
        }

        /// <summary>
        /// Tracks a gaze point from the tracker. Must run on the UI thread.
        /// </summary>
        void TrackGaze(Point p, bool keep = true, int keyhole = 100)
        {
            var distance = Math.Sqrt(Math.Pow(gaze.X - p.X, 2) + Math.Pow(gaze.Y - p.Y, 2));
            if (distance < keyhole) return;
            gaze = p;
            if (keep) model.Add(gaze, true); //TODO Add alwaysAdd argument, or remove it completely from the function declaration.      
        }

        void StartPainting()
        {
            if (paintingActive) return;
            paintingActive = true;
            paintTimer.Start();
            TrackGaze(gaze, paintingActive, 0);
            inactivityTimer.Stop();
        }

        void StopPainting()
        {
            paintingActive = false;
            paintTimer.Stop();
            inactivityTimer.Start();
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
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(painting));
            string filename = DateTime.Now.TimeOfDay.TotalSeconds + ".png";
            using (Stream stm = File.Create(filename))
            {
                encoder.Save(stm);
            }
        }

        void Reset()
        {
            SavePainting();
            AppStateMachine.Instance.Next();
        }

        #endregion

        #region PaintControl event handlers
        
        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
        }

        void OnSetRandomBackGroundClick(object sender, RoutedEventArgs e)
        {
            SavePainting();
            SetBackGroundToRandomColor();
            model.ResetModel();
        }

        void OnSaveClick(object sender, RoutedEventArgs e)
        {
            SavePainting();
        }

        private void gazeAwareButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && activeButton != null)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(activeButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
                e.Handled = true;
            }
        }

        private void paintingImage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                StartPainting();
                e.Handled = true;
            }
        }

        private void paintingImage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                StopPainting();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Window confirmBox = new ConfirmBox("Vill du starta om?");
                confirmBox.ShowDialog();
                if (confirmBox.DialogResult.HasValue && confirmBox.DialogResult.Value)
                {
                    Reset();
                }
            }
        }

        #endregion

        #region Mouse interaction

        /// <summary>
        /// Sets up mouse based interaction.
        /// </summary>
        void InitializeMouseControl()
        {
            this.MouseMove += (object s, MouseEventArgs e) =>
            {
                var mousePosition = new Point(Mouse.GetPosition(paintingImage).X, Mouse.GetPosition(paintingImage).Y);
                TrackGaze(mousePosition, paintingActive, 0);
            };
            this.MouseDown += (object s, MouseButtonEventArgs e) => { StartPainting(); };
            this.MouseUp += (object s, MouseButtonEventArgs e) => { StopPainting(); };
        }

        #endregion

        #region EyeTracking with EyeX

        /// <summary>
        /// Sets up the EyeX engine and enables eye tracking.
        /// </summary>
        void InitializeEyeTracking()
        {
            // create a context, register event handlers, and enable the connection to the engine.
            context = new InteractionContext(false);
            queryHandlerTicket = context.RegisterQueryHandlerForCurrentProcess(HandleQuery);
            eventHandlerTicket = context.RegisterEventHandler(HandleEvent);
            context.EnableConnection();

            // enable gaze point tracking over the entire window
            InitializeGlobalInteractorSnapshot();
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
        private void InitializeGlobalInteractorSnapshot()
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
            IntPtr windowHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle; // TODO: crashes on reset. Make IDisposable and do dispose
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

            if (!paintingActive && controlRect.IntersectsWith(queryBoundsRect))
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
                        this.Dispatcher.BeginInvoke(new Action<string, bool>(TrackInteractor), interactorId, r.HasGaze != EyeXBoolean.False);
                    }
                }
                else if (behavior.BehaviorType == InteractionBehaviorType.GazePointData)
                {
                    GazePointDataEventParams r;
                    if (behavior.TryGetGazePointDataEventParams(out r))
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (PresentationSource.FromVisual(paintingImage) != null)
                            {
                                Point p = new Point(r.X, r.Y);
                                p = paintingImage.PointFromScreen(p);
                                var paintingTopLeft = paintingImage.TranslatePoint(new Point(0, 0), this);
                                var paintingRect = new System.Windows.Rect(paintingTopLeft, paintingImage.RenderSize);

                                if (paintingRect.Contains(p))
                                {
                                    paintingImage.Focus();
                                    TrackGaze(p, paintingActive, 50); //TODO Set keyhole size dynamically based on how bad the calibration is.
                                    RasterizeModel();
                                }
                            }
                        }));
                    }
                }
            }
        }

        #endregion

        public void Dispose()
        {
            context.UnregisterHandler(queryHandlerTicket);
            context.UnregisterHandler(eventHandlerTicket);
            context.DisableConnection();
            context.Dispose();
            globalInteractorSnapshot.Dispose();
        }
    }
}
