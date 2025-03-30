using Repository.IRepositories;
using Repository.Models;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<User> GetAll()
        {
            return _unitOfWork.User.GetAll();
        }

        public User GetById(string id)
        {
            return _unitOfWork.User.GetById(id);
        }

        public (bool success, string message) Add(User entity)
        {
            try
            {
                _unitOfWork.User.Add(entity);
                _unitOfWork.Complete();
                return (true, "User added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding User: {ex.Message}");
            }
        }

        public (bool success, string message) Update(User entity)
        {
            try
            {
                _unitOfWork.User.Update(entity);
                _unitOfWork.Complete();
                return (true, "User updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating User: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.User.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.User.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<User> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.User.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
