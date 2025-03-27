using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class ThreadParticipantsRepository : RepositoryBase<ThreadParticipants>, IThreadParticipantsRepository
    {
        public ThreadParticipantsRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
