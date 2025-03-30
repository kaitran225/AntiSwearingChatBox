using Repository.Models;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
