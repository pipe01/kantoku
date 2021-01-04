using Kantoku.Shared;
using MessagePack;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

                pipe.Write(Encoding.UTF8.GetBytes("hello there"));

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
            int read;

            while ((read = pipe.Read(buffer)) != 0)
            {
                logger.Verbose("Read {Count} bytes", read);
            }

            logger.Verbose("Exited read loop");
        }
    }
}
