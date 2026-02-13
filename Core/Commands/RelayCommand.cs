using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0.Core.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object?> _canExecute;
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute, Predicate<object?> canExecute)
        {
            try
            {
                this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
                this._canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                try
                {
                    CommandManager.RequerySuggested += value;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                }
            }
            remove
            {
                try
                {
                    CommandManager.RequerySuggested -= value;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                }
            }
        }

        public bool CanExecute(object? parameter)
        {
            try
            {
                return _canExecute(parameter);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
                return false;
            }
        }

        public void Execute(object? parameter)
        {
            try
            {
                _execute(parameter);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}

