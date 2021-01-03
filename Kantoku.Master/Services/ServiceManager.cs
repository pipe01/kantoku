using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kantoku.Master.Services
{
    public interface IServiceManager
    {
        Task Start();
    }

    public class ServiceManager : IServiceManager
    {
        private readonly IReadOnlyCollection<IService> Services;
        private readonly ILogger Logger;

        public ServiceManager(IReadOnlyCollection<IService> services, ILogger logger)
        {
            this.Services = services;
            this.Logger = logger.ForContext<ServiceManager>();
        }

        public async Task Start()
        {
            Logger.Debug("Starting manager with {ProviderCount} services", Services.Count);

            foreach (var svc in Services)
            {
                Logger.Debug("Starting service {Name}", svc.GetType().Name);

                await svc.Start();
            }
        }
    }
}
