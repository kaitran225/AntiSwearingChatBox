using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IThreadsService
    {
        IEnumerable<Threads> GetAll();
        Threads GetById(string id);
        (bool success, string message) Add(Threads entity);
        (bool success, string message) Update(Threads entity);
        bool Delete(string id);
        IEnumerable<Threads> Search(string searchTerm);
    }
}
