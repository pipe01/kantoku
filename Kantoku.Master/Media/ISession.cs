using Kantoku.Master.Helpers.Fetchers;
using System;
using System.Threading.Tasks;

namespace Kantoku.Master.Media
{
    public interface ISession : IDisposable
    {
        event Action Closed;
        event Action Updated;

        Guid ID { get; }
        AppInfo App { get; }

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
