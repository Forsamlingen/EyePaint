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

        public ConfirmBox(string msg)
        {
            InitializeComponent();

            system = AppStateMachine.Instance.System;
            Message.Content = msg;
            gazeAwareButtons.Add(Confirm.Name, Confirm);
            gazeAwareButtons.Add(Cancel.Name, Cancel);
            InitializeEyeTracking();
        }

        void onConfirmClick(object s, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void onCancelClick(object s, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Method called when a gaze aware object has gaze.
        /// </summary>
        void onGaze(string interactorId, bool hasGaze)
        {
            var control = gazeAwareButtons[interactorId];
            Console.WriteLine(interactorId);
            if (control != null)
            {
                control.Focus();
                control.Background = new SolidColorBrush(Color.FromRgb(30,30,30));
            }
        }

        /// <summary>
        /// Sets up the EyeX engine and enables eye tracking.
        /// </summary>
        void InitializeEyeTracking()
        {
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
        void HandleQuery(InteractionQuery query)
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

        void HandleQueryOnUiThread(System.Windows.Rect queryBounds)
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

        void CreateGazeAwareInteractor(InteractorId id, Control control, string parentId, string windowId, InteractionSnapshot snapshot, System.Windows.Rect queryBoundsRect)
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
        void HandleEvent(InteractionEvent @event)
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
                        this.Dispatcher.BeginInvoke(new Action<string, bool>(onGaze), interactorId, r.HasGaze != EyeXBoolean.False);
                    }
                }
            }
        }
    }
}
