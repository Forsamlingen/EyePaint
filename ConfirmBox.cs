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
    public partial class ConfirmBox : GazeAwareForm
    {
        public ConfirmBox() : base()
        {
            InitializeComponent();

            gazeAwareButtons.Add(ConfirmButton.Parent.Name + ConfirmButton.Name, ConfirmButton);
            gazeAwareButtons.Add(AbortButton.Parent.Name + AbortButton.Name, AbortButton);
        }

        // Handle events from the EyeX engine.
        override protected void handleInteractionEvent(InteractionEvent e)
        {
            foreach (var behavior in e.Behaviors)
            {
                if (behavior.BehaviorType == InteractionBehaviorType.GazeAware)
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

        public DialogResult Show(IWin32Window owner, string text)
        {
            TextBox.Text = text;
            return this.ShowDialog(owner);
        }
    }
}
