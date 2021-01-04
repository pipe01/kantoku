using Kantoku.Master.Media;
using System.ComponentModel;
using System.Windows.Threading;

namespace Kantoku.Master.ViewModels
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public string FullName => Session.Media == null ? "Loading..." : $"{Session.Media.Value.Author} - {Session.Media.Value.Title}";

        public ISession Session { get; }

        public SessionViewModel(ISession session)
        {
            this.Session = session;

            session.Updated += () => PropertyChanged(this, new PropertyChangedEventArgs(nameof(FullName)));
        }
    }
}
