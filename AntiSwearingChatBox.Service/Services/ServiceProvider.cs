using AntiSwearingChatBox.Repository.IRepositories;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Repository.Repositories;
using AntiSwearingChatBox.Service.Interfaces;
using System;

namespace AntiSwearingChatBox.Service
{
    public class ServiceProvider : IDisposable
    {
        private readonly AntiSwearingChatBoxContext _context;
        private readonly IUnitOfWork _unitOfWork;
                private IFilteredWordsService _filteredwordsService;
                private IMessageHistoryService _messagehistoryService;
                private IThreadParticipantsService _threadparticipantsService;
                private IThreadsService _threadsService;
                private IUsersService _usersService;
                private IUserWarningsService _userwarningsService;
                
        public ServiceProvider()
        {
            _context = new AntiSwearingChatBoxContext();
            _unitOfWork = new UnitOfWork(_context);
        }
                public IFilteredWordsService FilteredWordsService
        {
            get
            {
                if (_filteredwordsService == null)
                {
                    _filteredwordsService = new FilteredWordsService(_unitOfWork);
                }
                return _filteredwordsService;
            }
        }
                public IMessageHistoryService MessageHistoryService
        {
            get
            {
                if (_messagehistoryService == null)
                {
                    _messagehistoryService = new MessageHistoryService(_unitOfWork);
                }
                return _messagehistoryService;
            }
        }
                public IThreadParticipantsService ThreadParticipantsService
        {
            get
            {
                if (_threadparticipantsService == null)
                {
                    _threadparticipantsService = new ThreadParticipantsService(_unitOfWork);
                }
                return _threadparticipantsService;
            }
        }
                public IThreadsService ThreadsService
        {
            get
            {
                if (_threadsService == null)
                {
                    _threadsService = new ThreadsService(_unitOfWork);
                }
                return _threadsService;
            }
        }
                public IUsersService UsersService
        {
            get
            {
                if (_usersService == null)
                {
                    _usersService = new UsersService(_unitOfWork);
                }
                return _usersService;
            }
        }
                public IUserWarningsService UserWarningsService
        {
            get
            {
                if (_userwarningsService == null)
                {
                    _userwarningsService = new UserWarningsService(_unitOfWork);
                }
                return _userwarningsService;
            }
        }
                public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
