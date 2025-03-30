using Repository.Models;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Repositories
{
    public class UserWarningRepository : RepositoryBase<UserWarning>, IUserWarningRepository
    {
        public UserWarningRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
