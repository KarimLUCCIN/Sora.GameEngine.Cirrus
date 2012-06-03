using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Sora.GameEngine.Cirrus.Design.Application
{
    /// <summary>
    /// Generic command class for UI
    /// </summary>
    public class GenericCommand : ICommand
    {
        private Action<object> actionCommand;

        public Action<object> ActionCommand
        {
            get { return actionCommand; }
            set
            {
                actionCommand = value;

                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        private Func<object, bool> validateCommand;

        public Func<object, bool> ValidateCommand
        {
            get { return validateCommand; }
            set
            {
                validateCommand = value;

                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, EventArgs.Empty);
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
    }
}
