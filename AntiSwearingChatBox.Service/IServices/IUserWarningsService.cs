using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IUserWarningsService
    {
        IEnumerable<UserWarnings> GetAll();
        UserWarnings GetById(string id);
        (bool success, string message) Add(UserWarnings entity);
        (bool success, string message) Update(UserWarnings entity);
        bool Delete(string id);
        IEnumerable<UserWarnings> Search(string searchTerm);
    }
}
