using Kantoku.Master.Helpers.Fetchers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Kantoku.Master.Media
{
    public delegate void SessionEventHandler(ISession session);

    public interface ISession : IDisposable, INotifyPropertyChanged
    {
        event SessionEventHandler Closed;

        Guid ID { get; }
        AppInfo? App { get; }

        TimeSpan Position { get; }
        bool IsPlaying { get; }
        MediaInfo? Media { get; }

        Task Pause();
        Task Play();
        Task Stop();

        Task Previous();
        Task Next();
    }
}
