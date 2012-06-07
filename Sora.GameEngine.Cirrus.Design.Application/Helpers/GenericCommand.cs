using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;

namespace Sora.GameEngine.Cirrus.Design.Application.Helpers
{
    /// <summary>
    /// Generic command class for UI
    /// </summary>
    public class GenericCommand : ICommand, INotifyPropertyChanged
    {
        private Action<object> actionCommand;

        public Action<object> ActionCommand
        {
            get { return actionCommand; }
            set
            {
                actionCommand = value;

                RaiseCanExecuteChanged();
            }
        }

        private Func<object, bool> validateCommand;

        public Func<object, bool> ValidateCommand
        {
            get { return validateCommand; }
            set
            {
                validateCommand = value;

                RaiseCanExecuteChanged();
            }
        }

        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set
            {
                displayName = value;
                RaisePropertyChanged("DisplayName");
            }
        }


        public GenericCommand(Action<object> actionCommand,
            Func<object, bool> validateCommand = null)
        {
            this.actionCommand = actionCommand;
            this.validateCommand = validateCommand;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (actionCommand == null)
                return false;
            else
            {
                if (validateCommand != null)
                    return validateCommand(parameter);
                else
                    return true;
            }
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (actionCommand != null)
                actionCommand(parameter);
        }

        #endregion

        private void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        public void Refresh()
        {
            RaiseCanExecuteChanged();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
