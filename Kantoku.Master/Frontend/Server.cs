﻿using Kantoku.Master.Helpers;
using Kantoku.Master.Media;
using Kantoku.Master.Media.Services;
using Kantoku.Master.ViewModels;
using LightInject;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Kantoku.Master.Frontend
{
    public interface IServer : IHosted
    {
        ObservableCollection<IConnection> Connections { get; }
    }

    public class Server : IServer
    {
        public ObservableCollection<IConnection> Connections { get; } = new ObservableCollection<IConnection>();

        private readonly ILogger Logger;
        private readonly HttpListener Listener;
        private readonly IServiceContainer Container;
        private readonly Config Config;

        public Server(ILogger<Server> logger, IServiceContainer container, INetwork network, Config config)
        {
            this.Logger = logger;
            this.Container = container;
            this.Listener = new HttpListener();
            this.Config = config;

            Container.Register<Behaviour>();
            Listener.Prefixes.Add($"http://0.0.0.0:{config.RemotePort}/");

            network.BroadcastDiscovery();
        }

        public Task Start()
        {
            Listener.Start();
            new Thread(AcceptLoop)
            {
                IsBackground = true,
                Name = "Websockets thread"
            }.Start();

            Logger.Debug("Listening for remotes on {Port}", Config.RemotePort);

            return Task.CompletedTask;
        }

        private void AcceptLoop()
        {
            var logger = Logger.For("HTTP accept loop");
            var sessions = new WebSocketSessionManager(new Logger());
            sessions.Start();

            while (true)
            {
                try
                {
                    var ctx = Listener.GetContext();

                    switch (ctx.Request.Url.AbsolutePath)
                    {
                        case "/ws":
                            logger.Debug("Requested websocket connection: {Query}", ctx.Request.Url.Query);

                            var ws = ctx.AcceptWebSocket(null);
                            var handler = Container.GetInstance<Behaviour>();
                            Connections.Add(handler);
                            handler.Closed += () => Connections.Remove(handler);

                            handler.Start(ws, sessions);
                            break;

                        case "/info":
                            logger.Debug("Requested info");

                            var data = JsonSerializer.SerializeToUtf8Bytes(new
                            {
                                hostName = Environment.MachineName
                            });

                            ctx.Response.ContentType = "application/json";
                            ctx.Response.ContentLength64 = data.Length;
                            ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");

                            ctx.Response.OutputStream.Write(data);
                            ctx.Response.OutputStream.Close();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception on loop");
                }
            }
        }

        private class Behaviour : WebSocketBehavior, IConnection, INotifyPropertyChanged
        {
            public event Action Closed = delegate { };
            public event PropertyChangedEventHandler? PropertyChanged;

            private ILogger Logger;

            public string? Name { get; private set; }
            string IConnection.Name => Name ?? "Unknown";

            private readonly DashboardViewModel Dashboard;
            private readonly IServiceManager ServiceManager;

            public Behaviour(DashboardViewModel dashboard, ILogger logger, IServiceManager serviceManager)
            {
                this.Dashboard = dashboard;
                this.Logger = logger;
                this.ServiceManager = serviceManager;
            }

            private void Send(EventKind kind, object? data = null)
            {
                if (this.State != WebSocketState.Open)
                    return;

                if (kind != EventKind.SessionUpdate)
                    Logger.Verbose("Sending event {Kind}", kind);

                var json = JsonSerializer.Serialize(new Event(kind, data));

                Send(json);
            }

            public void Close()
            {
                Context.WebSocket.Close();
            }

            protected override void OnOpen()
            {
                Logger = Logger.For($"Websocket {ID}");

                this.Name = Context.QueryString["name"];

                Logger.Debug("Opened connection. name: {Name}", Name);

                Dashboard.Sessions.CollectionChanged += Sessions_CollectionChanged;

                foreach (var item in Dashboard.Sessions)
                {
                    OnStartedSession(item.Session);
                }
            }

            protected override void OnClose(CloseEventArgs e)
            {
                Logger.Debug("Closed connection");

                Dashboard.Sessions.CollectionChanged -= Sessions_CollectionChanged;
                Closed();
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                Logger.Debug("Received message");

                var ev = JsonSerializer.Deserialize<Event>(e.Data);
                if (ev?.Session == null)
                    return;

                if (!Guid.TryParse(ev.Session, out var id))
                    return;

                if (!ServiceManager.Sessions.TryGetValue(id, out var session))
                    return;

                switch (ev.Kind)
                {
                    case EventKind.Pause:
                        session.Pause();
                        break;
                    case EventKind.Play:
                        session.Play();
                        break;
                    case EventKind.Stop:
                        session.Stop();
                        break;
                    case EventKind.Previous:
                        session.Previous();
                        break;
                    case EventKind.Next:
                        session.Next();
                        break;
                    case EventKind.SetPosition when ev.Data is JsonElement el && el.TryGetDouble(out var pos):
                        session.SetPosition(TimeSpan.FromSeconds(pos));
                        break;
                }
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
                        Send(EventKind.SessionEnd, item.Session.ID);
                    }
                }
            }

            private void OnStartedSession(ISession session)
            {
                Send(EventKind.SessionStart, SerializeSession(session));

                MediaInfo? oldMedia = null;

                session.PropertyChanged += Session_PropertyChanged;
                session.Closed += Session_Closed;
                this.Closed += Session_Closed;
                SubscribeMedia(session);

                void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
                {
                    Send(EventKind.SessionUpdate, SerializeSession(session, false));

                    if (e.PropertyName == nameof(ISession.Media))
                        SubscribeMedia(session);
                }

                void Session_Closed()
                {
                    this.Closed -= Session_Closed;

                    session.PropertyChanged -= Session_PropertyChanged;
                    session.Closed -= Session_Closed;

                    if (session.Media != null)
                        session.Media.PropertyChanged -= Media_PropertyChanged;
                }

                void SubscribeMedia(ISession session)
                {
                    if (oldMedia != null)
                        oldMedia.PropertyChanged -= Media_PropertyChanged;
                    oldMedia = session.Media;

                    if (session.Media != null)
                        session.Media.PropertyChanged += Media_PropertyChanged;
                }

                void Media_PropertyChanged(object? sender, PropertyChangedEventArgs e)
                {
                    Send(EventKind.SessionUpdate, new
                    {
                        id = session.ID,
                        media = session.Media == null ? null : SerializeMedia(session.Media)
                    });
                }
            }

            #region Serialization
            private static object SerializeSession(ISession session, bool includeIcon = true)
            {
                return new
                {
                    id = session.ID,
                    media = session.Media == null ? null : SerializeMedia(session.Media),
                    app = session.App == null ? null : new
                    {
                        name = session.App.Name,
                        icon = !includeIcon ? null : "data:image/png;base64, " + Base64ImageEncoder.Encode(session.App.Icon)
                    },
                    position = session.Position.TotalSeconds,
                    isPlaying = session.IsPlaying
                };
            }

            private static object SerializeMedia(MediaInfo media)
            {
                return new
                {
                    title = media.Title,
                    author = media.Author,
                    duration = media.Duration.TotalSeconds
                };
            }
            #endregion
        }
    }
}
