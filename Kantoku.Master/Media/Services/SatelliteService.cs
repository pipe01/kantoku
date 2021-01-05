using Kantoku.Master.Helpers.Fetchers;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kantoku.Master.Media.Services
{
    public class SatelliteService : IService
    {
        private enum EventKind
        {
            Started,
            Paused,
            Resumed,
            TimeUpdated,
        }

        public event EventHandler<ISession> SessionStarted = delegate { };

        private readonly IDictionary<string, Session> Sessions = new Dictionary<string, Session>();
        private readonly IList<NamedPipeServerStream> Servers = new List<NamedPipeServerStream>();
        private readonly ILogger Logger;
        private readonly SynchronizationContext SynchronizationContext;

        public SatelliteService(ILogger logger, SynchronizationContext synchronizationContext)
        {
            this.Logger = logger.For<SatelliteService>();
            this.SynchronizationContext = synchronizationContext;
        }

        private void OnSessionStarted(ISession session) => this.SynchronizationContext.Send(_ => SessionStarted(this, session), null);

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

            using var message = new MemoryStream();

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
                HandleMessage(message, pipe);
                message.SetLength(0);
            }

            logger.Verbose("Exited read loop");
        }

        private void HandleMessage(Stream data, NamedPipeServerStream pipe)
        {
            Logger.Debug("Handling message length {Message}", data.Length);

            var el = JsonDocument.Parse(data).RootElement;
            if (!(el.ValueKind == JsonValueKind.Array && (el.GetArrayLength() is 2 or 3)))
                return;

            var arrayData = el.EnumerateArray().ToArray();

            var eventKind = (EventKind)arrayData[0].GetInt32();

            var id = arrayData[1].GetString();

            Debug.Assert(id != null);
            Logger.Debug("Got event {Event} from browser session ID {ID}", eventKind, id);

            Session? session = null;

            if (eventKind == EventKind.Started)
            {
                if (Sessions.ContainsKey(id))
                {
                    Logger.Error("Tried to start already started session ID {ID}", id);
                    return;
                }

                this.SynchronizationContext.Send(_ =>
                {
                    session = Session.CreateNew(Logger, pipe, arrayData[2]);
                    Logger.Debug("Created session ID {ID}", session.ID);

                    Sessions.Add(id, session);
                    SessionStarted(this, session);
                }, null);
            }

            if (session == null && !Sessions.TryGetValue(id, out session))
            {
                Logger.Error("Unknown session {ID}", id);
                return;
            }

            session.HandleMessage(eventKind, arrayData.Length > 2 ? arrayData[2] : default);
        }

        private record BrowserMediaInfo(string Title, string Author, string IconUrl, string AppName);

        private class Session : ISession
        {
            public Guid ID { get; }

            public AppInfo App { get; }

            public TimeSpan Position { get; private set; }

            public bool IsPlaying { get; private set; }

            public MediaInfo? Media { get; private set; }

            public event Action Closed = delegate { };
            public event PropertyChangedEventHandler? PropertyChanged;

            private readonly ILogger Logger;
            private readonly NamedPipeServerStream Pipe;

            private Session(ILogger rootLogger, NamedPipeServerStream pipe, AppInfo app)
            {
                this.ID = Guid.NewGuid();
                this.Logger = rootLogger.For("Session " + ID);
                this.Pipe = pipe;
                this.App = app;
            }

            public static Session CreateNew(ILogger rootLogger, NamedPipeServerStream pipe, JsonElement initData)
            {
                if (initData.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("Invalid media data", nameof(initData));

                var info = initData.ToObject<BrowserMediaInfo>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Invalid media data");

                rootLogger.Debug("Media info: {Info}", info);

                var icon = new BitmapImage(new Uri(info.IconUrl ?? ""));

                var app = new AppInfo(info.AppName, icon);

                return new Session(rootLogger, pipe, app);
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void HandleMessage(EventKind kind, JsonElement data)
            {
                switch (kind)
                {
                    case EventKind.Paused:
                        break;
                    case EventKind.Resumed:
                        break;
                    case EventKind.TimeUpdated:
                        break;
                }
            }

            public Task Next()
            {
                throw new NotImplementedException();
            }

            public Task Pause()
            {
                throw new NotImplementedException();
            }

            public Task Play()
            {
                throw new NotImplementedException();
            }

            public Task Previous()
            {
                throw new NotImplementedException();
            }

            public Task Stop()
            {
                throw new NotImplementedException();
            }
        }
    }
}
