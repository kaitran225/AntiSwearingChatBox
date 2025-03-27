using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IThreadService
    {
        IEnumerable<Thread> GetAll();
        Thread GetById(string id);
        (bool success, string message) Add(Thread entity);
        (bool success, string message) Update(Thread entity);
        bool Delete(string id);
        IEnumerable<Thread> Search(string searchTerm);
    }
}
