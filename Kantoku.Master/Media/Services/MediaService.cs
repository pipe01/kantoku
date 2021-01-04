using Kantoku.Master.Helpers;
using Kantoku.Master.Helpers.Fetchers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Kantoku.Master.Media.Services
{
    using GSMTCSession = GlobalSystemMediaTransportControlsSession;

    public class MediaService : IService
    {
        public event EventHandler<ISession> SessionStarted = delegate { };

        private readonly ILogger Logger;
        private readonly IAppInfoFetcher AppInfoFetcher;
        private readonly IDictionary<GSMTCSession, Session> Sessions;
        private GlobalSystemMediaTransportControlsSessionManager? Manager;

        public MediaService(ILogger logger, IAppInfoFetcher appInfoFetcher)
        {
            this.Logger = logger.For<MediaService>();
            this.AppInfoFetcher = appInfoFetcher;
            this.Sessions = new Dictionary<GSMTCSession, Session>(new SessionComparer());
        }

        public async Task Start()
        {
            Logger.Debug("Requesting session manager");

            Manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            Logger.Debug("Got session manager");

            Manager.SessionsChanged += Manager_SessionsChanged;

            foreach (var item in Manager.GetSessions())
            {
                await StartSession(item);
            }
        }

        private async void Manager_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            Debug.Assert(Manager != null);

            Logger.Verbose("Sessions change detected");

            var sessions = Manager.GetSessions();

            foreach (var ses in sessions)
            {
                if (!Sessions.TryGetValue(ses, out _))
                {
                    Logger.Verbose("Starting session");

                    await StartSession(ses);
                }
            }
        }

        private async Task StartSession(GSMTCSession gsmtcSession)
        {
            var session = await Session.New(gsmtcSession, Logger, AppInfoFetcher);

            Logger.Debug("Started session with ID {ID}, app ID {Model}, hash {Hash}", session.ID, gsmtcSession.SourceAppUserModelId, gsmtcSession.GetHashCode());

            session.Closed += () =>
            {
                Logger.Debug("Removing session ID {ID}", session.ID);
                Sessions.Remove(gsmtcSession);

                session.Dispose();
            };

            Sessions.Add(gsmtcSession, session);
            SessionStarted(this, session);
        }

        private class Session : ISession
        {
            public TimeSpan Position => GSMTCSession.GetTimelineProperties().Position;

            public bool IsPlaying => GSMTCSession.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            public MediaInfo? Media => MediaProperties == null ? null : new MediaInfo(MediaProperties.Title, MediaProperties.Artist);

            public Guid ID { get; }

            public AppInfo App { get; }

            public event Action Closed = delegate { };
            public event Action Updated = delegate { };

            private readonly GSMTCSession GSMTCSession;
            private readonly ILogger Logger;

            private GlobalSystemMediaTransportControlsSessionMediaProperties? MediaProperties;
            private bool IsClosed;

            private Session(GSMTCSession gSMTCSession, ILogger rootLogger, AppInfo app)
            {
                this.GSMTCSession = gSMTCSession;
                this.ID = Guid.NewGuid();
                this.Logger = rootLogger.ForContext("Session", ID);
                this.App = app;

                GSMTCSession.MediaPropertiesChanged += GSMTCSession_MediaPropertiesChanged;
                GSMTCSession.PlaybackInfoChanged += GSMTCSession_PlaybackInfoChanged;
            }

            public static async Task<Session> New(GSMTCSession gsmtcSession, ILogger rootLogger, IAppInfoFetcher appInfoFetcher)
            {
                var appInfo = await appInfoFetcher.FetchInfo(gsmtcSession.SourceAppUserModelId);

                var session = new Session(gsmtcSession, rootLogger, appInfo);
                await session.LoadInfo();

                return session;
            }

            private async void GSMTCSession_MediaPropertiesChanged(GSMTCSession sender, MediaPropertiesChangedEventArgs args)
            {
                Logger.Verbose("Media properties changed");

                await LoadInfo();
            }

            private void GSMTCSession_PlaybackInfoChanged(GSMTCSession sender, PlaybackInfoChangedEventArgs args)
            {
                Logger.Verbose("Playback info changed");

                if (GSMTCSession.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed && !IsClosed)
                {
                    Logger.Debug("Closing session");

                    IsClosed = true;
                    Closed();
                }
            }

            public void Dispose()
            {
                GSMTCSession.MediaPropertiesChanged -= GSMTCSession_MediaPropertiesChanged;
                GSMTCSession.PlaybackInfoChanged -= GSMTCSession_PlaybackInfoChanged;
            }

            public async Task LoadInfo()
            {
                Logger.Verbose("Reloading media properties");

                MediaProperties = await GSMTCSession.TryGetMediaPropertiesAsync();
                Updated();
            }

            public async Task Pause()
            {
                Logger.Debug("Trying to pause");

                Logger.Verbose("Success: {Success}", await GSMTCSession.TryPauseAsync());
            }

            public async Task Play()
            {
                Logger.Debug("Trying to play");

                Logger.Verbose("Success: {Success}", await GSMTCSession.TryPlayAsync());
            }

            public async Task Stop()
            {
                Logger.Debug("Trying to stop");

                Logger.Verbose("Success: {Success}", await GSMTCSession.TryStopAsync());
            }

            public async Task Previous()
            {
                Logger.Debug("Trying to skip previous");

                Logger.Verbose("Success: {Success}", await GSMTCSession.TrySkipPreviousAsync());
            }

            public async Task Next()
            {
                Logger.Debug("Trying to skip next");

                Logger.Verbose("Success: {Success}", await GSMTCSession.TrySkipNextAsync());
            }
        }
    }
}
