using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository
{
    public class MessageHistoryRepository : RepositoryBase<MessageHistory>, IMessageHistoryRepository
    {
        public MessageHistoryRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
