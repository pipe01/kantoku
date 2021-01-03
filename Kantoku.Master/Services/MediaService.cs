using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantoku.Master.Services
{
    public class MediaService : IService
    {
        public Task Start()
        {
            return Task.CompletedTask;
        }
    }
}
