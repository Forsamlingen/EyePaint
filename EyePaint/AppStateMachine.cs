using System;
using System.ComponentModel;
using System.Windows.Controls;
using Tobii.EyeX.Client;

namespace EyePaint
{
    /// <summary>
    /// A state machine for the EyePaint app that controls flow between the
    /// different user controls and tracks state of the app. Acts as the
    /// ViewModel for MainWindow.
    /// </summary>
    public class AppStateMachine : INotifyPropertyChanged
    {
        static volatile AppStateMachine instance;
        static object m_lock = new object();
        /// <summary>
        /// Thread safe singleton instance
        /// </summary>
        public static AppStateMachine Instance {
            get
            {
                if (instance == null)
                {
                    lock (m_lock)
                    {
                        if (instance == null) { instance = new AppStateMachine(); }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Possible states
        /// </summary>
        public enum State
        {
            Start,
            Position,
            Calibrate,
            Paint
        }

        State state = State.Start;
        UserControl control = new StartControl();
        InteractionSystem system;

        private AppStateMachine()
        {
            // Initialize EyeX interaction "system". EyeX API only allows one
            // instance, so this must be set here in the AppStateMachine and
            // passed to EyeX enabled controls.
            system = InteractionSystem.Initialize(LogTarget.Trace);
        }

        /// <summary>
        /// Switch states
        /// </summary>
        public void Next()
        {
            switch (Instance.state)
            {
                case State.Start:
                    //TODO Dispose previous control.
                    //Instance.state = State.Position;
                    //Instance.Control = new PositioningControl();
                    Instance.state = State.Paint;
                    Instance.Control = new PaintControl(system);
                    break;
                case State.Position:
                    //TODO Dispose previous control.
                    Instance.state = State.Calibrate;
                    Instance.Control = new CalibrationControl();
                    break;
                case State.Calibrate:
                    //TODO Dispose previous control.
                    Instance.state = State.Paint;
                    Instance.Control = new PaintControl(system);
                    break;
                case State.Paint:
                    ((PaintControl)Instance.Control).Dispose();
                    Instance.state = State.Start;
                    Instance.Control = new StartControl();
                    break;
            }
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a PropertyChanged event
        /// </summary>
        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        /// <summary>
        /// Public property Control. Wired for TwoWay databinding.
        /// </summary>
        public UserControl Control
        {
            get { return control; }
            set
            {
                control = value;
                OnPropertyChanged("Control");
            }
        }
    }
}
