using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IMessageHistoryService
    {
        IEnumerable<MessageHistory> GetAll();
        MessageHistory GetById(string id);
        (bool success, string message) Add(MessageHistory entity);
        (bool success, string message) Update(MessageHistory entity);
        bool Delete(string id);
        IEnumerable<MessageHistory> Search(string searchTerm);
    }
}
