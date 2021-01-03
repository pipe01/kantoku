using Kantoku.Master.Media;
using System.ComponentModel;

namespace Kantoku.Master.ViewModels
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public string FullName => Session.Media == null ? "Loading..." : $"{Session.Media.Value.Author} - {Session.Media.Value.Title}";

        private readonly ISession Session;

        public SessionViewModel(ISession session)
        {
            this.Session = session;

            session.Updated += () => PropertyChanged(this, new PropertyChangedEventArgs(nameof(FullName)));
        }
    }
}
