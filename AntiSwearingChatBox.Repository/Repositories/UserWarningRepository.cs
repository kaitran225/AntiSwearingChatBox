using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class UserWarningRepository : RepositoryBase<UserWarning>, IUserWarningRepository
    {
        public UserWarningRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
