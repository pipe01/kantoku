using Kantoku.Master.Helpers;
using Kantoku.Master.Media;
using Kantoku.Master.Media.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kantoku.Master.ViewModels
{
    public class DashboardViewModel
    {
        public ObservableCollection<SessionViewModel> Sessions { get; set; } = new ObservableCollection<SessionViewModel>();

        public ICommand Pause { get; }
        public ICommand Play { get; }
        public ICommand Stop { get; }
        public ICommand Previous { get; }
        public ICommand Next { get; }

        public SessionViewModel? Selected { get; set; }

        private readonly IServiceManager ServiceManager;
        private readonly Dispatcher Dispatcher;

        public DashboardViewModel(IServiceManager serviceManager, Dispatcher dispatcher)
        {
            this.ServiceManager = serviceManager;
            this.Dispatcher = dispatcher;

            Pause = new ActionCommand(() => Selected?.Session.Pause());
            Play = new ActionCommand(() => Selected?.Session.Play());
            Stop = new ActionCommand(() => Selected?.Session.Stop());
            Previous = new ActionCommand(() => Selected?.Session.Previous());
            Next = new ActionCommand(() => Selected?.Session.Next());
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
