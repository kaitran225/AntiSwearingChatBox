using System;

namespace AntiSwearingChatBox.Repository.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        int Complete();        IFilteredWordsRepository FilteredWords { get; }
                IMessageHistoryRepository MessageHistory { get; }
                IThreadParticipantsRepository ThreadParticipants { get; }
                IThreadsRepository Threads { get; }
                IUsersRepository Users { get; }
                IUserWarningsRepository UserWarnings { get; }
            }
}
