using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Kantoku.Master.Media.Services
{
    public class SatelliteService : IService
    {
        public event EventHandler<ISession>? SessionStarted;

        private readonly IList<NamedPipeServerStream> Servers = new List<NamedPipeServerStream>();
        private readonly ILogger Logger;

        public SatelliteService(ILogger logger)
        {
            this.Logger = logger.For<SatelliteService>();
        }

        public Task Start()
        {
            StartPipe();

            return Task.CompletedTask;
        }

        private void StartPipe()
        {
            var pipe = new NamedPipeServerStream(Satellite.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances);

            Logger.Debug("Starting pipe {Number}", Servers.Count + 1);

            Servers.Add(pipe);

            Task.Run(() =>
            {
                var logger = Logger.For("Satellite " + Servers.Count);

                logger.Debug("Waiting for connection");
                pipe.WaitForConnection();

                try
                {
                    StartPipe();
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to start new pipe");
                }

                try
                {
                    logger.Debug("Starting read loop");

                    ReadLoop(pipe, logger);
                }
                finally
                {
                    logger.Debug("Removing pipe");

                    Servers.Remove(pipe);
                    pipe.Dispose();
                }
            });
        }

        private void ReadLoop(NamedPipeServerStream pipe, ILogger logger)
        {
            var buffer = new byte[1024];
            var sizeBuffer = new byte[4];
            int read, messageSize;

            var message = new MemoryStream();

            while (true)
            {
                if (pipe.Read(sizeBuffer) == 0)
                    break;

                messageSize = BitConverter.ToInt32(sizeBuffer);

                logger.Verbose($"Message size is {messageSize} bytes");
                logger.Verbose(BitConverter.ToString(sizeBuffer));

                while (message.Length < messageSize)
                {
                    if ((read = pipe.Read(buffer)) == 0)
                        return;

                    logger.Verbose($"Read {read} bytes from browser");
                    logger.Verbose(BitConverter.ToString(sizeBuffer));

                    message.Write(buffer, 0, read);
                }

                message.Position = 0;
                HandleMessage(Encoding.UTF8.GetString(message.ToArray()));
                message.SetLength(0);
            }

            logger.Verbose("Exited read loop");
        }

        private void HandleMessage(string data)
        {
            Logger.Debug("Handling message {Message}", data);
        }
    }
}
