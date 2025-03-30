using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AntiSwearingChatBox.Service
{
    public class FilteredWordService : IFilteredWordService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FilteredWordService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<FilteredWord> GetAll()
        {
            return _unitOfWork.FilteredWord.GetAll();
        }

        public FilteredWord GetById(int id)
        {
            return _unitOfWork.FilteredWord.GetById(id);
        }

        public (bool success, string message) Add(FilteredWord entity)
        {
            try
            {
                _unitOfWork.FilteredWord.Add(entity);
                _unitOfWork.Complete();
                return (true, "FilteredWord added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding FilteredWord: {ex.Message}");
            }
        }

        public (bool success, string message) Update(FilteredWord entity)
        {
            try
            {
                _unitOfWork.FilteredWord.Update(entity);
                _unitOfWork.Complete();
                return (true, "FilteredWord updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating FilteredWord: {ex.Message}");
            }
        }

        public bool Delete(int id)
        {
            var entity = _unitOfWork.FilteredWord.GetById(id);
            if (entity == null)
                return false;

            _unitOfWork.FilteredWord.Delete(entity);
            _unitOfWork.Complete();
            return true;
        }

        public IEnumerable<FilteredWord> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            return _unitOfWork.FilteredWord.Find(x => 
                x.ToString()!.ToLower().Contains(searchTerm.ToLower()));
        }
    }
}
