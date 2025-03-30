using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Service.Interface;


namespace AntiSwearingChatBox.Service
{
    public abstract class ServiceBase<T> : IServiceBase<T> where T : class
    {
        protected readonly IRepositoryBase<T> _repo;

        protected ServiceBase(IRepositoryBase<T> repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo), "Repository cannot be null.");
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _repo.GetAll();
        }

        public virtual T? GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");

            return _repo.GetById(id);
        }

        public virtual void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            _repo.Add(entity);
        }

        public virtual void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

            _repo.Update(entity);
        }

        public virtual void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");

            var entity = _repo.GetById(id);
            if (entity != null)
            {
                _repo.Delete(entity);
            }
        }

        public virtual IEnumerable<T> Search(string searchTerm)
        {
            return _repo.Search(searchTerm);
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return _repo.Find(expression);
        }

        public virtual int Count(Expression<Func<T, bool>> expression)
        {
            return _repo.Count(expression);
        }
    }
}
