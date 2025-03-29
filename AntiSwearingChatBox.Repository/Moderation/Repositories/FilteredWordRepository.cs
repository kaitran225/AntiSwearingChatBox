using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class FilteredWordRepository : RepositoryBase<FilteredWord>, IFilteredWordRepository
    {
        public FilteredWordRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
