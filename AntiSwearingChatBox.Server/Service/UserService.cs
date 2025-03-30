using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AntiSwearingChatBox.Service
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

        public User GetById(int id)
        {
            return _unitOfWork.User.GetById(id);
        }

        public User GetByUsername(string username)
        {
            var users = _unitOfWork.User.Find(u => u.Username.ToLower() == username.ToLower());
            return users.FirstOrDefault()!;
        }

        public (bool success, string message, User? user) Authenticate(string username, string password)
        {
            // Find user by username
            var user = GetByUsername(username);
            if (user == null)
                return (false, "User not found", null);

            // Verify password
            if (!VerifyPasswordHash(password, user.PasswordHash))
                return (false, "Incorrect password", null);

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            _unitOfWork.User.Update(user);
            _unitOfWork.Complete();

            return (true, "Authentication successful", user);
        }

        public (bool success, string message) Register(User user, string password)
        {
            try
            {
                // Check if username is already taken
                if (_unitOfWork.User.Find(u => u.Username.ToLower() == user.Username.ToLower()).Any())
                    return (false, "Username is already taken");

                // Check if email is already taken
                if (_unitOfWork.User.Find(u => u.Email.ToLower() == user.Email.ToLower()).Any())
                    return (false, "Email is already registered");

                // Hash password
                user.PasswordHash = CreatePasswordHash(password);
                
                // Set defaults
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.Role = "User";
                user.TrustScore = 1.0m;

                // Add user
                _unitOfWork.User.Add(user);
                _unitOfWork.Complete();
                
                return (true, "User registered successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error registering user: {ex.Message}");
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
                return (false, $"Error updating user: {ex.Message}");
            }
        }

        public bool Delete(int id)
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

        #region Password Helpers

        private string CreatePasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            using var sha256 = SHA256.Create();
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(passwordBytes);
            
            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (string.IsNullOrEmpty(storedHash))
                throw new ArgumentException("Stored hash cannot be null or empty", nameof(storedHash));

            var computedHash = CreatePasswordHash(password);
            return computedHash == storedHash;
        }

        #endregion
    }
}
