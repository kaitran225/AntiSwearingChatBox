using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class UsersRepository : RepositoryBase<Users>, IUsersRepository
    {
        public UsersRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
