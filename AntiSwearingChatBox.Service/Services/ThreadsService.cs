using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class ThreadsService : IThreadsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ThreadsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Threads> GetAll()
        {
            return _unitOfWork.Threads.GetAll();
        }

        public Threads GetById(string id)
        {
            return _unitOfWork.Threads.GetById(id);
        }

        public (bool success, string message) Add(Threads entity)
        {
            try
            {
                _unitOfWork.Threads.Add(entity);
                _unitOfWork.Complete();
                return (true, "Threads added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding Threads: {ex.Message}");
            }
        }

        public (bool success, string message) Update(Threads entity)
        {
            try
            {
                _unitOfWork.Threads.Update(entity);
                _unitOfWork.Complete();
                return (true, "Threads updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating Threads: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.Threads.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.Threads.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<Threads> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.Threads.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
