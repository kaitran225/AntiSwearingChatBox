using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class ChatThreadService : IChatThreadService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatThreadService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ChatThread> GetAll()
        {
            return _unitOfWork.ChatThread.GetAll();
        }

        public ChatThread GetById(int id)
        {
            return _unitOfWork.ChatThread.GetById(id);
        }

        public (bool success, string message) Add(ChatThread entity)
        {
            try
            {
                _unitOfWork.ChatThread.Add(entity);
                _unitOfWork.Complete();
                return (true, "Chat thread added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding chat thread: {ex.Message}");
            }
        }

        public (bool success, string message) Update(ChatThread entity)
        {
            try
            {
                _unitOfWork.ChatThread.Update(entity);
                _unitOfWork.Complete();
                return (true, "Chat thread updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating chat thread: {ex.Message}");
            }
        }

        public bool Delete(int id)
        {
            var entity = _unitOfWork.ChatThread.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.ChatThread.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<ChatThread> GetUserThreads(int userId)
        {
            // Get all thread participants for this user
            var threadParticipants = _unitOfWork.ThreadParticipant.Find(tp => tp.UserId == userId);
            
            // Get threads for each participant entry
            var threads = new List<ChatThread>();
            foreach (var participant in threadParticipants)
            {
                var thread = _unitOfWork.ChatThread.GetById(participant.ThreadId);
                if (thread != null)
                {
                    threads.Add(thread);
                }
            }
            
            return threads.OrderByDescending(t => t.LastMessageAt);
        }
    }
}
