using Kantoku.Master.Frontend;
using Kantoku.Master.Helpers;
using Kantoku.Master.Media;
using Kantoku.Master.Media.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Kantoku.Master.ViewModels
{
    public class DashboardViewModel : IHosted
    {
        public ObservableCollection<SessionViewModel> Sessions { get; set; } = new ObservableCollection<SessionViewModel>();

        public SessionViewModel? Selected { get; set; }

        public IConnectionCounter Connections { get; }

        private readonly IServiceManager ServiceManager;
        private readonly SynchronizationContext SynchronizationContext;

        public DashboardViewModel(IServiceManager serviceManager, SynchronizationContext synchronizationContext, IConnectionCounter connections)
        {
            this.ServiceManager = serviceManager;
            this.SynchronizationContext = synchronizationContext;
            this.Connections = connections;
        }

        public async Task Start()
        {
            ServiceManager.SessionStarted += ServiceManager_SessionStarted;

            await ServiceManager.Start();
        }

        private void ServiceManager_SessionStarted(object? sender, ISession e)
        {
            var vm = new SessionViewModel(e);

            SynchronizationContext.Send(() => Sessions.Add(vm));

            e.Closed += () => SynchronizationContext.Send(() => Sessions.Remove(vm));
        }
    }
}
