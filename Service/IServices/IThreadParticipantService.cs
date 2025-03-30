using Repository.Models;
using System.Collections.Generic;

namespace Service.Interfaces
{
    public interface IThreadParticipantService
    {
        IEnumerable<ThreadParticipant> GetAll();
        ThreadParticipant GetById(string id);
        (bool success, string message) Add(ThreadParticipant entity);
        (bool success, string message) Update(ThreadParticipant entity);
        bool Delete(string id);
        IEnumerable<ThreadParticipant> Search(string searchTerm);
    }
}
