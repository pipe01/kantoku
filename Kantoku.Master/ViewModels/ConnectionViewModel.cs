using System.Windows.Input;
using Kantoku.Master.Frontend;
using Kantoku.Master.Helpers;

namespace Kantoku.Master.ViewModels
{
    public class ConnectionViewModel
    {
        public IConnection Connection { get; }

        public ICommand Close { get; }

        public ConnectionViewModel(IConnection connection)
        {
            this.Connection = connection;

            this.Close = new ActionCommand(connection.Close);
        }
    }
}
