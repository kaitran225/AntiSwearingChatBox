using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interfaces
{
    public interface IChatThreadService
    {
        IEnumerable<ChatThread> GetAll();
        ChatThread GetById(string id);
        (bool success, string message) Add(ChatThread entity);
        (bool success, string message) Update(ChatThread entity);
        bool Delete(string id);
        IEnumerable<ChatThread> Search(string searchTerm);
    }
}
