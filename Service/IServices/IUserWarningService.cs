using Repository.Models;
using System.Collections.Generic;

namespace Service.Interfaces
{
    public interface IUserWarningService
    {
        IEnumerable<UserWarning> GetAll();
        UserWarning GetById(string id);
        (bool success, string message) Add(UserWarning entity);
        (bool success, string message) Update(UserWarning entity);
        bool Delete(string id);
        IEnumerable<UserWarning> Search(string searchTerm);
    }
}
