using Kantoku.Master.Helpers;
using Kantoku.Master.Helpers.Fetchers;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            Keepalive,
            Closed,

            Next,
            Previous,
            Play,
            Pause,
            Stop
        }

        public event EventHandler<ISession> SessionStarted = delegate { };

        private int SatelliteCounter;

        private readonly IDictionary<string, Session> Sessions = new Dictionary<string, Session>();
        private readonly IList<NamedPipeServerStream> Servers = new List<NamedPipeServerStream>();
        private readonly ILogger Logger;
        private readonly SynchronizationContext SynchronizationContext;

        public SatelliteService(ILogger logger, SynchronizationContext synchronizationContext)
        {
            this.Logger = logger.For<SatelliteService>();
            this.SynchronizationContext = synchronizationContext;
        }

        private void OnSessionStarted(ISession session) => this.SynchronizationContext.Send(() => SessionStarted(this, session));

        public Task Start()
        {
            StartPipe();

            return Task.CompletedTask;
        }

        private void StartPipe()
        {
            var pipe = new NamedPipeServerStream("Kantoku", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            SatelliteCounter++;

            Logger.Debug("Starting pipe {Number}", SatelliteCounter);

            Servers.Add(pipe);

            Task.Run(async () =>
            {
                var logger = Logger.For("Satellite " + SatelliteCounter);

                logger.Debug("Waiting for connection");
                pipe.WaitForConnection();

                using var writer = new MessageWriter(pipe);

                writer.Write(EventKind.Keepalive);

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

                    await ReadLoop(pipe, writer, logger);
                }
                finally
                {
                    logger.Debug("Removing pipe");

                    Servers.Remove(pipe);
                    pipe.Dispose();
                }
            });
        }

        private async Task ReadLoop(NamedPipeServerStream pipe, MessageWriter writer, ILogger logger)
        {
            var reader = new MessageReader(pipe);

            await foreach (var item in reader)
            {
                Debug.Assert(item != null);

                try
                {
                    HandleMessage(pipe, writer, item.RootElement);
                }
                finally
                {
                    item.Dispose();
                }
            }

            logger.Verbose("Exited read loop");
        }

        private void HandleMessage(NamedPipeServerStream pipe, MessageWriter writer, JsonElement data)
        {
            Logger.Debug("Handling message {Message}", data);

            if (!(data.ValueKind == JsonValueKind.Array && (data.GetArrayLength() is 2 or 3)))
                return;

            var arrayData = data.EnumerateArray().ToArray();

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

                session = new Session(Logger, writer, id);
                session.Closed += Session_Closed;

                Logger.Debug("Created session ID {ID}", session.ID);
                Sessions.Add(id, session);

                OnSessionStarted(session);

                void Session_Closed()
                {
                    using (session)
                    {
                        Logger.Debug("Closed session ID {ID}", session!.ID);

                        Sessions.Remove(session.BrowserID);
                        session.Closed -= Session_Closed;
                    }
                }
            }

            if (session == null && !Sessions.TryGetValue(id, out session))
            {
                Logger.Error("Unknown session {ID}", id);
                return;
            }

            this.SynchronizationContext.Send(() => session.HandleMessage(eventKind, arrayData.Length > 2 ? arrayData[2] : default));
        }

        private record BrowserMediaInfo(string Title, string Author, string IconUrl, string AppName, double Duration);

        private class Session : ISession
        {
            public Guid ID { get; }

            public AppInfo? App { get; private set; }

            public TimeSpan Position { get; private set; }

            public bool IsPlaying { get; private set; }

            public MediaInfo? Media { get; private set; }

            public string BrowserID { get; }

            public event Action Closed = delegate { };
            public event PropertyChangedEventHandler? PropertyChanged;

            private readonly ILogger Logger;
            private readonly MessageWriter Writer;
            private readonly Debouncer DeadDebouncer;

            public Session(ILogger rootLogger, MessageWriter writer, string browserID)
            {
                this.ID = Guid.NewGuid();
                this.Logger = rootLogger.For("Satellite Session " + ID);
                this.Writer = writer;
                this.BrowserID = browserID;
                this.DeadDebouncer = new Debouncer(10000);

                DeadDebouncer.Idled += DeadDebouncer_Idled;
            }

            public void Dispose()
            {
            }

            public void HandleMessage(EventKind kind, JsonElement data)
            {
                DeadDebouncer.Trigger();

                switch (kind)
                {
                    case EventKind.Started:
                        BrowserMediaInfo info;

                        try
                        {
                            info = data.ToObject<BrowserMediaInfo>(new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? throw new Exception("Invalid media data");
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to parse event data");
                            break;
                        }

                        Logger.Verbose("Media info: {Info}", info);

                        var icon = new BitmapImage(new Uri(info.IconUrl ?? ""));

                        this.App = new AppInfo(info.AppName, icon);
                        this.Media = new MediaInfo(info.Title, info.Author, TimeSpan.FromSeconds(info.Duration));
                        break;

                    case EventKind.Closed:
                        Closed();
                        break;

                    case EventKind.Paused:
                        IsPlaying = false;
                        break;

                    case EventKind.Resumed:
                        IsPlaying = true;
                        break;

                    case EventKind.TimeUpdated:
                        double[] times;

                        try
                        {
                            times = data.EnumerateArray().Select(o => o.GetDouble()).ToArray();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to parse event data");
                            break;
                        }

                        if (times.Length != 2)
                        {
#if DEBUG
                            throw new Exception($"Invalid time array length {times.Length}");
#else
                            break;
#endif
                        }

                        Position = TimeSpan.FromSeconds(times[0]);
                        if (Media != null)
                            Media.Duration = TimeSpan.FromSeconds(times[1]);

                        break;
                }
            }

            private void DeadDebouncer_Idled(object? sender, EventArgs e)
            {
                Closed();
            }

            private void SendMessage(EventKind kind)
            {
                Writer.Write(kind, BrowserID);
            }

            public Task Next()
            {
                Logger.Debug("Skip next");

                SendMessage(EventKind.Next);
                return Task.CompletedTask;
            }

            public Task Pause()
            {
                Logger.Debug("Pause");

                SendMessage(EventKind.Pause);
                return Task.CompletedTask;
            }

            public Task Play()
            {
                Logger.Debug("Play");

                SendMessage(EventKind.Play);
                return Task.CompletedTask;
            }

            public Task Previous()
            {
                Logger.Debug("Skip previous");

                SendMessage(EventKind.Previous);
                return Task.CompletedTask;
            }

            public Task Stop()
            {
                Logger.Debug("Stop");

                SendMessage(EventKind.Stop);
                return Task.CompletedTask;
            }
        }
    }
}
