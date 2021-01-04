using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantoku.Master.Media.Services
{
    public class SatelliteService : IService
    {
        public event EventHandler<ISession> SessionStarted;

        public Task Start()
        {
            var server = new NamedPipeServerStream("Kantoku", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

            return Task.CompletedTask;
        }
    }
}
