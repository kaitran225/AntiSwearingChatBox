using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User GetById(string id);
        (bool success, string message) Add(User entity);
        (bool success, string message) Update(User entity);
        bool Delete(string id);
        IEnumerable<User> Search(string searchTerm);
    }
}
