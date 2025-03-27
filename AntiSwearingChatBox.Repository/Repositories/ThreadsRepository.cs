using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class ThreadsRepository : RepositoryBase<Threads>, IThreadsRepository
    {
        public ThreadsRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
