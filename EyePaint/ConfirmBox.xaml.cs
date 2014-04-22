using System;
using System.Collections.Generic;
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
using Tobii.EyeX.Client;
using Tobii.EyeX.Framework;
using InteractorId = System.String;

namespace EyePaint
{
    /// <summary>
    /// Interaction logic for ConfirmBox.xaml
    /// </summary>
    public partial class ConfirmBox : Window
    {
        private const string InteractorId = "EyePaintConfirmBox";
        private InteractionSystem system;
        private InteractionContext context;

        Dictionary<InteractorId, Button> gazeAwareButtons = new Dictionary<InteractorId, Button>();

        public ConfirmBox()
        {
            InitializeComponent();

            gazeAwareButtons.Add(Confirm.Name, Confirm);
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// Method called when a gaze aware object has gaze.
        /// </summary>
        private void OnGaze(string interactorId, bool hasGaze)
        {
            var control = gazeAwareButtons[interactorId];
            Console.WriteLine(interactorId);
            if (control != null)
            {
                control.Focus();
            }
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
            }
        }
    }
}
