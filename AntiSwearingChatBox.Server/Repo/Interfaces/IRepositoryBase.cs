using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AntiSwearingChatBox.Repository.Interfaces
{
    public interface IRepositoryBase<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(object id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        int Count(Expression<Func<T, bool>> expression);
        IEnumerable<T> Search(string searchTerm);
    }
}
