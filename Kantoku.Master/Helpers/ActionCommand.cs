using System;
using System.Windows.Input;

namespace Kantoku.Master.Helpers
{
    public class ActionCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action Action;

        public ActionCommand(Action action)
        {
            this.Action = action;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => Action();
    }
}
