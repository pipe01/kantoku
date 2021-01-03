using System;
using System.Threading.Tasks;

namespace Kantoku.Master.Media.Services
{
    public interface IService
    {
        event EventHandler<ISession> SessionStarted;

        Task Start();
    }
}
