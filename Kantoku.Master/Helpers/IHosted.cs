using System.Threading.Tasks;

namespace Kantoku.Master.Helpers
{
    public interface IHosted
    {
        Task Start();
    }
}
