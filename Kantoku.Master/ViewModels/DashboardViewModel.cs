using Kantoku.Master.Media;
using Kantoku.Master.Media.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Kantoku.Master.ViewModels
{
    public class DashboardViewModel
    {
        public ObservableCollection<SessionViewModel> Sessions { get; set; } = new ObservableCollection<SessionViewModel>();

        public SessionViewModel? Selected { get; set; }

        private readonly IServiceManager ServiceManager;
        private readonly Dispatcher Dispatcher;

        public DashboardViewModel(IServiceManager serviceManager, Dispatcher dispatcher)
        {
            this.ServiceManager = serviceManager;
            this.Dispatcher = dispatcher;
        }

        public async Task Start()
        {
            ServiceManager.SessionStarted += ServiceManager_SessionStarted;

            await ServiceManager.Start();
        }

        private void ServiceManager_SessionStarted(object? sender, ISession e)
        {
            var vm = new SessionViewModel(e);

            Dispatcher.Invoke(() => Sessions.Add(vm));

            e.Closed += () => Dispatcher.Invoke(() => Sessions.Remove(vm));
        }
    }
}
