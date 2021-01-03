using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Kantoku.Master.Media
{
    public interface ISession : IDisposable
    {
        event Action Closed;
        event Action Updated;

        Guid ID { get; }
        ImageSource Icon { get; }

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
