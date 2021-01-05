using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kantoku.Master.Media.Services
{
    public interface IServiceManager
    {
        IReadOnlyDictionary<Guid, ISession> Sessions { get; }

        event EventHandler<ISession> SessionStarted;

        Task Start();
    }

    public class ServiceManager : IServiceManager
    {
        public IReadOnlyDictionary<Guid, ISession> Sessions => SessionsInner;
        private readonly Dictionary<Guid, ISession> SessionsInner = new Dictionary<Guid, ISession>();

        private readonly IReadOnlyCollection<IService> Services;
        private readonly ILogger Logger;
        private readonly SynchronizationContext SynchronizationContext;

        public event EventHandler<ISession> SessionStarted = delegate { };

        public ServiceManager(IReadOnlyCollection<IService> services, ILogger logger, SynchronizationContext synchronizationContext)
        {
            this.Services = services;
            this.Logger = logger.For<ServiceManager>();
            this.SynchronizationContext = synchronizationContext;
        }

        public async Task Start()
        {
            Logger.Debug("Starting manager with {ProviderCount} services", Services.Count);

            foreach (var svc in Services)
            {
                Logger.Debug("Starting service {Name}", svc.GetType().Name);

                svc.SessionStarted += Svc_SessionStarted;

                await svc.Start();
            }
        }

        private void Svc_SessionStarted(object? sender, ISession session)
        {
            Logger.Debug("Started session ID {ID}", session.ID);

            SessionsInner.Add(session.ID, session);

            SynchronizationContext.Post(() => SessionStarted(this, session));
        }
    }
}
