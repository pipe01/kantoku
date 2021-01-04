using Kantoku.Shared;
using MessagePack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kantoku.Master.Media.Services
{
    public class SatelliteService : IService
    {
        public event EventHandler<ISession>? SessionStarted;

        private readonly ILogger Logger;

        public SatelliteService(ILogger logger)
        {
            this.Logger = logger.For<SatelliteService>();
        }

        public Task Start()
        {
            var server = new NamedPipeServerStream("Kantoku", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            new Thread(ReadLoop)
            {
                IsBackground = true,
                Name = "Satellites thread"
            }.Start(server);

            return Task.CompletedTask;
        }

        private void ReadLoop(object? obj)
        {
            Debug.Assert(obj != null);

            var pipe = (NamedPipeServerStream)obj;

            var buffer = new byte[Message.MaxMessageSize];

            while (true)
            {
                if (!pipe.IsConnected)
                {
                    Logger.Debug("No satellites connected, waiting for connection");

                    pipe.WaitForConnection();
                }

                int read = pipe.Read(buffer);

                var message = MessagePackSerializer.Deserialize<Message>(buffer.AsMemory(0, read));
            }
        }
    }
}
