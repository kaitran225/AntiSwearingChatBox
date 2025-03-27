using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.IRepositories;
using System;

namespace AntiSwearingChatBox.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AntiSwearingChatBoxContext _context;
        private IFilteredWordRepository _filteredwordRepository;
        private IMessageHistoryRepository _messagehistoryRepository;
        private IThreadRepository _threadRepository;
        private IThreadParticipantRepository _threadparticipantRepository;
        private IUserRepository _userRepository;
        private IUserWarningRepository _userwarningRepository;

        public UnitOfWork(AntiSwearingChatBoxContext context)
        {
            _context = context;
        }

        public IFilteredWordRepository FilteredWord
        {
            get
            {
                if (_filteredwordRepository == null)
                {
                    _filteredwordRepository = new FilteredWordRepository(_context);
                }
                return _filteredwordRepository;
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
        public IThreadRepository Thread
        {
            get
            {
                if (_threadRepository == null)
                {
                    _threadRepository = new ThreadRepository(_context);
                }
                return _threadRepository;
            }
        }
        public IThreadParticipantRepository ThreadParticipant
        {
            get
            {
                if (_threadparticipantRepository == null)
                {
                    _threadparticipantRepository = new ThreadParticipantRepository(_context);
                }
                return _threadparticipantRepository;
            }
        }
        public IUserRepository User
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(_context);
                }
                return _userRepository;
            }
        }
        public IUserWarningRepository UserWarning
        {
            get
            {
                if (_userwarningRepository == null)
                {
                    _userwarningRepository = new UserWarningRepository(_context);
                }
                return _userwarningRepository;
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
