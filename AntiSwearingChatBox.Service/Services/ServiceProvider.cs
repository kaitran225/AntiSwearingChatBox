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
                private IFilteredWordService _filteredwordService;
                private IMessageHistoryService _messagehistoryService;
                private IThreadService _threadService;
                private IThreadParticipantService _threadparticipantService;
                private IUserService _userService;
                private IUserWarningService _userwarningService;
                
        public ServiceProvider()
        {
            _context = new AntiSwearingChatBoxContext();
            _unitOfWork = new UnitOfWork(_context);
        }
                public IFilteredWordService FilteredWordService
        {
            get
            {
                if (_filteredwordService == null)
                {
                    _filteredwordService = new FilteredWordService(_unitOfWork);
                }
                return _filteredwordService;
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
                public IThreadService ThreadService
        {
            get
            {
                if (_threadService == null)
                {
                    _threadService = new ThreadService(_unitOfWork);
                }
                return _threadService;
            }
        }
                public IThreadParticipantService ThreadParticipantService
        {
            get
            {
                if (_threadparticipantService == null)
                {
                    _threadparticipantService = new ThreadParticipantService(_unitOfWork);
                }
                return _threadparticipantService;
            }
        }
                public IUserService UserService
        {
            get
            {
                if (_userService == null)
                {
                    _userService = new UserService(_unitOfWork);
                }
                return _userService;
            }
        }
                public IUserWarningService UserWarningService
        {
            get
            {
                if (_userwarningService == null)
                {
                    _userwarningService = new UserWarningService(_unitOfWork);
                }
                return _userwarningService;
            }
        }
                public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
