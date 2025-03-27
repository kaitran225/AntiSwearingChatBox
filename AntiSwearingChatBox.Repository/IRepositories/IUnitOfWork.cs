using System;

namespace AntiSwearingChatBox.Repository.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();        IChatThreadRepository ChatThread { get; }
                IFilteredWordRepository FilteredWord { get; }
                IMessageHistoryRepository MessageHistory { get; }
                IThreadParticipantRepository ThreadParticipant { get; }
                IUserRepository User { get; }
                IUserWarningRepository UserWarning { get; }
            }
}
