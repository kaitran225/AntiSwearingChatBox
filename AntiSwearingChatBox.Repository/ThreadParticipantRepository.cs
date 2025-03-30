using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository
{
    public class ThreadParticipantRepository : RepositoryBase<ThreadParticipant>, IThreadParticipantRepository
    {
        public ThreadParticipantRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
