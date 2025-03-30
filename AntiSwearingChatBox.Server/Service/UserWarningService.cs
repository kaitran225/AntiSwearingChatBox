using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Server.Repo.Models;
using AntiSwearingChatBox.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class UserWarningService : IUserWarningService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserWarningService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<UserWarning> GetAll()
        {
            return _unitOfWork.UserWarning.GetAll();
        }

        public UserWarning GetById(int id)
        {
            return _unitOfWork.UserWarning.GetById(id);
        }

        public (bool success, string message) Add(UserWarning entity)
        {
            try
            {
                _unitOfWork.UserWarning.Add(entity);
                _unitOfWork.Complete();
                return (true, "UserWarning added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding UserWarning: {ex.Message}");
            }
        }

        public (bool success, string message) Update(UserWarning entity)
        {
            try
            {
                _unitOfWork.UserWarning.Update(entity);
                _unitOfWork.Complete();
                return (true, "UserWarning updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating UserWarning: {ex.Message}");
            }
        }

        public bool Delete(int id)
        {
            var entity = _unitOfWork.UserWarning.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.UserWarning.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<UserWarning> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.UserWarning.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
