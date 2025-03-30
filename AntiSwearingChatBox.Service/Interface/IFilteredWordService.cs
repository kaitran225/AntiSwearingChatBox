using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IFilteredWordService
    {
        IEnumerable<FilteredWord> GetAll();
        FilteredWord GetById(string id);
        (bool success, string message) Add(FilteredWord entity);
        (bool success, string message) Update(FilteredWord entity);
        bool Delete(string id);
        IEnumerable<FilteredWord> Search(string searchTerm);
    }
}
