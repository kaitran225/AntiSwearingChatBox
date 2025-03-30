using AntiSwearingChatBox.Server.Repo.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IUserWarningService
    {
        IEnumerable<UserWarning> GetAll();
        UserWarning GetById(int id);
        (bool success, string message) Add(UserWarning entity);
        (bool success, string message) Update(UserWarning entity);
        bool Delete(int id);
        IEnumerable<UserWarning> Search(string searchTerm);
    }
}
