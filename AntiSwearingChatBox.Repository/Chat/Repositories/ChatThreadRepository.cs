using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class ChatThreadRepository : RepositoryBase<ChatThread>, IChatThreadRepository
    {
        public ChatThreadRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
