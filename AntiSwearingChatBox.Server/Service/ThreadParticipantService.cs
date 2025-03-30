using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
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
        
        public IEnumerable<ThreadParticipant> GetByThreadId(int threadId)
        {
            return _unitOfWork.ThreadParticipant.Find(tp => tp.ThreadId == threadId);
        }
        
        public IEnumerable<ThreadParticipant> GetByUserId(int userId)
        {
            return _unitOfWork.ThreadParticipant.Find(tp => tp.UserId == userId);
        }

        public (bool success, string message) Add(ThreadParticipant entity)
        {
            try
            {
                // Check if participant already exists in thread
                var existingParticipant = _unitOfWork.ThreadParticipant.Find(
                    tp => tp.ThreadId == entity.ThreadId && tp.UserId == entity.UserId).FirstOrDefault();
                    
                if (existingParticipant != null)
                {
                    return (true, "User is already a participant in this thread");
                }
                
                _unitOfWork.ThreadParticipant.Add(entity);
                _unitOfWork.Complete();
                return (true, "Thread participant added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding thread participant: {ex.Message}");
            }
        }

        public (bool success, string message) Update(ThreadParticipant entity)
        {
            try
            {
                _unitOfWork.ThreadParticipant.Update(entity);
                _unitOfWork.Complete();
                return (true, "Thread participant updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating thread participant: {ex.Message}");
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
        
        public bool RemoveUserFromThread(int userId, int threadId)
        {
            try
            {
                var participant = _unitOfWork.ThreadParticipant.Find(
                    tp => tp.ThreadId == threadId && tp.UserId == userId).FirstOrDefault();
                    
                if (participant != null)
                {
                    _unitOfWork.ThreadParticipant.Delete(participant);
                    _unitOfWork.Complete();
                    return true;
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
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
