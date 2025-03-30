using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IMessageHistoryService
    {
        IEnumerable<MessageHistory> GetAll();
        MessageHistory GetById(int id);
        IEnumerable<MessageHistory> GetByThreadId(int threadId);
        IEnumerable<MessageHistory> GetLatestMessages(int count = 20);
        (bool success, string message) Add(MessageHistory entity);
        (bool success, string message) Update(MessageHistory entity);
        bool Delete(int id);
        IEnumerable<MessageHistory> Search(string searchTerm);
    }
}
