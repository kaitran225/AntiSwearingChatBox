using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class MessageHistoryRepository : RepositoryBase<MessageHistory>, IMessageHistoryRepository
    {
        public MessageHistoryRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
