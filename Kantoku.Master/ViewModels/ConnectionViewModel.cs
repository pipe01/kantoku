using Kantoku.Master.Frontend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantoku.Master.ViewModels
{
    public class ConnectionViewModel
    {
        public IConnection Connection { get; }

        public ConnectionViewModel(IConnection connection)
        {
            this.Connection = connection;
        }
    }
}
