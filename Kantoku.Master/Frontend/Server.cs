using Kantoku.Master.Helpers;
using Kantoku.Master.Media;
using Kantoku.Master.ViewModels;
using LightInject;
using Serilog;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Kantoku.Master.Frontend
{
    public interface IServer : IHosted
    {
    }

    public class Server : IServer
    {
        private const ushort Port = 4545;

        private readonly ILogger Logger;
        private readonly WebSocketServer WebServer;

        public Server(ILogger<Server> logger, IServiceContainer container)
        {
            this.Logger = logger;

            container.Register<Behaviour>();

            WebServer = new WebSocketServer(4545);
            WebServer.AddWebSocketService("/ws", () => container.GetInstance<Behaviour>());
        }

        public Task Start()
        {
            WebServer.Start();

            Logger.Debug("Listening on {Port}", Port);

            return Task.CompletedTask;
        }

        private class Behaviour : WebSocketBehavior
        {
            private readonly DashboardViewModel Dashboard;
            private readonly ILogger Logger;

            public Behaviour(DashboardViewModel dashboard, ILogger logger)
            {
                this.Dashboard = dashboard;
                this.Logger = logger.For($"Websocket {ID}");
            }

            private void Send(EventKind kind, object? data = null)
            {
                Logger.Verbose("Sending event {Kind}", kind);

                var json = JsonSerializer.Serialize(new Event(kind, data));

                Send(json);
            }

            protected override void OnOpen()
            {
                Logger.Debug("Opened connection");

                Dashboard.Sessions.CollectionChanged += Sessions_CollectionChanged;

                foreach (var item in Dashboard.Sessions)
                {
                    OnStartedSession(item.Session);
                }
            }

            protected override void OnClose(CloseEventArgs e)
            {
                Logger.Debug("Closed connection");
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                Logger.Debug("Received message");
            }

            private void Sessions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    Debug.Assert(e.NewItems != null);

                    foreach (SessionViewModel item in e.NewItems)
                    {
                        OnStartedSession(item.Session);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    Debug.Assert(e.OldItems != null);

                    foreach (SessionViewModel item in e.OldItems)
                    {
                        Send(EventKind.SessionEnd, new { id = item.Session.ID });
                    }
                }
            }

            private static object SerializeSession(ISession session)
            {
                return new
                {
                    id = session.ID,
                    media = session.Media == null ? null : new
                    {
                        title = session.Media.Title,
                        duration = session.Media.Duration.TotalSeconds
                    },
                    app = session.App == null ? null : new
                    {
                        name = session.App.Name,
                        icon = Base64ImageEncoder.Encode(session.App.Icon)
                    },
                    position = session.Position.TotalSeconds,
                    isPlaying = session.IsPlaying
                };
            }

            private void OnStartedSession(ISession session)
            {
                Send(EventKind.SessionStart, SerializeSession(session));

                session.PropertyChanged += Session_PropertyChanged;
            }

            private void Session_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if (sender == null || sender is not ISession session)
                    throw new ArgumentException("Invalid event sender");

                switch (e.PropertyName)
                {
                    case nameof(ISession.Media):
                        
                        break;
                }
            }
        }
    }
}
