using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class UsersService : IUsersService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsersService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Users> GetAll()
        {
            return _unitOfWork.Users.GetAll();
        }

        public Users GetById(string id)
        {
            return _unitOfWork.Users.GetById(id);
        }

        public (bool success, string message) Add(Users entity)
        {
            try
            {
                _unitOfWork.Users.Add(entity);
                _unitOfWork.Complete();
                return (true, "Users added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding Users: {ex.Message}");
            }
        }

        public (bool success, string message) Update(Users entity)
        {
            try
            {
                _unitOfWork.Users.Update(entity);
                _unitOfWork.Complete();
                return (true, "Users updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating Users: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.Users.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.Users.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<Users> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.Users.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
