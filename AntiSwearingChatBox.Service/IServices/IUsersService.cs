using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IUsersService
    {
        IEnumerable<Users> GetAll();
        Users GetById(string id);
        (bool success, string message) Add(Users entity);
        (bool success, string message) Update(Users entity);
        bool Delete(string id);
        IEnumerable<Users> Search(string searchTerm);
    }
}
