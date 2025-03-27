using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class ThreadService : IThreadService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ThreadService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Thread> GetAll()
        {
            return _unitOfWork.Thread.GetAll();
        }

        public Thread GetById(string id)
        {
            return _unitOfWork.Thread.GetById(id);
        }

        public (bool success, string message) Add(Thread entity)
        {
            try
            {
                _unitOfWork.Thread.Add(entity);
                _unitOfWork.Complete();
                return (true, "Thread added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding Thread: {ex.Message}");
            }
        }

        public (bool success, string message) Update(Thread entity)
        {
            try
            {
                _unitOfWork.Thread.Update(entity);
                _unitOfWork.Complete();
                return (true, "Thread updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating Thread: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.Thread.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.Thread.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<Thread> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.Thread.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
