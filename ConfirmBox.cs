using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tobii.EyeX.Client;
using Tobii.EyeX.Framework;
using InteractorId = System.String;
using System.Timers; //TODO typedef equivalent?

namespace EyePaint
{
    public partial class ConfirmBox : Form
    {
        InteractionContext context;
        InteractionSnapshot globalInteractorSnapshot;
        Dictionary<InteractorId, Button> gazeAwareButtons;

        public ConfirmBox()
        {
            InitializeComponent();

            gazeAwareButtons = new Dictionary<InteractorId, Button>();
            gazeAwareButtons.Add(ConfirmButton.Parent.Name + ConfirmButton.Name, ConfirmButton);
            gazeAwareButtons.Add(AbortButton.Parent.Name + AbortButton.Name, AbortButton);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Initialize eye tracking.
            context = new InteractionContext(false);
            initializeGlobalInteractorSnapshot();
            context.ConnectionStateChanged += (object s, ConnectionStateChangedEventArgs ce) =>
            {
                if (ce.State == ConnectionState.Connected)
                {
                    globalInteractorSnapshot.Commit((InteractionSnapshotResult isr) => { });
                }
            };
            context.RegisterQueryHandlerForCurrentProcess(handleInteractionQuery);
            context.RegisterEventHandler(handleInteractionEvent);
            context.EnableConnection();
        }

        // Handle events from the EyeX engine.
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

                if (IsHandleCreated) BeginInvoke(a); // Run on UI thread.
            }
        }

        // Handle events from the EyeX engine.
        void handleInteractionEvent(InteractionEvent e)
        {
            foreach (var behavior in e.Behaviors)
            {
                if (behavior.BehaviorType == InteractionBehaviorType.GazeAware)
                {
                    GazeAwareEventParams r;
                    if (behavior.TryGetGazeAwareEventParams(out r))
                    {
                        bool hasGaze = r.HasGaze != EyeXBoolean.False;
                        Action a = () => gazeAwareButtons[e.InteractorId].Focus();
                        if (hasGaze) BeginInvoke(a);
                    }
                }
            }
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

        public DialogResult Show(IWin32Window owner, string text)
        {
            TextBox.Text = text;
            return this.ShowDialog(owner);
        }
    }
}
