using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class ThreadRepository : RepositoryBase<Thread>, IThreadRepository
    {
        public ThreadRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
