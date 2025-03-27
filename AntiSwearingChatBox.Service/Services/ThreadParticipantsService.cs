using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class ThreadParticipantsService : IThreadParticipantsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ThreadParticipantsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ThreadParticipants> GetAll()
        {
            return _unitOfWork.ThreadParticipants.GetAll();
        }

        public ThreadParticipants GetById(string id)
        {
            return _unitOfWork.ThreadParticipants.GetById(id);
        }

        public (bool success, string message) Add(ThreadParticipants entity)
        {
            try
            {
                _unitOfWork.ThreadParticipants.Add(entity);
                _unitOfWork.Complete();
                return (true, "ThreadParticipants added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding ThreadParticipants: {ex.Message}");
            }
        }

        public (bool success, string message) Update(ThreadParticipants entity)
        {
            try
            {
                _unitOfWork.ThreadParticipants.Update(entity);
                _unitOfWork.Complete();
                return (true, "ThreadParticipants updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating ThreadParticipants: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.ThreadParticipants.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.ThreadParticipants.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<ThreadParticipants> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.ThreadParticipants.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
