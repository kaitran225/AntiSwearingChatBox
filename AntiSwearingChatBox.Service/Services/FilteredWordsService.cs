using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class FilteredWordsService : IFilteredWordsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FilteredWordsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<FilteredWords> GetAll()
        {
            return _unitOfWork.FilteredWords.GetAll();
        }

        public FilteredWords GetById(string id)
        {
            return _unitOfWork.FilteredWords.GetById(id);
        }

        public (bool success, string message) Add(FilteredWords entity)
        {
            try
            {
                _unitOfWork.FilteredWords.Add(entity);
                _unitOfWork.Complete();
                return (true, "FilteredWords added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding FilteredWords: {ex.Message}");
            }
        }

        public (bool success, string message) Update(FilteredWords entity)
        {
            try
            {
                _unitOfWork.FilteredWords.Update(entity);
                _unitOfWork.Complete();
                return (true, "FilteredWords updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating FilteredWords: {ex.Message}");
            }
        }

        public bool Delete(string id)
        {
            var entity = _unitOfWork.FilteredWords.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.FilteredWords.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<FilteredWords> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.FilteredWords.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
