using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IFilteredWordService
    {
        IEnumerable<FilteredWord> GetAll();
        FilteredWord GetById(int id);
        (bool success, string message) Add(FilteredWord entity);
        (bool success, string message) Update(FilteredWord entity);
        bool Delete(int id);
        IEnumerable<FilteredWord> Search(string searchTerm);
    }
}
