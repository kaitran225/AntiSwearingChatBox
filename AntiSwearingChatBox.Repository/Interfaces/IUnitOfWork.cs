using System;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();
        Task<int> CompleteAsync();
        IChatThreadRepository ChatThread { get; }
        IFilteredWordRepository FilteredWord { get; }
        IMessageHistoryRepository MessageHistory { get; }
        IThreadParticipantRepository ThreadParticipant { get; }
        IUserRepository User { get; }
        IUserWarningRepository UserWarning { get; }
    }
}
