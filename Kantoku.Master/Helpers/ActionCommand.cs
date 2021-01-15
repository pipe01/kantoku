using System;
using System.Windows.Input;

namespace Kantoku.Master.Helpers
{
    public class ActionCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action Action;
        private readonly Func<bool>? CanRun;

        public ActionCommand(Action action)
        {
            this.Action = action;
        }
        public ActionCommand(Action action, Func<bool> canExecute) : this(action)
        {
            this.CanRun = canExecute;
        }

        public bool CanExecute(object? parameter) => CanRun?.Invoke() ?? true;

        public void Execute(object? parameter) => Action();
    }
}
