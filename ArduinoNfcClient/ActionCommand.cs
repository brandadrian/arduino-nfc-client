using System;
using System.Windows.Input;

namespace ArduinoNfcClient
{
    public class ActionCommand : ICommand
    {
        /// <summary>
        /// The execute handler
        /// </summary>
        private readonly Action<object> executeHandler;
        /// <summary>
        /// The can execute handler
        /// </summary>
        private readonly Func<object, bool> canExecuteHandler;

        /// <summary>
        /// Create Action Command from type ICommand
        /// </summary>
        /// <param name="execute">Method which is called on Action Command</param>
        public ActionCommand(Action<object> execute)
        {
            if (execute == null)
                throw new ArgumentNullException("Execute cannot be null");

            this.executeHandler = execute;
        }

        /// <summary>
        /// Create Action Command from type ICommand
        /// </summary>
        /// <param name="execute">Method which is called on Action Command</param>
        /// <param name="canExecute">Check method if the command can be executed</param>
        public ActionCommand(Action<object> execute, Func<object, bool> canExecute)
            : this(execute)
        {
            this.canExecuteHandler = canExecute;
        }

        /// <summary>
        /// <c>Execute method</c> for ICommands
        /// </summary>
        /// <param name="parameter">optional parameter</param>
        public void Execute(object parameter)
        {
            this.executeHandler(parameter);
        }

        /// <summary>
        /// Check if the action is workable
        /// </summary>
        /// <param name="parameter">optional parameter</param>
        /// <returns>returns true if execute-method can execute, else false</returns>
        public bool CanExecute(object parameter)
        {
            if (this.canExecuteHandler == null)
                return true;

            return this.canExecuteHandler(parameter);
        }

        /// <summary>
        /// Event handler for executed method
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}