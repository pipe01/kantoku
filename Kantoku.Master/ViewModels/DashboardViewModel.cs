using Kantoku.Master.Frontend;
using Kantoku.Master.Helpers;
using Kantoku.Master.Media;
using Kantoku.Master.Media.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace Kantoku.Master.ViewModels
{
    public class DashboardViewModel : IHosted
    {
        public ObservableCollection<SessionViewModel> Sessions { get; set; } = new ObservableCollection<SessionViewModel>();
        public ObservableDictionary<IConnection, ConnectionViewModel> Connections { get; } = new ObservableDictionary<IConnection, ConnectionViewModel>();

        public SessionViewModel? Selected { get; set; }

        public IServer Server { get; }

        private readonly IServiceManager ServiceManager;
        private readonly SynchronizationContext SynchronizationContext;

        public DashboardViewModel(IServiceManager serviceManager, SynchronizationContext synchronizationContext, IServer server)
        {
            this.ServiceManager = serviceManager;
            this.SynchronizationContext = synchronizationContext;
            this.Server = server;

            server.Connections.CollectionChanged += Connections_CollectionChanged;
        }

        public async Task Start()
        {
            ServiceManager.SessionStarted += ServiceManager_SessionStarted;

            await ServiceManager.Start();
        }

        private void Connections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SynchronizationContext.Post(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (IConnection item in e.NewItems!)
                    {
                        Connections.Add(item, new ConnectionViewModel(item));
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (IConnection item in e.OldItems!)
                    {
                        Connections.Remove(item);
                    }
                }
            });
        }

        private void ServiceManager_SessionStarted(object? sender, ISession e)
        {
            var vm = new SessionViewModel(e);

            SynchronizationContext.Send(() => Sessions.Add(vm));

            e.Closed += () => SynchronizationContext.Send(() => Sessions.Remove(vm));
        }
    }
}
