using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AntiSwearingChatBoxContext _context;
        private IFilteredWordsRepository _filteredwordsRepository;
        private IMessageHistoryRepository _messagehistoryRepository;
        private IThreadParticipantsRepository _threadparticipantsRepository;
        private IThreadsRepository _threadsRepository;
        private IUsersRepository _usersRepository;
        private IUserWarningsRepository _userwarningsRepository;

        public UnitOfWork(AntiSwearingChatBoxContext context)
        {
            _context = context;
        }

        public IFilteredWordsRepository FilteredWords
        {
            get
            {
                if (_filteredwordsRepository == null)
                {
                    _filteredwordsRepository = new FilteredWordsRepository(_context);
                }
                return _filteredwordsRepository;
            }
        }
        public IMessageHistoryRepository MessageHistory
        {
            get
            {
                if (_messagehistoryRepository == null)
                {
                    _messagehistoryRepository = new MessageHistoryRepository(_context);
                }
                return _messagehistoryRepository;
            }
        }
        public IThreadParticipantsRepository ThreadParticipants
        {
            get
            {
                if (_threadparticipantsRepository == null)
                {
                    _threadparticipantsRepository = new ThreadParticipantsRepository(_context);
                }
                return _threadparticipantsRepository;
            }
        }
        public IThreadsRepository Threads
        {
            get
            {
                if (_threadsRepository == null)
                {
                    _threadsRepository = new ThreadsRepository(_context);
                }
                return _threadsRepository;
            }
        }
        public IUsersRepository Users
        {
            get
            {
                if (_usersRepository == null)
                {
                    _usersRepository = new UsersRepository(_context);
                }
                return _usersRepository;
            }
        }
        public IUserWarningsRepository UserWarnings
        {
            get
            {
                if (_userwarningsRepository == null)
                {
                    _userwarningsRepository = new UserWarningsRepository(_context);
                }
                return _userwarningsRepository;
            }
        }
        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
