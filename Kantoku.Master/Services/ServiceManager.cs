using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantoku.Master.Services
{
    public interface IServiceManager
    {
        Task Start();
    }

    public class ServiceManager : IServiceManager
    {
        private readonly IReadOnlyCollection<IServiceProvider> Providers;
        private readonly ILogger Logger;

        public ServiceManager(IReadOnlyCollection<IServiceProvider> providers, ILogger logger)
        {
            this.Providers = providers;
            this.Logger = logger.ForContext<ServiceManager>();
        }

        public async Task Start()
        {
            Logger.Debug("Starting manager with {ProviderCount} providers", Providers.Count);

            foreach (var provider in Providers)
            {
                await provider.Start();
            }
        }
    }
}
