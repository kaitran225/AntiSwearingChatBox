using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IFilteredWordsService
    {
        IEnumerable<FilteredWords> GetAll();
        FilteredWords GetById(string id);
        (bool success, string message) Add(FilteredWords entity);
        (bool success, string message) Update(FilteredWords entity);
        bool Delete(string id);
        IEnumerable<FilteredWords> Search(string searchTerm);
    }
}
