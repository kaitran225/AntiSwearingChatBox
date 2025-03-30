using AntiSwearingChatBox.Repository.Models;
using System;
using System.Threading.Tasks;
using AntiSwearingChatBox.Repository;
using AntiSwearingChatBox.Repository.Interfaces;

namespace AntiSwearingChatBox.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AntiSwearingChatBoxContext _context;
        private IChatThreadRepository? _chatthreadRepository;
        private IFilteredWordRepository? _filteredwordRepository;
        private IMessageHistoryRepository? _messagehistoryRepository;
        private IThreadParticipantRepository? _threadparticipantRepository;
        private IUserRepository? _userRepository;
        private IUserWarningRepository? _userwarningRepository;

        public UnitOfWork(AntiSwearingChatBoxContext context)
        {
            _context = context;
        }

        public IChatThreadRepository ChatThread
        {
            get
            {
                _chatthreadRepository ??= new ChatThreadRepository(_context);
                return _chatthreadRepository;
            }
        }
        public IFilteredWordRepository FilteredWord
        {
            get
            {
                _filteredwordRepository ??= new FilteredWordRepository(_context);
                return _filteredwordRepository;
            }
        }
        public IMessageHistoryRepository MessageHistory
        {
            get
            {
                _messagehistoryRepository ??= new MessageHistoryRepository(_context);
                return _messagehistoryRepository;
            }
        }
        public IThreadParticipantRepository ThreadParticipant
        {
            get
            {
                _threadparticipantRepository ??= new ThreadParticipantRepository(_context);
                return _threadparticipantRepository;
            }
        }
        public IUserRepository User
        {
            get
            {
                _userRepository ??= new UserRepository(_context);
                return _userRepository;
            }
        }
        public IUserWarningRepository UserWarning
        {
            get
            {
                _userwarningRepository ??= new UserWarningRepository(_context);
                return _userwarningRepository;
            }
        }


        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
