using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
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

        public ChatThread GetById(string id)
        {
            return _unitOfWork.ChatThread.GetById(id);
        }

        public (bool success, string message) Add(ChatThread entity)
        {
            try
            {
                _unitOfWork.ChatThread.Add(entity);
                _unitOfWork.Complete();
                return (true, "ChatThread added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding ChatThread: {ex.Message}");
            }
        }

        public (bool success, string message) Update(ChatThread entity)
        {
            try
            {
                _unitOfWork.ChatThread.Update(entity);
                _unitOfWork.Complete();
                return (true, "ChatThread updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating ChatThread: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.ChatThread.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.ChatThread.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<ChatThread> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.ChatThread.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
