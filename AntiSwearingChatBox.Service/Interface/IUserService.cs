using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User GetById(int id);
        User GetByUsername(string username);
        (bool success, string message, User? user) Authenticate(string username, string password);
        (bool success, string message) Register(User user, string password);
        (bool success, string message) Update(User entity);
        bool Delete(int id);
    }
}
