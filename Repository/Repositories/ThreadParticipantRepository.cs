using Repository.Models;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Repositories
{
    public class ThreadParticipantRepository : RepositoryBase<ThreadParticipant>, IThreadParticipantRepository
    {
        public ThreadParticipantRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
