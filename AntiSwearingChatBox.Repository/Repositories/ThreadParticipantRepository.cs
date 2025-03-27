using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class ThreadParticipantRepository : RepositoryBase<ThreadParticipant>, IThreadParticipantRepository
    {
        public ThreadParticipantRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
