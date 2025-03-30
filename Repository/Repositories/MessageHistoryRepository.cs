using Repository.Models;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Repositories
{
    public class MessageHistoryRepository : RepositoryBase<MessageHistory>, IMessageHistoryRepository
    {
        public MessageHistoryRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
