using AntiSwearingChatBox.Repository.Models;
using System.Collections.Generic;

namespace AntiSwearingChatBox.Service.Interface
{
    public interface IChatThreadService
    {
        IEnumerable<ChatThread> GetAll();
        ChatThread GetById(int id);
        (bool success, string message) Add(ChatThread entity);
        (bool success, string message) Update(ChatThread entity);
        bool Delete(int id);
        IEnumerable<ChatThread> GetUserThreads(int userId);
    }
}
