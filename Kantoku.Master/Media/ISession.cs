using Kantoku.Master.Helpers.Fetchers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Kantoku.Master.Media
{
    public interface ISession : IDisposable, INotifyPropertyChanged
    {
        event Action Closed;

        Guid ID { get; }
        AppInfo? App { get; }

        TimeSpan Position { get; }
        bool IsPlaying { get; }
        MediaInfo? Media { get; }

        Task Pause();
        Task Play();
        Task Stop();

        Task SetPosition(TimeSpan time);
        Task Previous();
        Task Next();
    }
}
