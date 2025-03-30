using Repository.Models;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Repositories
{
    public class ChatThreadRepository : RepositoryBase<ChatThread>, IChatThreadRepository
    {
        public ChatThreadRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
