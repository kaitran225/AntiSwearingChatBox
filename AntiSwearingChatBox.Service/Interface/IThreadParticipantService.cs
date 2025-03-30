using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IThreadParticipantService
    {
        IEnumerable<ThreadParticipant> GetAll();
        ThreadParticipant GetById(string id);
        IEnumerable<ThreadParticipant> GetByThreadId(int threadId);
        IEnumerable<ThreadParticipant> GetByUserId(int userId);
        (bool success, string message) Add(ThreadParticipant entity);
        (bool success, string message) Update(ThreadParticipant entity);
        bool Delete(string id);
        bool RemoveUserFromThread(int userId, int threadId);
        IEnumerable<ThreadParticipant> Search(string searchTerm);
    }
}
