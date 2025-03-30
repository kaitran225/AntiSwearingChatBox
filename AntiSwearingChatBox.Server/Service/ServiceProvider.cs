using AntiSwearingChatBox.Repository;
using AntiSwearingChatBox.Repository.Interfaces;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;
using System;

namespace AntiSwearingChatBox.Service
{
    public class ServiceProvider : IDisposable
    {
        private readonly AntiSwearingChatBoxContext _context;
        private readonly IUnitOfWork _unitOfWork;
                private IChatThreadService _chatthreadService;
                private IFilteredWordService _filteredwordService;
                private IMessageHistoryService _messagehistoryService;
                private IThreadParticipantService _threadparticipantService;
                private IUserService _userService;
                private IUserWarningService _userwarningService;
                
        public ServiceProvider()
        {
            _context = new AntiSwearingChatBoxContext();
            _unitOfWork = new UnitOfWork(_context);
        }
                public IChatThreadService ChatThreadService
        {
            get
            {
                if (_chatthreadService == null)
                {
                    _chatthreadService = new ChatThreadService(_unitOfWork);
                }
                return _chatthreadService;
            }
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
