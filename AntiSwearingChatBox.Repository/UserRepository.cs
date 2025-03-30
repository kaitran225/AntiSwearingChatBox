using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
