using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IThreadParticipantsService
    {
        IEnumerable<ThreadParticipants> GetAll();
        ThreadParticipants GetById(string id);
        (bool success, string message) Add(ThreadParticipants entity);
        (bool success, string message) Update(ThreadParticipants entity);
        bool Delete(string id);
        IEnumerable<ThreadParticipants> Search(string searchTerm);
    }
}
