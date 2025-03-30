using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Repository
{
    public class FilteredWordRepository : RepositoryBase<FilteredWord>, IFilteredWordRepository
    {
        public FilteredWordRepository(AntiSwearingChatBoxContext context) : base(context)
        {
        }
    }
}
