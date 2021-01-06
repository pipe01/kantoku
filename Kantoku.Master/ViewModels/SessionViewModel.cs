using Kantoku.Master.Helpers;
using Kantoku.Master.Media;
using System.ComponentModel;
using System.Windows.Input;

namespace Kantoku.Master.ViewModels
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public string FullName => Session.Media == null ? "Loading..." : $"{Session.Media.Author} - {Session.Media.Title}";

        public ISession Session { get; }

        public ICommand Pause { get; }
        public ICommand Play { get; }
        public ICommand Stop { get; }
        public ICommand Previous { get; }
        public ICommand Next { get; }

        public SessionViewModel(ISession session)
        {
            this.Session = session;

            Session.PropertyChanged += Session_PropertyChanged;

            Pause = new ActionCommand(() => Session.Pause());
            Play = new ActionCommand(() => Session.Play());
            Stop = new ActionCommand(() => Session.Stop());
            Previous = new ActionCommand(() => Session.Previous());
            Next = new ActionCommand(() => Session.Next());
        }

        private void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISession.Media))
                OnPropertyChanged(nameof(FullName));
        }
    }
}
