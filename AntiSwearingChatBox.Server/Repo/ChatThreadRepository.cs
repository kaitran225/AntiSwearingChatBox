using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository
{
    public class ChatThreadRepository : RepositoryBase<ChatThread>, IChatThreadRepository
    {
        public ChatThreadRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
