using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Server.Repo.Models;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository
{
    public class UserWarningRepository : RepositoryBase<UserWarning>, IUserWarningRepository
    {
        public UserWarningRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
