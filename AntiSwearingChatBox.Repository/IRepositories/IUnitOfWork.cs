using System;

namespace AntiSwearingChatBox.Repository.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();        IFilteredWordRepository FilteredWord { get; }
                IMessageHistoryRepository MessageHistory { get; }
                IThreadRepository Thread { get; }
                IThreadParticipantRepository ThreadParticipant { get; }
                IUserRepository User { get; }
                IUserWarningRepository UserWarning { get; }
            }
}
