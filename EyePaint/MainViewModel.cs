using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace EyePaint
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private UserControl control = new Welcome();

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a PropertyChanged event
        /// </summary>
        protected void OnPropertyChanged(UserControl property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property.ToString()));
            }
        }

        #endregion

        void OnControlChanged(UserControl value)
        {
            OnPropertyChanged(control);
        }

        public UserControl Control
        {
            get { return control; }
            set { control = value; }
        }
    }
}
