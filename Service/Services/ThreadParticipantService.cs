using Repository.IRepositories;
using Repository.Models;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service
{
    public class ThreadParticipantService : IThreadParticipantService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ThreadParticipantService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ThreadParticipant> GetAll()
        {
            return _unitOfWork.ThreadParticipant.GetAll();
        }

        public ThreadParticipant GetById(string id)
        {
            return _unitOfWork.ThreadParticipant.GetById(id);
        }

        public (bool success, string message) Add(ThreadParticipant entity)
        {
            try
            {
                _unitOfWork.ThreadParticipant.Add(entity);
                _unitOfWork.Complete();
                return (true, "ThreadParticipant added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding ThreadParticipant: {ex.Message}");
            }
        }

        public (bool success, string message) Update(ThreadParticipant entity)
        {
            try
            {
                _unitOfWork.ThreadParticipant.Update(entity);
                _unitOfWork.Complete();
                return (true, "ThreadParticipant updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating ThreadParticipant: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.ThreadParticipant.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.ThreadParticipant.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<ThreadParticipant> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.ThreadParticipant.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
