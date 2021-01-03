using Kantoku.Master.Media.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Kantoku.Master.Media.Services
{
    public class MediaService : IService
    {
        public event EventHandler<ISession> SessionStarted = delegate { };

        private readonly ILogger Logger;
        private readonly IDictionary<GlobalSystemMediaTransportControlsSession, Session> Sessions = new Dictionary<GlobalSystemMediaTransportControlsSession, Session>();
        private GlobalSystemMediaTransportControlsSessionManager Manager;

        public MediaService(ILogger logger)
        {
            this.Logger = logger.ForContext<MediaService>();
        }

        public async Task Start()
        {
            Logger.Debug("Requesting session manager");

            Manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();

            Logger.Debug("Got session manager");

            Manager.SessionsChanged += Manager_SessionsChanged;

            foreach (var item in Manager.GetSessions())
            {
                StartSession(item);
            }
        }

        private void Manager_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            Logger.Verbose("Sessions change detected");

            var sessions = Manager.GetSessions();

            foreach (var ses in sessions)
            {
                if (!Sessions.TryGetValue(ses, out _))
                {
                    Logger.Verbose("Starting session");

                    StartSession(ses);
                }
            }

            foreach (var ses in Sessions)
            {
                if (!sessions.Contains(ses.Key))
                {
                    Logger.Verbose("Stopping session ID {ID}", ses.Value.ID);

                    ses.Value.Dispose();
                    Sessions.Remove(ses.Key);
                }
            }
        }

        private void StartSession(GlobalSystemMediaTransportControlsSession gsmtcSession)
        {
            var session = new Session(gsmtcSession, Logger);

            Logger.Debug("Started session with ID {ID}", session.ID);

            Sessions.Add(gsmtcSession, session);
            SessionStarted(this, session);
        }

        private class Session : ISession
        {
            public TimeSpan Position => GSMTCSession.GetTimelineProperties().Position;

            public bool IsPlaying => GSMTCSession.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            public MediaInfo? Media => !IsPlaying || MediaProperties == null ? null : new MediaInfo(MediaProperties.Title, MediaProperties.Artist);

            public Guid ID { get; }

            public event Action Closed = delegate { };

            private readonly GlobalSystemMediaTransportControlsSession GSMTCSession;
            private readonly ILogger Logger;
            private GlobalSystemMediaTransportControlsSessionMediaProperties? MediaProperties;

            public Session(GlobalSystemMediaTransportControlsSession gSMTCSession, ILogger rootLogger)
            {
                this.GSMTCSession = gSMTCSession;
                this.ID = Guid.NewGuid();
                this.Logger = rootLogger.ForContext("Session", ID);

                GSMTCSession.MediaPropertiesChanged += GSMTCSession_MediaPropertiesChanged;
                GSMTCSession.PlaybackInfoChanged += GSMTCSession_PlaybackInfoChanged;
            }

            private async void GSMTCSession_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
            {
                Logger.Verbose("Media properties changed");

                await LoadInfo();
            }

            private void GSMTCSession_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
            {
                Logger.Verbose("Playback info changed");

                if (GSMTCSession.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed)
                {
                    Logger.Debug("Closing session");

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
