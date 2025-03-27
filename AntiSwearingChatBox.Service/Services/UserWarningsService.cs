using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class UserWarningsService : IUserWarningsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserWarningsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<UserWarnings> GetAll()
        {
            return _unitOfWork.UserWarnings.GetAll();
        }

        public UserWarnings GetById(string id)
        {
            return _unitOfWork.UserWarnings.GetById(id);
        }

        public (bool success, string message) Add(UserWarnings entity)
        {
            try
            {
                _unitOfWork.UserWarnings.Add(entity);
                _unitOfWork.Complete();
                return (true, "UserWarnings added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding UserWarnings: {ex.Message}");
            }
        }

        public (bool success, string message) Update(UserWarnings entity)
        {
            try
            {
                _unitOfWork.UserWarnings.Update(entity);
                _unitOfWork.Complete();
                return (true, "UserWarnings updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating UserWarnings: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.UserWarnings.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.UserWarnings.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<UserWarnings> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.UserWarnings.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
