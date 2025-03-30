using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class MessageHistoryService : IMessageHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessageHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<MessageHistory> GetAll()
        {
            return _unitOfWork.MessageHistory.GetAll();
        }

        public MessageHistory GetById(int id)
        {
            return _unitOfWork.MessageHistory.GetById(id);
        }

        public IEnumerable<MessageHistory> GetByThreadId(int threadId)
        {
            return _unitOfWork.MessageHistory.Find(m => m.ThreadId == threadId)
                .OrderBy(m => m.CreatedAt);
        }

        public IEnumerable<MessageHistory> GetLatestMessages(int count = 20)
        {
            return _unitOfWork.MessageHistory.GetAll()
                .OrderByDescending(m => m.CreatedAt)
                .Take(count);
        }

        public (bool success, string message) Add(MessageHistory entity)
        {
            try
            {
                _unitOfWork.MessageHistory.Add(entity);
                _unitOfWork.Complete();
                return (true, "MessageHistory added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding MessageHistory: {ex.Message}");
            }
        }

        public (bool success, string message) Update(MessageHistory entity)
        {
            try
            {
                _unitOfWork.MessageHistory.Update(entity);
                _unitOfWork.Complete();
                return (true, "MessageHistory updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating MessageHistory: {ex.Message}");
            }
        }

        public bool Delete(int id)
        {
            var entity = _unitOfWork.MessageHistory.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.MessageHistory.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<MessageHistory> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.MessageHistory.Find(x => 
                (x.OriginalMessage != null && x.OriginalMessage.ToLower().Contains(searchTerm.ToLower())) ||
                (x.ModeratedMessage != null && x.ModeratedMessage.ToLower().Contains(searchTerm.ToLower())));
        }
    }
}
