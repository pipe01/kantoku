using Kantoku.Master.Helpers;
using Kantoku.Master.Media;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kantoku.Master.ViewModels
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public string FullName => Session.Media == null ? "Loading..." : $"{Session.Media.Value.Author} - {Session.Media.Value.Title}";

        public ISession Session { get; }

        public ICommand Pause { get; }
        public ICommand Play { get; }
        public ICommand Stop { get; }
        public ICommand Previous { get; }
        public ICommand Next { get; }

        public SessionViewModel(ISession session)
        {
            this.Session = session;

            session.Updated += () => PropertyChanged(this, new PropertyChangedEventArgs(nameof(FullName)));

            Pause = new ActionCommand(() => Session.Pause());
            Play = new ActionCommand(() => Session.Play());
            Stop = new ActionCommand(() => Session.Stop());
            Previous = new ActionCommand(() => Session.Previous());
            Next = new ActionCommand(() => Session.Next());
        }
    }
}
