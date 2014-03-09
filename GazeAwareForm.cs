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
    /**
     * Intended only for use as a super class. Throws error if used without subclassing.
     */
    public partial class GazeAwareForm : Form
    {
        protected InteractionContext context;
        protected InteractionSnapshot globalInteractorSnapshot;
        protected Dictionary<InteractorId, Button> gazeAwareButtons;

        public GazeAwareForm()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Initialize eye tracking.
            context = new InteractionContext(false);
            gazeAwareButtons = new Dictionary<InteractorId, Button>();
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

        // Initialize a global interactor snapshot that this application responds to the EyeX engine with whenever queried.
        protected void initializeGlobalInteractorSnapshot()
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
        protected void handleInteractionQuery(InteractionQuery q)
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
        virtual protected void handleInteractionEvent(InteractionEvent e)
        {
            throw new NotImplementedException();
        }

        // Focus the button, wait a short period of time and if the button is still focused: click it.
        protected void clickCountdown(Button b)
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

        protected void onButtonFocus(object s, EventArgs e)
        {
            //TODO Animate
            var b = (Button)s;
            b.FlatAppearance.BorderColor = Color.Gray;
        }

        protected void onButtonBlur(object s, EventArgs e)
        {
            //TODO Animate
            var b = (Button)s;
            b.FlatAppearance.BorderColor = Color.White;
        }

        protected void onButtonClicked(object s, EventArgs e)
        {
            var b = (Button)s;
            b.FlatAppearance.BorderColor = Color.Black;
        }
    }
}
