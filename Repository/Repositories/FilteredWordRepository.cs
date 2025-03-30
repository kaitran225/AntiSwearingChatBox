using Repository.Models;
using Repository.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace Repository.Repositories
{
    public class FilteredWordRepository : RepositoryBase<FilteredWord>, IFilteredWordRepository
    {
        public FilteredWordRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
