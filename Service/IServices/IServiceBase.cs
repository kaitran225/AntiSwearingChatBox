using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Service.IServices
{
    public interface IServiceBase<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        IEnumerable<T> Search(string searchTerm);
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        int Count(Expression<Func<T, bool>> expression);
    }
}
